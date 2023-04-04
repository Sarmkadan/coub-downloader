# ICredentialManager

`ICredentialManager` is an interface responsible for managing API key credentials in the `coub-downloader` application. It provides methods for securely storing, retrieving, validating, and deleting API keys, along with contextual metadata tracking for operational tracing and auditing purposes.

## API

### `void StoreApiKey(string apiKey)`
Stores an API key for later retrieval. The key is typically encrypted or obfuscated in the underlying implementation.

- **Parameters**: `apiKey` (string) - The API key to store.
- **Returns**: void.
- **Throws**: 
  - `ArgumentNullException` if `apiKey` is null.
  - `InvalidOperationException` if the credential store is unavailable or write-protected.

### `string? GetApiKey()`
Retrieves the stored API key. Returns null if no key has been stored.

- **Parameters**: None.
- **Returns**: The stored API key as a string, or null if not found.
- **Throws**: 
  - `InvalidOperationException` if the credential store is unavailable or read-protected.

### `void DeleteApiKey()`
Removes the stored API key from the credential store.

- **Parameters**: None.
- **Returns**: void.
- **Throws**: 
  - `InvalidOperationException` if the credential store is unavailable or write-protected.

### `bool ValidateApiKey(string apiKey)`
Validates whether the provided API key is syntactically correct and potentially functional.

- **Parameters**: `apiKey` (string) - The API key to validate.
- **Returns**: True if the key is valid; false otherwise.
- **Throws**: 
  - `ArgumentNullException` if `apiKey` is null.

### `EncryptedCredentialManager EncryptedCredentialManager { get; }`
Provides access to an encrypted credential management implementation.

- **Parameters**: None.
- **Returns**: An instance of `EncryptedCredentialManager`.
- **Throws**: 
  - `InvalidOperationException` if the encrypted credential manager is not initialized.

### `string TraceId { get; }`
Gets the unique identifier for the current operation trace.

- **Parameters**: None.
- **Returns**: A string representing the trace ID.

### `DateTime StartTime { get; }`
Gets the timestamp when the current operation context was initialized.

- **Parameters**: None.
- **Returns**: A `DateTime` value indicating the start time.

### `string? UserId { get; set; }`
Gets or sets the user identifier associated with the current context.

- **Parameters**: None for get; string for set.
- **Returns**: The user ID as a string, or null if not set.

### `string? OperationName { get; set; }`
Gets or sets the name of the current operation for logging/tracing purposes.

- **Parameters**: None for get; string for set.
- **Returns**: The operation name as a string, or null if not set.

### `Dictionary<string, object> Metadata { get; }`
Gets a dictionary for storing arbitrary metadata related to the current context.

- **Parameters**: None.
- **Returns**: A `Dictionary<string, object>` instance.

### `override string ToString()`
Returns a string representation of the credential manager state, including trace information and metadata.

- **Parameters**: None.
- **Returns**: A formatted string summarizing the manager's current state.

### `void SetContext(string traceId, DateTime startTime, string? userId, string? operationName)`
Initializes the operational context with trace and user information.

- **Parameters**: 
  - `traceId` (string) - Unique trace identifier.
  - `startTime` (DateTime) - Operation start time.
  - `userId` (string?) - Optional user identifier.
  - `operationName` (string?) - Optional operation name.
- **Returns**: void.
- **Throws**: 
  - `ArgumentNullException` if `traceId` is null.

### `void ClearContext()`
Resets the operational context to its default state.

- **Parameters**: None.
- **Returns**: void.

## Usage

### Storing and Retrieving an API Key
```csharp
var credentialManager = new CredentialManager();
credentialManager.StoreApiKey("my-api-key-123");

string? retrievedKey = credentialManager.GetApiKey();
Console.WriteLine($"Retrieved key: {retrievedKey}");
```

### Validating and Managing Context
```csharp
var credentialManager = new CredentialManager();
bool isValid = credentialManager.ValidateApiKey("test-key");

credentialManager.SetContext(
    traceId: "trace-001",
    startTime: DateTime.UtcNow,
    userId: "user-456",
    operationName: "DownloadOperation"
);

Console.WriteLine($"Trace ID: {credentialManager.TraceId}");
Console.WriteLine($"User ID: {credentialManager.UserId}");

credentialManager.ClearContext();
```

## Notes

- Implementations should ensure thread safety for `Metadata` and context-related properties (`TraceId`, `StartTime`, etc.) if accessed concurrently.
- `GetApiKey` may return null if no key has been stored or if the underlying store is inaccessible.
- `ValidateApiKey` performs syntactic validation only; it does not verify the key against external services.
- The `EncryptedCredentialManager` property implies that sensitive operations are delegated to a dedicated encryption-aware implementation.
- `SetContext` must be called before accessing context properties to avoid null reference exceptions.
- `Metadata` modifications are not thread-safe by default; external synchronization is required for concurrent writes.
