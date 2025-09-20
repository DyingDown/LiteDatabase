using LiteDatabase.Catalog;
using LiteDatabase.Sql;
using LiteDatabase.Sql.Token;
using LiteDatabase.Storage;
using LiteDatabase.Table;
using LiteDatabase.Tests;
using LiteDatabase.Transaction;
using Microsoft.Extensions.Logging;

namespace LiteDatabase;

class Program
{
    public static ILoggerFactory LoggerFactory { get; private set; } = null!;

    static void Main(string[] args) {
        // 创建全局 LoggerFactory
        LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        Console.WriteLine("程序启动！");

        // 运行 Parser 测试
        ParserTest.RunAllTests();

        // 运行表达式类型推断测试
        ExpressionTypeInferrerTest.TestTypeCaching();

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

        // var parser = new Parser(""); // Parser需要SQL字符串参数
        var planner = new Planner();
        var executor = new Executor();
    }
}
