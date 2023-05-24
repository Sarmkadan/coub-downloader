#nullable enable

using CoubDownloader.Domain.Constants;
using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Exceptions;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Extension methods for <see cref="VideoConversionService"/> providing convenient utility operations.
/// </summary>
public static class VideoConversionServiceExtensions
{
    /// <summary>
    /// Converts video to a square format (1:1 aspect ratio) suitable for Instagram posts.
    /// </summary>
    /// <param name="service">The video conversion service instance</param>
    /// <param name="inputPath">Path to the input video file</param>
    /// <param name="outputPath">Path where the output video will be saved</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The path to the converted video file</returns>
    /// <exception cref="ArgumentException"><paramref name="inputPath"/> or <paramref name="outputPath"/> is null or whitespace</exception>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null</exception>
    /// <exception cref="FileOperationException">Input video file not found</exception>
    /// <exception cref="ProcessExecutionException">FFmpeg execution failed</exception>
    /// <exception cref="VideoConversionException">Video conversion failed</exception>
    public static async Task<string> ConvertToSquareAsync(
        this VideoConversionService service,
        string inputPath,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(service);

        if (!File.Exists(inputPath))
            throw new FileOperationException("Video file not found", inputPath, FileOperationType.Read);

        var settings = new ConversionSettings
        {
            Width = 1080,
            Height = 1080,
            PreserveAspectRatio = true,
            FrameRate = 30,
            VideoCodec = "h264",
            AudioCodec = "aac",
            AudioBitrate = 128,
            VideoBitrate = 2500
        };

        return await service.ConvertVideoAsync(inputPath, outputPath, settings, null, cancellationToken);
    }

    /// <summary>
    /// Extracts audio from video file and saves it as a separate audio file.
    /// </summary>
    /// <param name="service">The video conversion service instance</param>
    /// <param name="videoPath">Path to the input video file</param>
    /// <param name="audioOutputPath">Path where the extracted audio will be saved</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The path to the extracted audio file</returns>
    /// <exception cref="ArgumentException"><paramref name="videoPath"/> or <paramref name="audioOutputPath"/> is null or whitespace</exception>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null</exception>
    /// <exception cref="FileOperationException">Video file not found</exception>
    /// <exception cref="ProcessExecutionException">FFmpeg execution failed</exception>
    /// <exception cref="VideoConversionException">Audio extraction failed</exception>
    public static async Task<string> ExtractAudioAsync(
        this VideoConversionService service,
        string videoPath,
        string audioOutputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(audioOutputPath);
        ArgumentNullException.ThrowIfNull(service);

        try
        {
            if (!File.Exists(videoPath))
                throw new FileOperationException("Video file not found", videoPath, FileOperationType.Read);

            var args = $@"-i ""{videoPath}"" -vn -acodec copy ""{audioOutputPath}"" -y";

            // Use reflection to access the private RunFfmpegAsync method
            var runFfmpegMethod = typeof(VideoConversionService).GetMethod(
                "RunFfmpegAsync",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (runFfmpegMethod == null)
                throw new InvalidOperationException("RunFfmpegAsync method not found");

            var result = await (Task<(int ExitCode, string StandardOutput, string StandardError)>)
                runFfmpegMethod.Invoke(service, new object[] { args, null, cancellationToken });
            var exitCode = result.ExitCode;
            var standardError = result.StandardError;

            if (exitCode != 0)
            {
                standardError ??= string.Empty;
                throw new ProcessExecutionException(
                    "Failed to extract audio from video",
                    ApplicationConstants.FFmpegExecutable,
                    args,
                    exitCode,
                    standardError);
            }

            return audioOutputPath;
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not FileOperationException and not ProcessExecutionException)
        {
            throw new VideoConversionException(ex.Message, videoPath, audioOutputPath, ex);
        }
    }

    /// <summary>
    /// Creates a video thumbnail at the specified timestamp.
    /// </summary>
    /// <param name="service">The video conversion service instance</param>
    /// <param name="videoPath">Path to the input video file</param>
    /// <param name="thumbnailPath">Path where the thumbnail will be saved</param>
    /// <param name="timestampSeconds">Timestamp in seconds for the thumbnail (default: 5 seconds)</param>
    /// <param name="width">Width of the thumbnail (default: 320)</param>
    /// <param name="height">Height of the thumbnail (default: 240)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The path to the generated thumbnail</returns>
    /// <exception cref="ArgumentException"><paramref name="videoPath"/> or <paramref name="thumbnailPath"/> is null or whitespace</exception>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null</exception>
    /// <exception cref="ValidationException">Invalid parameters provided</exception>
    /// <exception cref="FileOperationException">Video file not found</exception>
    /// <exception cref="ProcessExecutionException">FFmpeg execution failed</exception>
    /// <exception cref="VideoConversionException">Thumbnail creation failed</exception>
    public static async Task<string> CreateThumbnailAsync(
        this VideoConversionService service,
        string videoPath,
        string thumbnailPath,
        int timestampSeconds = 5,
        int width = 320,
        int height = 240,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(thumbnailPath);
        ArgumentNullException.ThrowIfNull(service);

        if (timestampSeconds <= 0)
            throw new ValidationException("Timestamp must be greater than 0", nameof(timestampSeconds), timestampSeconds);

        if (width <= 0 || height <= 0)
            throw new ValidationException("Width and height must be greater than 0", nameof(width), width);

        try
        {
            if (!File.Exists(videoPath))
                throw new FileOperationException("Video file not found", videoPath, FileOperationType.Read);

            var directory = Path.GetDirectoryName(thumbnailPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var args = $@"-i ""{videoPath}"" -ss {timestampSeconds} -vframes 1 -vf scale={width}:{height} -y ""{thumbnailPath}""";

            // Use reflection to access the private RunFfmpegAsync method
            var runFfmpegMethod = typeof(VideoConversionService).GetMethod(
                "RunFfmpegAsync",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (runFfmpegMethod == null)
                throw new InvalidOperationException("RunFfmpegAsync method not found");

            var result = await (Task<(int ExitCode, string StandardOutput, string StandardError)>)
                runFfmpegMethod.Invoke(service, new object[] { args, null, cancellationToken });
            var exitCode = result.ExitCode;
            var standardError = result.StandardError;

            if (exitCode != 0)
            {
                standardError ??= string.Empty;
                throw new ProcessExecutionException(
                    "Failed to create thumbnail",
                    ApplicationConstants.FFmpegExecutable,
                    args,
                    exitCode,
                    standardError);
            }

            return thumbnailPath;
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not FileOperationException and not ProcessExecutionException and not ValidationException)
        {
            throw new VideoConversionException(ex.Message, videoPath, thumbnailPath, ex);
        }
    }

    /// <summary>
    /// Gets video duration in a human-readable format (HH:MM:SS).
    /// </summary>
    /// <param name="service">The video conversion service instance</param>
    /// <param name="filePath">Path to the video file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Formatted duration string (HH:MM:SS)</returns>
    /// <exception cref="ArgumentException"><paramref name="filePath"/> is null or whitespace</exception>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null</exception>
    /// <exception cref="FileOperationException">Video file not found</exception>
    public static async Task<string> GetVideoDurationFormattedAsync(
        this VideoConversionService service,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(service);

        var metadata = await service.GetVideoMetadataAsync(filePath, cancellationToken);

        if (metadata.Duration <= 0)
            return "00:00:00";

        var timeSpan = TimeSpan.FromSeconds(metadata.Duration);
        return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }
}