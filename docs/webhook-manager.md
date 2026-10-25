# WebhookManager

`WebhookManager` (`Infrastructure/Integration/WebhookManager.cs`, namespace `CoubDownloader.Infrastructure.Integration`) keeps an in-memory list of webhook subscriptions and delivers application events to them as JSON `POST` requests. It is registered as a singleton in `Infrastructure/DependencyInjectionExtended.cs`, so all services share one subscription list.

The file also defines the supporting types `WebhookSubscription` (subscription record) and `WebhookEventType` (event enum). Extension helpers live in `Infrastructure/Integration/WebhookManagerExtensions.cs`.

## Contents

- [Constructor](#constructor)
- [Subscribe](#subscribe)
- [Unsubscribe](#unsubscribe)
- [SendEventAsync](#sendeventasync)
- [DisableSubscription](#disablesubscription)
- [GetSubscriptions](#getsubscriptions)
- [Delivery and payload format](#delivery-and-payload-format)
- [WebhookSubscription](#webhooksubscription)
- [WebhookEventType](#webhookeventtype)
- [Usage example](#usage-example)
- [Limitations](#limitations)

## Constructor

```csharp
public WebhookManager(HttpClient httpClient, ILoggingService logger)
```

| Parameter | Description |
|---|---|
| `httpClient` | Client used for every webhook `POST`. Its timeout, handler, and default headers apply to all deliveries. |
| `logger` | Receives activity logs under the category `"Webhooks"`. |

Neither argument is null-checked. The class is not disposable and does not own the `HttpClient`.

## Subscribe

```csharp
public void Subscribe(string webhookUrl, WebhookEventType eventType, string? secret = null)
```

Adds an active subscription for `eventType` that posts to `webhookUrl`. Each call creates a new subscription, so the same URL and event can be added more than once.

| Parameter | Description |
|---|---|
| `webhookUrl` | Absolute URL that receives the `POST`. It is not validated when subscribing. |
| `eventType` | The `WebhookEventType` to listen for. |
| `secret` | Optional value stored on the subscription as `Secret`. It is **not used** by the current code: no signature or header is produced from it. |

- **Returns:** `void`. The generated subscription ID is not returned. Find it with [`GetSubscriptions`](#getsubscriptions) if you need to unsubscribe later.
- **Side effects:** Assigns a new GUID as `Id`, sets `CreatedAt` to `DateTime.UtcNow`, sets `IsActive = true`, and logs the URL and event type at info level.
- **Exceptions:** None.

## Unsubscribe

```csharp
public bool Unsubscribe(string subscriptionId)
```

Removes the subscription with the given `Id` from the list entirely. This works for inactive subscriptions too.

- **Returns:** `true` if a subscription was removed, `false` if no subscription has that ID.
- **Exceptions:** None.

## SendEventAsync

```csharp
public async Task SendEventAsync(WebhookEventType eventType, object eventData)
```

Sends `eventData` to every **active** subscription whose `EventType` equals `eventType`. Deliveries run concurrently and the method waits for all of them.

| Parameter | Description |
|---|---|
| `eventType` | Event being raised. Only matching subscriptions are notified. |
| `eventData` | Any object. It is serialized to JSON as the payload's `data` field. |

- **Returns:** `Task`. Completes after every matching delivery has finished or failed.
- **Exceptions:** Does not throw for delivery problems. HTTP errors, network errors, and serialization errors are caught per subscriber and logged. If no subscription matches, the method returns immediately.
- **Concurrency:** Subscribers are chosen under a lock. The deliveries themselves run outside the lock.

Delivery behavior is described in [Delivery and payload format](#delivery-and-payload-format).

## DisableSubscription

```csharp
public void DisableSubscription(string subscriptionId)
```

Sets `IsActive = false` on the subscription with the given `Id`. The subscription stays in the list but stops receiving events and is hidden from `GetSubscriptions`. Unknown IDs are ignored silently. There is no public method to re-enable a subscription. Call `Subscribe` again to create a new one.

- **Returns:** `void`.
- **Exceptions:** None.

## GetSubscriptions

```csharp
public List<WebhookSubscription> GetSubscriptions(WebhookEventType? eventType = null)
```

Returns the **active** subscriptions, optionally filtered by event type.

| Parameter | Description |
|---|---|
| `eventType` | When `null` (default), returns active subscriptions for all event types. Otherwise only those for that type. |

- **Returns:** A new `List<WebhookSubscription>`. The list is a snapshot, but the elements are the live objects held by the manager. Changes to their properties are visible to the manager.
- **Exceptions:** None.

## Delivery and payload format

For each matching subscriber, `SendEventAsync` posts this JSON body with `HttpClient.PostAsJsonAsync`:

```json
{
  "id": "<new GUID for this delivery>",
  "timestamp": "<UTC time of this delivery attempt>",
  "eventType": 0,
  "data": { "...": "the eventData object" }
}
```

- `id` is a new GUID for each delivery attempt, not the subscription ID.
- `eventType` is serialized with the default `System.Text.Json` settings, so the enum is sent as its integer value (see [WebhookEventType](#webhookeventtype)).
- `data` is the serialized `eventData`.

Outcome handling, per subscription:

| Outcome | Effect on the subscription | Log |
|---|---|---|
| HTTP 2xx | `FailureCount` reset to `0`; `LastSuccessAt` set to now | Debug |
| Non-2xx response | `FailureCount` incremented. When it reaches **10**, `IsActive` is set to `false` and the subscription stops receiving events. | Warning, including the status code and response body |
| Exception (network, timeout, serialization, and so on) | `FailureCount` incremented. The subscription is **not** deactivated on this path. | Error |

There are no retries. A failed delivery is not queued and is not sent again.

## WebhookSubscription

```csharp
public class WebhookSubscription
```

| Property | Type | Description |
|---|---|---|
| `Id` | `string` | Unique subscription ID (GUID string). Default `""`. |
| `Url` | `string` | Destination URL. Default `""`. |
| `EventType` | `WebhookEventType` | Event type this subscription listens for. |
| `Secret` | `string?` | Stored optional secret. Not used for signing. |
| `CreatedAt` | `DateTime` | UTC time the subscription was created. |
| `LastSuccessAt` | `DateTime?` | UTC time of the last successful delivery, or `null`. |
| `FailureCount` | `int` | Consecutive delivery failures. Reset to `0` on success. |
| `IsActive` | `bool` | `false` after `DisableSubscription` or after 10 consecutive HTTP failures. |

All properties have public setters. Changing them directly bypasses the manager's lock. Prefer the manager's methods.

## WebhookEventType

```csharp
public enum WebhookEventType
```

| Value | Integer | Raised when |
|---|---|---|
| `VideoDownloadStarted` | 0 | A video download begins |
| `VideoDownloadCompleted` | 1 | A video download completes |
| `VideoDownloadFailed` | 2 | A video download fails |
| `ConversionStarted` | 3 | A video conversion begins |
| `ConversionCompleted` | 4 | A video conversion completes |
| `ConversionFailed` | 5 | A video conversion fails |
| `BatchJobCreated` | 6 | A batch job is created |
| `BatchJobCompleted` | 7 | A batch job completes |

The integer values are the default enum values, which are what appear in the payload. Do not reorder the enum without considering receivers that depend on these numbers.

## Usage example

```csharp
// Resolve the singleton from DI.
public class DownloadNotifier(WebhookManager webhooks)
{
    public async Task NotifyCompletedAsync(CoubVideo video)
    {
        // Each subscription gets its own POST; failures are logged, not thrown.
        await webhooks.SendEventAsync(
            WebhookEventType.VideoDownloadCompleted,
            new { video.Id, video.Title });
    }
}

// Register a receiver at startup.
webhooks.Subscribe(
    "https://hooks.example.com/coub",
    WebhookEventType.VideoDownloadCompleted);

// Find the subscription later and remove it.
var sub = webhooks.GetSubscriptions(WebhookEventType.VideoDownloadCompleted)
    .First(s => s.Url == "https://hooks.example.com/coub");
webhooks.Unsubscribe(sub.Id);
```

Because `Subscribe` does not return an ID, look it up with `GetSubscriptions` as shown above.

## Limitations

- **In-memory only.** Subscriptions are lost when the process restarts.
- **No signing.** `Secret` is stored but never used. Receivers cannot verify that a request came from this application.
- **No URL validation.** Malformed or unreachable URLs are accepted and fail at delivery time.
- **No retries or queue.** A delivery is attempted once. Events raised while a receiver is down are lost.
- **Exceptions do not count toward deactivation.** Only non-success HTTP responses reach the 10-failure threshold. A receiver that always times out or refuses connections keeps its subscription active.
- **Thread safety is partial.** Subscription list access is locked, but `FailureCount`, `IsActive`, and `LastSuccessAt` are updated outside the lock during delivery. Concurrent updates to the same subscription can race.
- **Timeout is not set here.** Delivery time limits come from the `HttpClient` that is passed in.
- **Disabled subscriptions are hidden.** After automatic deactivation, `GetSubscriptions` no longer returns the subscription, so its ID must be known to remove it with `Unsubscribe`.
