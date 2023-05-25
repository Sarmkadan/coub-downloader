#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using CoubDownloader.Domain.Constants;
using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Exceptions;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Service for FFmpeg-based video conversion and processing.
/// </summary>
public partial class VideoConversionService : IVideoConversionService
{
    [GeneratedRegex(@"Duration:\s*(\d{2,}:\d{2}:\d{2}\.\d+)", RegexOptions.CultureInvariant)]
    private static partial Regex DurationRegex();

    [GeneratedRegex(@"\btime=(\d{2,}:\d{2}:\d{2}\.\d+)", RegexOptions.CultureInvariant)]
    private static partial Regex ProgressTimeRegex();

    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;

    public VideoConversionService()
    {
        _ffmpegPath = ResolveExecutable(ApplicationConstants.FFmpegExecutable);
        _ffprobePath = ResolveExecutable(ApplicationConstants.FFprobeExecutable);
    }

    public async Task<string> ConvertVideoAsync(
        string inputPath,
        string outputPath,
        ConversionSettings settings,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            if (!File.Exists(inputPath))
                throw new FileOperationException("Input video file not found", inputPath, FileOperationType.Read);

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var ffmpegArgs = BuildConversionCommand(inputPath, outputPath, settings);

            var (exitCode, _, standardError) = await RunFfmpegAsync(ffmpegArgs, progress, cancellationToken);

            if (exitCode != 0)
                throw new ProcessExecutionException(
                    $"FFmpeg exited with code {exitCode}",
                    ApplicationConstants.FFmpegExecutable,
                    ffmpegArgs,
                    exitCode,
                    standardError);

            return outputPath;
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not FileOperationException and not ProcessExecutionException)
        {
            throw new VideoConversionException(ex.Message, inputPath, outputPath, ex);
        }
    }

    public async Task<VideoMetadata> GetVideoMetadataAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        try
        {
            if (!File.Exists(filePath))
                throw new FileOperationException("Video file not found", filePath, FileOperationType.Read);

            var ffprobeArgs = $"{_ffprobePath} -v error -select_streams v:0 -show_entries stream=width,height,duration,codec_name,r_frame_rate,bit_rate -show_entries format=size,duration,bit_rate,format_name -of json \"{filePath}\"";

            var (exitCode, standardOutput, standardError) = await RunFfmpegAsync(ffprobeArgs, null, cancellationToken);

            if (exitCode != 0)
                throw new ProcessExecutionException(
                    $"FFprobe exited with code {exitCode}",
                    ApplicationConstants.FFprobeExecutable,
                    ffprobeArgs,
                    exitCode,
                    standardError);

            var metadata = new VideoMetadata();

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(standardOutput))
                {
                    var root = doc.RootElement;

                    // Format information
                    if (root.TryGetProperty("format", out JsonElement formatElement))
                    {
                        metadata.Format = formatElement.TryGetProperty("format_name", out var formatName) ? formatName.GetString() : null;
                        metadata.FileSizeBytes = formatElement.TryGetProperty("size", out var size) && long.TryParse(size.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var fileSize) ? fileSize : 0;
                        metadata.Duration = formatElement.TryGetProperty("duration", out var duration) && double.TryParse(duration.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var dur) ? dur : 0;
                        metadata.VideoBitrate = formatElement.TryGetProperty("bit_rate", out var formatBitRate) && int.TryParse(formatBitRate.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var fb) ? fb : 0;
                    }

                    // Stream information
                    if (root.TryGetProperty("streams", out JsonElement streamsElement) && streamsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var stream in streamsElement.EnumerateArray())
                        {
                            var codecType = stream.TryGetProperty("codec_type", out var ct) ? ct.GetString() : null;

                            if (codecType == "video")
                            {
                                metadata.Width = stream.TryGetProperty("width", out var width) ? width.GetInt32() : 0;
                                metadata.Height = stream.TryGetProperty("height", out var height) ? height.GetInt32() : 0;
                                metadata.VideoCodec = stream.TryGetProperty("codec_name", out var codecName) ? codecName.GetString() : null;
                                if (stream.TryGetProperty("r_frame_rate", out var frameRateString))
                                {
                                    var parts = frameRateString.GetString()?.Split('/');
                                    if (parts?.Length == 2
                                        && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var num)
                                        && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var den)
                                        && den != 0)
                                    {
                                        metadata.FrameRate = (int)Math.Round((double)num / den);
                                    }
                                }
                                // If stream has its own bitrate, use it. Otherwise, rely on format bitrate.
                                metadata.VideoBitrate = stream.TryGetProperty("bit_rate", out var streamBitRate) && int.TryParse(streamBitRate.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var sb) ? sb : metadata.VideoBitrate;
                            }
                            else if (codecType == "audio")
                            {
                                metadata.AudioCodec = stream.TryGetProperty("codec_name", out var codecName) ? codecName.GetString() : null;
                                metadata.AudioBitrate = stream.TryGetProperty("bit_rate", out var streamBitRate) && int.TryParse(streamBitRate.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var sb) ? sb : 0;
                                metadata.HasAudio = true;
                            }
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                throw new ProcessExecutionException(
                    $"Failed to parse ffprobe JSON output: {ex.Message}",
                    ApplicationConstants.FFprobeExecutable,
                    ffprobeArgs,
                    0,
                    ex.Message);
            }

            return metadata;
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not FileOperationException and not ProcessExecutionException)
        {
            throw new VideoConversionException("Failed to get video metadata", filePath, "metadata_extraction", ex);
        }
    }

    public async Task<string> ApplyAudioTrackAsync(
        string videoPath,
        string audioPath,
        string outputPath,
        ConversionSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(audioPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            if (!File.Exists(videoPath))
                throw new FileOperationException("Video file not found", videoPath, FileOperationType.Read);
            if (!File.Exists(audioPath))
                throw new FileOperationException("Audio file not found", audioPath, FileOperationType.Read);

            var args = $"-i \"{videoPath}\" -i \"{audioPath}\" -c:v copy -c:a {settings.AudioCodec} " +
                      $"-b:a {settings.AudioBitrate}k -shortest \"{outputPath}\" -y";

            var (exitCode, _, standardError) = await RunFfmpegAsync(args, null, cancellationToken);

            if (exitCode != 0)
                throw new ProcessExecutionException(
                    "Failed to apply audio track",
                    ApplicationConstants.FFmpegExecutable,
                    args,
                    exitCode,
                    standardError);

            return outputPath;
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not FileOperationException and not ProcessExecutionException)
        {
            throw new VideoConversionException(ex.Message, videoPath, outputPath, ex);
        }
    }

    public virtual async Task<bool> IsFfmpegAvailableAsync()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = "-version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process is null)
                return false;

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            throw new ToolNotFoundException(ApplicationConstants.FFmpegExecutable, ex);
        }
    }

    public virtual async Task<string> GetFfmpegVersionAsync()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(_ffmpegPath);

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = "-version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process is null)
                throw new ToolNotFoundException(ApplicationConstants.FFmpegExecutable);

            var output = await process.StandardOutput.ReadLineAsync();
            await process.WaitForExitAsync();

            return output ?? "Unknown version";
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ToolNotFoundException(ApplicationConstants.FFmpegExecutable, ex);
        }
    }

    public async Task<string> RescaleVideoAsync(
        string inputPath,
        string outputPath,
        int width,
        int height,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        try
        {
            if (width <= 0 || height <= 0)
                throw new ValidationException("Width and height must be greater than 0", nameof(width), width);

            if (!File.Exists(inputPath))
                throw new FileOperationException("Input video file not found", inputPath, FileOperationType.Read);

            var args = $"-i \"{inputPath}\" -vf scale={width}:{height} -c:v h264 -crf 23 " +
                      $"-c:a aac -b:a 128k \"{outputPath}\" -y";

            var (exitCode, _, standardError) = await RunFfmpegAsync(args, null, cancellationToken);

            if (exitCode != 0)
                throw new ProcessExecutionException(
                    $"Failed to rescale video to {width}x{height}",
                    ApplicationConstants.FFmpegExecutable,
                    args,
                    exitCode,
                    standardError);

            return outputPath;
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not FileOperationException and not ProcessExecutionException and not ValidationException)
        {
            throw new VideoConversionException(ex.Message, inputPath, outputPath, ex);
        }
    }

    /// <summary>Build FFmpeg command line arguments for conversion</summary>
    private static string BuildConversionCommand(string inputPath, string outputPath, ConversionSettings settings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            var codecParams = settings.GetFFmpegCodecParams();
            var scaleFilter = settings.PreserveAspectRatio
                ? $"scale={settings.Width}:{settings.Height}:force_original_aspect_ratio=decrease"
                : $"scale={settings.Width}:{settings.Height}";

            var fades = string.Empty;
            if (settings.ApplyFades)
            {
                var fadeIn = settings.FadeInMs / 1000.0;
                var fadeOut = settings.FadeOutMs / 1000.0;
                fades = FormattableString.Invariant($",fade=t=in:st=0:d={fadeIn},fade=t=out:st=10:d={fadeOut}");
            }

            var args = $"-i \"{inputPath}\" -vf \"{scaleFilter}{fades}\" -r {settings.FrameRate} " +
                      $"{codecParams} -preset {VideoProcessingConstants.FFmpegPreset} " +
                      $"\"{outputPath}\" -y";

            return args;
        }
        catch (Exception ex)
        {
            throw new ValidationException("Failed to build FFmpeg conversion command", nameof(settings), settings, ex);
        }
    }

    /// <summary>
    /// Run an FFmpeg tool process with arguments. When the argument string starts with the
    /// resolved ffprobe or ffmpeg path followed by a space, that executable is used and the
    /// prefix is stripped; otherwise the arguments are passed to ffmpeg.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="arguments"/> is null or whitespace.</exception>
    /// <exception cref="ToolNotFoundException">Thrown when the process cannot be started.</exception>
    /// <exception cref="ProcessExecutionException">Thrown when process execution fails unexpectedly.</exception>
    protected internal virtual async Task<(int ExitCode, string StandardOutput, string StandardError)> RunFfmpegAsync(
        string arguments,
        IProgress<int>? progress,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(arguments);

        var fileName = _ffmpegPath;
        if (arguments.StartsWith(_ffprobePath + " ", StringComparison.Ordinal))
        {
            fileName = _ffprobePath;
            arguments = arguments[(_ffprobePath.Length + 1)..];
        }
        else if (arguments.StartsWith(_ffmpegPath + " ", StringComparison.Ordinal))
        {
            arguments = arguments[(_ffmpegPath.Length + 1)..];
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process is null)
                throw new ToolNotFoundException(ApplicationConstants.FFmpegExecutable);

            var stdOutput = new StringWriter();
            var stdError = new StringWriter();
            var totalDurationSeconds = 0.0;
            var errorLock = new object();

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    lock (stdOutput)
                    {
                        stdOutput.WriteLine(e.Data);
                    }
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data is null)
                    return;

                lock (errorLock)
                {
                    stdError.WriteLine(e.Data);
                }

                if (progress is null)
                    return;

                // FFmpeg writes "Duration: HH:MM:SS.cc" once per input and
                // "time=HH:MM:SS.cc" on every status line; combine them into a percentage.
                var durationMatch = DurationRegex().Match(e.Data);
                if (durationMatch.Success && TryParseFfmpegTimestamp(durationMatch.Groups[1].Value, out var total))
                {
                    Interlocked.Exchange(ref totalDurationSeconds, total);
                    return;
                }

                var timeMatch = ProgressTimeRegex().Match(e.Data);
                var knownDuration = Volatile.Read(ref totalDurationSeconds);
                if (timeMatch.Success && knownDuration > 0 && TryParseFfmpegTimestamp(timeMatch.Groups[1].Value, out var elapsed))
                {
                    var percent = (int)Math.Clamp(elapsed / knownDuration * 100.0, 0, 100);
                    progress.Report(percent);
                }
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            try
            {
                await process.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    if (!process.HasExited)
                        process.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException)
                {
                    // Process exited between the check and the kill.
                }

                throw;
            }

            if (process.ExitCode == 0)
                progress?.Report(100);

            string standardOutput;
            string standardError;
            lock (stdOutput)
            {
                standardOutput = stdOutput.ToString();
            }
            lock (errorLock)
            {
                standardError = stdError.ToString();
            }

            return (process.ExitCode, standardOutput, standardError);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not ToolNotFoundException)
        {
            throw new ProcessExecutionException(
                $"Failed to execute FFmpeg process",
                ApplicationConstants.FFmpegExecutable,
                arguments,
                0,
                ex.Message);
        }
    }

    /// <summary>Convert video to YouTube Shorts / TikTok 9:16 vertical format (1080x1920)</summary>
    public async Task<string> ConvertToShortsAsync(
        string inputPath,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        try
        {
            if (!File.Exists(inputPath))
                throw new FileOperationException("Input video file not found", inputPath, FileOperationType.Read);

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            // Scale source to fill 1080x1920 and blur it for the background, then overlay
            // the source scaled to fit (letterbox) centred on top.
            const string shortsFilter =
                "[0:v]split[a][b];" +
                "[a]scale=1080:1920:force_original_aspect_ratio=increase,crop=1080:1920,boxblur=20:5[blurred];" +
                "[b]scale=1080:1920:force_original_aspect_ratio=decrease[fg];" +
                "[blurred][fg]overlay=(W-w)/2:(H-h)/2";

            var args =
                $"-i \"{inputPath}\" " +
                $"-vf \"{shortsFilter}\" " +
                $"-c:v h264 -crf {VideoProcessingConstants.FFmpegCRF} -preset {VideoProcessingConstants.FFmpegPreset} " +
                $"-c:a aac -b:a {VideoProcessingConstants.DefaultAudioBitrate}k " +
                $"\"{outputPath}\" -y";

            var (exitCode, _, standardError) = await RunFfmpegAsync(args, null, cancellationToken);

            if (exitCode != 0)
                throw new ProcessExecutionException(
                    "Failed to convert video to Shorts format",
                    ApplicationConstants.FFmpegExecutable,
                    args,
                    exitCode,
                    standardError);

            return outputPath;
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not FileOperationException and not ProcessExecutionException)
        {
            throw new VideoConversionException(ex.Message, inputPath, outputPath, ex);
        }
    }

    /// <summary>Parse an FFmpeg "HH:MM:SS.cc" timestamp into total seconds</summary>
    private static bool TryParseFfmpegTimestamp(string value, out double seconds)
    {
        seconds = 0;

        var parts = value.Split(':');
        if (parts.Length != 3)
            return false;

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var hours)
            || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutes)
            || !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var secondsPart))
        {
            return false;
        }

        seconds = hours * 3600 + minutes * 60 + secondsPart;
        return true;
    }

    /// <summary>Resolve executable path from PATH environment variable</summary>
    private static string ResolveExecutable(string executableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executableName);

        try
        {
            if (File.Exists(executableName))
                return executableName;

            var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            var paths = pathEnv.Split(Path.PathSeparator);

            foreach (var path in paths)
            {
                var fullPath = Path.Combine(path, executableName);
                if (File.Exists(fullPath))
                    return fullPath;

                var exePath = Path.Combine(path, $"{executableName}.exe");
                if (File.Exists(exePath))
                    return exePath;
            }

            return executableName;
        }
        catch (Exception ex)
        {
            throw new ToolNotFoundException(executableName, ex);
        }
    }
}