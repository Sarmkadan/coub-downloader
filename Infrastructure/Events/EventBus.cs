#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Infrastructure.Events;

/// <summary>Base class for all events in the system</summary>
public abstract class DomainEvent
{
    /// <summary>Unique identifier of the event instance</summary>
    public string Id { get; } = Guid.NewGuid().ToString();

    /// <summary>UTC timestamp at which the event was created</summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>Event handler interface</summary>
public interface IEventHandler<in TEvent> where TEvent : DomainEvent
{
    /// <summary>Handles the given event</summary>
    /// <param name="event">The event to handle</param>
    /// <returns>A task that completes when handling is finished</returns>
    Task HandleAsync(TEvent @event);
}

/// <summary>Event bus for publishing and subscribing to events</summary>
public interface IEventBus
{
    /// <summary>Registers a handler for events of type <typeparamref name="TEvent"/></summary>
    /// <param name="handler">The handler to register</param>
    void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent;

    /// <summary>Publishes an event to all handlers subscribed to its type</summary>
    /// <param name="event">The event to publish</param>
    /// <returns>A task that completes when all handlers have finished</returns>
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : DomainEvent;

    /// <summary>Removes a previously registered handler for events of type <typeparamref name="TEvent"/></summary>
    /// <param name="handler">The handler to remove</param>
    void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent;
}

/// <summary>In-process event bus implementation</summary>
public class InProcessEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = [];
    private readonly object _lockObj = new();

    /// <inheritdoc/>
    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent
    {
        lock (_lockObj)
        {
            var eventType = typeof(TEvent);

            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = [];
            }

            _handlers[eventType].Add((Func<TEvent, Task>)(async e => await handler.HandleAsync(e)));
        }
    }

    /// <inheritdoc/>
    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : DomainEvent
    {
        var eventType = typeof(TEvent);
        List<Delegate>? handlerList;

        lock (_lockObj)
        {
            _handlers.TryGetValue(eventType, out handlerList);
        }

        if (handlerList is null)
        {
            return;
        }

        var tasks = handlerList
            .Cast<Func<TEvent, Task>>()
            .Select(handler => handler(@event))
            .ToList();

        await Task.WhenAll(tasks);
    }

    /// <inheritdoc/>
    public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent
    {
        lock (_lockObj)
        {
            var eventType = typeof(TEvent);

            if (_handlers.TryGetValue(eventType, out var list))
            {
                list.RemoveAll(h => h.Target == handler);
            }
        }
    }
}

/// <summary>Application-specific events</summary>
public class VideoDownloadStartedEvent : DomainEvent
{
    /// <summary>Identifier of the Coub video being downloaded</summary>
    public string VideoId { get; set; } = "";

    /// <summary>Source URL of the download</summary>
    public string Url { get; set; } = "";
}

/// <summary>Event raised when a video download completes</summary>
public class VideoDownloadCompletedEvent : DomainEvent
{
    /// <summary>Identifier of the downloaded Coub video</summary>
    public string VideoId { get; set; } = "";

    /// <summary>Path of the file written to disk</summary>
    public string FilePath { get; set; } = "";

    /// <summary>Size of the downloaded file in bytes</summary>
    public long FileSize { get; set; }
}

/// <summary>Event raised when a video download fails</summary>
public class VideoDownloadFailedEvent : DomainEvent
{
    /// <summary>Identifier of the Coub video whose download failed</summary>
    public string VideoId { get; set; } = "";

    /// <summary>Description of the error that caused the failure</summary>
    public string Error { get; set; } = "";
}

/// <summary>Event raised when conversion starts</summary>
public class ConversionStartedEvent : DomainEvent
{
    /// <summary>Identifier of the video being converted</summary>
    public string VideoId { get; set; } = "";

    /// <summary>Path of the source file to convert</summary>
    public string InputFile { get; set; } = "";

    /// <summary>Path where the converted file will be written</summary>
    public string OutputFile { get; set; } = "";
}

/// <summary>Event raised when conversion completes</summary>
public class ConversionCompletedEvent : DomainEvent
{
    /// <summary>Identifier of the converted video</summary>
    public string VideoId { get; set; } = "";

    /// <summary>Path of the converted output file</summary>
    public string OutputFile { get; set; } = "";

    /// <summary>Time taken by the conversion</summary>
    public TimeSpan Duration { get; set; }
}

/// <summary>Event raised when a batch job is created</summary>
public class BatchJobCreatedEvent : DomainEvent
{
    /// <summary>Identifier of the batch job</summary>
    public string BatchId { get; set; } = "";

    /// <summary>Display name of the batch job</summary>
    public string Name { get; set; } = "";

    /// <summary>Number of tasks in the batch job</summary>
    public int TaskCount { get; set; }
}

/// <summary>Event raised when a batch job completes</summary>
public class BatchJobCompletedEvent : DomainEvent
{
    /// <summary>Identifier of the completed batch job</summary>
    public string BatchId { get; set; } = "";

    /// <summary>Number of tasks that completed successfully</summary>
    public int SuccessfulTasks { get; set; }

    /// <summary>Number of tasks that failed</summary>
    public int FailedTasks { get; set; }
}
