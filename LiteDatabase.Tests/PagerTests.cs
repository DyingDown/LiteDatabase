using System;
using System.IO;
using Xunit;
using LiteDatabase.Storage;

public class PagerTests
{
    // 简易 Mock BufferPool
    private class SimpleBufferPool : IBufferPool<uint, Page>
    {
        private readonly Dictionary<uint, Page> dict = new();
        public bool TryGet(uint key, out Page value) => dict.TryGetValue(key, out value);
        public void Put(uint key, Page value) => dict[key] = value;
        public void Remove(uint key) => dict.Remove(key);
    }

    // 简易 Mock FileIO
    private class SimpleFileIO : FileIO
    {
        private readonly string path;
        public SimpleFileIO(string path) : base(path) { this.path = path; }
    }

    [Fact]
    public void WriteAndReadPage_ShouldBeConsistent()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var fileIO = new SimpleFileIO(tempFile);
            var bufferPool = new SimpleBufferPool();
            var pager = new Pager(fileIO, bufferPool);
            var page = new Page
            {
                PageType = PageType.DataPage,
                PageNo = 1,
                PageData = new RecordData { Raw = new byte[] { 1, 2, 3, 4 } }
            };
            pager.WritePage(page);

            var readPage = pager.ReadPage(1);
            Assert.Equal(page.PageNo, readPage.PageNo);
            Assert.Equal(page.PageType, readPage.PageType);
            Assert.Equal(((RecordData)page.PageData).Raw, ((RecordData)readPage.PageData).Raw);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void MultiplePages_ShouldBeConsistent()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var fileIO = new SimpleFileIO(tempFile);
            var bufferPool = new SimpleBufferPool();
            var pager = new Pager(fileIO, bufferPool);
            for (uint i = 1; i <= 10; i++)
            {
                var page = new Page
                {
                    PageType = PageType.DataPage,
                    PageNo = i,
                    PageData = new RecordData { Raw = new byte[] { (byte)i } }
                };
                pager.WritePage(page);
            }
            for (uint i = 1; i <= 10; i++)
            {
                var page = pager.ReadPage(i);
                Assert.Equal(i, page.PageNo);
                Assert.Equal(PageType.DataPage, page.PageType);
                Assert.Equal(new byte[] { (byte)i }, ((RecordData)page.PageData).Raw);
            }
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Pager_ShouldPersistData_AfterReopen()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            {
                var fileIO = new SimpleFileIO(tempFile);
                var bufferPool = new SimpleBufferPool();
                var pager = new Pager(fileIO, bufferPool);
                var page = new Page
                {
                    PageType = PageType.DataPage,
                    PageNo = 42,
                    PageData = new RecordData { Raw = new byte[] { 99, 100 } }
                };
                pager.WritePage(page);
            }
            {
                var fileIO = new SimpleFileIO(tempFile);
                var bufferPool = new SimpleBufferPool();
                var pager = new Pager(fileIO, bufferPool);
                var page = pager.ReadPage(42);
                Assert.Equal(42u, page.PageNo);
                Assert.Equal(PageType.DataPage, page.PageType);
                Assert.Equal(new byte[] { 99, 100 }, ((RecordData)page.PageData).Raw);
            }
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ReadPage_InvalidId_ShouldThrow()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var fileIO = new SimpleFileIO(tempFile);
            var bufferPool = new SimpleBufferPool();
            var pager = new Pager(fileIO, bufferPool);
            Assert.Throws<Exception>(() => pager.ReadPage(999));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
