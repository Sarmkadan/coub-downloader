#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using CoubDownloader.Domain.Models;
using CoubDownloader.Infrastructure.Caching;
using CoubDownloader.Infrastructure.Middleware;

namespace CoubDownloader.Infrastructure.Integration;

/// <summary>HTTP client for Coub API integration</summary>
public interface ICoubApiClient
{
    /// <summary>Gets video info asynchronously</summary>
    /// <param name="url">The URL of the video</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The video information, or null if not found</returns>
    Task<CoubVideoInfo?> GetVideoInfoAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>Verifies video exists asynchronously</summary>
    /// <param name="url">The URL of the video</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the video exists, otherwise false</returns>
    Task<bool> VerifyVideoExistsAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>Searches videos asynchronously</summary>
    /// <param name="query">The search query</param>
    /// <param name="limit">Maximum number of results to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of matching video information</returns>
    Task<List<CoubVideoInfo>> SearchVideosAsync(string query, int limit = 10, CancellationToken cancellationToken = default);
}

/// <summary>Coub API client implementation</summary>
public class CoubApiClient : ICoubApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILoggingService _logger;
    private readonly ICacheService _cache;
    private readonly RateLimitingService _rateLimiter;

    private const string BaseUrl = "https://coub.com/api/v2";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    private const string VideoInfoKeyPrefix = "video_info_";
    private const string VideoExistsKeyPrefix = "video_exists_";
    private const string SearchKeyPrefix = "search_";
    private const string SearchEndpoint = "/search/coubs";
    private const string RateLimitKeyCoubApi = "coub_api";
    private const string RateLimitKeyCoubSearch = "coub_search";
    private static readonly TimeSpan VideoExistsCacheTtl = TimeSpan.FromHours(24);
    private const int RateLimitMaxRequestsPerWindow = 30;

    /// <summary>Initializes a new instance of the <see cref="CoubApiClient"/> class</summary>
    /// <param name="httpClient">The HTTP client to use for requests</param>
    /// <param name="logger">The logging service</param>
    /// <param name="cache">The cache service</param>
    public CoubApiClient(HttpClient httpClient, ILoggingService logger, ICacheService cache)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cache);

        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
        _rateLimiter = new RateLimitingService(maxRequestsPerWindow: RateLimitMaxRequestsPerWindow);
    }

    /// <summary>Gets video info asynchronously</summary>
    /// <param name="url">The URL of the video</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The video information, or null if not found</returns>
    public async Task<CoubVideoInfo?> GetVideoInfoAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url);

        if (string.IsNullOrWhiteSpace(url))
            return null;

        var cacheKey = $"{VideoInfoKeyPrefix}{url}";

        if (_cache.TryGet(cacheKey, out CoubVideoInfo? cached))
        {
            _logger.LogDebug($"Cache hit for {url}", "CoubApiClient");
            return cached;
        }

        if (!_rateLimiter.IsAllowed(RateLimitKeyCoubApi))
        {
            _logger.LogWarning("Rate limit exceeded for Coub API", "CoubApiClient");
            return null;
        }

        try
        {
            var videoId = ExtractVideoId(url);
            if (string.IsNullOrEmpty(videoId))
                return null;

            var response = await _httpClient.GetAsync($"{BaseUrl}/coubs/{videoId}", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"Video not found (404) for {url}", "CoubApiClient");
                return null;
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var info = JsonSerializer.Deserialize<CoubVideoInfo>(json);

            if (info is not null)
            {
                _cache.Set(cacheKey, info, CacheTtl);
                _logger.LogInfo($"Retrieved video info for {url}", "CoubApiClient");
            }

            return info;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Failed to fetch video info for {url}", ex, "CoubApiClient");
            return null;
        }
    }

    /// <summary>Verifies video exists asynchronously</summary>
    /// <param name="url">The URL of the video</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the video exists, otherwise false</returns>
    public async Task<bool> VerifyVideoExistsAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url);

        if (string.IsNullOrWhiteSpace(url))
            return false;

        var cacheKey = $"{VideoExistsKeyPrefix}{url}";

        if (_cache.TryGet(cacheKey, out bool cached))
            return cached;

        var info = await GetVideoInfoAsync(url, cancellationToken);
        var exists = info is not null;

        _cache.Set(cacheKey, exists, VideoExistsCacheTtl);
        return exists;
    }

    /// <summary>Searches videos asynchronously</summary>
    /// <param name="query">The search query</param>
    /// <param name="limit">Maximum number of results to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of matching video information</returns>
    public async Task<List<CoubVideoInfo>> SearchVideosAsync(string query, int limit = 10, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(query))
            return [];

        var cacheKey = $"{SearchKeyPrefix}{query}_{limit}".ToLowerInvariant();

        if (_cache.TryGet(cacheKey, out List<CoubVideoInfo>? cached))
            return cached ?? [];

        if (!_rateLimiter.IsAllowed(RateLimitKeyCoubSearch))
            return [];

        try
        {
            var encodedQuery = Uri.EscapeDataString(query);
            var response = await _httpClient.GetAsync(
                $"{BaseUrl}{SearchEndpoint}?q={encodedQuery}&limit={limit}", cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var root = JsonSerializer.Deserialize<JsonElement>(json);

            var videos = root.GetProperty("coubs")
                .EnumerateArray()
                .Take(limit)
                .Select(elem => new CoubVideoInfo
                {
                    Id = elem.GetProperty("id").GetString() ?? "",
                    Title = elem.GetProperty("title").GetString() ?? ""
                })
                .ToList();

            _cache.Set(cacheKey, videos, CacheTtl);
            return videos;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Search failed for query: {query}", ex, "CoubApiClient");
            return [];
        }
    }

    private static string? ExtractVideoId(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return null;

        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.LastOrDefault();
    }
}

/// <summary>Coub video information from API</summary>
public class CoubVideoInfo
{
    /// <summary>Gets or sets the video ID</summary>
    public string Id { get; set; } = "";
    /// <summary>Gets or sets the video title</summary>
    public string Title { get; set; } = "";
    /// <summary>Gets or sets the video description</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the view count</summary>
    public int ViewCount { get; set; }
    /// <summary>Gets or sets the duration in seconds</summary>
    public double Duration { get; set; }
    /// <summary>Gets or sets the channel URL</summary>
    public string? ChannelUrl { get; set; }
    /// <summary>Gets or sets a value indicating whether the video has audio</summary>
    public bool HasAudio { get; set; }
}
