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
        
        // 运行语义分析器测试
        SemanticAnalyzerTest.RunAllTests();

        IStorageEngine storageEngine = new StorageEngine("test.db");

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
