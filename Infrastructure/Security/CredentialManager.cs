#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;

namespace CoubDownloader.Infrastructure.Security;

/// <summary>Credential and API key management</summary>
public interface ICredentialManager
{
    /// <summary>Stores an API key for the given service, replacing any existing key.</summary>
    /// <param name="service">Service name.</param>
    /// <param name="apiKey">API key to store.</param>
    void StoreApiKey(string service, string apiKey);

    /// <summary>Gets the API key stored for the given service.</summary>
    /// <param name="service">Service name.</param>
    /// <returns>The stored API key, or null if none is stored.</returns>
    string? GetApiKey(string service);

    /// <summary>Removes the API key stored for the given service, if any.</summary>
    /// <param name="service">Service name.</param>
    void DeleteApiKey(string service);

    /// <summary>Checks whether the given API key matches the one stored for the service.</summary>
    /// <param name="service">Service name.</param>
    /// <param name="apiKey">API key to compare.</param>
    /// <returns>True if a key is stored for the service and matches; otherwise false.</returns>
    bool ValidateApiKey(string service, string apiKey);
}

/// <summary>In-memory credential store (for development)</summary>
public class InMemoryCredentialManager : ICredentialManager
{
    private readonly Dictionary<string, string> _credentials = [];
    private readonly object _lockObj = new();

    /// <inheritdoc />
    public void StoreApiKey(string service, string apiKey)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(apiKey);

        lock (_lockObj)
        {
            _credentials[service.ToLowerInvariant()] = apiKey;
        }
    }

    /// <inheritdoc />
    public string? GetApiKey(string service)
    {
        ArgumentNullException.ThrowIfNull(service);

        lock (_lockObj)
        {
            return _credentials.TryGetValue(service.ToLowerInvariant(), out var key) ? key : null;
        }
    }

    /// <inheritdoc />
    public void DeleteApiKey(string service)
    {
        ArgumentNullException.ThrowIfNull(service);

        lock (_lockObj)
        {
            _credentials.Remove(service.ToLowerInvariant());
        }
    }

    /// <inheritdoc />
    public bool ValidateApiKey(string service, string apiKey)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(apiKey);

        var stored = GetApiKey(service);
        return !string.IsNullOrEmpty(stored) && stored == apiKey;
    }
}

/// <summary>Encrypted credential storage</summary>
public class EncryptedCredentialManager : ICredentialManager
{
    private readonly string _storePath;
    private readonly byte[] _encryptionKey;
    private readonly Dictionary<string, string> _cache = [];
    private readonly object _lockObj = new();

    /// <summary>Creates a credential manager that persists encrypted keys to a file.</summary>
    /// <param name="storePath">Path to the credential store file. Defaults to "./credentials.enc".</param>
    /// <param name="encryptionKey">
    /// Key material for AES encryption. Padded or truncated to 32 characters.
    /// When null or empty, a built-in fallback key is used.
    /// </param>
    public EncryptedCredentialManager(string storePath = "./credentials.enc", string? encryptionKey = null)
    {
        ArgumentNullException.ThrowIfNull(storePath);

        _storePath = storePath;

        // Use provided key or derive from machine key
        _encryptionKey = string.IsNullOrEmpty(encryptionKey)
            ? Encoding.UTF8.GetBytes("default-key-32-characters-long!")
            : Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32));
    }

    /// <inheritdoc />
    public void StoreApiKey(string service, string apiKey)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(apiKey);

        lock (_lockObj)
        {
            var encrypted = EncryptString(apiKey);
            _cache[service.ToLowerInvariant()] = encrypted;
            SaveToFile();
        }
    }

    /// <inheritdoc />
    /// <remarks>Returns null if the stored value cannot be decrypted.</remarks>
    public string? GetApiKey(string service)
    {
        ArgumentNullException.ThrowIfNull(service);

        lock (_lockObj)
        {
            if (_cache.TryGetValue(service.ToLowerInvariant(), out var encrypted))
            {
                try
                {
                    return DecryptString(encrypted);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
    }

    /// <inheritdoc />
    public void DeleteApiKey(string service)
    {
        ArgumentNullException.ThrowIfNull(service);

        lock (_lockObj)
        {
            _cache.Remove(service.ToLowerInvariant());
            SaveToFile();
        }
    }

    /// <inheritdoc />
    public bool ValidateApiKey(string service, string apiKey)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(apiKey);

        var stored = GetApiKey(service);
        return !string.IsNullOrEmpty(stored) && stored == apiKey;
    }

    private string EncryptString(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();

        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    private string DecryptString(string cipherText)
    {
        var buffer = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var iv = new byte[aes.IV.Length];
        Array.Copy(buffer, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(buffer, iv.Length, buffer.Length - iv.Length);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }

    private void SaveToFile()
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(_cache);
            File.WriteAllText(_storePath, json);
        }
        catch
        {
            // Failed to save - continue in memory
        }
    }
}

/// <summary>Request context for tracing and correlation</summary>
public class RequestContext
{
    /// <summary>Unique identifier used to correlate logs and operations.</summary>
    public string TraceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>UTC time when the request started.</summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>Identifier of the user the request runs for, if known.</summary>
    public string? UserId { get; set; }

    /// <summary>Name of the operation being traced, if set.</summary>
    public string? OperationName { get; set; }

    /// <summary>Additional key/value data attached to the request.</summary>
    public Dictionary<string, object> Metadata { get; set; } = [];

    /// <summary>Time elapsed since <see cref="StartTime"/>.</summary>
    public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime;

    /// <summary>Returns a short summary with trace ID, operation name and elapsed time.</summary>
    /// <returns>Formatted trace summary.</returns>
    public override string ToString()
    {
        return $"TraceId={TraceId}, Operation={OperationName}, Elapsed={ElapsedTime.TotalMilliseconds}ms";
    }
}

/// <summary>Request context accessor (for async contexts)</summary>
public class RequestContextAccessor
{
    private static readonly AsyncLocal<RequestContext?> _context = new();

    /// <summary>Gets or sets the request context for the current async flow.</summary>
    public RequestContext? Current
    {
        get => _context.Value;
        set => _context.Value = value;
    }

    /// <summary>Sets the request context for the current async flow.</summary>
    /// <param name="context">Context to set.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public void SetContext(RequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context.Value = context;
    }

    /// <summary>Clears the request context for the current async flow.</summary>
    public void ClearContext()
    {
        _context.Value = null;
    }
}
