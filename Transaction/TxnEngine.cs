using LiteDatabase.Storage;

namespace LiteDatabase.Transaction;

public class TxnEngine : ITxnEngine
{
    private readonly LockManager lockManager;

    private readonly VersionManager versionManager;

    private readonly LogManager logManager;

    public TxnEngine(LockManager lockManager, VersionManager versionManager, LogManager logManager)
    {
        this.logManager = logManager;
        this.lockManager = lockManager;
        this.versionManager = versionManager;
    }
}