#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
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
public class VideoConversionService : IVideoConversionService
{
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

        if (!File.Exists(inputPath))
            throw new FileNotFoundException($"Input video file not found: {inputPath}");

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var ffmpegArgs = BuildConversionCommand(inputPath, outputPath, settings);

        try
        {
            var (exitCode, _, standardError) = await RunFfmpegAsync(ffmpegArgs, progress, cancellationToken);
            if (exitCode != 0)
                throw new VideoConversionException($"FFmpeg exited with code {exitCode}. Error: {standardError}", inputPath, outputPath);

            return outputPath;
        }
        catch (Exception ex) when (!(ex is VideoConversionException))
        {
            throw new VideoConversionException(ex.Message, inputPath, outputPath);
        }
    }

    public async Task<VideoMetadata> GetVideoMetadataAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Video file not found: {filePath}");

        var ffprobeArgs = $"-v error -select_streams v:0 -show_entries stream=width,height,duration,codec_name,r_frame_rate,bit_rate -show_entries format=size,duration,bit_rate,format_name -of json \"{filePath}\"";

        var (exitCode, standardOutput, standardError) = await RunFfmpegAsync(ffprobeArgs, null, cancellationToken);

        if (exitCode != 0)
            throw new VideoConversionException($"FFprobe exited with code {exitCode}. Error: {standardError}", filePath, "metadata_extraction");

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
                    metadata.FileSizeBytes = formatElement.TryGetProperty("size", out var size) && long.TryParse(size.GetString(), out var fileSize) ? fileSize : 0;
                    metadata.Duration = formatElement.TryGetProperty("duration", out var duration) && double.TryParse(duration.GetString(), out var dur) ? dur : 0;
                    metadata.VideoBitrate = formatElement.TryGetProperty("bit_rate", out var formatBitRate) && int.TryParse(formatBitRate.GetString(), out var fb) ? fb : 0;
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
                                if (parts?.Length == 2 && int.TryParse(parts[0], out var num) && int.TryParse(parts[1], out var den) && den != 0)
                                {
                                    metadata.FrameRate = num / den;
                                }
                            }
                            // If stream has its own bitrate, use it. Otherwise, rely on format bitrate.
                            metadata.VideoBitrate = stream.TryGetProperty("bit_rate", out var streamBitRate) && int.TryParse(streamBitRate.GetString(), out var sb) ? sb : metadata.VideoBitrate;
                        }
                        else if (codecType == "audio")
                        {
                            metadata.AudioCodec = stream.TryGetProperty("codec_name", out var codecName) ? codecName.GetString() : null;
                            metadata.AudioBitrate = stream.TryGetProperty("bit_rate", out var streamBitRate) && int.TryParse(streamBitRate.GetString(), out var sb) ? sb : 0;
                            metadata.HasAudio = true;
                        }
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            throw new VideoConversionException($"Failed to parse ffprobe JSON output: {ex.Message}", filePath, "metadata_extraction", ex);
        }

        return metadata;
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

        if (!File.Exists(videoPath))
            throw new FileNotFoundException($"Video file not found: {videoPath}");
        if (!File.Exists(audioPath))
            throw new FileNotFoundException($"Audio file not found: {audioPath}");

        var args = $"-i \"{videoPath}\" -i \"{audioPath}\" -c:v copy -c:a {settings.AudioCodec} " +
                   $"-b:a {settings.AudioBitrate}k -shortest \"{outputPath}\" -y";

        var (exitCode, _, standardError) = await RunFfmpegAsync(args, null, cancellationToken);
        if (exitCode != 0)
            throw new VideoConversionException($"Failed to apply audio track. Error: {standardError}", videoPath, outputPath);

        return outputPath;
    }

    public async Task<bool> IsFfmpegAvailableAsync()
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
            if (process is null) return false;

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetFfmpegVersionAsync()
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

    public async Task<string> RescaleVideoAsync(
        string inputPath,
        string outputPath,
        int width,
        int height,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        if (width <= 0 || height <= 0)
            throw new ArgumentException("Width and height must be greater than 0");

        var args = $"-i \"{inputPath}\" -vf scale={width}:{height} -c:v h264 -crf 23 " +
                   $"-c:a aac -b:a 128k \"{outputPath}\" -y";

        var (exitCode, _, standardError) = await RunFfmpegAsync(args, null, cancellationToken);
        if (exitCode != 0)
            throw new VideoConversionException($"Failed to rescale video to {width}x{height}. Error: {standardError}", inputPath, outputPath);

        return outputPath;
    }

    /// <summary>Build FFmpeg command line arguments for conversion</summary>
    private static string BuildConversionCommand(string inputPath, string outputPath, ConversionSettings settings)
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
            fades = $",fade=t=in:st=0:d={fadeIn},fade=t=out:st=10:d={fadeOut}";
        }

        var args = $"-i \"{inputPath}\" -vf \"{scaleFilter}{fades}\" -r {settings.FrameRate} " +
                   $"{codecParams} -preset {VideoProcessingConstants.FFmpegPreset} " +
                   $"\"{outputPath}\" -y";

        return args;
    }

    /// <summary>Run FFmpeg process with arguments</summary>
    private async Task<(int ExitCode, string StandardOutput, string StandardError)> RunFfmpegAsync(
        string arguments,
        IProgress<int>? progress,
        CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = _ffmpegPath,
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

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null) stdOutput.WriteLine(e.Data);
        };
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null) stdError.WriteLine(e.Data);
            // Optionally, parse FFmpeg progress here and report to 'progress'
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);
        
        return (process.ExitCode, stdOutput.ToString(), stdError.ToString());
    }

    /// <summary>Convert video to YouTube Shorts / TikTok 9:16 vertical format (1080x1920)</summary>
    public async Task<string> ConvertToShortsAsync(
        string inputPath,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        if (!File.Exists(inputPath))
            throw new FileNotFoundException($"Input video file not found: {inputPath}");

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
            throw new VideoConversionException(
                $"Failed to convert video to Shorts format. Error: {standardError}",
                inputPath, outputPath);

        return outputPath;
    }

    /// <summary>Resolve executable path from PATH environment variable</summary>
    private static string ResolveExecutable(string executableName)
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
}
