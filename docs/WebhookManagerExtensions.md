# WebhookManagerExtensions

The `WebhookManagerExtensions` class provides a set of static extension methods designed to simplify the querying and management of `WebhookSubscription` collections within the `coub-downloader` application. It facilitates efficient retrieval of active subscriptions, aggregation of failure metrics, and grouping of subscriptions by event type, enabling robust monitoring and maintenance logic without requiring direct manipulation of the underlying data structures.

## API

### SubscribeMultiple
Registers multiple webhook subscriptions to the manager in a single operation. This method streamlines the initialization process when subscribing to several events or targets simultaneously.
*   **Parameters**: Accepts the target `WebhookManager` instance and a collection of `WebhookSubscription` objects to register.
*   **Return Value**: `void`.
*   **Exceptions**: Throws an exception if the manager instance is null or if any subscription in the provided collection is invalid or already registered.

### GetActiveSubscription
Retrieves a single active subscription from the manager. If multiple active subscriptions exist, the selection criteria depend on the internal implementation order (typically the first encountered).
*   **Parameters**: The target `WebhookManager` instance.
*   **Return Value**: Returns a `WebhookSubscription?` object if an active subscription exists; otherwise, returns `null`.
*   **Exceptions**: Throws if the manager instance is null.

### GetSubscriptionsBy
Filters and retrieves a list of subscriptions based on specific criteria, such as event type or target URL.
*   **Parameters**: The target `WebhookManager` instance and a predicate or filter definition specifying the matching criteria.
*   **Return Value**: Returns an `IReadOnlyList<WebhookSubscription>` containing all matching subscriptions. Returns an empty list if no matches are found.
*   **Exceptions**: Throws if the manager instance is null or if the filter criteria are invalid.

### HasActiveSubscriptions
Determines whether the manager currently holds any subscriptions that are in an active state.
*   **Parameters**: The target `WebhookManager` instance.
*   **Return Value**: Returns `true` if at least one active subscription exists; otherwise, `false`.
*   **Exceptions**: Throws if the manager instance is null.

### GetTotalFailureCount
Aggregates the failure counters across all subscriptions managed by the instance to provide a global health metric.
*   **Parameters**: The target `WebhookManager` instance.
*   **Return Value**: Returns an `int` representing the sum of failure counts for all subscriptions.
*   **Exceptions**: Throws if the manager instance is null.

### GetOldestActiveSubscription
Identifies and returns the active subscription that was created earliest among all currently active subscriptions.
*   **Parameters**: The target `WebhookManager` instance.
*   **Return Value**: Returns a `WebhookSubscription?` representing the oldest active subscription, or `null` if no active subscriptions exist.
*   **Exceptions**: Throws if the manager instance is null.

### GroupByEventType
Organizes all subscriptions into a dictionary keyed by their respective `WebhookEventType`.
*   **Parameters**: The target `WebhookManager` instance.
*   **Return Value**: Returns an `IReadOnlyDictionary<WebhookEventType, IReadOnlyList<WebhookSubscription>>` where each key is an event type and the value is a list of subscriptions associated with that type.
*   **Exceptions**: Throws if the manager instance is null.

## Usage

### Example 1: Health Check and Failure Monitoring
This example demonstrates how to verify the existence of active subscriptions and retrieve global failure metrics to determine if manual intervention is required.

```csharp
using CoubDownloader.Webhooks;

public void PerformHealthCheck(WebhookManager manager)
{
    if (!manager.HasActiveSubscriptions())
    {
        Console.WriteLine("Warning: No active webhook subscriptions found.");
        return;
    }

    int totalFailures = manager.GetTotalFailureCount();
    if (totalFailures > 100)
    {
        Console.WriteLine($"Critical: Total failure count ({totalFailures}) exceeds threshold.");
        // Trigger alerting logic here
    }
    
    var oldest = manager.GetOldestActiveSubscription();
    if (oldest != null)
    {
        Console.WriteLine($"Oldest active subscription ID: {oldest.Id}");
    }
}
```

### Example 2: Event-Specific Subscription Analysis
This example illustrates grouping subscriptions by event type to generate a report on which events are being monitored and how many subscribers exist for each.

```csharp
using CoubDownloader.Webhooks;
using System.Linq;

public void GenerateSubscriptionReport(WebhookManager manager)
{
    var grouped = manager.GroupByEventType();

    foreach (var group in grouped)
    {
        Console.WriteLine($"Event Type: {group.Key}");
        Console.WriteLine($"  Active Subscribers: {group.Value.Count}");
        
        // Find specific subscriptions within this event type
        var specificSubs = manager.GetSubscriptionsBy(s => s.EventType == group.Key && s.IsEnabled);
        
        foreach (var sub in specificSubs)
        {
            Console.WriteLine($"    - {sub.TargetUrl} (Failures: {sub.FailureCount})");
        }
    }
}
```

## Notes

*   **Thread Safety**: As these methods operate on the state of the `WebhookManager`, callers must ensure that the manager instance is not being modified concurrently by other threads during enumeration operations (such as `GroupByEventType` or `GetSubscriptionsBy`) to avoid collection modification exceptions. The returned `IReadOnlyList` and `IReadOnlyDictionary` instances reflect the state at the time of the call and are not live views.
*   **Null Handling**: Methods returning nullable types (`WebhookSubscription?`) explicitly handle scenarios where no matching data exists by returning `null` rather than throwing. Callers must perform null checks before accessing properties of the returned subscription objects.
*   **Empty Collections**: `GetSubscriptionsBy` and `GroupByEventType` will return empty collections rather than `null` if no data matches the criteria, allowing for safe iteration without prior count checks.
*   **State Consistency**: The `GetOldestActiveSubscription` method relies on the creation timestamp or registration order of subscriptions. If subscriptions are added or removed rapidly, the "oldest" result may change between consecutive calls.
