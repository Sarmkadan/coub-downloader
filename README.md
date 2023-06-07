// entire file content ...
// ... goes in between

## WebhookManager

`WebhookManager` is a class responsible for managing webhook subscriptions and sending events to registered webhooks. It provides methods for subscribing to webhooks, unsubscribing, sending events, disabling subscriptions, and retrieving active subscriptions.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Integration;

// Create a new WebhookManager instance
var webhookManager = new WebhookManager(new HttpClient(), new LoggingService());

// Subscribe to a webhook
webhookManager.Subscribe("https://example.com/webhook", WebhookEventType.VideoDownloadStarted);

// Send an event to all subscribers
await webhookManager.SendEventAsync(WebhookEventType.VideoDownloadStarted, new { VideoId = "abc123" });

// Unsubscribe from a webhook
webhookManager.Unsubscribe("subscription-id");

// Disable a subscription
webhookManager.DisableSubscription("subscription-id");

// Get all active subscriptions
var subscriptions = webhookManager.GetSubscriptions();
```

// ... rest of code ...
