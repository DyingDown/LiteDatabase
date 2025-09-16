using LiteDatabase.Catalog;
using LiteDatabase.Storage;
using LiteDatabase.Transaction;

namespace LiteDatabase.Table;

public class TableManager : ITableManager {
    private IStorageEngine storageEngine;

    private ITxnEngine txnEngine;

    private ICatalogManager catalogManager;

    public TableManager(IStorageEngine storageEngine, ITxnEngine txnEngine, ICatalogManager catalogManager) {
        this.storageEngine = storageEngine;
        this.txnEngine = txnEngine;
        this.catalogManager = catalogManager;
    }
}