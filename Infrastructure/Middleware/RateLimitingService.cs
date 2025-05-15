// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Infrastructure.Middleware;

/// <summary>Rate limiting service to prevent API abuse and overload</summary>
public class RateLimitingService
{
    private class RateLimitBucket
    {
        public int RequestsRemaining { get; set; }
        public DateTime ResetTime { get; set; }
    }

    private readonly Dictionary<string, RateLimitBucket> _buckets = [];
    private readonly int _maxRequests;
    private readonly TimeSpan _windowDuration;
    private readonly object _lockObj = new();

    /// <summary>Initialize rate limiter with requests per time window</summary>
    public RateLimitingService(int maxRequestsPerWindow = 100, int windowSeconds = 60)
    {
        _maxRequests = maxRequestsPerWindow;
        _windowDuration = TimeSpan.FromSeconds(windowSeconds);
    }

    /// <summary>Check if a request is allowed for the given identifier</summary>
    public bool IsAllowed(string identifier)
    {
        lock (_lockObj)
        {
            var now = DateTime.UtcNow;

            if (!_buckets.TryGetValue(identifier, out var bucket))
            {
                bucket = new RateLimitBucket
                {
                    RequestsRemaining = _maxRequests - 1,
                    ResetTime = now.Add(_windowDuration)
                };
                _buckets[identifier] = bucket;
                return true;
            }

            if (now >= bucket.ResetTime)
            {
                bucket.RequestsRemaining = _maxRequests - 1;
                bucket.ResetTime = now.Add(_windowDuration);
                return true;
            }

            if (bucket.RequestsRemaining > 0)
            {
                bucket.RequestsRemaining--;
                return true;
            }

            return false;
        }
    }

    /// <summary>Get rate limit status for an identifier</summary>
    public RateLimitStatus GetStatus(string identifier)
    {
        lock (_lockObj)
        {
            if (!_buckets.TryGetValue(identifier, out var bucket))
            {
                return new RateLimitStatus
                {
                    RequestsRemaining = _maxRequests,
                    ResetTime = DateTime.UtcNow.Add(_windowDuration),
                    IsAllowed = true
                };
            }

            return new RateLimitStatus
            {
                RequestsRemaining = Math.Max(0, bucket.RequestsRemaining),
                ResetTime = bucket.ResetTime,
                IsAllowed = bucket.RequestsRemaining > 0 || DateTime.UtcNow >= bucket.ResetTime
            };
        }
    }

    /// <summary>Reset the rate limit for an identifier</summary>
    public void Reset(string identifier)
    {
        lock (_lockObj)
        {
            if (_buckets.ContainsKey(identifier))
                _buckets.Remove(identifier);
        }
    }

    /// <summary>Clear all rate limit buckets</summary>
    public void ClearAll()
    {
        lock (_lockObj)
        {
            _buckets.Clear();
        }
    }
}

/// <summary>Rate limit status information</summary>
public class RateLimitStatus
{
    public int RequestsRemaining { get; set; }
    public DateTime ResetTime { get; set; }
    public bool IsAllowed { get; set; }
    public int SecondsUntilReset => Math.Max(0, (int)(ResetTime - DateTime.UtcNow).TotalSeconds);
}

/// <summary>Throttling service with token bucket algorithm</summary>
public class ThrottlingService
{
    private class TokenBucket
    {
        public double TokensAvailable { get; set; }
        public DateTime LastRefillTime { get; set; }
    }

    private readonly Dictionary<string, TokenBucket> _buckets = [];
    private readonly double _tokensPerSecond;
    private readonly double _maxTokens;
    private readonly object _lockObj = new();

    /// <summary>Initialize throttle with tokens per second and max burst</summary>
    public ThrottlingService(double tokensPerSecond, double maxTokens)
    {
        _tokensPerSecond = tokensPerSecond;
        _maxTokens = maxTokens;
    }

    /// <summary>Try to consume tokens, blocking until available</summary>
    public async Task ConsumeAsync(string identifier, double tokens = 1.0)
    {
        while (!TryConsume(identifier, tokens))
        {
            await Task.Delay(10);
        }
    }

    /// <summary>Try to consume tokens without blocking</summary>
    public bool TryConsume(string identifier, double tokens = 1.0)
    {
        lock (_lockObj)
        {
            var now = DateTime.UtcNow;

            if (!_buckets.TryGetValue(identifier, out var bucket))
            {
                bucket = new TokenBucket
                {
                    TokensAvailable = _maxTokens,
                    LastRefillTime = now
                };
                _buckets[identifier] = bucket;
            }

            // Refill tokens based on time elapsed
            var elapsed = (now - bucket.LastRefillTime).TotalSeconds;
            bucket.TokensAvailable = Math.Min(_maxTokens, bucket.TokensAvailable + elapsed * _tokensPerSecond);
            bucket.LastRefillTime = now;

            if (bucket.TokensAvailable >= tokens)
            {
                bucket.TokensAvailable -= tokens;
                return true;
            }

            return false;
        }
    }

    /// <summary>Get remaining tokens for an identifier</summary>
    public double GetRemainingTokens(string identifier)
    {
        lock (_lockObj)
        {
            if (!_buckets.TryGetValue(identifier, out var bucket))
                return _maxTokens;

            var now = DateTime.UtcNow;
            var elapsed = (now - bucket.LastRefillTime).TotalSeconds;
            return Math.Min(_maxTokens, bucket.TokensAvailable + elapsed * _tokensPerSecond);
        }
    }
}
