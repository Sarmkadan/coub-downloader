# ObjectPoolExtensions

Provides extension methods for working with object pools, enabling safe acquisition, usage, and return of pooled resources in both synchronous and asynchronous contexts.

## API

### `public static PooledObject<T> GetPooledObject<T>(this IObjectPool<T> pool)`
- **Purpose:** Retrieves a wrapped instance from the pool.
- **Parameters:**  
  - `pool` – The object pool to draw from.
- **Return Value:** A `PooledObject<T>` that encapsulates the leased item and ensures it is returned to the pool when disposed.
- **Exceptions:**  
  - `ArgumentNullException` if `pool` is `null`.  
  - `ObjectDisposedException` if the pool has been disposed.

### `public static void Return<T>(this IObjectPool<T> pool, T obj)`
- **Purpose:** Returns a single item to the pool.
- **Parameters:**  
  - `pool` – The pool to which the item is returned.  
  - `obj` – The item to return; must have been previously obtained from the same pool.
- **Return Value:** None.
- **Exceptions:**  
  - `ArgumentNullException` if `pool` or `obj` is `null`.  
  - `InvalidOperationException` if `obj` does not belong to `pool`.

### `public static void ReturnRange<T>(this IObjectPool<T> pool, IEnumerable<T> items)`
- **Purpose:** Returns a collection of items to the pool.
- **Parameters:**  
  - `pool` – The target pool.  
  - `items` – Sequence of items to return.
- **Return Value:** None.
- **Exceptions:**  
  - `ArgumentNullException` if `pool` or `items` is `null`.  
  - `InvalidOperationException` if any item in `items` does not originate from `pool`.

### `public static void Use<T>(this IObjectPool<T> pool, Action<T> action)`
- **Purpose:** Executes a synchronous action with a pooled object, guaranteeing its return even if the action throws.
- **Parameters:**  
  - `pool` – The pool supplying the object.  
  - `action` – Delegate that receives the pooled instance.
- **Return Value:** None.
- **Exceptions:**  
  - `ArgumentNullException` if `pool` or `action` is `null`.  
  - Any exception thrown by `action` is propagated after the object is returned to the pool.

### `public static TResult Use<T, TResult>(this IObjectPool<T> pool, Func<T, TResult> func)`
- **Purpose:** Executes a synchronous function with a pooled object and returns its result, ensuring the object is returned to the pool.
- **Parameters:**  
  - `pool` – The pool supplying the object.  
  - `func` – Delegate that receives the pooled instance and returns a result.
- **Return Value:** The result produced by `func`.
- **Exceptions:**  
  - `ArgumentNullException` if `pool` or `func` is `null`.  
  - Any exception thrown by `func` is propagated after the object is returned.

### `public static async Task UseAsync<T>(this IObjectPool<T> pool, Func<T, Task> action)`
- **Purpose:** Executes an asynchronous action with a pooled object, guaranteeing its return upon completion or fault.
- **Parameters:**  
  - `pool` – The pool supplying the object.  
  - `action` – Async delegate that receives the pooled instance.
- **Return Value:** A `Task` representing the asynchronous operation.
- **Exceptions:**  
  - `ArgumentNullException` if `pool` or `action` is `null`.  
  - Any exception thrown by `action` is propagated after the object is returned.

### `public static async Task UseConnectionAsync(this IObjectPool<DbConnection> pool, Func<DbConnection, Task> action)`
- **Purpose:** Executes an asynchronous action with a pooled database connection, ensuring the connection is returned to the pool.
- **Parameters:**  
  - `pool` – The pool of `DbConnection` instances.  
  - `action` – Async delegate that receives the connection.
- **Return Value:** A `Task` representing the asynchronous operation.
- **Exceptions:**  
  - `ArgumentNullException` if `pool` or `action` is `null`.  
  - Any exception thrown by `action` is propagated after the connection is returned.

### `public static async Task<TResult> UseConnectionAsync<TResult>(this IObjectPool<DbConnection> pool, Func<DbConnection, Task<TResult>> func)`
- **Purpose:** Executes an asynchronous function with a pooled database connection and returns its result, guaranteeing the connection is returned to the pool.
- **Parameters:**  
  - `pool` – The pool of `DbConnection` instances.  
  - `func` – Async delegate that receives the connection and returns a result.
- **Return Value:** A `Task<TResult>` that completes with the result of `func`.
- **Exceptions:**  
  - `ArgumentNullException` if `pool` or `func` is `null`.  
  - Any exception thrown by `func` is propagated after the connection is returned.

### `public static async Task UseMultipleConnectionsAsync(this IObjectPool<DbConnection> pool, int count, Func<IEnumerable<DbConnection>, Task> action)`
- **Purpose:** Executes an asynchronous action with multiple pooled database connections, returning all connections to the pool upon completion.
- **Parameters:**  
  - `pool` – The pool of `DbConnection` instances.  
  - `count` – Number of connections to acquire; must be greater than zero.  
  - `action` – Async delegate that receives an enumerable of the acquired connections.
- **Return Value:** A `Task` representing the asynchronous operation.
- **Exceptions:**  
  - `ArgumentNullException` if `pool` or `action` is `null`.  
  - `ArgumentOutOfRangeException` if `count` is less than or equal to zero.  
  - Any exception thrown by `action` is propagated after all connections are returned.

## Usage

### Example 1: Synchronous usage with a custom object pool
```csharp
var pool = new DefaultObjectPool<Buffer>(new BufferPolicy(), maximumRetained: 100);

// Acquire, use, and automatically return the buffer.
pool.Use(buffer =>
{
    // Fill the buffer with data.
    GetData(buffer);
});
```

### Example 2: Asynchronous usage with a database connection pool
```csharp
var connectionPool = new DefaultObjectPool<SqlConnection>(
    new PooledObjectPolicy<SqlConnection>(() => new SqlConnection(connectionString)),
    maximumRetained: 20);

// Execute a query and ensure the connection is returned.
await connectionPool.UseConnectionAsync(async conn =>
{
    await conn.OpenAsync();
    using var cmd = new SqlCommand("SELECT * FROM Users WHERE Active = 1", conn);
    using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        // Process each row.
    }
});
```

## Notes
- All extension methods treat the supplied `IObjectPool<T>` instance as the source of pooled objects; passing `null` results in an `ArgumentNullException`.
- The pooled object is always returned to the pool, even if the supplied delegate throws an exception; the original exception is propagated after the return operation.
- For connection‑specific methods (`UseConnectionAsync` overloads and `UseMultipleConnectionsAsync`), the pool must be configured to produce `DbConnection` derivatives; otherwise, invalid cast exceptions may occur at runtime.
- The pool itself is not thread‑safe by default; however, the default implementations provided by the library (`DefaultObjectPool<T>`) are thread‑safe for concurrent `GetPooledObject` and `Return` calls. Consumers should verify the thread‑safety guarantees of any custom pool implementation.
- When using `UseMultipleConnectionsAsync`, the acquired connections are returned to the pool in the order they were received; if the delegate disposes of any connection manually, subsequent attempts to return it may result in undefined behavior. It is recommended to rely solely on the pool’s return mechanism.
