using Xunit;
using LiteDatabase.Storage.Cache;
using LiteDatabase.Storage.PageData;

public class LRUCacheTests
{
    [Fact]
    public void LRUCache_EvictsLeastRecentlyUsed()
    {
        var evicted = new List<uint>();
        var cache = new LRUCache<uint, Page>(2, (key, page) => evicted.Add(key));

        var page1 = new Page { PageNo = 1 };
        var page2 = new Page { PageNo = 2 };
        var page3 = new Page { PageNo = 3 };

        cache.Put(1, page1);
        cache.Put(2, page2);
        cache.TryGet(1, out _); // 访问1，变为最近使用
        cache.Put(3, page3);    // 应该淘汰2

        Assert.False(cache.TryGet(2, out _)); // 2已被淘汰
        Assert.Contains<uint>(2, evicted);          // 回调被触发
        Assert.True(cache.TryGet(1, out var p1) && p1 == page1);
        Assert.True(cache.TryGet(3, out var p3) && p3 == page3);
    }
}