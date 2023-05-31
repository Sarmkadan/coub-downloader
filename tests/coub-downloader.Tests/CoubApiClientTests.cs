using Xunit;
using Moq;
using Moq.Protected;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CoubDownloader.Infrastructure.Integration;
using CoubDownloader.Infrastructure.Caching;
using CoubDownloader.Infrastructure.Middleware;
using System.Text.Json;

namespace CoubDownloader.Tests;

/// <summary>
/// Unit tests for the <see cref="CoubApiClient"/> class.
/// Tests various scenarios including cache hits, API calls, error handling, and invalid inputs.
/// </summary>
public class CoubApiClientTests
{
    private readonly Mock<ILoggingService> _mockLogger;
    private readonly Mock<ICacheService> _mockCache;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly CoubApiClient _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoubApiClientTests"/> class.
    /// Sets up mock dependencies for testing the <see cref="CoubApiClient"/> functionality.
    /// </summary>
    public CoubApiClientTests()
    {
        _mockLogger = new Mock<ILoggingService>();
        _mockCache = new Mock<ICacheService>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict); // Strict to ensure all calls are set up
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://coub.com/api/v2/")
        };

        // For RateLimitingService, since it's internal and not injected,
        // we'll let the real instance be created in the constructor and assume it allows requests for tests.
        // If specific rate limit scenarios need to be tested, it would require refactoring CoubApiClient
        // to allow injecting IRateLimitingService or making IsAllowed virtual.
        _sut = new CoubApiClient(_httpClient, _mockLogger.Object, _mockCache.Object);
    }

    /// <summary>
    /// Helper method to configure the mock HTTP message handler with specific responses.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to return in the response.</param>
    /// <param name="content">Optional response content as a string. If null, no content is returned.</param>
    private void SetupHttpResponse(HttpStatusCode statusCode, string? content = null)
    {
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = content != null ? new StringContent(content) : null
            });
    }

    /// <summary>
    /// Tests that <see cref="CoubApiClient.GetVideoInfoAsync"/> returns cached video info when available,
    /// avoiding an actual API call.
    /// </summary>
    [Fact]
    public async Task GetVideoInfoAsync_CacheHit_ReturnsCachedInfo()
    {
        // Arrange
        var url = "https://coub.com/view/testcoub";
        var expectedInfo = new CoubVideoInfo { Id = "testcoub", Title = "Cached Coub" };
        _mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out expectedInfo!))
            .Returns(true);

        // Act
        var result = await _sut.GetVideoInfoAsync(url);

        // Assert
        result.Should().BeEquivalentTo(expectedInfo);
        _mockCache.Verify(c => c.TryGet(It.IsAny<string>(), out It.Ref<CoubVideoInfo?>.IsAny), Times.Once);
        // Ensure no actual HTTP call is made
        _mockHttpMessageHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that <see cref="CoubApiClient.GetVideoInfoAsync"/> successfully fetches video info from the API when not cached,
    /// deserializes the response, and caches the result for future use.
    /// </summary>
    [Fact]
    public async Task GetVideoInfoAsync_SuccessfulApiCall_ReturnsVideoInfoAndCaches()
    {
        // Arrange
        var url = "https://coub.com/view/testcoub";
        var videoId = "testcoub";
        var apiResponse = new CoubVideoInfo
        {
            Id = videoId,
            Title = "API Fetched Coub",
            Duration = 10,
            HasAudio = true
        };
        var jsonResponse = JsonSerializer.Serialize(apiResponse);

        CoubVideoInfo? outInfo = null; // for TryGet
        _mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out outInfo!))
            .Returns(false);
        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);
        _mockCache.Setup(c => c.Set(It.IsAny<string>(), It.IsAny<CoubVideoInfo>(), It.IsAny<TimeSpan>()));

        // Act
        var result = await _sut.GetVideoInfoAsync(url);

        // Assert
        result.Should().BeEquivalentTo(apiResponse);
        _mockCache.Verify(c => c.TryGet(It.IsAny<string>(), out It.Ref<CoubVideoInfo?>.IsAny), Times.Once);
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri == new Uri("https://coub.com/api/v2/coubs/testcoub")),
            ItExpr.IsAny<CancellationToken>());
        _mockCache.Verify(c => c.Set(It.IsAny<string>(), It.Is<CoubVideoInfo>(info => info.Id == videoId), It.IsAny<TimeSpan>()), Times.Once);
        _mockLogger.Verify(l => l.LogInfo(It.IsAny<string>(), "CoubApiClient"), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="CoubApiClient.GetVideoInfoAsync"/> returns null and logs a warning when the API returns a 404 Not Found response.
    /// </summary>
    [Fact]
    public async Task GetVideoInfoAsync_ApiReturnsNotFound_ReturnsNullAndLogsWarning()
    {
        // Arrange
        var url = "https://coub.com/view/nonexistent";
        CoubVideoInfo? outInfo = null;
        _mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out outInfo!))
            .Returns(false);
        SetupHttpResponse(HttpStatusCode.NotFound);

        // Act
        var result = await _sut.GetVideoInfoAsync(url);

        // Assert
        result.Should().BeNull();
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri == new Uri("https://coub.com/api/v2/coubs/nonexistent")),
            ItExpr.IsAny<CancellationToken>());
        _mockLogger.Verify(l => l.LogWarning(It.IsAny<string>(), "CoubApiClient"), Times.Once);
        _mockCache.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<CoubVideoInfo>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="CoubApiClient.GetVideoInfoAsync"/> returns null and logs an error when an HTTP request exception occurs.
    /// </summary>
    [Fact]
    public async Task GetVideoInfoAsync_HttpRequestException_ReturnsNullAndLogsError()
    {
        // Arrange
        var url = "https://coub.com/view/errorcoub";
        CoubVideoInfo? outInfo = null;
        _mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out outInfo!))
            .Returns(false);
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Simulated HTTP error"));

        // Act
        var result = await _sut.GetVideoInfoAsync(url);

        // Assert
        result.Should().BeNull();
        _mockLogger.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<HttpRequestException>(), "CoubApiClient"), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="CoubApiClient.GetVideoInfoAsync"/> returns null for various invalid URL formats without making API calls.
    /// </summary>
    /// <param name="url">The invalid URL to test. Can be null, empty, whitespace, or malformed.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-url")]
    public async Task GetVideoInfoAsync_InvalidUrl_ReturnsNull(string url)
    {
        // Arrange
        CoubVideoInfo? outInfo = null;
        _mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out outInfo!))
            .Returns(false);

        // Act
        var result = await _sut.GetVideoInfoAsync(url);

        // Assert
        result.Should().BeNull();
        _mockHttpMessageHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that <see cref="CoubApiClient.VerifyVideoExistsAsync"/> returns cached boolean value when available,
    /// avoiding an actual API call.
    /// </summary>
    [Fact]
    public async Task VerifyVideoExistsAsync_CacheHit_ReturnsCachedValue()
    {
        // Arrange
        var url = "https://coub.com/view/testexists";
        bool expectedExists = true;
        _mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out expectedExists))
            .Returns(true);

        // Act
        var result = await _sut.VerifyVideoExistsAsync(url);

        // Assert
        result.Should().BeTrue();
        _mockCache.Verify(c => c.TryGet(It.IsAny<string>(), out It.Ref<bool>.IsAny), Times.Once);
        _mockHttpMessageHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that <see cref="CoubApiClient.VerifyVideoExistsAsync"/> returns true and caches the result when the video exists in the API.
    /// </summary>
    [Fact]
    public async Task VerifyVideoExistsAsync_VideoExists_ReturnsTrueAndCaches()
    {
        // Arrange
        var url = "https://coub.com/view/testexists";
        var apiResponse = new CoubVideoInfo { Id = "testexists", Title = "Existing Coub" };
        var jsonResponse = JsonSerializer.Serialize(apiResponse);

        bool outExists = false;
        _mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out outExists))
            .Returns(false);
        _mockCache.Setup(c => c.Set(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<TimeSpan>()));
        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        // Act
        var result = await _sut.VerifyVideoExistsAsync(url);

        // Assert
        result.Should().BeTrue();
        _mockCache.Verify(c => c.TryGet(It.IsAny<string>(), out It.Ref<bool>.IsAny), Times.Once);
        _mockCache.Verify(c => c.Set(It.IsAny<string>(), true, It.IsAny<TimeSpan>()), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="CoubApiClient.VerifyVideoExistsAsync"/> returns false and caches the result when the video does not exist in the API.
    /// </summary>
    [Fact]
    public async Task VerifyVideoExistsAsync_VideoDoesNotExist_ReturnsFalseAndCaches()
    {
        // Arrange
        var url = "https://coub.com/view/nonexistent";

        bool outExists = false;
        _mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out outExists))
            .Returns(false);
        _mockCache.Setup(c => c.Set(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<TimeSpan>()));
        SetupHttpResponse(HttpStatusCode.NotFound); // Simulate 404

        // Act
        var result = await _sut.VerifyVideoExistsAsync(url);

        // Assert
        result.Should().BeFalse();
        _mockCache.Verify(c => c.TryGet(It.IsAny<string>(), out It.Ref<bool>.IsAny), Times.Once);
        _mockCache.Verify(c => c.Set(It.IsAny<string>(), false, It.IsAny<TimeSpan>()), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="CoubApiClient.SearchVideosAsync"/> returns cached list of videos when available,
    /// avoiding an actual API call.
    /// </summary>
    [Fact]
    public async Task SearchVideosAsync_CacheHit_ReturnsCachedList()
    {
        // Arrange
        var query = "test query";
        var limit = 5;
        var expectedList = new List<CoubVideoInfo> { new() { Id = "c1", Title = "Cached 1" } };
        _mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out expectedList!))
            .Returns(true);

        // Act
        var result = await _sut.SearchVideosAsync(query, limit);

        // Assert
        result.Should().BeEquivalentTo(expectedList);
        _mockCache.Verify(c => c.TryGet(It.IsAny<string>(), out It.Ref<List<CoubVideoInfo>?>.IsAny), Times.Once);
        _mockHttpMessageHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that <see cref="CoubApiClient.SearchVideosAsync"/> successfully fetches a limited list of videos from the API when not cached,
    /// deserializes the response, and caches the result for future use.
    /// </summary>
    [Fact]
    public async Task SearchVideosAsync_SuccessfulApiCall_ReturnsVideosAndCaches()
    {
        // Arrange
        var query = "test query";
        var limit = 2;
        var apiResponse = new
        {
            coubs = new[]
            {
                new { id = "c1", title = "Coub 1" },
                new { id = "c2", title = "Coub 2" },
                new { id = "c3", title = "Coub 3" } // More than limit
            }
        };
        var jsonResponse = JsonSerializer.Serialize(apiResponse);

        List<CoubVideoInfo>? outList = null;
        _mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out outList!))
            .Returns(false);
        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);
        _mockCache.Setup(c => c.Set(It.IsAny<string>(), It.IsAny<List<CoubVideoInfo>>(), It.IsAny<TimeSpan>()));

        // Act
        var result = await _sut.SearchVideosAsync(query, limit);

        // Assert
        result.Should().HaveCount(limit);
        result.First().Id.Should().Be("c1");
        result.Last().Id.Should().Be("c2");
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.Query.Contains($"q={Uri.EscapeDataString(query)}") && req.RequestUri.Query.Contains($"limit={limit}")),
            ItExpr.IsAny<CancellationToken>());
        _mockCache.Verify(c => c.Set(It.IsAny<string>(), It.Is<List<CoubVideoInfo>>(list => list.Count == limit), It.IsAny<TimeSpan>()), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="CoubApiClient.SearchVideosAsync"/> returns an empty list and logs an error when an HTTP request exception occurs.
    /// </summary>
    [Fact]
    public async Task SearchVideosAsync_HttpRequestException_ReturnsEmptyListAndLogsError()
    {
        // Arrange
        var query = "error query";
        var limit = 10;

        List<CoubVideoInfo>? outList = null;
        _mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out outList!))
            .Returns(false);
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Simulated search error"));

        // Act
        var result = await _sut.SearchVideosAsync(query, limit);

        // Assert
        result.Should().BeEmpty();
        _mockLogger.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<HttpRequestException>(), "CoubApiClient"), Times.Once);
        _mockCache.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<List<CoubVideoInfo>>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="CoubApiClient.SearchVideosAsync"/> returns an empty list and logs an error when the API returns malformed JSON.
    /// </summary>
    [Fact]
    public async Task SearchVideosAsync_ApiReturnsMalformedJson_ReturnsEmptyListAndLogsError()
    {
        // Arrange
        var query = "malformed json";
        var limit = 10;
        var malformedJsonResponse = @"{ ""coubs"": [ { ""id"": ""c1"", ""title"": ""c1"" } ]"; // Incomplete JSON

        List<CoubVideoInfo>? outList = null;
        _mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out outList!))
            .Returns(false);
        SetupHttpResponse(HttpStatusCode.OK, malformedJsonResponse);

        // Act
        var result = await _sut.SearchVideosAsync(query, limit);

        // Assert
        result.Should().BeEmpty();
        _mockLogger.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<JsonException>(), "CoubApiClient"), Times.Once);
        _mockCache.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<List<CoubVideoInfo>>(), It.IsAny<TimeSpan>()), Times.Never);
    }
}
