// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Infrastructure.Caching;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoubDownloader.Tests;

public class MemoryCacheServiceTests
{
    private readonly MemoryCacheService _cache = new(defaultTtlSeconds: 3600);

    [Fact]
    public void Set_ThenGet_ReturnsStoredValue()
    {
        _cache.Set("key1", "hello");
        var result = _cache.Get<string>("key1");
        result.Should().Be("hello");
    }

    [Fact]
    public void TryGet_ExistingKey_ReturnsTrueAndValue()
    {
        _cache.Set("count", 42);
        var found = _cache.TryGet<int>("count", out var value);

        found.Should().BeTrue();
        value.Should().Be(42);
    }

    [Fact]
    public void TryGet_MissingKey_ReturnsFalseAndDefault()
    {
        var found = _cache.TryGet<string>("nonexistent", out var value);

        found.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void Remove_ExistingKey_KeyNoLongerRetrievable()
    {
        _cache.Set("temp", "data");
        _cache.Remove("temp");

        var found = _cache.TryGet<string>("temp", out _);
        found.Should().BeFalse();
    }

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

    [Fact]
    public void GetStatistics_EmptyCache_HitRateIsZero()
    {
        var stats = _cache.GetStatistics();
        stats.HitRate.Should().Be(0);
    }

    [Fact]
    public void Set_ExpiredTtl_EntryNotRetrievable()
    {
        _cache.Set("expiring", "value", TimeSpan.FromMilliseconds(1));
        Thread.Sleep(50);

        var found = _cache.TryGet<string>("expiring", out _);
        found.Should().BeFalse();
    }

    [Fact]
    public void Set_OverwritesExistingKey()
    {
        _cache.Set("key", "first");
        _cache.Set("key", "second");

        _cache.Get<string>("key").Should().Be("second");
    }

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
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }
}

public class DistributedCacheAdapterTests
{
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
