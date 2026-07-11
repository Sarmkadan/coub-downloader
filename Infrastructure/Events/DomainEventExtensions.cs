#nullable enable
// =====================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace CoubDownloader.Infrastructure.Events;

/// <summary>
/// Extension methods for <see cref="DomainEvent"/> providing common utility operations
/// for working with domain events in the CoubDownloader system.
/// </summary>
public static class DomainEventExtensions
{
    /// <summary>
    /// Creates a deep clone of the domain event using JSON serialization.
    /// </summary>
    /// <typeparam name="TEvent">Type of the domain event.</typeparam>
    /// <param name="event">The event to clone.</param>
    /// <returns>A deep copy of the event.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
    /// <exception cref="JsonException">Thrown when JSON serialization or deserialization fails.</exception>
    public static TEvent DeepClone<TEvent>(this TEvent @event) where TEvent : DomainEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        var json = JsonSerializer.Serialize(@event, new JsonSerializerOptions { WriteIndented = false });
        return JsonSerializer.Deserialize<TEvent>(json) ?? throw new JsonException("Deserialization returned null");
    }

    /// <summary>
    /// Safely gets the VideoId property from the event if it exists, otherwise returns null.
    /// </summary>
    /// <param name="event">The domain event.</param>
    /// <returns>VideoId if available, otherwise null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
    public static string? GetVideoId(this DomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return @event switch
        {
            VideoDownloadStartedEvent e => e.VideoId,
            VideoDownloadCompletedEvent e => e.VideoId,
            VideoDownloadFailedEvent e => e.VideoId,
            ConversionStartedEvent e => e.VideoId,
            ConversionCompletedEvent e => e.VideoId,
            _ => null
        };
    }

    /// <summary>
    /// Safely gets the OutputFile property from the event if it exists, otherwise returns null.
    /// </summary>
    /// <param name="event">The domain event.</param>
    /// <returns>OutputFile if available, otherwise null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
    public static string? GetOutputFile(this DomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return @event switch
        {
            VideoDownloadCompletedEvent e => e.FilePath,
            ConversionStartedEvent e => e.OutputFile,
            ConversionCompletedEvent e => e.OutputFile,
            _ => null
        };
    }

    /// <summary>
    /// Safely gets the Error property from the event if it exists, otherwise returns null.
    /// </summary>
    /// <param name="event">The domain event.</param>
    /// <returns>Error message if available, otherwise null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
    public static string? GetError(this DomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return @event switch
        {
            VideoDownloadFailedEvent e => e.Error,
            _ => null
        };
    }

    /// <summary>
    /// Determines if the event represents a failure state.
    /// </summary>
    /// <param name="event">The domain event.</param>
    /// <returns>True if the event is a failure event, otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
    public static bool IsFailure(this DomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return @event is VideoDownloadFailedEvent;
    }

    /// <summary>
    /// Determines if the event represents a successful completion.
    /// </summary>
    /// <param name="event">The domain event.</param>
    /// <returns>True if the event is a completion event, otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
    public static bool IsSuccess(this DomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return @event is VideoDownloadCompletedEvent or ConversionCompletedEvent;
    }

    /// <summary>
    /// Gets the duration of the operation from the event if available.
    /// </summary>
    /// <param name="event">The domain event.</param>
    /// <returns>Duration if available, otherwise <see cref="TimeSpan.Zero"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
    public static TimeSpan GetDuration(this DomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return @event switch
        {
            ConversionCompletedEvent e => e.Duration,
            _ => TimeSpan.Zero
        };
    }

    /// <summary>
    /// Gets the file size in bytes from the event if available.
    /// </summary>
    /// <param name="event">The domain event.</param>
    /// <returns>File size in bytes if available, otherwise 0.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
    public static long GetFileSize(this DomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return @event switch
        {
            VideoDownloadCompletedEvent e => e.FileSize,
            _ => 0
        };
    }

    /// <summary>
    /// Creates a standardized log message for the event.
    /// </summary>
    /// <param name="event">The domain event.</param>
    /// <param name="includeTimestamp">Whether to include timestamp in the message.</param>
    /// <returns>Formatted log message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
    public static string ToLogMessage(this DomainEvent @event, bool includeTimestamp = true)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var message = @event switch
        {
            VideoDownloadStartedEvent e =>
                $"Video download started: VideoId={e.VideoId}, Url={e.Url}",
            VideoDownloadCompletedEvent e =>
                $"Video download completed: VideoId={e.VideoId}, FileSize={e.FileSize} bytes, FilePath={e.FilePath}",
            VideoDownloadFailedEvent e =>
                $"Video download failed: VideoId={e.VideoId}, Error={e.Error}",
            ConversionStartedEvent e =>
                $"Conversion started: VideoId={e.VideoId}, Input={e.InputFile}, Output={e.OutputFile}",
            ConversionCompletedEvent e =>
                $"Conversion completed: VideoId={e.VideoId}, Output={e.OutputFile}, Duration={e.Duration}",
            BatchJobCreatedEvent e =>
                $"Batch job created: BatchId={e.BatchId}, Name={e.Name}, Tasks={e.TaskCount}",
            BatchJobCompletedEvent e =>
                $"Batch job completed: BatchId={e.BatchId}, Success={e.SuccessfulTasks}, Failed={e.FailedTasks}",
            _ => $"Event occurred: Type={@event.GetType().Name}, Id={@event.Id}"
        };

        return includeTimestamp
            ? $"[{@event.OccurredAt:yyyy-MM-dd HH:mm:ss}] {message}"
            : message;
    }

    /// <summary>
    /// Checks if the event is related to a specific video ID.
    /// </summary>
    /// <param name="event">The domain event.</param>
    /// <param name="videoId">The video ID to check against.</param>
    /// <returns>True if the event is related to the specified video ID.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="videoId"/> is null or whitespace.</exception>
    public static bool IsForVideo(this DomainEvent @event, string videoId)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentException.ThrowIfNullOrWhiteSpace(videoId);

        var eventVideoId = @event.GetVideoId();
        return string.Equals(eventVideoId, videoId, StringComparison.OrdinalIgnoreCase);
    }
}