using Xunit;
using Moq;
using Moq.Protected; // Added for mocking protected methods of HttpClient
using FluentAssertions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Models;
using CoubDownloader.Infrastructure.Integration;
using CoubDownloader.Infrastructure.Repositories;
using CoubDownloader.Domain.Exceptions;
using System;
using System.IO;
using System.Net;

namespace CoubDownloader.Tests;


public class CoubDownloadServiceTests
{
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly Mock<ICoubVideoRepository> _mockVideoRepository;
    private readonly Mock<ICoubApiClient> _mockCoubApiClient;
    private readonly CoubDownloadService _sut; // System Under Test

    public CoubDownloadServiceTests()
    {
        _mockHttpClient = new Mock<HttpClient>();
        _mockVideoRepository = new Mock<ICoubVideoRepository>();
        _mockCoubApiClient = new Mock<ICoubApiClient>();

        // For HttpClient, usually you'd mock HttpMessageHandler.
        // For simplicity and given the current HttpClient usage in CoubDownloadService,
        // we'll mock the HttpClient directly only if specific methods need to be called.
        // Otherwise, it might not be directly involved in some unit tests.

        _sut = new CoubDownloadService(
            _mockHttpClient.Object,
            _mockVideoRepository.Object,
            _mockCoubApiClient.Object);
    }

    // Test cases for DownloadVideoAsync
    [Fact]
    public async Task DownloadVideoAsync_ValidCoubUrl_ReturnsCoubVideo()
    {
        // Arrange
        var coubUrl = "https://coub.com/view/123";
        var mockVideoInfo = new CoubVideoInfo
        {
            Id = "123",
            Title = "Test Coub",
            Duration = 10,
            HasAudio = true,
            ChannelUrl = "test_channel",
            ViewCount = 100
        };
        var expectedCoubVideo = new CoubVideo
        {
            Id = "123",
            Url = coubUrl,
            Title = "Test Coub",
            Duration = 10,
            Width = 1920,
            Height = 1080,
            CreatorName = "test_channel",
            ViewCount = 100,
            HasAudio = true,
            SourceUrl = "https://media-source.coub.com/videos/123/webm/high.webm"
        };

        _mockCoubApiClient.Setup(api => api.GetVideoInfoAsync(coubUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockVideoInfo);
        _mockVideoRepository.Setup(repo => repo.CreateAsync(It.IsAny<CoubVideo>()))
            .ReturnsAsync(expectedCoubVideo);

        // Act
        var result = await _sut.DownloadVideoAsync(coubUrl);

        // Assert
        result.Should().BeEquivalentTo(expectedCoubVideo, options => options.Excluding(o => o.UploadedDate));
        _mockCoubApiClient.Verify(api => api.GetVideoInfoAsync(coubUrl, It.IsAny<CancellationToken>()), Times.Exactly(2)); // Called by FetchMetadata and ExtractVideoSource
        _mockVideoRepository.Verify(repo => repo.CreateAsync(It.IsAny<CoubVideo>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task DownloadVideoAsync_InvalidCoubUrl_ThrowsArgumentException(string invalidCoubUrl)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.DownloadVideoAsync(invalidCoubUrl));
    }

    [Fact]
    public async Task DownloadVideoAsync_MetadataFetchingFails_ThrowsMetadataExtractionException()
    {
        // Arrange
        var coubUrl = "https://coub.com/view/123";
        _mockCoubApiClient.Setup(api => api.GetVideoInfoAsync(coubUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CoubVideoInfo)null!);

        // Act & Assert
        await Assert.ThrowsAsync<MetadataExtractionException>(() => _sut.DownloadVideoAsync(coubUrl));
        _mockCoubApiClient.Verify(api => api.GetVideoInfoAsync(coubUrl, It.IsAny<CancellationToken>()), Times.Once);
        _mockVideoRepository.Verify(repo => repo.CreateAsync(It.IsAny<CoubVideo>()), Times.Never);
    }

    [Fact]
    public async Task DownloadVideoAsync_SourceExtractionFails_ThrowsMetadataExtractionException()
    {
        // Arrange
        var coubUrl = "https://coub.com/view/123";
        var mockVideoInfo = new CoubVideoInfo { Id = "123", Title = "Test Coub" }; // Valid metadata for Fetch
        _mockCoubApiClient.SetupSequence(api => api.GetVideoInfoAsync(coubUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockVideoInfo) // First call for FetchMetadataAsync
            .ReturnsAsync((CoubVideoInfo)null!); // Second call for ExtractVideoSourceAsync

        // Act & Assert
        await Assert.ThrowsAsync<MetadataExtractionException>(() => _sut.DownloadVideoAsync(coubUrl));
        _mockCoubApiClient.Verify(api => api.GetVideoInfoAsync(coubUrl, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockVideoRepository.Verify(repo => repo.CreateAsync(It.IsAny<CoubVideo>()), Times.Never);
    }

    // Test cases for FetchMetadataAsync
    [Fact]
    public async Task FetchMetadataAsync_ValidCoubUrl_ReturnsCoubVideoWithMetadata()
    {
        // Arrange
        var coubUrl = "https://coub.com/view/testcoub";
        var mockVideoInfo = new CoubVideoInfo
        {
            Id = "testcoub",
            Title = "My Test Coub",
            Duration = 15,
            HasAudio = true,
            ChannelUrl = "my_channel",
            ViewCount = 500
        };
        _mockCoubApiClient.Setup(api => api.GetVideoInfoAsync(coubUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockVideoInfo);

        // Act
        var result = await _sut.FetchMetadataAsync(coubUrl);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("testcoub");
        result.Url.Should().Be(coubUrl);
        result.Title.Should().Be("My Test Coub");
        result.Duration.Should().Be(15);
        result.HasAudio.Should().BeTrue();
        result.CreatorName.Should().Be("my_channel");
        result.ViewCount.Should().Be(500);
        result.Width.Should().Be(1920); // Default value
        result.Height.Should().Be(1080); // Default value
        _mockCoubApiClient.Verify(api => api.GetVideoInfoAsync(coubUrl, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task FetchMetadataAsync_InvalidCoubUrl_ThrowsArgumentException(string invalidCoubUrl)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.FetchMetadataAsync(invalidCoubUrl));
    }

    [Fact]
    public async Task FetchMetadataAsync_ApiReturnsNull_ThrowsMetadataExtractionException()
    {
        // Arrange
        var coubUrl = "https://coub.com/view/nonexistent";
        _mockCoubApiClient.Setup(api => api.GetVideoInfoAsync(coubUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CoubVideoInfo)null!);

        // Act & Assert
        await Assert.ThrowsAsync<MetadataExtractionException>(() => _sut.FetchMetadataAsync(coubUrl))
            .WithMessage("Failed to fetch video metadata");
    }

    // Test cases for ExtractVideoSourceAsync
    [Fact]
    public async Task ExtractVideoSourceAsync_ValidCoubUrl_ReturnsExpectedSourceUrl()
    {
        // Arrange
        var coubUrl = "https://coub.com/view/456";
        var mockVideoInfo = new CoubVideoInfo { Id = "456", Title = "Another Coub" };
        var expectedSourceUrl = "https://media-source.coub.com/videos/456/webm/high.webm";

        _mockCoubApiClient.Setup(api => api.GetVideoInfoAsync(coubUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockVideoInfo);

        // Act
        var result = await _sut.ExtractVideoSourceAsync(coubUrl);

        // Assert
        result.Should().Be(expectedSourceUrl);
        _mockCoubApiClient.Verify(api => api.GetVideoInfoAsync(coubUrl, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ExtractVideoSourceAsync_InvalidCoubUrl_ThrowsArgumentException(string invalidCoubUrl)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.ExtractVideoSourceAsync(invalidCoubUrl));
    }

    [Fact]
    public async Task ExtractVideoSourceAsync_ApiReturnsNull_ThrowsMetadataExtractionException()
    {
        // Arrange
        var coubUrl = "https://coub.com/view/invalid";
        _mockCoubApiClient.Setup(api => api.GetVideoInfoAsync(coubUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CoubVideoInfo)null!);

        // Act & Assert
        await Assert.ThrowsAsync<MetadataExtractionException>(() => _sut.ExtractVideoSourceAsync(coubUrl))
            .WithMessage("Failed to get video ID for source extraction");
    }

    [Fact]
    public async Task ExtractVideoSourceAsync_ApiReturnsVideoInfoWithNullId_ThrowsMetadataExtractionException()
    {
        // Arrange
        var coubUrl = "https://coub.com/view/invalidid";
        var mockVideoInfo = new CoubVideoInfo { Id = null!, Title = "Coub with null ID" };
        _mockCoubApiClient.Setup(api => api.GetVideoInfoAsync(coubUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockVideoInfo);

        // Act & Assert
        await Assert.ThrowsAsync<MetadataExtractionException>(() => _sut.ExtractVideoSourceAsync(coubUrl))
            .WithMessage("Failed to get video ID for source extraction");
    }

    // Test cases for VerifyDownloadAsync
    [Fact]
    public async Task VerifyDownloadAsync_FileExistsAndIsNotEmpty_ReturnsTrue()
    {
        // Arrange
        var filePath = Path.Combine(Path.GetTempPath(), "testfile.txt");
        await File.WriteAllTextAsync(filePath, "dummy content");

        // Act
        var result = await _sut.VerifyDownloadAsync(filePath);

        // Assert
        result.Should().BeTrue();

        // Clean up
        File.Delete(filePath);
    }

    [Fact]
    public async Task VerifyDownloadAsync_FileDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var filePath = Path.Combine(Path.GetTempPath(), "nonexistentfile.txt");

        // Act
        var result = await _sut.VerifyDownloadAsync(filePath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyDownloadAsync_FileExistsButIsEmpty_ReturnsFalse()
    {
        // Arrange
        var filePath = Path.Combine(Path.GetTempPath(), "emptyfile.txt");
        await File.WriteAllTextAsync(filePath, ""); // Create an empty file

        // Act
        var result = await _sut.VerifyDownloadAsync(filePath);

        // Assert
        result.Should().BeFalse();

        // Clean up
        File.Delete(filePath);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task VerifyDownloadAsync_InvalidFilePath_ThrowsArgumentException(string invalidFilePath)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.VerifyDownloadAsync(invalidFilePath));
    }

    // Test cases for DownloadVideoFileAsync (basic test due to complexity of mocking stream ops)
    [Fact]
    public async Task DownloadVideoFileAsync_ValidInputs_AttemptsFileDownload()
    {
        // Arrange
        var sourceUrl = "http://example.com/video.webm";
        var outputPath = Path.Combine(Path.GetTempPath(), "downloaded_video.webm");
        var content = "dummy video content";
        var contentStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StreamContent(contentStream)
            });

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var sutWithMockedHttpClient = new CoubDownloadService(
            httpClient,
            _mockVideoRepository.Object,
            _mockCoubApiClient.Object);

        // Act
        var result = await sutWithMockedHttpClient.DownloadVideoFileAsync(sourceUrl, outputPath);

        // Assert
        result.Should().Be(outputPath);
        File.Exists(outputPath).Should().BeTrue();
        await File.ReadAllTextAsync(outputPath).Should().BeEquivalentTo(content);

        // Clean up
        File.Delete(outputPath);
    }

    [Theory]
    [InlineData(null, "path.webm")]
    [InlineData("url", null)]
    [InlineData("", "path.webm")]
    [InlineData("url", "")]
    public async Task DownloadVideoFileAsync_InvalidInputs_ThrowsArgumentException(string sourceUrl, string outputPath)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.DownloadVideoFileAsync(sourceUrl, outputPath));
    }

    [Fact]
    public async Task DownloadVideoFileAsync_HttpRequestFails_ThrowsHttpRequestException()
    {
        // Arrange
        var sourceUrl = "http://example.com/video.webm";
        var outputPath = Path.Combine(Path.GetTempPath(), "downloaded_video.webm");

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound // Simulate an HTTP error
            });

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var sutWithMockedHttpClient = new CoubDownloadService(
            httpClient,
            _mockVideoRepository.Object,
            _mockCoubApiClient.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => sutWithMockedHttpClient.DownloadVideoFileAsync(sourceUrl, outputPath));
        File.Exists(outputPath).Should().BeFalse(); // Ensure no partial file is left
    }
}
