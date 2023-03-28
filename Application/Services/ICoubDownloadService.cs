#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Models;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Service for downloading Coub videos.
/// </summary>
public interface ICoubDownloadService
{
    /// <summary>
    /// Downloads a Coub video from the specified URL.
    /// </summary>
    /// <param name="coubUrl">The URL of the Coub video.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="CoubVideo"/> object representing the downloaded video.</returns>
    Task<CoubVideo> DownloadVideoAsync(string coubUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches video metadata from Coub.
    /// </summary>
    /// <param name="coubUrl">The URL of the Coub video.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="CoubVideo"/> object representing the video metadata.</returns>
    Task<CoubVideo> FetchMetadataAsync(string coubUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts the video source URL from Coub.
    /// </summary>
    /// <param name="coubUrl">The URL of the Coub video.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The URL of the video source.</returns>
    Task<string> ExtractVideoSourceAsync(string coubUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the video file to disk.
    /// </summary>
    /// <param name="sourceUrl">The URL of the video source.</param>
    /// <param name="outputPath">The file path where the video will be saved.</param>
    /// <param name="progress">An optional progress reporter.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The file path of the downloaded video.</returns>
    Task<string> DownloadVideoFileAsync(string sourceUrl, string outputPath, IProgress<int>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies the integrity of the downloaded file.
    /// </summary>
    /// <param name="filePath">The file path to the downloaded video.</param>
    /// <returns><c>true</c> if the file is valid; otherwise, <c>false</c>.</returns>
    Task<bool> VerifyDownloadAsync(string filePath);
}
