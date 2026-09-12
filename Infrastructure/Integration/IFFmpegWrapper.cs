#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Infrastructure.Integration;
using CoubDownloader.Infrastructure.Middleware;

namespace CoubDownloader.Infrastructure.Integration;

/// <summary>
/// Defines operations for interacting with FFmpeg and inspecting media files.
/// </summary>
public interface IFFmpegWrapper
{
    /// <summary>
    /// Determines whether FFmpeg is available for use.
    /// </summary>
    /// <returns>A task whose result is <see langword="true"/> when FFmpeg is available; otherwise, <see langword="false"/>.</returns>
    Task<bool> IsAvailableAsync();

    /// <summary>
    /// Gets the installed FFmpeg version.
    /// </summary>
    /// <returns>A task whose result contains the FFmpeg version.</returns>
    Task<string> GetVersionAsync();

    /// <summary>
    /// Executes FFmpeg with the specified command-line arguments.
    /// </summary>
    /// <param name="arguments">The command-line arguments to pass to FFmpeg.</param>
    /// <param name="timeout">The optional maximum duration allowed for the operation.</param>
    /// <returns>A task whose result describes the FFmpeg execution outcome.</returns>
    Task<FFmpegResult> ExecuteAsync(string[] arguments, TimeSpan? timeout = null);

    /// <summary>
    /// Converts a video file using the specified conversion parameters.
    /// </summary>
    /// <param name="inputFile">The path to the input video file.</param>
    /// <param name="outputFile">The path to the output video file.</param>
    /// <param name="parameters">The parameters that control the conversion.</param>
    /// <param name="progress">The optional receiver for conversion progress updates.</param>
    /// <returns>A task whose result describes the FFmpeg conversion outcome.</returns>
    Task<FFmpegResult> ConvertVideoAsync(string inputFile, string outputFile, ConversionParameters parameters, IProgress<int>? progress = null);

    /// <summary>
    /// Extracts the audio stream from a media file.
    /// </summary>
    /// <param name="inputFile">The path to the input media file.</param>
    /// <param name="outputFile">The path to the output audio file.</param>
    /// <returns>A task whose result describes the FFmpeg extraction outcome.</returns>
    Task<FFmpegResult> ExtractAudioAsync(string inputFile, string outputFile);

    /// <summary>
    /// Concatenates multiple video files into a single output file.
    /// </summary>
    /// <param name="inputFiles">The ordered list of input video file paths.</param>
    /// <param name="outputFile">The path to the concatenated output file.</param>
    /// <returns>A task whose result describes the FFmpeg concatenation outcome.</returns>
    Task<FFmpegResult> ConcatenateVideosAsync(List<string> inputFiles, string outputFile);

    /// <summary>
    /// Loops an audio file to the specified target duration.
    /// </summary>
    /// <param name="audioFile">The path to the input audio file.</param>
    /// <param name="targetDuration">The desired output duration in seconds.</param>
    /// <param name="outputFile">The path to the output audio file.</param>
    /// <returns>A task whose result describes the FFmpeg looping outcome.</returns>
    Task<FFmpegResult> LoopAudioAsync(string audioFile, double targetDuration, string outputFile);

    /// <summary>
    /// Gets media information for the specified file.
    /// </summary>
    /// <param name="filePath">The path to the media file to inspect.</param>
    /// <returns>A task whose result contains the media information, or <see langword="null"/> when it cannot be obtained.</returns>
    Task<MediaInfo?> GetMediaInfoAsync(string filePath);
}
