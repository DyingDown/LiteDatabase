namespace LiteDatabase.Storage.Cache;

public interface IBufferPool<K, V> {
    public bool TryGet(K Key, out V value);
    public void Put(K Key, V Value);
    public void Close();
}