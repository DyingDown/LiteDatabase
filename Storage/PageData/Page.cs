namespace LiteDatabase.Storage.PageData;

using LiteDatabase.Recovery;

public class Page {
    public PageType PageType { get; set; }
    public uint PageNo { get; set; }
    public uint PrevPageNo { get; set; }
    public uint NextPageNo { get; set; }
    public bool Dirty { get; set; }
    public long LSN { get; set; }
    public IPageData PageData { get; set; }

    public List<Log> Logs { get; set; } = [];
    public ReaderWriterLockSlim Lock { get; set; } = new();
}