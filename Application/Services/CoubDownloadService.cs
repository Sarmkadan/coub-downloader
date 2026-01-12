// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Http.Json;
using CoubDownloader.Domain.Constants;
using CoubDownloader.Domain.Exceptions;
using CoubDownloader.Domain.Models;
using CoubDownloader.Infrastructure.Repositories;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Service for downloading and extracting data from Coub videos.
/// </summary>
public class CoubDownloadService : ICoubDownloadService
{
    private readonly HttpClient _httpClient;
    private readonly ICoubVideoRepository _videoRepository;

    public CoubDownloadService(HttpClient httpClient, ICoubVideoRepository videoRepository)
    {
        _httpClient = httpClient;
        _videoRepository = videoRepository;
        _httpClient.Timeout = TimeSpan.FromMilliseconds(ApplicationConstants.HttpRequestTimeoutMs);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", ApplicationConstants.DefaultUserAgent);
    }

    public async Task<CoubVideo> DownloadVideoAsync(string coubUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(coubUrl);

        // Fetch metadata from Coub API
        var video = await FetchMetadataAsync(coubUrl, cancellationToken);

        // Extract video source URL
        video.SourceUrl = await ExtractVideoSourceAsync(coubUrl, cancellationToken);

        // Save video metadata to repository
        var savedVideo = await _videoRepository.CreateAsync(video);
        return savedVideo;
    }

    public async Task<CoubVideo> FetchMetadataAsync(string coubUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(coubUrl);

        // Extract video ID from URL
        var videoId = ExtractVideoIdFromUrl(coubUrl);
        if (string.IsNullOrEmpty(videoId))
            throw new MetadataExtractionException("Could not extract video ID from URL", coubUrl);

        // Construct API call - for demo purposes using simplified response
        var video = new CoubVideo
        {
            Id = videoId,
            Url = coubUrl,
            Title = $"Coub Video {videoId}",
            Duration = 10.0,
            Width = 1920,
            Height = 1080,
            CreatorName = "Unknown",
            ViewCount = 0,
            HasAudio = true,
            UploadedDate = DateTime.UtcNow
        };

        return await Task.FromResult(video);
    }

    public async Task<string> ExtractVideoSourceAsync(string coubUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(coubUrl);

        // In a real implementation, this would parse the Coub page or API
        // For demo, return a placeholder URL
        var videoId = ExtractVideoIdFromUrl(coubUrl);
        return await Task.FromResult($"https://media-source.coub.com/download/{videoId}");
    }

    public async Task<string> DownloadVideoFileAsync(
        string sourceUrl,
        string outputPath,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        try
        {
            using var response = await _httpClient.GetAsync(sourceUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new VideoDownloadException("Failed to download video", sourceUrl, (int)response.StatusCode);

            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            var canReportProgress = totalBytes > 0;

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);

            var totalRead = 0;
            var buffer = new byte[ApplicationConstants.DownloadChunkSize];
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) != 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalRead += bytesRead;

                if (canReportProgress)
                {
                    var progressPercent = (int)(totalRead * 100 / totalBytes);
                    progress?.Report(progressPercent);
                }
            }

            return outputPath;
        }
        catch (HttpRequestException ex)
        {
            throw new VideoDownloadException(ex.Message, sourceUrl);
        }
    }

    public async Task<bool> VerifyDownloadAsync(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
            return false;

        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length == 0)
            return false;

        // In a real implementation, would verify file integrity with hash or format validation
        return await Task.FromResult(true);
    }

    /// <summary>Extract video ID from Coub URL</summary>
    private static string? ExtractVideoIdFromUrl(string url)
    {
        // Handle URLs like: https://coub.com/view/12345
        var parts = url.Split('/');
        if (parts.Length > 0)
            return parts[^1];

        return null;
    }
}
