#nullable enable
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
    private readonly ICoubApiClient _coubApiClient;

    public CoubDownloadService(HttpClient httpClient, ICoubVideoRepository videoRepository, ICoubApiClient coubApiClient)
    {
        _httpClient = httpClient;
        _videoRepository = videoRepository;
        _coubApiClient = coubApiClient;
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

        var videoInfo = await _coubApiClient.GetVideoInfoAsync(coubUrl, cancellationToken);

        if (videoInfo == null)
            throw new MetadataExtractionException("Failed to fetch video metadata", coubUrl);

        var video = new CoubVideo
        {
            Id = videoInfo.Id,
            Url = coubUrl,
            Title = videoInfo.Title,
            Duration = videoInfo.Duration,
            // Assuming default width/height if not provided by API
            Width = 1920, 
            Height = 1080,
            CreatorName = videoInfo.ChannelUrl ?? "Unknown", // Using ChannelUrl as creator name for now
            ViewCount = videoInfo.ViewCount,
            HasAudio = videoInfo.HasAudio,
            UploadedDate = DateTime.UtcNow // API doesn't provide this, using current time
        };

        return video;
    }

    public async Task<string> ExtractVideoSourceAsync(string coubUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(coubUrl);

        var videoInfo = await _coubApiClient.GetVideoInfoAsync(coubUrl, cancellationToken);

        if (videoInfo == null || string.IsNullOrEmpty(videoInfo.Id))
            throw new MetadataExtractionException("Failed to get video ID for source extraction", coubUrl);

        // Assuming this is the pattern for direct video download based on the previous implementation
        // This can be further refined if the Coub API provides a direct download link in the future.
        return await Task.FromResult($"https://media-source.coub.com/videos/{videoInfo.Id}/webm/high.webm");
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
    }
