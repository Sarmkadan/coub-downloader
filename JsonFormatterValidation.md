# JsonFormatterValidation

Provides extension methods for validating parameters passed to `JsonFormatter` formatting methods before serialization. This class contains validation helpers for `JsonFormatter`, `CoubVideo`, `IEnumerable<CoubVideo>`, `BatchJob`, and `ConversionSettings` types, ensuring that data is valid before being formatted to JSON.

## API

### `IReadOnlyList<string> Validate(this JsonFormatter value)`

Validates a `JsonFormatter` instance.

- **Parameters:** `value` – The formatter instance to validate
- **Returns:** An empty list if the formatter is valid; otherwise returns an empty list since `JsonFormatter` has no state to validate
- **Throws:** `ArgumentNullException` if `value` is null

### `IReadOnlyList<string> Validate(this CoubVideo video)`

Validates a `CoubVideo` instance for formatting.

- **Parameters:** `video` – The video to validate
- **Returns:** A list of human-readable validation problems; empty if valid
- **Throws:** `ArgumentNullException` if `video` is null
- **Validation rules:**
  - `Id` must not be null or whitespace
  - `Title` must not be null or whitespace and must not exceed 500 characters
  - `Url` must not be null or whitespace
  - `Duration` must be greater than 0 seconds
  - `Width` must be between 100 and 7680 pixels
  - `Height` must be between 100 and 7680 pixels
  - `ViewCount` must not be negative
  - `UploadedDate` must not be the default `DateTime` value if set

### `IReadOnlyList<string> Validate(this IEnumerable<CoubVideo> videos)`

Validates a collection of `CoubVideo` instances for formatting.

- **Parameters:** `videos` – The videos to validate
- **Returns:** A list of human-readable validation problems; empty if valid
- **Throws:** `ArgumentNullException` if `videos` is null
- **Validation rules:**
  - The collection must not be empty
  - Each video in the collection must be non-null
  - Each video's individual validation rules apply (see `Validate(CoubVideo)`)

### `IReadOnlyList<string> Validate(this BatchJob batch)`

Validates a `BatchJob` instance for formatting.

- **Parameters:** `batch` – The batch job to validate
- **Returns:** A list of human-readable validation problems; empty if valid
- **Throws:** `ArgumentNullException` if `batch` is null
- **Validation rules:**
  - `Id` must not be null or whitespace
  - `Name` must not be null or whitespace and must not exceed 255 characters
  - `OutputDirectory` must not be null or whitespace
  - `TotalTasks` must not be negative
  - `MaxParallelTasks` must be between 1 and 10
  - `CreatedAt` must not be the default `DateTime` value
  - `UpdatedAt` must not be the default `DateTime` value
  - `Tasks` collection must not be null

### `IReadOnlyList<string> Validate(this ConversionSettings settings)`

Validates a `ConversionSettings` instance for formatting.

- **Parameters:** `settings` – The conversion settings to validate
- **Returns:** A list of human-readable validation problems; empty if valid
- **Throws:** `ArgumentNullException` if `settings` is null
- **Validation rules:**
  - `Id` must not be null or whitespace
  - `VideoBitrate` must be between 500 and 20000 kbps
  - `AudioBitrate` must be between 32 and 320 kbps
  - `VideoCodec` must not be null or whitespace and must not exceed 50 characters
  - `AudioCodec` must not be null or whitespace and must not exceed 50 characters
  - `FrameRate` must be between 15 and 120 fps
  - `Width` must be between 100 and 7680 pixels
  - `Height` must be between 100 and 7680 pixels
  - `ThreadCount` must be between 1 and 32
  - `FadeInMs` must be between 0 and 5000 milliseconds
  - `FadeOutMs` must be between 0 and 5000 milliseconds
  - `CreatedAt` must not be the default `DateTime` value


### `bool IsValid(this JsonFormatter value)`

Checks if a `JsonFormatter` instance is valid.

- **Parameters:** `value` – The formatter instance to check
- **Returns:** `true` if valid; otherwise `false`
- **Remarks:** Returns `true` when the validation list is empty


### `bool IsValid(this CoubVideo video)`

Checks if a `CoubVideo` instance is valid for formatting.

- **Parameters:** `video` – The video to check
- **Returns:** `true` if valid; otherwise `false`

- **Remarks:** Returns `true` when the validation list is empty


### `bool IsValid(this IEnumerable<CoubVideo> videos)`

Checks if an enumerable of `CoubVideo` instances is valid for formatting.

- **Parameters:** `videos` – The videos to check
- **Returns:** `true` if valid; otherwise `false`
- **Remarks:** Returns `true` when the validation list is empty


### `bool IsValid(this BatchJob batch)`

Checks if a `BatchJob` instance is valid for formatting.

- **Parameters:** `batch` – The batch job to check
- **Returns:** `true` if valid; otherwise `false`
- **Remarks:** Returns `true` when the validation list is empty


### `bool IsValid(this ConversionSettings settings)`

Checks if a `ConversionSettings` instance is valid for formatting.

- **Parameters:** `settings` – The conversion settings to check
- **Returns:** `true` if valid; otherwise `false`
- **Remarks:** Returns `true` when the validation list is empty


### `void EnsureValid(this JsonFormatter value)`

Ensures that a `JsonFormatter` instance is valid, throwing an exception if not.

- **Parameters:** `value` – The formatter instance to validate
- **Throws:**
  - `ArgumentNullException` if `value` is null
  - `ArgumentException` if validation fails, containing the list of problems
- **Remarks:** Throws with a formatted message listing all validation problems if any exist


### `void EnsureValid(this CoubVideo video)`

Ensures that a `CoubVideo` instance is valid for formatting, throwing an exception if not.

- **Parameters:** `video` – The video to validate
- **Throws:**
  - `ArgumentNullException` if `video` is null
  - `ArgumentException` if validation fails, containing the list of problems
- **Remarks:** Throws with a formatted message listing all validation problems if any exist

### `void EnsureValid(this IEnumerable<CoubVideo> videos)`

Ensures that an enumerable of `CoubVideo` instances is valid for formatting, throwing an exception if not.

- **Parameters:** `videos` – The videos to validate
- **Throws:**
  - `ArgumentNullException` if `videos` is null
  - `ArgumentException` if validation fails, containing the list of problems
- **Remarks:** Throws with a formatted message listing all validation problems if any exist

### `void EnsureValid(this BatchJob batch)`

Ensures that a `BatchJob` instance is valid for formatting, throwing an exception if not.

- **Parameters:** `batch` – The batch job to validate
- **Throws:**
  - `ArgumentNullException` if `batch` is null
  - `ArgumentException` if validation fails, containing the list of problems
- **Remarks:** Throws with a formatted message listing all validation problems if any exist

### `void EnsureValid(this ConversionSettings settings)`

Ensures that a `ConversionSettings` instance is valid for formatting, throwing an exception if not.

- **Parameters:** `settings` – The conversion settings to validate
- **Throws:**
  - `ArgumentNullException` if `settings` is null
  - `ArgumentException` if validation fails, containing the list of problems
- **Remarks:** Throws with a formatted message listing all validation problems if any exist


## Usage

### Example 1: Validating a video before formatting

```csharp
var video = new CoubVideo
{
    Id = "abc123",
    Title = "My Awesome Video",
    Url = "https://coub.com/view/abc123",
    Duration = 30,
    Width = 1920,
    Height = 1080,
    ViewCount = 1500,
    UploadedDate = DateTime.Now
};

var problems = video.Validate();
if (problems.Count > 0)
{
    foreach (var problem in problems)
    {
        Console.WriteLine(problem);
    }
    return;
}

// Safe to format
var json = JsonFormatter.FormatVideo(video);
```

### Example 2: Using EnsureValid for immediate validation

```csharp
public void ProcessVideo(CoubVideo video)
{
    try
    {
        video.EnsureValid();
        
        // Process the valid video
        var json = JsonFormatter.FormatVideo(video);
        Console.WriteLine("Video processed successfully");
    }
    catch (ArgumentException ex) when (ex.ParamName == nameof(video))
    {
        Console.WriteLine("Invalid video:");
        Console.WriteLine(ex.Message);
    }
}
```

## Notes

- All validation methods are implemented as extension methods for the respective types
- Validation is performed on the actual data values, not on any internal state
- The `Validate()` methods return an `IReadOnlyList<string>` for flexibility in handling results
- The `IsValid()` methods provide a convenient boolean check based on the validation results
- The `EnsureValid()` methods throw descriptive exceptions when validation fails, making them suitable for guard clauses
- All methods are thread-safe as they only read input parameters and create new collections for results
- Validation rules are comprehensive and cover all public properties of the validated types
- Empty or null collections are considered invalid by the `Validate(IEnumerable<CoubVideo>)` method
- Default `DateTime` values are rejected for timestamp properties that should have meaningful values