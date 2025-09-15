using LiteDatabase.Recovery;

namespace LiteDatabase.Transaction;

public class LogManager
{
    private RedoLog redoLog;

    private Checkpoint checkpoint;

    public LogManager()
    {
        redoLog = new RedoLog();
        checkpoint = new Checkpoint();
    }
}