namespace LiteDatabase.Storage.PageData;

using System.Drawing;
using LiteDatabase.Config;
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

    public byte[] Encode() {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write((byte)PageType);
        bw.Write(PageNo);
        bw.Write(PrevPageNo);
        bw.Write(NextPageNo);
        bw.Write(LSN);
        byte[] data = PageData.Encode();
        bw.Write(data.Length);
        bw.Write(data);
        if (ms.Length < StorageConfig.PAGE_SIZE) {
            ms.Write(new byte[StorageConfig.PAGE_SIZE - ms.Length]);
        }
        return ms.ToArray();
    }

    public static Page Decode(byte[] buffer) {
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

    private static IPageData DecodePageData(PageType type, byte[] data) {
        IPageData pageData = type switch {
            PageType.MetaPage => new MetaData(),
            PageType.DataPage => new RecordData(),
            _ => throw new NotSupportedException($"Unknown PageType: {type}")
        };
        using var ms = new MemoryStream(data);
        pageData.Decode(ms);
        return pageData;
    }

    public int Size() => Encode().Length;
}