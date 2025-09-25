namespace LiteDatabase.Storage;

public class StorageEngine : IStorageEngine {
    private Pager pager;

    private IBufferPool bufferPool;

    private FileIO fileIO;

    public StorageEngine(Pager pager, IBufferPool bufferPool, FileIO fileIO) {
        this.pager = pager;
        this.bufferPool = bufferPool;
        this.fileIO = fileIO;
    }
}