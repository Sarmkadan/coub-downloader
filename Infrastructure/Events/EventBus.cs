// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Infrastructure.Events;

/// <summary>Base class for all events in the system</summary>
public abstract class DomainEvent
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>Event handler interface</summary>
public interface IEventHandler<in TEvent> where TEvent : DomainEvent
{
    Task HandleAsync(TEvent @event);
}

/// <summary>Event bus for publishing and subscribing to events</summary>
public interface IEventBus
{
    void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent;
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : DomainEvent;
    void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent;
}

/// <summary>In-process event bus implementation</summary>
public class InProcessEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = [];
    private readonly object _lockObj = new();

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent
    {
        lock (_lockObj)
        {
            var eventType = typeof(TEvent);

            if (!_handlers.ContainsKey(eventType))
                _handlers[eventType] = [];

            _handlers[eventType].Add((Func<TEvent, Task>)(async e => await handler.HandleAsync(e)));
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : DomainEvent
    {
        var eventType = typeof(TEvent);
        List<Delegate>? handlerList;

        lock (_lockObj)
        {
            _handlers.TryGetValue(eventType, out handlerList);
        }

        if (handlerList == null) return;

        var tasks = handlerList
            .Cast<Func<TEvent, Task>>()
            .Select(handler => handler(@event))
            .ToList();

        await Task.WhenAll(tasks);
    }

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
    public string VideoId { get; set; } = "";
    public string Url { get; set; } = "";
}

/// <summary>Event raised when a video download completes</summary>
public class VideoDownloadCompletedEvent : DomainEvent
{
    public string VideoId { get; set; } = "";
    public string FilePath { get; set; } = "";
    public long FileSize { get; set; }
}

/// <summary>Event raised when a video download fails</summary>
public class VideoDownloadFailedEvent : DomainEvent
{
    public string VideoId { get; set; } = "";
    public string Error { get; set; } = "";
}

/// <summary>Event raised when conversion starts</summary>
public class ConversionStartedEvent : DomainEvent
{
    public string VideoId { get; set; } = "";
    public string InputFile { get; set; } = "";
    public string OutputFile { get; set; } = "";
}

/// <summary>Event raised when conversion completes</summary>
public class ConversionCompletedEvent : DomainEvent
{
    public string VideoId { get; set; } = "";
    public string OutputFile { get; set; } = "";
    public TimeSpan Duration { get; set; }
}

/// <summary>Event raised when a batch job is created</summary>
public class BatchJobCreatedEvent : DomainEvent
{
    public string BatchId { get; set; } = "";
    public string Name { get; set; } = "";
    public int TaskCount { get; set; }
}

/// <summary>Event raised when a batch job completes</summary>
public class BatchJobCompletedEvent : DomainEvent
{
    public string BatchId { get; set; } = "";
    public int SuccessfulTasks { get; set; }
    public int FailedTasks { get; set; }
}
