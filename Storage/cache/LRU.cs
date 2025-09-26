namespace LiteDatabase.Storage.Cache;

public class LRUCache<K, V> : IBufferPool<K, V> {

    private readonly int _capacity;

    private readonly Dictionary<K, LinkedListNode<(K Key, V Value)>> _cache;

    private readonly LinkedList<(K Key, V Value)> _lruList;

    private readonly object _lock = new();

    private Action<K, V>? _expireCallback;

    public LRUCache(int capacity, Action<K, V>? expireCallback = null) {
        _capacity = capacity;
        _cache = new Dictionary<K, LinkedListNode<(K key, V value)>>(capacity);
        _lruList = new LinkedList<(K key, V value)>();
        _expireCallback = expireCallback;
    }

    public void AddExpireCallback(Action<K, V>? expireCallback) {
        _expireCallback = expireCallback;
    }


    public bool TryGet(K Key, out V value) {
        lock (_lock) {
            if (_cache.TryGetValue(Key, out var node)) {
                _lruList.Remove(node);
                _lruList.AddFirst(node);
                value = node.Value.Value;
                return true;
            }
        }
        value = default!;
        return false;
    }

    public void Put(K Key, V Value) {
        lock (_lock) {
            if (_cache.TryGetValue(Key, out var node)) {
                node.Value = (Key, Value);
                _lruList.Remove(node);
                _lruList.AddFirst(node);
            }
            else {
                if (_cache.Count >= _capacity) {
                    var lastNode = _lruList.Last!;
                    _lruList.RemoveLast();
                    _cache.Remove(lastNode.Value.Key);
                    _expireCallback?.Invoke(lastNode.Value.Key, lastNode.Value.Value);
                }
                var newNode = new LinkedListNode<(K, V)>((Key, Value));
                _lruList.AddFirst(newNode);
                _cache[Key] = newNode;
            }
        }
    }

    public void Close() {
        if (_expireCallback == null) return;
        lock (_lock) {
            foreach (var (key, value) in _lruList) {
                _expireCallback(key, value);
            }
        }
    }
}