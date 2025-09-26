namespace LiteDatabase.Storage;

public class StorageEngine : IStorageEngine {
    private Pager pager;

    public StorageEngine(Pager pager) {
        this.pager = pager;
    }
}