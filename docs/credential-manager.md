# CredentialManager

`Infrastructure/Security/CredentialManager.cs` holds the credential abstraction for `coub-downloader`: the `ICredentialManager` interface, two implementations (`InMemoryCredentialManager`, `EncryptedCredentialManager`), and two request-tracing helpers (`RequestContext`, `RequestContextAccessor`) that share the file but are unrelated to credentials.

This document describes what the code does today, including its limitations. It does not change behavior.

## Purpose

The application needs to hold API keys for external services without scattering raw strings through the code. `ICredentialManager` gives one place to:

- store a key per service name,
- read it back,
- delete it,
- check whether a candidate key matches the stored one.

Service names are case-insensitive. `"Coub"` and `"coub"` refer to the same entry, because both implementations lowercase the name with `ToLowerInvariant()`.

## Components

| Type | Kind | Storage | Survives restart |
|------|------|---------|------------------|
| `ICredentialManager` | interface | - | - |
| `InMemoryCredentialManager` | class | `Dictionary<string, string>` in memory, plaintext | No |
| `EncryptedCredentialManager` | class | In-memory cache of AES-encrypted values, written to a JSON file | Writes yes, reads back no (see below) |
| `RequestContext` | class | Plain properties (`TraceId`, `StartTime`, `UserId`, `OperationName`, `Metadata`) | No |
| `RequestContextAccessor` | class | `AsyncLocal<RequestContext?>` | No |

### `ICredentialManager`

```csharp
void StoreApiKey(string service, string apiKey);
string? GetApiKey(string service);
void DeleteApiKey(string service);
bool ValidateApiKey(string service, string apiKey);
```

- `StoreApiKey` replaces any existing key for the service.
- `GetApiKey` returns `null` when nothing is stored.
- `DeleteApiKey` is a no-op when nothing is stored.
- `ValidateApiKey` returns `true` only when a key is stored for the service and equals `apiKey`.

### `InMemoryCredentialManager`

Keeps keys in a dictionary guarded by a lock. Nothing is written to disk. Keys are held in plaintext in process memory and are lost when the process exits.

It is intended for development and tests.

### `EncryptedCredentialManager`

Constructor:

```csharp
public EncryptedCredentialManager(string storePath = "./credentials.enc", string? encryptionKey = null)
```

- `storePath` is the JSON file that receives the encrypted cache. The default is relative to the current working directory.
- `encryptionKey` is the key material. The code pads it with spaces or truncates it to 32 characters, then uses the UTF-8 bytes as the AES-256 key. If it is `null` or empty, a built-in fallback key is used (see [Known issues](#known-issues)).

Each value is encrypted with AES-256-CBC, PKCS7 padding, and a random IV generated per value. The IV is prepended to the ciphertext, and the result is stored as Base64.

On every `StoreApiKey` or `DeleteApiKey`, the whole cache is serialized to `storePath` as JSON. Write failures are swallowed, so the in-memory cache stays correct even when the file cannot be written.

## Usage

### Current wiring

`Infrastructure/DependencyInjectionExtended.cs` registers the in-memory implementation:

```csharp
services.AddSingleton<ICredentialManager>(new InMemoryCredentialManager());
services.AddSingleton<RequestContextAccessor>();
```

So in the running application, credentials are not persisted. Code that resolves `ICredentialManager` from DI gets the in-memory store. If you need persistence, you must change this registration yourself. Check the Known issues section first.

### Basic use

```csharp
public class CoubApiKeyProvider
{
    private readonly ICredentialManager _credentials;

    public CoubApiKeyProvider(ICredentialManager credentials)
    {
        _credentials = credentials;
    }

    public string GetCoubKey()
    {
        return _credentials.GetApiKey("coub")
            ?? throw new InvalidOperationException("No API key configured for 'coub'.");
    }
}
```

Store a key once at startup, from configuration or user input rather than from source code:

```csharp
var key = Environment.GetEnvironmentVariable("COUB_API_KEY");
if (!string.IsNullOrEmpty(key))
{
    credentials.StoreApiKey("coub", key);
}
```

### Using `ValidateApiKey` for comparison

Use `ValidateApiKey` to check a key the user has supplied against the stored one. Do not compare with `GetApiKey(...) == input` yourself, because the call sequence and nullability are easy to get wrong. The method already handles the "not stored" case.

### Request context

`RequestContextAccessor` stores a `RequestContext` in an `AsyncLocal`, so it flows across `await` calls:

```csharp
var accessor = serviceProvider.GetRequiredService<RequestContextAccessor>();
accessor.SetContext(new RequestContext { OperationName = "Download", UserId = userId });
try
{
    // ... work that logs accessor.Current?.TraceId ...
}
finally
{
    accessor.ClearContext();
}
```

`RequestContext` is for correlating logs and operations. It holds no secrets. Do not put API keys in `Metadata`.

## Security guidance

- **Never log, print, or commit key values.** Log the service name at most. Keep real credentials out of code, Markdown files, and command output.
- **Load keys from the environment or a secret store,** then pass them to `StoreApiKey`. Do not hardcode them in source or in `appsettings.json`.
- **Supply your own `encryptionKey` for `EncryptedCredentialManager`.** Do not rely on the fallback key. Keep the key outside the store file and out of the repository. Losing the key makes the stored values unreadable.
- **Protect the store file.** Put `storePath` somewhere the service account can read and write, and nobody else can. Keep it out of version control. Add it to `.gitignore` if it is ever created inside the repository.
- **Use `InMemoryCredentialManager` only for development and tests.** Its plaintext storage offers no protection against memory inspection or core dumps.
- **Treat `ValidateApiKey` as a convenience, not a security control.** It uses ordinary `string ==`, which is not constant-time. If the comparison is exposed to a remote caller where timing matters, compare with `CryptographicOperations.FixedTimeEquals` on the UTF-8 bytes in your own code.

## Known issues

These are in the current code. This document describes them so callers can work around them. They are not fixed here.

1. **The default fallback key is invalid for AES-256.** `"default-key-32-characters-long!"` is 31 bytes in UTF-8, but AES-256 requires 32. `EncryptedCredentialManager` created with a `null` or empty key therefore throws `CryptographicException` ("Specified key is not a valid size for this algorithm") from `StoreApiKey`. `GetApiKey` catches decryption errors and returns `null`, so reads quietly return nothing. Always pass an explicit key of at least one character. Verified with a standalone `Aes.Create().Key` check on .NET 10.

2. **Keys are not reloaded from the store file.** The constructor does not read `storePath`. `EncryptedCredentialManager` writes the file, but a new instance starts with an empty cache. Persisted values are therefore not available after a restart, even with the correct key. Loading must be added before this class can be used for persistence.

3. **Non-ASCII key material is truncated by characters, not bytes.** The constructor uses `PadRight(32).Substring(0, 32)` and then UTF-8 encodes the result. A key with non-ASCII characters can exceed 32 bytes and fail as in issue 1.

4. **Persistence failures are silent.** `SaveToFile` catches all exceptions and discards them. Callers get no signal that a store did not reach disk.

5. **`DeleteApiKey` writes the file even when the service is absent.** This is harmless but unnecessary I/O.

6. **Service names are the only key.** There is no namespace, version, or expiry. Rotating a key means calling `StoreApiKey` again.

## Testing

Use `InMemoryCredentialManager` in unit tests. It needs no file system, so it cannot leave files behind. For `EncryptedCredentialManager`, pass an explicit 32-character key and a temporary path, and delete the file afterwards. The repository tests live in `tests/coub-downloader.Tests/` and use xUnit, FluentAssertions, and Moq.

Keep test key values inside the test. Do not print them, and do not copy them into documentation.

## Related

- `ICredentialManager.md` - older per-interface notes (its signatures are out of date; this document reflects the source).
- `Infrastructure/DependencyInjectionExtended.cs` - DI registration.
