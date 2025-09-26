using System;
using System.IO;
using Xunit;
using LiteDatabase.Storage;
using LiteDatabase.Storage.PageData;
using LiteDatabase.Storage.Cache;

public class StorageEngineTests
{
    [Fact]
    public void WriteAndReadPage_ShouldBeConsistent()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var engine = new StorageEngine(tempFile);
            var page = new Page
            {
                PageType = PageType.DataPage,
                PageNo = 1,
                PageData = new RecordData { Raw = new byte[] { 1, 2, 3, 4 } }
            };
            engine.WritePage(page);

            var readPage = engine.ReadPage(1);
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
    public void StorageEngine_ShouldPersistData_AfterReopen()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            {
                var engine = new StorageEngine(tempFile);
                var page = new Page
                {
                    PageType = PageType.DataPage,
                    PageNo = 42,
                    PageData = new RecordData { Raw = new byte[] { 99, 100 } }
                };
                engine.WritePage(page);
            }
            {
                var engine = new StorageEngine(tempFile);
                var page = engine.ReadPage(42);
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
}
