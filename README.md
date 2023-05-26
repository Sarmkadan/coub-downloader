// ... (rest of the README.md content remains the same)

## ObjectPoolExtensions

The `ObjectPoolExtensions` class provides a set of utility methods for working with object pools in .NET. These extensions simplify the process of obtaining and returning objects to the pool.

### Usage Example

```csharp
using Infrastructure.Utilities;

// Create an object pool
var pool = new ObjectPool<MyClass>(new MyClassFactory());

// Get a pooled object
using var pooledObject = pool.GetPooledObject();
var myObject = pooledObject.Object;

// Use the object
myObject.DoSomething();

// The object is automatically returned to the pool when it goes out of scope
```

Alternatively, you can use the `Use` method to ensure the object is returned to the pool:

```csharp
await pool.UseAsync<MyClass>(async (obj) =>
{
    // Use the object
    obj.DoSomething();
    // The object will be returned to the pool when this block completes
});
```

### Public Members

The following public members are available:

*   `GetPooledObject<T>()`: Gets a pooled object of the specified type.
*   `ReturnRange<T>(IEnumerable<T>)`: Returns a range of objects to the pool.
*   `Return<T>(T)`: Returns an object to the pool.
*   `Use<T>(Action<T>)`: Uses a pooled object and automatically returns it to the pool.
*   `Use<T, TResult>(Func<T, TResult>)`: Uses a pooled object and automatically returns it to the pool, returning a result.
*   `UseAsync<T>(Func<T, Task>)`: Asynchronously uses a pooled object and automatically returns it to the pool.
*   `UseConnectionAsync<T>(Func<T, Task>)`: Asynchronously uses a pooled object as a connection and automatically returns it to the pool.
*   `UseMultipleConnectionsAsync<T>(Func<T, Task>[])`: Asynchronously uses multiple pooled objects as connections and automatically returns them to the pool.

// ... (rest of the README.md content remains the same)

## CoubVideoTestsExtensions

`CoubVideoTestsExtensions` provides a collection of helper methods used in unit‑tests to create `CoubVideo` instances with specific characteristics and to query common video‑related properties. These methods make it easy to set up test data for different resolutions, durations, and processing requirements.

### Usage Example

```csharp
using CoubDownloader.Tests;

// Create various test videos
var verticalVideo   = CoubVideoTestsExtensions.CreateVerticalVideo();
var hdLandscapeVideo = CoubVideoTestsExtensions.CreateHdLandscapeVideo();
var video4k         = CoubVideoTestsExtensions.Create4kVideo();
var shortVideo      = CoubVideoTestsExtensions.CreateShortDurationVideo();

// Query properties of the created videos
bool isPopular          = CoubVideoTestsExtensions.IsPopular(verticalVideo);
string resolutionCategory = CoubVideoTestsExtensions.GetResolutionCategory(hdLandscapeVideo);
double totalDuration    = CoubVideoTestsExtensions.GetTotalDurationWithAudio(video4k);
bool canBeProcessed     = CoubVideoTestsExtensions.IsProcessable(shortVideo);
```

These helpers simplify test setup and make test code more expressive and maintainable.

// ... (rest of the README.md content remains the same)
