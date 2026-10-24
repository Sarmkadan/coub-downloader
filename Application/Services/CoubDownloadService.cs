#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoubDownloader.Domain.Constants;
using CoubDownloader.Domain.Exceptions;
using CoubDownloader.Domain.Models;
using CoubDownloader.Infrastructure.Integration;
using CoubDownloader.Infrastructure.Repositories;
using CoubDownloader.Infrastructure.Utilities;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Service for downloading and extracting data from Coub videos.
/// </summary>
public class CoubDownloadService : ICoubDownloadService
{
    private readonly HttpClient _httpClient;
    private readonly ICoubVideoRepository _videoRepository;
    private readonly ICoubApiClient _coubApiClient;

    private const string PartFileExtension = ".part";

    public CoubDownloadService(HttpClient httpClient, ICoubVideoRepository videoRepository, ICoubApiClient coubApiClient)
    {
        _httpClient = httpClient;
        _videoRepository = videoRepository;
        _coubApiClient = coubApiClient;

        if (_httpClient.Timeout == TimeSpan.FromSeconds(100))
        {
            _httpClient.Timeout = TimeSpan.FromMilliseconds(ApplicationConstants.HttpRequestTimeoutMs);
        }

        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", ApplicationConstants.DefaultUserAgent);
        }
    }

    public async Task<CoubVideo> DownloadVideoAsync(string coubUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(coubUrl);

        return await ExecuteWithErrorMappingAsync(async () =>
        {
            // Fetch metadata from Coub API
            var video = await FetchMetadataAsync(coubUrl, cancellationToken);

            // Extract video source URL
            video.SourceUrl = await ExtractVideoSourceAsync(coubUrl, cancellationToken);

            // Save video metadata to repository
            var savedVideo = await _videoRepository.CreateAsync(video);
            return savedVideo;
        }, coubUrl, ex => new VideoDownloadException("Failed to download video", coubUrl, ex));
    }

    public async Task<CoubVideo> FetchMetadataAsync(string coubUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(coubUrl);

        return await ExecuteWithErrorMappingAsync(async () =>
        {
            var videoInfo = await RetryHelper.ExecuteWithRetryAsync(
                () => _coubApiClient.GetVideoInfoAsync(coubUrl, cancellationToken),
                maxRetries: 3,
                shouldRetry: (ex, _) => IsRetryableNetworkException(ex));

            if (videoInfo == null)
            {
                throw new MetadataExtractionException("Failed to fetch video metadata", coubUrl);
            }

            var video = new CoubVideo
            {
                Id = videoInfo.Id,
                Url = coubUrl,
                Title = videoInfo.Title ?? throw new MetadataExtractionException("Video title is null", coubUrl),
                Duration = videoInfo.Duration,
                Width = 1920, // Default HD width
                Height = 1080, // Default HD height
                CreatorName = videoInfo.ChannelUrl ?? "Unknown",
                ViewCount = videoInfo.ViewCount,
                HasAudio = videoInfo.HasAudio,
                UploadedDate = null // API doesn't provide upload date
            };

            return video;
        }, coubUrl, ex => new MetadataExtractionException("Failed to extract video metadata", coubUrl, ex));
    }

    public async Task<string> ExtractVideoSourceAsync(string coubUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(coubUrl);

        try
        {
            var videoInfo = await RetryHelper.ExecuteWithRetryAsync(
                () => _coubApiClient.GetVideoInfoAsync(coubUrl, cancellationToken),
                maxRetries: 3,
                shouldRetry: (ex, _) => IsRetryableNetworkException(ex));

            if (videoInfo == null || string.IsNullOrEmpty(videoInfo.Id))
            {
                throw new MetadataExtractionException("Failed to get video ID for source extraction", coubUrl);
            }

            return $"https://media-source.coub.com/videos/{videoInfo.Id}/webm/high.webm";
        }
        catch (HttpRequestException ex)
        {
            throw new NetworkException("HTTP request to Coub API failed", coubUrl, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new NetworkException("Request to Coub API timed out", coubUrl, ex) { IsTimeout = true };
        }
        catch (Exception ex) when (ex is not CoubDownloaderException)
        {
            throw new MetadataExtractionException("Failed to extract video source URL", coubUrl, ex);
        }
    }

    public async Task<bool> VerifyDownloadAsync(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        try
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length == 0)
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new FileOperationException("Failed to verify downloaded file", filePath, FileOperationType.ExistsCheck, ex);
        }
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
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Bytes are written to a .part file and moved into place only after the size check passes.
            // Each attempt re-reads the .part length, so a retry resumes where the previous one stopped.
            var partPath = outputPath + PartFileExtension;

            // Use retry logic for the network operations
            return await RetryTransientOperationAsync(async () =>
            {
                var resumeFrom = File.Exists(partPath) ? new FileInfo(partPath).Length : 0L;

                using var request = new HttpRequestMessage(HttpMethod.Get, sourceUrl);
                if (resumeFrom > 0)
                {
                    request.Headers.Range = new RangeHeaderValue(resumeFrom, null);
                }

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new VideoDownloadException("Video not found on server", sourceUrl, (int)HttpStatusCode.NotFound);
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new VideoDownloadException("Access forbidden to video", sourceUrl, (int)HttpStatusCode.Forbidden);
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new VideoDownloadException("Rate limit exceeded", sourceUrl, (int)HttpStatusCode.TooManyRequests);
                }

                // 206 continues the .part file; a 200 means the server ignored Range, so the file is rewritten
                var resumed = resumeFrom > 0 && response.StatusCode == HttpStatusCode.PartialContent;
                if (resumed && response.Content.Headers.ContentRange?.From != resumeFrom)
                {
                    File.Delete(partPath);
                    throw new IOException($"Server resumed at a different offset than {resumeFrom}; restarting download");
                }

                if (response.StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable && resumeFrom > 0)
                {
                    // The stored partial no longer matches the remote file; start clean on the next attempt
                    File.Delete(partPath);
                    throw new IOException($"Range request from byte {resumeFrom} was not satisfiable; restarting download");
                }

                response.EnsureSuccessStatusCode();

                var expectedBytes = GetExpectedTotalBytes(response);
                var bytesDownloaded = resumed ? resumeFrom : 0L;
                var fileMode = resumed ? FileMode.Append : FileMode.Create;

                {
                    await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    await using var fileStream = new FileStream(partPath, fileMode, FileAccess.Write, FileShare.None, ApplicationConstants.DownloadChunkSize, useAsync: true);

                    var buffer = new byte[ApplicationConstants.DownloadChunkSize];
                    int bytesRead;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                        bytesDownloaded += bytesRead;

                        if (progress is not null && expectedBytes > 0)
                        {
                            progress.Report((int)(bytesDownloaded * 100 / expectedBytes));
                        }
                    }
                }

                var downloadedSize = new FileInfo(partPath).Length;
                if (expectedBytes > 0 && downloadedSize != expectedBytes)
                {
                    File.Delete(partPath);
                    throw new VideoDownloadException($"Size mismatch: expected {expectedBytes} bytes, got {downloadedSize}", sourceUrl);
                }

                File.Move(partPath, outputPath, overwrite: true);
                return outputPath;
            }, "Download video file", sourceUrl);
        }
        catch (Exception ex) when (ex is not CoubDownloaderException)
        {
            throw new VideoDownloadException("Failed to download video file", sourceUrl, ex);
        }
    }

    private static async Task<T> ExecuteWithErrorMappingAsync<T>(
        Func<Task<T>> operation,
        string url,
        Func<Exception, CoubDownloaderException> createFallbackException)
    {
        try
        {
            return await operation();
        }
        catch (HttpRequestException ex)
        {
            throw new NetworkException("HTTP request to Coub API failed", url, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new NetworkException("Request to Coub API timed out", url, ex) { IsTimeout = true };
        }
        catch (Exception ex) when (ex is not CoubDownloaderException)
        {
            throw createFallbackException(ex);
        }
    }

    private async Task<T> RetryTransientOperationAsync<T>(Func<Task<T>> operation, string operationName, string url, int maxAttempts = 3, int initialDelayMs = 1000)
    {
        int attempt = 0;
        Exception? lastException = null;

        while (attempt < maxAttempts)
        {
            attempt++;
            try
            {
                return await operation();
            }
            catch (HttpRequestException ex) when (IsTransientHttpError(ex.StatusCode))
            {
                lastException = ex;
                if (attempt < maxAttempts)
                {
                    var delayMs = initialDelayMs * (int)Math.Pow(2, attempt - 1);
                    await Task.Delay(delayMs, CancellationToken.None);
                }
            }
            catch (IOException ex)
            {
                lastException = ex;
                if (attempt < maxAttempts)
                {
                    var delayMs = initialDelayMs * (int)Math.Pow(2, attempt - 1);
                    await Task.Delay(delayMs, CancellationToken.None);
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                lastException = ex;
                if (attempt < maxAttempts)
                {
                    var delayMs = initialDelayMs * (int)Math.Pow(2, attempt - 1);
                    await Task.Delay(delayMs, CancellationToken.None);
                }
            }
        }

        throw new NetworkException($"{operationName} failed after {maxAttempts} attempts", url, lastException ?? new Exception("Unknown error"));
    }

    private bool IsTransientHttpError(HttpStatusCode? statusCode)
    {
        if (!statusCode.HasValue)
        {
            return false;
        }

        return statusCode.Value == HttpStatusCode.RequestTimeout
            || statusCode.Value == HttpStatusCode.TooManyRequests
            || statusCode.Value == HttpStatusCode.ServiceUnavailable
            || statusCode.Value == HttpStatusCode.GatewayTimeout
            || statusCode.Value == HttpStatusCode.InternalServerError
            || statusCode.Value == HttpStatusCode.BadGateway;
    }

    private static long GetExpectedTotalBytes(HttpResponseMessage response)
    {
        // For a 206 the Content-Length covers only the requested range, so the full size comes from Content-Range
        var total = response.StatusCode == HttpStatusCode.PartialContent
            ? response.Content.Headers.ContentRange?.Length
            : response.Content.Headers.ContentLength;

        return total ?? -1L;
    }

    private static bool IsRetryableNetworkException(Exception exception)
    {
        return exception is HttpRequestException
            || exception is TaskCanceledException { InnerException: TimeoutException };
    }
}
