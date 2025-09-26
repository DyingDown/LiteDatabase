namespace LiteDatabase.Storage;

using System.IO;
using LiteDatabase.Config;
using LiteDatabase.Storage.Cache;
using LiteDatabase.Storage.PageData;
using Microsoft.VisualBasic;

public class Pager {

    private readonly FileIO fileIO;
    private readonly IBufferPool<uint, Page> bufferPool;

    public Pager(FileIO fileIO, IBufferPool<uint, Page> bufferPool) {
        this.fileIO = fileIO;
        this.bufferPool = bufferPool;
    }

    public Page GetPage(uint pageNo) {
        Page page;
        var success = bufferPool.TryGet(pageNo, out page);
        if (!success || page == null) {
            
            var bytes = fileIO.ReadPage(pageNo);
            page = Page.Decode(bytes);
            bufferPool.Put(pageNo, page);
        }
        return page;
    }

    public Page ReadPage(uint pageNo) {
        byte[] buffer = fileIO.ReadPage(pageNo);
        return PageDecode(buffer);
    }

    public void WritePage(Page page)
    {
        byte[] buffer = page.Encode();
        if (buffer.Length != StorageConfig.PAGE_SIZE)
            throw new Exception("Page size mismatch or overflow");
        fileIO.WritePage(page.PageNo, buffer);
    }

    public uint AllocatePage(PageType type)
    {
        uint pageNo = (uint)(fileIO.FileSize / StorageConfig.PAGE_SIZE);
        var page = new Page
        {
            PageType = type,
            PageNo = pageNo,
            PageData = type == PageType.MetaPage ? new MetaData() : new RecordData()
        };
        WritePage(page);
        return pageNo;
    }

    private Page PageDecode(byte[] buffer)
    {
        using var ms = new MemoryStream(buffer);
        using var br = new BinaryReader(ms);
        var page = new Page();
        page.PageType = (PageType)br.ReadByte();
        page.PageNo = br.ReadUInt32();
        page.PrevPageNo = br.ReadUInt32();
        page.NextPageNo = br.ReadUInt32();
        page.LSN = br.ReadInt64();
        int dataLen = br.ReadInt32();
        byte[] data = br.ReadBytes(dataLen);
        page.PageData = DecodePageData(page.PageType, data);
        return page;
    }

    private IPageData DecodePageData(PageType type, byte[] data)
    {
        IPageData pageData = type switch
        {
            PageType.MetaPage => new MetaData(),
            PageType.DataPage => new RecordData(),
            _ => throw new NotSupportedException($"Unknown PageType: {type}")
        };
        using var ms = new MemoryStream(data);
        pageData.Decode(ms);
        return pageData;
    }

}