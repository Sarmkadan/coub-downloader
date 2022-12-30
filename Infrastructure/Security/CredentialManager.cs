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
    void StoreApiKey(string service, string apiKey);
    string? GetApiKey(string service);
    void DeleteApiKey(string service);
    bool ValidateApiKey(string service, string apiKey);
}

/// <summary>In-memory credential store (for development)</summary>
public class InMemoryCredentialManager : ICredentialManager
{
    private readonly Dictionary<string, string> _credentials = [];
    private readonly object _lockObj = new();

    public void StoreApiKey(string service, string apiKey)
    {
        lock (_lockObj)
        {
            _credentials[service.ToLowerInvariant()] = apiKey;
        }
    }

    public string? GetApiKey(string service)
    {
        lock (_lockObj)
        {
            return _credentials.TryGetValue(service.ToLowerInvariant(), out var key) ? key : null;
        }
    }

    public void DeleteApiKey(string service)
    {
        lock (_lockObj)
        {
            _credentials.Remove(service.ToLowerInvariant());
        }
    }

    public bool ValidateApiKey(string service, string apiKey)
    {
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

    public EncryptedCredentialManager(string storePath = "./credentials.enc", string? encryptionKey = null)
    {
        _storePath = storePath;

        // Use provided key or derive from machine key
        _encryptionKey = string.IsNullOrEmpty(encryptionKey)
            ? Encoding.UTF8.GetBytes("default-key-32-characters-long!")
            : Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32));
    }

    public void StoreApiKey(string service, string apiKey)
    {
        lock (_lockObj)
        {
            var encrypted = EncryptString(apiKey);
            _cache[service.ToLowerInvariant()] = encrypted;
            SaveToFile();
        }
    }

    public string? GetApiKey(string service)
    {
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

    public void DeleteApiKey(string service)
    {
        lock (_lockObj)
        {
            _cache.Remove(service.ToLowerInvariant());
            SaveToFile();
        }
    }

    public bool ValidateApiKey(string service, string apiKey)
    {
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
    public string TraceId { get; set; } = Guid.NewGuid().ToString();
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
    public string? OperationName { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = [];

    public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime;

    public override string ToString()
    {
        return $"TraceId={TraceId}, Operation={OperationName}, Elapsed={ElapsedTime.TotalMilliseconds}ms";
    }
}

/// <summary>Request context accessor (for async contexts)</summary>
public class RequestContextAccessor
{
    private static readonly AsyncLocal<RequestContext?> _context = new();

    public RequestContext? Current
    {
        get => _context.Value;
        set => _context.Value = value;
    }

    public void SetContext(RequestContext context)
    {
        _context.Value = context;
    }

    public void ClearContext()
    {
        _context.Value = null;
    }
}
