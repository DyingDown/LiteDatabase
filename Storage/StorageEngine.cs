using LiteDatabase.Storage.Cache;
using LiteDatabase.Storage.PageData;
using LiteDatabase.Config;

namespace LiteDatabase.Storage;

public class StorageEngine : IStorageEngine {
    private Pager pager;

    public StorageEngine(string filePath) {
        FileIO fileIO = new FileIO(filePath);

        var cache = new LRUCache<uint, Page>(StorageConfig.BUFFER_POOL_SIZE);
        pager = new Pager(fileIO, cache);

        // 传入一个自定义函数作为 LRUCache 的回收回调（例如 onEvict）
        Action<uint, Page> onEvict = (key, page) => {
            pager.WritePage(page);
        };
        cache.AddExpireCallback(onEvict);
    }
}