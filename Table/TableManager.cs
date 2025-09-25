using LiteDatabase.Catalog;
using LiteDatabase.Storage;
using LiteDatabase.Transaction;
using LiteDatabase.Sql.Ast;

namespace LiteDatabase.Table;

public class TableManager : ITableManager {
    private IStorageEngine storageEngine;

    private ITxnEngine txnEngine; //transaction

    private ICatalogManager catalogManager;

    public TableManager(IStorageEngine storageEngine, ITxnEngine txnEngine, ICatalogManager catalogManager) {
        this.storageEngine = storageEngine;
        this.txnEngine = txnEngine;
        this.catalogManager = catalogManager;
    }

    public void CreateTable(string tableName, List<ColumnDefinition> columns) {
        
    }

    
    // public void DropTable(string tableName);
    // public bool TableExists(string tableName);

    // public List<ColumnDefinition> GetTableColumns(string tableName);
    // public ColumnDefinition? GetColumn(string tableName, string columnName);

    // public void InsertRow(string tableName, Dictionary<string, object> values);
    // public List<Dictionary<string, object>> SelectRows(string tableName, Expression? whereClause);
    // public void UpdateRows(string tableName, Dictionary<string, object> updates, Expression? whereClause);
    // public void DeleteRows(string tableName, Expression? whereClause);
}