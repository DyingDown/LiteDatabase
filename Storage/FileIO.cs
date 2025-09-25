namespace LiteDatabase.Storage;

using LiteDatabase.Config;

public class FileIO : IDisposable {
    private readonly FileStream filestream;

    public FileIO(string path) {
        filestream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }

    public byte[] ReadPage(uint pageNo) {
        long offset = pageNo * StorageConfig.PAGE_SIZE;
        filestream.Seek(offset, SeekOrigin.Begin);
        byte[] buffer = new byte[StorageConfig.PAGE_SIZE];
        int read = filestream.Read(buffer, 0, StorageConfig.PAGE_SIZE);
        if (read < StorageConfig.PAGE_SIZE)
            throw new EndOfStreamException("Page not found or file truncated");
        return buffer;
    }

    public void WritePage(uint pageNo, byte[] buffer) {
        if (buffer.Length != StorageConfig.PAGE_SIZE) {
            throw new ArgumentException("Page data must be exactly PageSize bytes.");
        }
        long offset = pageNo * StorageConfig.PAGE_SIZE;
        filestream.Seek(offset, SeekOrigin.Begin);
        filestream.Write(buffer, 0, StorageConfig.PAGE_SIZE);
    }

    public void Flush() {
        filestream.Flush();
    }

    public void Dispose() {
        filestream.Dispose();
    }

    public long FileSize => filestream.Length;
}