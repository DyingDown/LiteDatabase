using LiteDatabase.Catalog;
using LiteDatabase.Sql;
using LiteDatabase.Storage;
using LiteDatabase.Table;
using LiteDatabase.Transaction;

namespace LiteDatabase;

class Program
{
    static void Main(string[] args)
    {
        // Console.WriteLine("Hello, World!");

        var pager = new Pager();
        var bufferPool = new BufferPool();
        var fileIO = new FileIO();

        IStorageEngine storageEngine = new StorageEngine(pager, bufferPool, fileIO);


        var logManager = new LogManager();
        var lockManager = new LockManager();
        var versionManager = new VersionManager();
        ITxnEngine txnEngine = new TxnEngine(lockManager, versionManager, logManager);

        ICatalogManager catalogManager = new CatalogManager();

        ITableManager tableManager = new TableManager(storageEngine, txnEngine, catalogManager);

        var parser = new Parser();
        var planner = new Planner();
        var executor = new Executor();
    }
}
