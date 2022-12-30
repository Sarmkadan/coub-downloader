// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Infrastructure.Utilities;

/// <summary>Generic object pool for reusing expensive resources</summary>
public class ObjectPool<T> where T : class
{
    private readonly Stack<T> _available = [];
    private readonly HashSet<T> _inUse = [];
    private readonly Func<T> _factory;
    private readonly Action<T>? _reset;
    private readonly int _maxPoolSize;
    private readonly object _lockObj = new();

    public int AvailableCount => _available.Count;
    public int InUseCount => _inUse.Count;

    /// <summary>Initialize object pool with factory and optional reset action</summary>
    public ObjectPool(Func<T> factory, Action<T>? reset = null, int maxPoolSize = 10)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _reset = reset;
        _maxPoolSize = maxPoolSize;
    }

    /// <summary>Rent an object from the pool</summary>
    public T Rent()
    {
        lock (_lockObj)
        {
            T item;

            if (_available.Count > 0)
            {
                item = _available.Pop();
            }
            else
            {
                item = _factory();
            }

            _inUse.Add(item);
            return item;
        }
    }

    /// <summary>Return an object to the pool</summary>
    public void Return(T item)
    {
        if (item == null) return;

        lock (_lockObj)
        {
            _inUse.Remove(item);

            if (_available.Count < _maxPoolSize)
            {
                _reset?.Invoke(item);
                _available.Push(item);
            }
            else if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    /// <summary>Clear the pool and dispose all items</summary>
    public void Clear()
    {
        lock (_lockObj)
        {
            while (_available.Count > 0)
            {
                var item = _available.Pop();
                (item as IDisposable)?.Dispose();
            }

            foreach (var item in _inUse)
            {
                (item as IDisposable)?.Dispose();
            }

            _inUse.Clear();
        }
    }
}

/// <summary>Disposable helper for renting from pool</summary>
public struct PooledObject<T> : IDisposable where T : class
{
    private readonly ObjectPool<T> _pool;
    private T _object;
    private bool _disposed;

    public T Object => _object;

    public PooledObject(ObjectPool<T> pool)
    {
        _pool = pool;
        _object = pool.Rent();
        _disposed = false;
    }

    public void Dispose()
    {
        if (_disposed) return;

        if (_object != null)
            _pool.Return(_object);

        _disposed = true;
    }
}

/// <summary>Connection pool for managing multiple connections</summary>
public class ConnectionPool
{
    private readonly Stack<ConnectionHandle> _available = [];
    private readonly HashSet<ConnectionHandle> _inUse = [];
    private readonly int _maxConnections;
    private readonly Func<Task<ConnectionHandle>> _connectionFactory;
    private readonly object _lockObj = new();

    public int TotalConnections => _available.Count + _inUse.Count;
    public int AvailableConnections => _available.Count;
    public int InUseConnections => _inUse.Count;

    public ConnectionPool(Func<Task<ConnectionHandle>> factory, int maxConnections = 10)
    {
        _connectionFactory = factory;
        _maxConnections = maxConnections;
    }

    /// <summary>Get a connection from the pool</summary>
    public async Task<ConnectionHandle> AcquireAsync()
    {
        lock (_lockObj)
        {
            if (_available.Count > 0)
            {
                var connection = _available.Pop();
                _inUse.Add(connection);
                return connection;
            }

            if (_inUse.Count + _available.Count < _maxConnections)
            {
                // Create new connection
                var connection = _connectionFactory().Result;
                _inUse.Add(connection);
                return connection;
            }
        }

        // Wait for connection to become available
        while (true)
        {
            await Task.Delay(100);

            lock (_lockObj)
            {
                if (_available.Count > 0)
                {
                    var connection = _available.Pop();
                    _inUse.Add(connection);
                    return connection;
                }
            }
        }
    }

    /// <summary>Release a connection back to the pool</summary>
    public void Release(ConnectionHandle connection)
    {
        lock (_lockObj)
        {
            _inUse.Remove(connection);

            if (_available.Count < _maxConnections)
                _available.Push(connection);
            else
                connection.Dispose();
        }
    }

    /// <summary>Close all connections</summary>
    public void Close()
    {
        lock (_lockObj)
        {
            while (_available.Count > 0)
                _available.Pop().Dispose();

            foreach (var conn in _inUse)
                conn.Dispose();

            _inUse.Clear();
        }
    }
}

/// <summary>Represents a connection in the pool</summary>
public class ConnectionHandle : IDisposable
{
    private bool _disposed;

    public string Id { get; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public bool IsOpen { get; set; } = true;

    public void Dispose()
    {
        if (_disposed) return;

        IsOpen = false;
        _disposed = true;
    }
}
