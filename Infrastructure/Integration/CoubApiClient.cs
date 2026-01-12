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
    Task<CoubVideoInfo?> GetVideoInfoAsync(string url);
    Task<bool> VerifyVideoExistsAsync(string url);
    Task<List<CoubVideoInfo>> SearchVideosAsync(string query, int limit = 10);
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

    public CoubApiClient(HttpClient httpClient, ILoggingService logger, ICacheService cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
        _rateLimiter = new RateLimitingService(maxRequestsPerWindow: 30);
    }

    public async Task<CoubVideoInfo?> GetVideoInfoAsync(string url)
    {
        var cacheKey = $"video_info_{url.GetHashCode()}";

        if (_cache.TryGet(cacheKey, out CoubVideoInfo? cached))
        {
            _logger.LogDebug($"Cache hit for {url}", "CoubApiClient");
            return cached;
        }

        if (!_rateLimiter.IsAllowed("coub_api"))
        {
            _logger.LogWarning("Rate limit exceeded for Coub API", "CoubApiClient");
            return null;
        }

        try
        {
            var videoId = ExtractVideoId(url);
            if (string.IsNullOrEmpty(videoId))
                return null;

            var response = await _httpClient.GetAsync($"{BaseUrl}/coubs/{videoId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var info = JsonSerializer.Deserialize<CoubVideoInfo>(json);

            if (info != null)
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

    public async Task<bool> VerifyVideoExistsAsync(string url)
    {
        var cacheKey = $"video_exists_{url.GetHashCode()}";

        if (_cache.TryGet(cacheKey, out bool cached))
            return cached;

        var info = await GetVideoInfoAsync(url);
        var exists = info != null;

        _cache.Set(cacheKey, exists, TimeSpan.FromHours(24));
        return exists;
    }

    public async Task<List<CoubVideoInfo>> SearchVideosAsync(string query, int limit = 10)
    {
        var cacheKey = $"search_{query}_{limit}".ToLower();

        if (_cache.TryGet(cacheKey, out List<CoubVideoInfo>? cached))
            return cached ?? [];

        if (!_rateLimiter.IsAllowed("coub_search"))
            return [];

        try
        {
            var encodedQuery = Uri.EscapeDataString(query);
            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/search/coubs?q={encodedQuery}&limit={limit}");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
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

    private string? ExtractVideoId(string url)
    {
        var uri = new Uri(url);
        var segments = uri.PathAndQuery.Split('/');
        return segments.LastOrDefault()?.Split('?').FirstOrDefault();
    }
}

/// <summary>Coub video information from API</summary>
public class CoubVideoInfo
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int ViewCount { get; set; }
    public double Duration { get; set; }
    public string? ChannelUrl { get; set; }
    public bool HasAudio { get; set; }
}
