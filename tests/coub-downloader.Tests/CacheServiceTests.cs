#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Infrastructure.Caching;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoubDownloader.Tests;

/// <summary>
/// Unit tests for <see cref="MemoryCacheService"/> implementation.
/// Tests basic cache operations including set, get, remove, clear, and statistics tracking.
/// </summary>
public class MemoryCacheServiceTests
{
    private readonly MemoryCacheService _cache = new(defaultTtlSeconds: 3600);

    /// <summary>
    /// Tests that a value can be stored and retrieved from the cache.
    /// Verifies that <see cref="MemoryCacheService.Set"/> and <see cref="MemoryCacheService.Get"/> work correctly together.
    /// </summary>
    [Fact]
    public void Set_ThenGet_ReturnsStoredValue()
    {
        _cache.Set("key1", "hello");
        var result = _cache.Get<string>("key1");
        result.Should().Be("hello");
    }

    /// <summary>
    /// Tests that TryGet can retrieve an existing value from the cache.
    /// Verifies that <see cref="MemoryCacheService.TryGet"/> returns true and the correct value when the key exists.
    /// </summary>
    [Fact]
    public void TryGet_ExistingKey_ReturnsTrueAndValue()
    {
        _cache.Set("count", 42);
        var found = _cache.TryGet<int>("count", out var value);

        found.Should().BeTrue();
        value.Should().Be(42);
    }

    /// <summary>
    /// Tests that TryGet handles missing keys gracefully.
    /// Verifies that <see cref="MemoryCacheService.TryGet"/> returns false and default value when the key does not exist.
    /// </summary>
    [Fact]
    public void TryGet_MissingKey_ReturnsFalseAndDefault()
    {
        var found = _cache.TryGet<string>("nonexistent", out var value);

        found.Should().BeFalse();
        value.Should().BeNull();
    }

    /// <summary>
    /// Tests that Remove deletes a key from the cache.
    /// Verifies that <see cref="MemoryCacheService.Remove"/> prevents subsequent retrieval of the deleted key.
    /// </summary>
    [Fact]
    public void Remove_ExistingKey_KeyNoLongerRetrievable()
    {
        _cache.Set("temp", "data");
        _cache.Remove("temp");

        var found = _cache.TryGet<string>("temp", out _);
        found.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Clear removes all entries from the cache.
    /// Verifies that <see cref="MemoryCacheService.Clear"/> empties the cache and all previously stored values become inaccessible.
    /// </summary>
    [Fact]
    public void Clear_AfterMultipleSets_CacheIsEmpty()
    {
        _cache.Set("a", 1);
        _cache.Set("b", 2);
        _cache.Set("c", 3);

        _cache.Clear();
        _cache.TryGet<int>("a", out _).Should().BeFalse();
        _cache.TryGet<int>("b", out _).Should().BeFalse();
    }

    /// <summary>
    /// Tests that cache statistics track hits and misses accurately.
    /// Verifies that <see cref="MemoryCacheService.GetStatistics"/> returns correct hit/miss counts and hit rate calculation.
    /// </summary>
    [Fact]
    public void GetStatistics_AfterHitsAndMisses_TracksAccurately()
    {
        _cache.Set("present", true);
        _cache.TryGet<bool>("present", out _);   // hit
        _cache.TryGet<bool>("present", out _);   // hit
        _cache.TryGet<bool>("absent", out _);    // miss

        var stats = _cache.GetStatistics();

        stats.Hits.Should().Be(2);
        stats.Misses.Should().Be(1);
        stats.HitRate.Should().BeApproximately(2.0 / 3.0, 0.001);
    }

    /// <summary>
    /// Tests that hit rate is zero when cache is empty.
    /// Verifies that <see cref="MemoryCacheService.GetStatistics"/> returns 0 for hit rate when no operations have been performed.
    /// </summary>
    [Fact]
    public void GetStatistics_EmptyCache_HitRateIsZero()
    {
        var stats = _cache.GetStatistics();
        stats.HitRate.Should().Be(0);
    }

    /// <summary>
    /// Tests that an entry with an expired TTL is not retrievable.
    /// Verifies that <see cref="MemoryCacheService.Set"/> with a TTL correctly expires the entry.
    /// </summary>
    [Fact]
    public void Set_ExpiredTtl_EntryNotRetrievable()
    {
        _cache.Set("expiring", "value", TimeSpan.FromMilliseconds(1));
        Thread.Sleep(50);

        var found = _cache.TryGet<string>("expiring", out _);
        found.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Set overwrites an existing key in the cache.
    /// Verifies that <see cref="MemoryCacheService.Set"/> updates the value for an existing key.
    /// </summary>
    [Fact]
    public void Set_OverwritesExistingKey()
    {
        _cache.Set("key", "first");
        _cache.Set("key", "second");

        _cache.Get<string>("key").Should().Be("second");
    }

    /// <summary>
    /// Tests that TryGet can deserialize a complex type from the cache.
    /// Verifies that <see cref="MemoryCacheService.TryGet"/> correctly retrieves and deserializes a complex object.
    /// </summary>
    [Fact]
    public void TryGet_ComplexType_DeserializesCorrectly()
    {
        var record = new CachePayload { Name = "coub-abc", Value = 99 };
        _cache.Set("record", record);

        _cache.TryGet<CachePayload>("record", out var retrieved);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("coub-abc");
        retrieved.Value.Should().Be(99);
    }

    private sealed class CachePayload
    {
        /// <summary>
        /// Gets or sets the name of the cache payload.
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// Gets or sets the value of the cache payload.
        /// </summary>
        public int Value { get; set; }
    }
}

/// <summary>
/// Unit tests for <see cref="DistributedCacheAdapter"/> implementation.
/// Tests basic cache operations including set, get, remove, clear, and statistics tracking in a distributed scenario.
/// </summary>
public class DistributedCacheAdapterTests
{
    /// <summary>
    /// Tests that Set propagates the value to the remote cache.
    /// Verifies that <see cref="DistributedCacheAdapter.Set"/> correctly updates the remote cache.
    /// </summary>
    [Fact]
    public void Set_PropagatesValueToRemoteCache()
    {
        var mockRemote = new Mock<ICacheService>();
        var localCache = new MemoryCacheService();
        var adapter = new DistributedCacheAdapter(localCache);
        adapter.AddRemoteCache(mockRemote.Object);

        adapter.Set("video-url", "https://coub.com/view/xyz");

        mockRemote.Verify(r => r.Set("video-url", "https://coub.com/view/xyz", null), Times.Once);
    }

    /// <summary>
    /// Tests that TryGet hits the local cache and does not query the remote cache when the key is present locally.
    /// Verifies that <see cref="DistributedCacheAdapter.TryGet"/> correctly retrieves the value from the local cache.
    /// </summary>
    [Fact]
    public void TryGet_HitOnLocal_DoesNotQueryRemote()
    {
        var mockRemote = new Mock<ICacheService>();
        var localCache = new MemoryCacheService();
        localCache.Set("cached-key", "local-value");

        var adapter = new DistributedCacheAdapter(localCache);
        adapter.AddRemoteCache(mockRemote.Object);

        adapter.TryGet<string>("cached-key", out var value);

        value.Should().Be("local-value");
        mockRemote.Verify(r => r.TryGet<string>(It.IsAny<string>(), out It.Ref<string?>.IsAny), Times.Never);
    }

    /// <summary>
    /// Tests that TryGet caches the value locally and returns it when the key is not present locally but is present in the remote cache.
    /// Verifies that <see cref="DistributedCacheAdapter.TryGet"/> correctly retrieves and caches the value from the remote cache.
    /// </summary>
    [Fact]
    public void TryGet_LocalMissRemoteHit_CachesLocallyAndReturnsValue()
    {
        var mockRemote = new Mock<ICacheService>();
        string? remoteVal = "from-remote";
        mockRemote.Setup(r => r.TryGet<string>("miss-key", out remoteVal)).Returns(true);

        var localCache = new MemoryCacheService();
        var adapter = new DistributedCacheAdapter(localCache);
        adapter.AddRemoteCache(mockRemote.Object);

        var found = adapter.TryGet<string>("miss-key", out var value);

        found.Should().BeTrue();
        value.Should().Be("from-remote");
        // Subsequent local lookup should now succeed
        localCache.TryGet<string>("miss-key", out var localValue);
        localValue.Should().Be("from-remote");
    }

    /// <summary>
    /// Tests that Remove propagates the deletion to the remote cache.
    /// Verifies that <see cref="DistributedCacheAdapter.Remove"/> correctly removes the key from the remote cache.
    /// </summary>
    [Fact]
    public void Remove_PropagatesDeletionToRemoteCache()
    {
        var mockRemote = new Mock<ICacheService>();
        var localCache = new MemoryCacheService();
        localCache.Set("to-delete", "data");

        var adapter = new DistributedCacheAdapter(localCache);
        adapter.AddRemoteCache(mockRemote.Object);

        adapter.Remove("to-delete");

        mockRemote.Verify(r => r.Remove("to-delete"), Times.Once);
        localCache.TryGet<string>("to-delete", out _).Should().BeFalse();
    }

    /// <summary>
    /// Tests that Clear propagates the clear operation to the remote cache.
    /// Verifies that <see cref="DistributedCacheAdapter.Clear"/> correctly clears the remote cache.
    /// </summary>
    [Fact]
    public void Clear_PropagatesClearToRemoteCache()
    {
        var mockRemote = new Mock<ICacheService>();
        var localCache = new MemoryCacheService();

        var adapter = new DistributedCacheAdapter(localCache);
        adapter.AddRemoteCache(mockRemote.Object);

        adapter.Clear();

        mockRemote.Verify(r => r.Clear(), Times.Once);
    }

    /// <summary>
    /// Tests that Set does not bubble an exception when the remote cache throws.
    /// Verifies that <see cref="DistributedCacheAdapter.Set"/> handles exceptions from the remote cache correctly.
    /// </summary>
    [Fact]
    public void Set_RemoteThrows_DoesNotBubbleException()
    {
        var mockRemote = new Mock<ICacheService>();
        mockRemote.Setup(r => r.Set(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>()))
                  .Throws<InvalidOperationException>();

        var adapter = new DistributedCacheAdapter(new MemoryCacheService());
        adapter.AddRemoteCache(mockRemote.Object);

        var act = () => adapter.Set("key", "value");
        act.Should().NotThrow();
    }
}
