// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Domain.Exceptions;

/// <summary>
/// Base exception for all Coub Downloader related errors.
/// </summary>
public class CoubDownloaderException : Exception
{
    public CoubDownloaderException(string message) : base(message) { }
    public CoubDownloaderException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Exception thrown when a video cannot be downloaded from Coub.
/// </summary>
public class VideoDownloadException : CoubDownloaderException
{
    public string VideoUrl { get; set; }
    public int? HttpStatusCode { get; set; }

    public VideoDownloadException(string message, string videoUrl)
        : base($"Failed to download video from {videoUrl}: {message}")
    {
        VideoUrl = videoUrl;
    }

    public VideoDownloadException(string message, string videoUrl, int statusCode)
        : base($"Failed to download video from {videoUrl} (HTTP {statusCode}): {message}")
    {
        VideoUrl = videoUrl;
        HttpStatusCode = statusCode;
    }
}

/// <summary>
/// Exception thrown during video format conversion.
/// </summary>
public class VideoConversionException : CoubDownloaderException
{
    public string InputPath { get; set; }
    public string OutputPath { get; set; }

    public VideoConversionException(string message, string inputPath, string outputPath)
        : base($"Failed to convert video from {inputPath} to {outputPath}: {message}")
    {
        InputPath = inputPath;
        OutputPath = outputPath;
    }
}

/// <summary>
/// Exception thrown when audio processing fails.
/// </summary>
public class AudioProcessingException : CoubDownloaderException
{
    public string AudioFilePath { get; set; }

    public AudioProcessingException(string message, string audioPath)
        : base($"Failed to process audio from {audioPath}: {message}")
    {
        AudioFilePath = audioPath;
    }
}

/// <summary>
/// Exception thrown when a required external tool (FFmpeg, etc.) is not found.
/// </summary>
public class ToolNotFoundException : CoubDownloaderException
{
    public string ToolName { get; set; }

    public ToolNotFoundException(string toolName)
        : base($"Required tool '{toolName}' not found. Please ensure {toolName} is installed and available in PATH.")
    {
        ToolName = toolName;
    }
}

/// <summary>
/// Exception thrown when metadata extraction fails.
/// </summary>
public class MetadataExtractionException : CoubDownloaderException
{
    public string SourceUrl { get; set; }

    public MetadataExtractionException(string message, string sourceUrl)
        : base($"Failed to extract metadata from {sourceUrl}: {message}")
    {
        SourceUrl = sourceUrl;
    }
}

/// <summary>
/// Exception thrown for invalid configuration or parameters.
/// </summary>
public class InvalidConfigurationException : CoubDownloaderException
{
    public InvalidConfigurationException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when a resource is not found.
/// </summary>
public class ResourceNotFoundException : CoubDownloaderException
{
    public string ResourceId { get; set; }
    public string ResourceType { get; set; }

    public ResourceNotFoundException(string resourceType, string resourceId)
        : base($"{resourceType} with ID '{resourceId}' not found.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

/// <summary>
/// Exception thrown during batch job processing.
/// </summary>
public class BatchProcessingException : CoubDownloaderException
{
    public string BatchJobId { get; set; }
    public int FailedTaskCount { get; set; }

    public BatchProcessingException(string batchId, int failedCount, string message)
        : base($"Batch job '{batchId}' failed with {failedCount} failed tasks: {message}")
    {
        BatchJobId = batchId;
        FailedTaskCount = failedCount;
    }
}
