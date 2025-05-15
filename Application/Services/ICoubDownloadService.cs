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
    /// <summary>Download video from Coub URL</summary>
    Task<CoubVideo> DownloadVideoAsync(string coubUrl, CancellationToken cancellationToken = default);

    /// <summary>Fetch video metadata from Coub</summary>
    Task<CoubVideo> FetchMetadataAsync(string coubUrl, CancellationToken cancellationToken = default);

    /// <summary>Extract video source URL from Coub</summary>
    Task<string> ExtractVideoSourceAsync(string coubUrl, CancellationToken cancellationToken = default);

    /// <summary>Download video file to disk</summary>
    Task<string> DownloadVideoFileAsync(string sourceUrl, string outputPath, IProgress<int>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>Verify downloaded file integrity</summary>
    Task<bool> VerifyDownloadAsync(string filePath);
}
