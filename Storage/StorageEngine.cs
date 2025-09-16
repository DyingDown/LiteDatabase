namespace LiteDatabase.Storage;

public class StorageEngine : IStorageEngine {
    private Pager pager;

    private BufferPool bufferPool;

    private FileIO fileIO;

    public StorageEngine(Pager pager, BufferPool bufferPool, FileIO fileIO) {
        this.pager = pager;
        this.bufferPool = bufferPool;
        this.fileIO = fileIO;
    }
}