// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Http.Json;
using System.Text.Json;
using CoubDownloader.Infrastructure.Middleware;

namespace CoubDownloader.Infrastructure.Integration;

/// <summary>Webhook manager for event notifications</summary>
public class WebhookManager
{
    private readonly HttpClient _httpClient;
    private readonly ILoggingService _logger;
    private readonly List<WebhookSubscription> _subscriptions = [];
    private readonly object _lockObj = new();

    public WebhookManager(HttpClient httpClient, ILoggingService logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>Register a webhook subscription</summary>
    public void Subscribe(string webhookUrl, WebhookEventType eventType, string? secret = null)
    {
        lock (_lockObj)
        {
            var subscription = new WebhookSubscription
            {
                Id = Guid.NewGuid().ToString(),
                Url = webhookUrl,
                EventType = eventType,
                Secret = secret,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _subscriptions.Add(subscription);
            _logger.LogInfo($"Webhook subscription created: {webhookUrl} for {eventType}", "Webhooks");
        }
    }

    /// <summary>Unsubscribe from webhooks</summary>
    public bool Unsubscribe(string subscriptionId)
    {
        lock (_lockObj)
        {
            var subscription = _subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
            if (subscription == null) return false;

            _subscriptions.Remove(subscription);
            _logger.LogInfo($"Webhook subscription removed: {subscriptionId}", "Webhooks");
            return true;
        }
    }

    /// <summary>Send webhook event to all subscribers</summary>
    public async Task SendEventAsync(WebhookEventType eventType, object eventData)
    {
        List<WebhookSubscription> subscribers;

        lock (_lockObj)
        {
            subscribers = _subscriptions
                .Where(s => s.EventType == eventType && s.IsActive)
                .ToList();
        }

        var tasks = subscribers.Select(sub => SendToWebhookAsync(sub, eventData));
        await Task.WhenAll(tasks);
    }

    /// <summary>Disable a webhook subscription</summary>
    public void DisableSubscription(string subscriptionId)
    {
        lock (_lockObj)
        {
            var subscription = _subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
            if (subscription != null)
                subscription.IsActive = false;
        }
    }

    /// <summary>Get all active subscriptions</summary>
    public List<WebhookSubscription> GetSubscriptions(WebhookEventType? eventType = null)
    {
        lock (_lockObj)
        {
            return _subscriptions
                .Where(s => s.IsActive && (eventType == null || s.EventType == eventType))
                .ToList();
        }
    }

    private async Task SendToWebhookAsync(WebhookSubscription subscription, object eventData)
    {
        try
        {
            var payload = new
            {
                id = Guid.NewGuid().ToString(),
                timestamp = DateTime.UtcNow,
                eventType = subscription.EventType,
                data = eventData
            };

            var response = await _httpClient.PostAsJsonAsync(subscription.Url, payload,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = false });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    $"Webhook delivery failed: {subscription.Url} (HTTP {response.StatusCode})",
                    "Webhooks");

                // Increment failure count
                subscription.FailureCount++;

                // Disable after too many failures
                if (subscription.FailureCount >= 10)
                {
                    subscription.IsActive = false;
                    _logger.LogWarning($"Webhook disabled after repeated failures: {subscription.Url}", "Webhooks");
                }
            }
            else
            {
                subscription.FailureCount = 0;
                subscription.LastSuccessAt = DateTime.UtcNow;
                _logger.LogDebug($"Webhook delivered successfully: {subscription.Url}", "Webhooks");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send webhook to {subscription.Url}", ex, "Webhooks");
            subscription.FailureCount++;
        }
    }
}

/// <summary>Webhook subscription information</summary>
public class WebhookSubscription
{
    public string Id { get; set; } = "";
    public string Url { get; set; } = "";
    public WebhookEventType EventType { get; set; }
    public string? Secret { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastSuccessAt { get; set; }
    public int FailureCount { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>Webhook event types</summary>
public enum WebhookEventType
{
    VideoDownloadStarted,
    VideoDownloadCompleted,
    VideoDownloadFailed,
    ConversionStarted,
    ConversionCompleted,
    ConversionFailed,
    BatchJobCreated,
    BatchJobCompleted
}
