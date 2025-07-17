// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using CoubDownloader.Domain.Constants;
using CoubDownloader.Domain.Exceptions;
using CoubDownloader.Domain.Models;
using CoubDownloader.Infrastructure.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Fetches Coub channel or tag feeds and converts them into batch processing queues.
/// </summary>
public sealed class PlaylistProcessingService : IPlaylistProcessingService
{
    private readonly HttpClient _httpClient;
    private readonly IBatchProcessingService _batchService;
    private readonly ILoggingService _logger;

    private const string Category = "PlaylistProcessing";
    private const int PageSize = 25;
    private const int MaxFetchableVideos = 500;

    /// <summary>Initializes a new instance of <see cref="PlaylistProcessingService"/>.</summary>
    /// <param name="httpClient">Configured HTTP client for Coub API calls.</param>
    /// <param name="batchService">Service used to create and manage batch jobs.</param>
    /// <param name="logger">Logging service for diagnostic output.</param>
    public PlaylistProcessingService(
        HttpClient httpClient,
        IBatchProcessingService batchService,
        ILoggingService logger)
    {
        _httpClient = httpClient;
        _batchService = batchService;
        _logger = logger;
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", ApplicationConstants.DefaultUserAgent);
    }

    /// <inheritdoc/>
    public async Task<CoubPlaylist> FetchPlaylistAsync(string playlistUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistUrl);

        _logger.LogInfo($"Fetching playlist: {playlistUrl}", Category);

        var (slug, type) = ParsePlaylistUrl(playlistUrl);
        if (string.IsNullOrEmpty(slug))
            throw new MetadataExtractionException("Could not determine channel or tag slug from URL", playlistUrl);

        var videoUrls = new List<string>();

        for (var page = 1; videoUrls.Count < MaxFetchableVideos; page++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var apiUrl = type == "tag"
                ? $"{ApplicationConstants.CoubApiBaseUrl}/tags/{Uri.EscapeDataString(slug)}/coubs?page={page}&per_page={PageSize}&order_by=newest"
                : $"{ApplicationConstants.CoubApiBaseUrl}/channels/{Uri.EscapeDataString(slug)}/coubs?page={page}&per_page={PageSize}&order_by=newest";

            var response = await _httpClient.GetAsync(apiUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"API returned {(int)response.StatusCode} on page {page} — stopping fetch", Category);
                break;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var root = JsonSerializer.Deserialize<JsonElement>(json);

            if (!root.TryGetProperty("coubs", out var coubsArray))
                break;

            var pageItems = coubsArray.EnumerateArray().ToList();
            if (pageItems.Count == 0)
                break;

            foreach (var item in pageItems)
            {
                if (item.TryGetProperty("permalink", out var pl) && pl.GetString() is { Length: > 0 } permalink)
                    videoUrls.Add($"https://coub.com/view/{permalink}");
            }

            var totalPages = root.TryGetProperty("total_pages", out var tp) ? tp.GetInt32() : 1;
            if (page >= totalPages)
                break;
        }

        _logger.LogInfo($"Playlist '{slug}' fetched — {videoUrls.Count} video(s) discovered", Category);

        return new CoubPlaylist
        {
            Id = $"{type}_{slug}",
            Title = slug,
            PlaylistUrl = playlistUrl,
            VideoUrls = videoUrls,
            FetchedAt = DateTime.UtcNow
        };
    }

    /// <inheritdoc/>
    public async Task<BatchJob> QueuePlaylistAsync(
        string playlistUrl,
        string outputDirectory,
        ConversionSettings? settings = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        var playlist = await FetchPlaylistAsync(playlistUrl, cancellationToken);
        return await QueuePlaylistAsync(playlist, outputDirectory, settings, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<BatchJob> QueuePlaylistAsync(
        CoubPlaylist playlist,
        string outputDirectory,
        ConversionSettings? settings = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(playlist);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        if (!playlist.IsValid())
            throw new InvalidConfigurationException($"Playlist '{playlist.Id}' is invalid or contains no video URLs.");

        var effectiveUrls = playlist.GetEffectiveVideoUrls().ToList();
        _logger.LogInfo($"Queuing {effectiveUrls.Count} video(s) from playlist '{playlist.Title}'", Category);

        var batch = await _batchService.CreateBatchJobAsync(
            name: $"Playlist: {playlist.Title}",
            outputDirectory: outputDirectory,
            sharedSettings: settings,
            cancellationToken: cancellationToken);

        batch.Description = $"Source playlist: {playlist.PlaylistUrl}";

        var tasks = effectiveUrls.Select((url, index) =>
        {
            var videoId = ExtractVideoId(url) ?? $"video_{index:D4}";
            return new DownloadTask
            {
                Id = Guid.NewGuid().ToString(),
                VideoId = videoId,
                Url = url,
                OutputPath = Path.Combine(outputDirectory, $"{videoId}.mp4"),
                BatchJobId = batch.Id
            };
        });

        await _batchService.AddTasksAsync(batch.Id, tasks, cancellationToken);

        _logger.LogInfo($"Batch job '{batch.Id}' created for playlist '{playlist.Title}'", Category);
        return batch;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<BatchJob>> GetActivePlaylistJobsAsync(CancellationToken cancellationToken = default)
        => _batchService.GetActiveBatchesAsync(cancellationToken);

    private static (string slug, string type) ParsePlaylistUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return (string.Empty, string.Empty);

        var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length >= 2 && segments[0].Equals("tags", StringComparison.OrdinalIgnoreCase))
            return (segments[1], "tag");

        if (segments.Length >= 1 && !string.IsNullOrEmpty(segments[0]))
            return (segments[0], "channel");

        return (string.Empty, string.Empty);
    }

    private static string? ExtractVideoId(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return null;

        return uri.AbsolutePath.Trim('/').Split('/').LastOrDefault();
    }
}

/// <summary>
/// Extension methods for registering playlist processing services in the DI container.
/// </summary>
public static class PlaylistServiceExtensions
{
    /// <summary>
    /// Add <see cref="IPlaylistProcessingService"/> and its typed <see cref="HttpClient"/> to the DI container.
    /// Call this from your composition root alongside <c>AddCoubDownloaderServices()</c>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddPlaylistProcessing(this IServiceCollection services)
    {
        services.AddHttpClient<IPlaylistProcessingService, PlaylistProcessingService>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(ApplicationConstants.HttpRequestTimeoutMs / 1000);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

        return services;
    }
}
