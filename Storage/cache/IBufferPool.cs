
using LiteDatabase.Storage.PageData;

namespace LiteDatabase.Storage;

public interface IBufferPool {
    Page GetPage(uint pageNo);
    void PutPage(Page page);
    void RemovePage(uint pageNo);
}