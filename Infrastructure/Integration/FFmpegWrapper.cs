#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoubDownloader.Infrastructure.Middleware;

namespace CoubDownloader.Infrastructure.Integration;

/// <summary>Wrapper for FFmpeg command-line tool</summary>
public class FFmpegWrapper : IFFmpegWrapper
{
    private static class FFmpegConstants
    {
        public const string FFmpegCategory = "FFmpeg";
        public const string FFprobeCategory = "FFprobe";
        public const string PipePrefix = "pipe:";
        public const string UnknownVersion = "Unknown";
    }

    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;
    private readonly ILoggingService _logger;
    private readonly TimeSpan _executionTimeout;

    public FFmpegWrapper(string ffmpegPath = "ffmpeg", string ffprobePath = "ffprobe", ILoggingService? logger = null, TimeSpan? executionTimeout = null)
    {
        _ffmpegPath = ffmpegPath;
        _ffprobePath = ffprobePath;
        _logger = logger ?? new MemoryLoggingService();
        _executionTimeout = executionTimeout ?? TimeSpan.FromMinutes(10);
    }

    /// <summary>Check if FFmpeg is available</summary>
    public virtual async Task<bool> IsAvailableAsync()
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
    public virtual async Task<string> GetVersionAsync()
    {
        try
        {
            var result = await ExecuteAsync(new[] { "-version" }, TimeSpan.FromSeconds(5));
            if (!result.Success) return FFmpegConstants.UnknownVersion;

            var lines = result.Output.Split(Environment.NewLine);
            return lines.FirstOrDefault()?.Trim() ?? FFmpegConstants.UnknownVersion;
        }
        catch
        {
            return FFmpegConstants.UnknownVersion;
        }
    }

    /// <summary>Execute FFmpeg command</summary>
    public virtual async Task<FFmpegResult> ExecuteAsync(string[] arguments, TimeSpan? timeout = null)
    {
        var processTimeout = timeout ?? _executionTimeout;

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

            using var process = new Process { StartInfo = psi };
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            var exitTask = process.WaitForExitAsync();

            var completedTask = await Task.WhenAny(exitTask, Task.Delay(processTimeout));

            if (completedTask != exitTask)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException)
                {
                    // Process exited between the timeout and the kill.
                }

                // Wait for pipes to close
                await Task.WhenAny(Task.WhenAll(outputTask, errorTask), Task.Delay(1000));
                throw new TimeoutException($"FFmpeg operation timed out after {processTimeout.TotalSeconds} seconds");
            }

            await Task.WhenAll(outputTask, errorTask);
            var output = await outputTask;
            var error = await errorTask;
            var success = process.ExitCode == 0;

            _logger.LogDebug(
                $"FFmpeg command: {string.Join(" ", arguments)} - Exit code: {process.ExitCode}",
                FFmpegConstants.FFmpegCategory);

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
            _logger.LogError("FFmpeg execution failed", ex, FFmpegConstants.FFmpegCategory);
            return new FFmpegResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    /// <summary>Convert video file with progress callback</summary>
    public virtual async Task<FFmpegResult> ConvertVideoAsync(
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

        if (progress is null)
            return await ExecuteAsync(args.ToArray());

        var mediaInfo = await GetMediaInfoAsync(inputFile);
        var totalDurationSeconds = mediaInfo?.DurationInSeconds ?? 0;

        return await ExecuteWithProgressAsync(args.ToArray(), totalDurationSeconds, progress);
    }

    /// <summary>
    /// Execute FFmpeg while parsing "-progress pipe:1" key=value output from stdout
    /// and reporting completion percentage based on the input duration.
    /// </summary>
    private async Task<FFmpegResult> ExecuteWithProgressAsync(
        string[] arguments,
        double totalDurationSeconds,
        IProgress<int> progress,
        TimeSpan? timeout = null)
    {
        var processTimeout = timeout ?? _executionTimeout;

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

            using var process = new Process { StartInfo = psi };
            process.Start();

            using var timeoutCts = new CancellationTokenSource(processTimeout);
            var errorTask = process.StandardError.ReadToEndAsync();
            var output = new StringBuilder();

            try
            {
                string? line;
                while ((line = await process.StandardOutput.ReadLineAsync(timeoutCts.Token)) is not null)
                {
                    output.AppendLine(line);

                    // FFmpeg reports position as "out_time_us=<microseconds>"
                    // (older builds emit the same microsecond value as "out_time_ms=").
                    var valueStart = line.StartsWith("out_time_us=", StringComparison.Ordinal)
                        ? "out_time_us=".Length
                        : line.StartsWith("out_time_ms=", StringComparison.Ordinal)
                            ? "out_time_ms=".Length
                            : -1;

                    if (valueStart > 0
                        && totalDurationSeconds > 0
                        && long.TryParse(line.AsSpan(valueStart), NumberStyles.Integer, CultureInfo.InvariantCulture, out var microseconds))
                    {
                        var percent = (int)Math.Clamp(microseconds / 1_000_000.0 / totalDurationSeconds * 100.0, 0, 100);
                        progress.Report(percent);
                    }
                }

                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException)
                {
                    // Process exited between the timeout and the kill.
                }

                throw new TimeoutException($"FFmpeg operation timed out after {processTimeout.TotalSeconds} seconds");
            }

            var error = await errorTask;
            var success = process.ExitCode == 0;

            if (success)
                progress.Report(100);

            _logger.LogDebug(
                $"FFmpeg command: {string.Join(" ", arguments)} - Exit code: {process.ExitCode}",
                FFmpegConstants.FFmpegCategory);

            return new FFmpegResult
            {
                Success = success,
                Output = output.ToString(),
                Error = error,
                ExitCode = process.ExitCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("FFmpeg execution failed", ex, FFmpegConstants.FFmpegCategory);
            return new FFmpegResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    /// <summary>Extract audio from video</summary>
    public virtual async Task<FFmpegResult> ExtractAudioAsync(string inputFile, string outputFile)
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
    public virtual async Task<FFmpegResult> ConcatenateVideosAsync(
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
    public virtual async Task<FFmpegResult> LoopAudioAsync(
        string audioFile,
        double targetDuration,
        string outputFile)
    {
        var args = new[]
        {
            "-stream_loop", "-1",
            "-i", audioFile,
            "-t", targetDuration.ToString("F2", CultureInfo.InvariantCulture),
            "-c:a", "aac",
            "-y", outputFile
        };

        return await ExecuteAsync(args);
    }

    /// <summary>Get media information using ffprobe</summary>
    public virtual async Task<MediaInfo?> GetMediaInfoAsync(string filePath)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = _ffprobePath,
                Arguments = $"-v error -show_entries format=duration,size,bit_rate -of json \"{filePath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process is null) return null;

            var jsonOutput = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _logger.LogWarning($"ffprobe exited with code {process.ExitCode} for {filePath}", FFmpegConstants.FFprobeCategory);
                return null;
            }

            var mediaInfo = JsonSerializer.Deserialize<MediaInfoWrapper>(jsonOutput);
            return mediaInfo?.Format;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to get media info for {filePath}: {ex.Message}", ex, FFmpegConstants.FFprobeCategory);
            return null;
        }
    }

    internal class MediaInfoWrapper
    {
        public MediaInfo? Format { get; set; }
    }
}

/// <summary>Parameters for video conversion</summary>
public sealed class ConversionParameters
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
public sealed class FFmpegResult
{
    public bool Success { get; set; }
    public string Output { get; set; } = "";
    public string Error { get; set; } = "";
    public int ExitCode { get; set; }
}

/// <summary>Media information obtained from ffprobe</summary>
public record MediaInfo
{
    [JsonPropertyName("duration")]
    [JsonConverter(typeof(FfprobeDoubleConverter))]
    public double? DurationInSeconds { get; init; }

    [JsonPropertyName("size")]
    [JsonConverter(typeof(FfprobeLongConverter))]
    public long? Size { get; init; }

    [JsonPropertyName("bit_rate")]
    [JsonConverter(typeof(FfprobeLongConverter))]
    public long? BitRate { get; init; }
}

/// <summary>
/// Reads ffprobe numeric fields that are serialized as JSON strings
/// (e.g. "duration": "12.345000"); "N/A" and other non-numeric values map to null.
/// </summary>
internal sealed class FfprobeDoubleConverter : JsonConverter<double?>
{
    public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetDouble();

        if (reader.TokenType == JsonTokenType.String
            && double.TryParse(reader.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteNumberValue(value.Value);
        else
            writer.WriteNullValue();
    }
}

/// <summary>
/// Reads ffprobe integer fields that are serialized as JSON strings
/// (e.g. "size": "1048576"); "N/A" and other non-numeric values map to null.
/// </summary>
internal sealed class FfprobeLongConverter : JsonConverter<long?>
{
    public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetInt64();

        if (reader.TokenType == JsonTokenType.String
            && long.TryParse(reader.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteNumberValue(value.Value);
        else
            writer.WriteNullValue();
    }
}
