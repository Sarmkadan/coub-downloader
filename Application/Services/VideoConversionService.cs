// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
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
            var exitCode = await RunFfmpegAsync(ffmpegArgs, progress, cancellationToken);
            if (exitCode != 0)
                throw new VideoConversionException($"FFmpeg exited with code {exitCode}", inputPath, outputPath);

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

        var fileInfo = new FileInfo(filePath);
        var metadata = new VideoMetadata { FileSizeBytes = fileInfo.Length };

        // In a real implementation, would parse ffprobe JSON output
        // For now, return basic metadata
        return await Task.FromResult(metadata);
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

        var exitCode = await RunFfmpegAsync(args, null, cancellationToken);
        if (exitCode != 0)
            throw new VideoConversionException($"Failed to apply audio track", videoPath, outputPath);

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

        var exitCode = await RunFfmpegAsync(args, null, cancellationToken);
        if (exitCode != 0)
            throw new VideoConversionException($"Failed to rescale video to {width}x{height}", inputPath, outputPath);

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
    private async Task<int> RunFfmpegAsync(string arguments, IProgress<int>? progress, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = _ffmpegPath,
            Arguments = arguments,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process is null)
            throw new ToolNotFoundException(ApplicationConstants.FFmpegExecutable);

        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
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
