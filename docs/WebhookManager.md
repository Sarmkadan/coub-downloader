# WebhookManager

Centralizes subscription management and event dispatching for webhook integrations, tracking delivery status and failures while providing thread-safe operations for subscribing, unsubscribing, and sending events.

## API

### `WebhookManager`
Instantiates a new manager with a unique identifier and target URL. The instance is immediately active and ready to accept subscriptions.

### `Subscribe`
