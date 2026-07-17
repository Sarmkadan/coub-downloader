#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace CoubDownloader.Infrastructure.Integration;

/// <summary>
/// Extension methods for <see cref="WebhookManager"/> providing additional webhook management functionality
/// </summary>
public static class WebhookManagerExtensions
{
    /// <summary>
    /// Subscribes multiple webhook URLs for the same event type with optional secret
    /// </summary>
    /// <param name="manager">The webhook manager instance</param>
    /// <param name="webhookUrls">Collection of webhook URLs to subscribe</param>
    /// <param name="eventType">The event type to subscribe to</param>
    /// <param name="secret">Optional secret for webhook verification</param>
    /// <exception cref="ArgumentNullException">Thrown when manager or webhookUrls is null</exception>
    public static void SubscribeMultiple(this WebhookManager manager, IEnumerable<string> webhookUrls, WebhookEventType eventType, string? secret = null)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(webhookUrls);

        foreach (var url in webhookUrls)
        {
            manager.Subscribe(url, eventType, secret);
        }
    }

    /// <summary>
    /// Gets the first active subscription for the specified event type
    /// </summary>
    /// <param name="manager">The webhook manager instance</param>
    /// <param name="eventType">The event type to find</param>
    /// <returns>The first active subscription or null if not found</returns>
    /// <exception cref="ArgumentNullException">Thrown when manager is null</exception>
    public static WebhookSubscription? GetActiveSubscription(this WebhookManager manager, WebhookEventType eventType)
    {
        ArgumentNullException.ThrowIfNull(manager);

        return manager.GetSubscriptions(eventType)
            .FirstOrDefault(s => s.IsActive);
    }

    /// <summary>
    /// Gets all subscriptions matching the specified predicate
    /// </summary>
    /// <param name="manager">The webhook manager instance</param>
    /// <param name="predicate">Filter predicate for subscriptions</param>
    /// <returns>Read-only list of matching subscriptions</returns>
    /// <exception cref="ArgumentNullException">Thrown when manager or predicate is null</exception>
    public static IReadOnlyList<WebhookSubscription> GetSubscriptionsBy(this WebhookManager manager, Func<WebhookSubscription, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(predicate);

        return manager.GetSubscriptions()
            .Where(predicate)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Checks if there are any active subscriptions for the specified event type
    /// </summary>
    /// <param name="manager">The webhook manager instance</param>
    /// <param name="eventType">The event type to check</param>
    /// <returns>True if active subscriptions exist, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when manager is null</exception>
    public static bool HasActiveSubscriptions(this WebhookManager manager, WebhookEventType eventType)
    {
        ArgumentNullException.ThrowIfNull(manager);

        return manager.GetSubscriptions(eventType)
            .Any(s => s.IsActive);
    }

    /// <summary>
    /// Gets the total failure count across all subscriptions
    /// </summary>
    /// <param name="manager">The webhook manager instance</param>
    /// <returns>Total failure count</returns>
    /// <exception cref="ArgumentNullException">Thrown when manager is null</exception>
    public static int GetTotalFailureCount(this WebhookManager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);

        return manager.GetSubscriptions()
            .Sum(s => s.FailureCount);
    }

    /// <summary>
    /// Gets the oldest active subscription based on creation date
    /// </summary>
    /// <param name="manager">The webhook manager instance</param>
    /// <returns>The oldest active subscription or null if none exist</returns>
    /// <exception cref="ArgumentNullException">Thrown when manager is null</exception>
    public static WebhookSubscription? GetOldestActiveSubscription(this WebhookManager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);

        return manager.GetSubscriptions()
            .Where(s => s.IsActive)
            .OrderBy(s => s.CreatedAt)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets subscriptions grouped by event type
    /// </summary>
    /// <param name="manager">The webhook manager instance</param>
    /// <returns>Dictionary mapping event types to their active subscriptions</returns>
    /// <exception cref="ArgumentNullException">Thrown when manager is null</exception>
    public static IReadOnlyDictionary<WebhookEventType, IReadOnlyList<WebhookSubscription>> GroupByEventType(this WebhookManager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);

        var result = new Dictionary<WebhookEventType, List<WebhookSubscription>>();

        foreach (var subscription in manager.GetSubscriptions())
        {
            if (!result.TryGetValue(subscription.EventType, out var list))
            {
                list = [];
                result[subscription.EventType] = list;
            }

            if (subscription.IsActive)
            {
                list.Add(subscription);
            }
        }

        return result.ToImmutableDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<WebhookSubscription>)kvp.Value.AsReadOnly()
        );
    }
}