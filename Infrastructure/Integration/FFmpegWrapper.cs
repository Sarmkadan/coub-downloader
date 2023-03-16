// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using CoubDownloader.Infrastructure.Middleware;

namespace CoubDownloader.Infrastructure.Integration;

/// <summary>Wrapper for FFmpeg command-line tool</summary>
public class FFmpegWrapper
{
    private readonly string _ffmpegPath;
    private readonly ILoggingService _logger;

    public FFmpegWrapper(string ffmpegPath = "ffmpeg", ILoggingService? logger = null)
    {
        _ffmpegPath = ffmpegPath;
        _logger = logger ?? new MemoryLoggingService();
    }

    /// <summary>Check if FFmpeg is available</summary>
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var result = await ExecuteAsync(new[] { "-version" }, TimeSpan.FromSeconds(5));
            return result.Success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Get FFmpeg version</summary>
    public async Task<string> GetVersionAsync()
    {
        try
        {
            var result = await ExecuteAsync(new[] { "-version" }, TimeSpan.FromSeconds(5));
            if (!result.Success) return "Unknown";

            var lines = result.Output.Split(Environment.NewLine);
            return lines.FirstOrDefault()?.Trim() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>Execute FFmpeg command</summary>
    public async Task<FFmpegResult> ExecuteAsync(string[] arguments, TimeSpan? timeout = null)
    {
        var processTimeout = timeout ?? TimeSpan.FromMinutes(10);

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            foreach (var arg in arguments)
                psi.ArgumentList.Add(arg);

            using var process = Process.Start(psi);

            if (process == null)
                return new FFmpegResult { Success = false, Error = "Failed to start FFmpeg process" };

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit((int)processTimeout.TotalMilliseconds))
            {
                process.Kill();
                return new FFmpegResult
                {
                    Success = false,
                    Error = "FFmpeg operation timed out"
                };
            }

            var success = process.ExitCode == 0;

            _logger.LogDebug(
                $"FFmpeg command: {string.Join(" ", arguments)} - Exit code: {process.ExitCode}",
                "FFmpeg");

            return new FFmpegResult
            {
                Success = success,
                Output = output,
                Error = error,
                ExitCode = process.ExitCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("FFmpeg execution failed", ex, "FFmpeg");
            return new FFmpegResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    /// <summary>Convert video file with progress callback</summary>
    public async Task<FFmpegResult> ConvertVideoAsync(
        string inputFile,
        string outputFile,
        ConversionParameters parameters,
        IProgress<int>? progress = null)
    {
        var args = new List<string>
        {
            "-i", inputFile,
            "-c:v", parameters.VideoCodec,
            "-c:a", parameters.AudioCodec,
            "-b:v", $"{parameters.VideoBitrate}k",
            "-b:a", $"{parameters.AudioBitrate}k",
            "-r", parameters.FrameRate.ToString()
        };

        if (parameters.Width > 0 && parameters.Height > 0)
            args.AddRange(new[] { "-vf", $"scale={parameters.Width}:{parameters.Height}" });

        if (parameters.UseHardwareAcceleration)
            args.InsertRange(0, new[] { "-hwaccel", "auto" });

        args.AddRange(new[] { "-progress", "pipe:1", "-y", outputFile });

        return await ExecuteAsync(args.ToArray());
    }

    /// <summary>Extract audio from video</summary>
    public async Task<FFmpegResult> ExtractAudioAsync(string inputFile, string outputFile)
    {
        var args = new[]
        {
            "-i", inputFile,
            "-q:a", "0",
            "-map", "a",
            "-y", outputFile
        };

        return await ExecuteAsync(args);
    }

    /// <summary>Concatenate multiple videos</summary>
    public async Task<FFmpegResult> ConcatenateVideosAsync(
        List<string> inputFiles,
        string outputFile)
    {
        var concatFile = Path.GetTempFileName();

        try
        {
            var concatContent = string.Join(Environment.NewLine,
                inputFiles.Select(f => $"file '{Path.GetFullPath(f)}'"));

            File.WriteAllText(concatFile, concatContent);

            var args = new[]
            {
                "-f", "concat",
                "-safe", "0",
                "-i", concatFile,
                "-c", "copy",
                "-y", outputFile
            };

            return await ExecuteAsync(args);
        }
        finally
        {
            try { File.Delete(concatFile); } catch { }
        }
    }

    /// <summary>Loop audio to match video duration</summary>
    public async Task<FFmpegResult> LoopAudioAsync(
        string audioFile,
        double targetDuration,
        string outputFile)
    {
        var args = new[]
        {
            "-stream_loop", "-1",
            "-i", audioFile,
            "-t", targetDuration.ToString("F2"),
            "-c:a", "aac",
            "-y", outputFile
        };

        return await ExecuteAsync(args);
    }
}

/// <summary>Parameters for video conversion</summary>
public class ConversionParameters
{
    public string VideoCodec { get; set; } = "libx264";
    public string AudioCodec { get; set; } = "aac";
    public int VideoBitrate { get; set; } = 5000; // kbps
    public int AudioBitrate { get; set; } = 128; // kbps
    public int FrameRate { get; set; } = 30;
    public int Width { get; set; }
    public int Height { get; set; }
    public bool UseHardwareAcceleration { get; set; }
}

/// <summary>FFmpeg command execution result</summary>
public class FFmpegResult
{
    public bool Success { get; set; }
    public string Output { get; set; } = "";
    public string Error { get; set; } = "";
    public int ExitCode { get; set; }
}
