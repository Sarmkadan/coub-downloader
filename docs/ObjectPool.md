# ObjectPool

A lightweight, connection-oriented object pool for managing reusable resources such as network connections or pooled objects. It provides mechanisms to rent resources for temporary use, return them to the pool when no longer needed, and manage lifecycle events including disposal and cleanup.

## API

### `public ObjectPool`

Initializes a new instance of the `ObjectPool` class. The pool manages a collection of reusable objects and connection handles, providing controlled access and lifecycle management.

### `public T Rent()`

Acquires an object of type `T` from the pool. If an object is available, it is returned immediately; otherwise, a new instance may be created depending on pool configuration.

- **Return value**: An object of type `T` ready for use.
- **Exceptions**: Throws `InvalidOperationException` if the pool is closed or disposed.

### `public void Return(T obj)`

Returns an object of type `T` to the pool for reuse. The object must have been previously acquired via `Rent()`.

- **Parameters**:
  - `obj`: The object to return to the pool.
- **Exceptions**: Throws `ArgumentNullException` if `obj` is `null`. Throws `InvalidOperationException` if the pool is closed or disposed.

### `public void Clear()`

Removes all objects from the pool, releasing any held resources. This operation invalidates all previously rented objects.

- **Exceptions**: Throws `InvalidOperationException` if the pool is closed or disposed.

### `public PooledObject<T>`

A wrapper type representing a pooled object along with its lifetime management. Used to ensure proper disposal and return of pooled resources.

### `public void Dispose()`

Releases all resources held by the pool, including all pooled objects and connection handles. After disposal, the pool cannot be used.

### `public ConnectionPool`

A nested or related pool type specifically managing connection handles. Provides connection acquisition and release semantics.

### `public async Task<ConnectionHandle> AcquireAsync()`

Asynchronously acquires a connection handle from the pool. Useful in scenarios where connection establishment is asynchronous.

- **Return value**: A `Task<ConnectionHandle>` representing the asynchronous acquisition.
- **Exceptions**: Throws `InvalidOperationException` if the pool is closed or disposed.

### `public void Release(ConnectionHandle handle)`

Returns a connection handle to the pool for reuse. The handle must have been acquired via `AcquireAsync()` or similar.

- **Parameters**:
  - `handle`: The connection handle to release.
- **Exceptions**: Throws `ArgumentNullException` if `handle` is `null`. Throws `InvalidOperationException` if the pool is closed or disposed.

### `public void Close()`

Closes the pool, preventing new rentals or acquisitions. Existing rented objects remain valid until returned.

- **Exceptions**: Throws `InvalidOperationException` if the pool is already closed.

### `public string Id`

Gets a unique identifier for the pool instance. Useful for logging and diagnostics.

### `public DateTime CreatedAt`

Gets the timestamp when the pool was created.

### `public bool IsOpen`

Gets a value indicating whether the pool is currently open and accepting new operations.

### `public void Dispose()`

Releases all resources associated with the pool, including all pooled objects and connection handles. After disposal, the pool cannot be used.

## Usage

### Example 1: Basic Object Pool Usage
