#nullable enable

namespace CoubDownloader.Infrastructure.Utilities;

/// <summary>
/// Extension methods for <see cref="ObjectPool{T}"/>
/// </summary>
public static class ObjectPoolExtensions
{
    /// <summary>
    /// Creates a disposable wrapper for renting an object from the pool.
    /// Useful for 'using' statements to ensure proper disposal and return to pool.
    /// </summary>
    /// <typeparam name="T">The type of objects in the pool</typeparam>
    /// <param name="pool">The object pool</param>
    /// <returns>A disposable pooled object</returns>
    public static PooledObject<T> GetPooledObject<T>(this ObjectPool<T> pool) where T : class
    {
        return new PooledObject<T>(pool);
    }

    /// <summary>
    /// Returns multiple objects to the pool at once.
    /// More efficient than calling Return() multiple times.
    /// </summary>
    /// <typeparam name="T">The type of objects in the pool</typeparam>
    /// <param name="pool">The object pool</param>
    /// <param name="items">The objects to return</param>
    public static void ReturnRange<T>(this ObjectPool<T> pool, IEnumerable<T> items) where T : class
    {
        if (items is null)
            return;

        foreach (var item in items)
        {
            pool.Return(item);
        }
    }

    /// <summary>
    /// Returns an object to the pool with an optional reset action.
    /// If the item implements IDisposable and the pool is full, it will be disposed.
    /// </summary>
    /// <typeparam name="T">The type of objects in the pool</typeparam>
    /// <param name="pool">The object pool</param>
    /// <param name="item">The object to return</param>
    /// <param name="customReset">Optional custom reset action</param>
    public static void Return<T>(this ObjectPool<T> pool, T item, Action<T>? customReset = null) where T : class
    {
        if (item is null)
            return;

        customReset?.Invoke(item);
        pool.Return(item);
    }

    /// <summary>
    /// Gets an object from the pool and executes an action with it.
    /// The object is automatically returned to the pool after the action completes.
    /// </summary>
    /// <typeparam name="T">The type of objects in the pool</typeparam>
    /// <param name="pool">The object pool</param>
    /// <param name="action">The action to execute with the pooled object</param>
    public static void Use<T>(this ObjectPool<T> pool, Action<T> action) where T : class
    {
        if (pool is null || action is null)
            return;

        using var pooledObj = pool.GetPooledObject();
        action(pooledObj.Object);
    }

    /// <summary>
    /// Gets an object from the pool and executes a function with it.
    /// The object is automatically returned to the pool after the function completes.
    /// </summary>
    /// <typeparam name="T">The type of objects in the pool</typeparam>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <param name="pool">The object pool</param>
    /// <param name="func">The function to execute with the pooled object</param>
    /// <returns>The result of the function</returns>
    public static TResult Use<T, TResult>(this ObjectPool<T> pool, Func<T, TResult> func) where T : class
    {
        if (pool is null || func is null)
            throw new ArgumentNullException(pool is null ? nameof(pool) : nameof(func));

        using var pooledObj = pool.GetPooledObject();
        return func(pooledObj.Object);
    }

    /// <summary>
    /// Gets an object from the pool asynchronously and executes an async action with it.
    /// The object is automatically returned to the pool after the action completes.
    /// </summary>
    /// <typeparam name="T">The type of objects in the pool</typeparam>
    /// <param name="pool">The object pool</param>
    /// <param name="asyncAction">The async action to execute with the pooled object</param>
    /// <returns>A task representing the operation</returns>
    public static async Task UseAsync<T>(this ObjectPool<T> pool, Func<T, Task> asyncAction) where T : class
    {
        if (pool is null || asyncAction is null)
            return;

        using var pooledObj = pool.GetPooledObject();
        await asyncAction(pooledObj.Object);
    }

    /// <summary>
    /// Gets a connection from the connection pool and executes an async action with it.
    /// The connection is automatically released back to the pool after the action completes.
    /// </summary>
    /// <param name="pool">The connection pool</param>
    /// <param name="asyncAction">The async action to execute with the connection</param>
    /// <returns>A task representing the operation</returns>
    public static async Task UseConnectionAsync(this ConnectionPool pool, Func<ConnectionHandle, Task> asyncAction)
    {
        if (pool is null || asyncAction is null)
            return;

        var connection = await pool.AcquireAsync();
        try
        {
            await asyncAction(connection);
        }
        finally
        {
            pool.Release(connection);
        }
    }

    /// <summary>
    /// Gets a connection from the connection pool and executes a function with it.
    /// The connection is automatically released back to the pool after the function completes.
    /// </summary>
    /// <param name="pool">The connection pool</param>
    /// <param name="func">The function to execute with the connection</param>
    /// <returns>The result of the function</returns>
    public static async Task<TResult> UseConnectionAsync<TResult>(this ConnectionPool pool, Func<ConnectionHandle, Task<TResult>> func)
    {
        if (pool is null || func is null)
            throw new ArgumentNullException(pool is null ? nameof(pool) : nameof(func));

        var connection = await pool.AcquireAsync();
        try
        {
            return await func(connection);
        }
        finally
        {
            pool.Release(connection);
        }
    }

    /// <summary>
    /// Gets multiple connections from the pool and executes an action with each.
    /// Connections are automatically released back to the pool.
    /// </summary>
    /// <param name="pool">The connection pool</param>
    /// <param name="count">Number of connections to acquire</param>
    /// <param name="asyncAction">The async action to execute with each connection</param>
    /// <returns>A task representing the operation</returns>
    public static async Task UseMultipleConnectionsAsync(this ConnectionPool pool, int count, Func<ConnectionHandle, Task> asyncAction)
    {
        if (pool is null || asyncAction is null || count <= 0)
            return;

        var connections = new List<ConnectionHandle>();
        try
        {
            for (int i = 0; i < count; i++)
            {
                connections.Add(await pool.AcquireAsync());
            }

            foreach (var connection in connections)
            {
                await asyncAction(connection);
            }
        }
        finally
        {
            foreach (var connection in connections)
            {
                pool.Release(connection);
            }
        }
    }
}