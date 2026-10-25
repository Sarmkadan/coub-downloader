# Object Pool

Source: `Infrastructure/Utilities/ObjectPool.cs` (with extension methods in `Infrastructure/Utilities/ObjectPoolExtensions.cs`).

The file contains four public types and one extension class:

| Type | Purpose |
|------|---------|
| `ObjectPool<T>` | Generic, thread-safe pool of reusable reference-type instances. |
| `PooledObject<T>` | Struct wrapper that rents on construction and returns on `Dispose()`. |
| `ConnectionPool` | Pool of `ConnectionHandle` tokens with a hard cap on total connections and blocking acquisition. |
| `ConnectionHandle` | Lightweight handle that the connection pool hands out. |
| `ObjectPoolExtensions` | Convenience methods (`Use`, `UseAsync`, `GetPooledObject`, `ReturnRange`, `UseConnectionAsync`, `UseMultipleConnectionsAsync`). |

## Pooling strategy

### `ObjectPool<T>`

- **Reuse order is LIFO.** Idle instances live in a `Stack<T>`, so `Rent()` returns the most recently returned instance, which is the one most likely to still be warm in cache.
- **Lazy creation.** An instance is created by the factory only when no idle instance is available. The pool never pre-allocates.
- **Idle retention is bounded, total is not.** `maxPoolSize` limits how many *idle* instances are kept. Instances that are currently rented are not counted, so the pool can create more objects than `maxPoolSize` under load.
- **Overflow handling.** When `Return()` finds the idle stack already full, the item is not reset or kept. If it implements `IDisposable` it is disposed; otherwise it is dropped for the garbage collector.
- **Reset hook.** The optional `reset` action runs on each item just before it goes back on the idle stack. It does not run for overflow items.
- **Synchronization.** Every mutation of the idle stack and the in-use set runs under a single private lock. `AvailableCount` and `InUseCount` read the collection counts without taking that lock, so they are only approximate under concurrency.

### `ConnectionPool`

- **Hard cap on total connections.** `maxConnections` bounds idle plus in-use handles. When the cap is reached, `AcquireAsync()` polls every 100 ms until a handle is released.
- **Excess handles are disposed.** `Release()` keeps a handle only while the idle count is below the cap; otherwise it disposes it.
- **No timeout.** The wait loop has no timeout or cancellation token. A caller waits as long as every handle stays in use.

## Public API

### `ObjectPool<T>` (`sealed`, `where T : class`)

#### `public ObjectPool(Func<T> factory, Action<T>? reset = null, int maxPoolSize = 10)`

Creates a pool.

- **Parameters**
  - `factory` – Creates a new instance when the idle stack is empty. Required.
  - `reset` – Optional. Cleans an instance before it is reused. Runs inside the pool lock.
  - `maxPoolSize` – Maximum number of idle instances kept. Default `10`. Values are not validated; `0` means no instance is ever kept idle.
- **Exceptions**: `ArgumentNullException` if `factory` is `null`.

#### `public int AvailableCount { get; }`

Number of idle instances currently held by the pool.

#### `public int InUseCount { get; }`

Number of instances rented and not yet returned.

#### `public T Rent()`

Takes an idle instance, or creates one with the factory if none is idle, and records it as in use.

- **Returns**: A non-null instance of `T`.
- **Note**: The factory runs while the pool lock is held. Keep factories cheap and non-blocking.

#### `public void Return(T item)`

Gives an instance back to the pool.

- **Parameters**
  - `item` – The instance to return. `null` is ignored.
- **Behavior**
  1. Removes `item` from the in-use set.
  2. If the idle count is below `maxPoolSize`, runs `reset` (if provided) and pushes `item` onto the idle stack.
  3. Otherwise, if `item` implements `IDisposable`, disposes it.
- **Note**: `Return` does not check that `item` was rented from this pool. See [Known limitations](#known-limitations).

#### `public void Clear()`

Disposes every idle instance and every in-use instance that implements `IDisposable`, then empties the in-use set.

- **Note**: The pool stays usable afterwards. There is no closed state. Instances that were in use when `Clear()` ran must not be used further. See [Known limitations](#known-limitations).

### `PooledObject<T>` (`struct`, `IDisposable`, `where T : class`)

A lease on one pooled instance. The lease rents on construction and returns on `Dispose()`.

#### `public PooledObject(ObjectPool<T> pool)`

Rents an instance from `pool` immediately.

#### `public T Object { get; }`

The rented instance. Use it only while the lease is alive.

#### `public void Dispose()`

Returns the instance to the pool. A second call on the same lease does nothing. A `default(PooledObject<T>)` is safe to dispose and does nothing.

- **Note**: Because this is a struct, each copy tracks disposal separately. Do not copy a lease; use it through a single `using` declaration.

### `ConnectionPool` (`sealed`)

#### `public ConnectionPool(Func<Task<ConnectionHandle>> factory, int maxConnections = 10)`

Creates a connection pool.

- **Parameters**
  - `factory` – Opens a new connection when the pool has room and no idle handle exists. Not null-checked; a `null` factory throws `NullReferenceException` on the first acquisition that needs it.
  - `maxConnections` – Maximum total handles (idle plus in use). Default `10`.

#### `public int TotalConnections { get; }`

`AvailableConnections + InUseConnections`.

#### `public int AvailableConnections { get; }`

Number of idle handles.

#### `public int InUseConnections { get; }`

Number of handles currently acquired.

#### `public async Task<ConnectionHandle> AcquireAsync()`

Returns an idle handle if one exists. Otherwise, if the pool is below `maxConnections`, calls the factory and returns the new handle. Otherwise, polls every 100 ms until a handle is released.

- **Returns**: A handle recorded as in use.
- **Note**: There is no timeout and no cancellation token.

#### `public void Release(ConnectionHandle connection)`

Returns a handle to the pool. Removes it from the in-use set, then keeps it as idle if the idle count is below `maxConnections`, or disposes it otherwise.

#### `public void Close()`

Disposes all idle handles and all in-use handles, then empties the in-use set. The pool stays usable afterwards.

### `ConnectionHandle` (`sealed`, `IDisposable`)

A token representing one connection. The class holds no network resources itself. The factory supplied to `ConnectionPool` is responsible for opening whatever the handle stands for.

| Member | Description |
|--------|-------------|
| `public string Id { get; }` | A new GUID string, set when the handle is constructed. |
| `public DateTime CreatedAt { get; }` | UTC creation time. |
| `public bool IsOpen { get; set; }` | `true` initially. `Dispose()` sets it to `false`. Settable by callers. |
| `public void Dispose()` | Sets `IsOpen` to `false`. Idempotent. |

### `ObjectPoolExtensions`

| Method | Behavior |
|--------|----------|
| `GetPooledObject<T>(this ObjectPool<T>)` | Returns a `PooledObject<T>` lease. Use with `using`. |
| `ReturnRange<T>(this ObjectPool<T>, IEnumerable<T>)` | Calls `Return` for each item. Throws `ArgumentNullException` if `items` is `null`. |
| `Return<T>(this ObjectPool<T>, T, Action<T>? customReset = null)` | Runs `customReset` (if given), then `pool.Return(item)`. Throws `ArgumentNullException` if `item` is `null`. Both the custom reset and the pool's own reset run. |
| `Use<T>(this ObjectPool<T>, Action<T>)` | Rents, runs the action, returns the instance even if the action throws. |
| `Use<T, TResult>(this ObjectPool<T>, Func<T, TResult>)` | As above, returns the function's result. |
| `UseAsync<T>(this ObjectPool<T>, Func<T, Task>)` | Async version of `Use`. |
| `UseConnectionAsync(this ConnectionPool, Func<ConnectionHandle, Task>)` | Acquires a handle, awaits the action, releases in `finally`. |
| `UseConnectionAsync<TResult>(this ConnectionPool, Func<ConnectionHandle, Task<TResult>>)` | As above, returns the result. |
| `UseMultipleConnectionsAsync(this ConnectionPool, int count, Func<ConnectionHandle, Task>)` | Acquires `count` handles one at a time, runs the action on each in order, releases all in `finally`. `count` must be greater than zero. |

## Usage

### Scoped lease

```csharp
var pool = new ObjectPool<StringBuilder>(
    factory: () => new StringBuilder(256),
    reset: sb => sb.Clear(),
    maxPoolSize: 16);

using (var lease = pool.GetPooledObject())
{
    lease.Object.Append("hello");
    Console.WriteLine(lease.Object.ToString());
}
```

### Manual rent and return

```csharp
var sb = pool.Rent();
try
{
    sb.Append("hello");
}
finally
{
    pool.Return(sb);
}
```

### Callback form

```csharp
string text = pool.Use(sb =>
{
    sb.Append("hello");
    return sb.ToString();
});
```

### Connection pool

```csharp
var connections = new ConnectionPool(
    factory: () => Task.FromResult(new ConnectionHandle()),
    maxConnections: 4);

await connections.UseMultipleConnectionsAsync(2, async conn =>
{
    // conn.Id identifies the handle; attach real resources to it in your own code.
    await Task.Delay(10);
});
```

## Thread safety

- `ObjectPool<T>.Rent`, `Return` and `Clear`, and `ConnectionPool.AcquireAsync`, `Release` and `Close`, lock their shared state and are safe to call from multiple threads (subject to limitation 5 below).
- Counter properties read unlocked, so their values are a snapshot.
- `ConnectionHandle.IsOpen` and the instances inside the pool are not synchronized. A single rented instance must be used by one caller at a time.

## Known limitations

These follow from the current implementation. The code is not changed by this document.

1. **Double return.** `Return` does not verify ownership. Returning the same instance twice pushes it onto the idle stack twice, so two later `Rent()` calls can hand the same instance to two callers.
2. **Returning after `Clear()` or `Close()`.** Those methods empty the in-use set without telling callers. A later `Return` or `Release` of such an instance puts a disposed object back on the idle stack, if there is room. `ConnectionPool.Release` does not check `IsOpen`.
3. **Reset failure drops the item.** If `reset` throws inside `Return`, the item has already been removed from the in-use set and is not pushed back. It is neither disposed nor reusable.
4. **Unbounded total for `ObjectPool<T>`.** `maxPoolSize` caps idle items only. Under sustained load the pool can create many more instances than that.
5. **Connection cap can be briefly exceeded.** `AcquireAsync` reads `TotalConnections` outside the lock before calling the factory. Two concurrent acquirers can both pass the check and both create a handle, so the total can momentarily exceed `maxConnections`.
6. **No acquisition timeout.** `ConnectionPool.AcquireAsync` waits indefinitely when all handles are in use and none are released.
7. **Struct lease copies.** `PooledObject<T>` is a struct. A copy has its own `_disposed` flag, so disposing both copies returns the instance twice.
8. **No tests in the repository.** No test file references `ObjectPool`, `ConnectionPool` or the extension methods, so the behavior above is not covered by the test suite.
