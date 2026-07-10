# CoubVideoTests

This class contains unit tests for the `CoubVideo` model in the `coub-downloader` project. Each test method validates a specific behavior or property of `CoubVideo`, such as input validation, format detection, duration categorization, view count formatting, quality checks, audio duration calculation, progress tracking, and state management. The tests are designed to be run with a standard unit testing framework (e.g., xUnit, NUnit) and assume that the `CoubVideo` class exposes the corresponding public members.

## API

### `IsValid_AllRequiredFieldsPresent_ReturnsTrue`
Verifies that `IsValid` returns `true` when all required fields (e.g., `Id`, `Duration`) are present and valid.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly; test failures are reported by the test framework.

### `IsValid_MissingId_ReturnsFalse`
Verifies that `IsValid` returns `false` when the `Id` field is missing or empty.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `IsValid_ZeroDuration_ReturnsFalse`
Verifies that `IsValid` returns `false` when the `Duration` is zero.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `GetAspectRatio_LandscapeVideo_ReturnsRatioGreaterThanOne`
Verifies that `GetAspectRatio` returns a value greater than 1.0 for a landscape video (width > height).  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `IsVerticalFormat_PortraitDimensions_ReturnsTrue`
Verifies that `IsVerticalFormat` returns `true` when the video dimensions are portrait (height > width).  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `IsVerticalFormat_LandscapeDimensions_ReturnsFalse`
Verifies that `IsVerticalFormat` returns `false` when the video dimensions are landscape (width > height).  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `GetDurationCategory_VariousDurations_ReturnsCorrectCategory`
Verifies that `GetDurationCategory` returns the correct category (e.g., "short", "medium", "long") for a range of duration values.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `GetFormattedViewCount_VariousCounts_FormatsCorrectly`
Verifies that `GetFormattedViewCount` returns a human-readable string (e.g., "1.2K", "3.5M") for various numeric view counts.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `IsHdQuality_VideoAbove720p_ReturnsTrue`
Verifies that `IsHdQuality` returns `true` when the video resolution is above 720p (e.g., 1080p, 1440p).  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `Is4kQuality_StandardHD_ReturnsFalse`
Verifies that `Is4kQuality` returns `false` for standard HD resolutions (e.g., 720p, 1080p).  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `Is4kQuality_UltraHDDimensions_ReturnsTrue`
Verifies that `Is4kQuality` returns `true` for ultra HD dimensions (e.g., 3840x2160).  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `CalculateRequiredAudioDuration_AudioShorterThanVideo_ReturnsNextMultiple`
Verifies that `CalculateRequiredAudioDuration` returns the next multiple of the audio duration that is at least as long as the video duration when the audio is shorter than the video.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `CalculateRequiredAudioDuration_NoAudioTrack_ReturnsZero`
Verifies that `CalculateRequiredAudioDuration` returns 0 when there is no audio track.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `GetProgressPercent_NoTasks_ReturnsZero`
Verifies that `GetProgressPercent` returns 0 when there are no tasks.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `GetProgressPercent_HalfDone_ReturnsFifty`
Verifies that `GetProgressPercent` returns 50 when exactly half of the tasks are completed.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `GetProgressPercent_AllCompleted_Returns100`
Verifies that `GetProgressPercent` returns 100 when all tasks are completed.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `IsCompleted_AllTasksDone_ReturnsTrue`
Verifies that `IsCompleted` returns `true` when all tasks have been completed.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `IsCompleted_TasksStillPending_ReturnsFalse`
Verifies that `IsCompleted` returns `false` when there are still pending tasks.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `CanStart_PendingStateWithTasks_ReturnsTrue`
Verifies that `CanStart` returns `true` when the video is in a pending state and has tasks to process.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

### `CanStart_AlreadyRunning_ReturnsFalse`
Verifies that `CanStart` returns `false` when the video is already in a running state.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** No exceptions directly.

## Usage

The following examples demonstrate how to use the `CoubVideoTests` class in a typical test project.

### Example 1: Running a specific test with dotnet test

```csharp
// File: CoubVideoTests.cs (part of the test project)
using Xunit;

public class CoubVideoTests
{
    [Fact]
    public void IsValid_AllRequiredFieldsPresent_ReturnsTrue()
    {
        // Arrange
        var video = new CoubVideo
        {
            Id = "abc123",
            Duration = 30.0,
            // ... other required fields
        };

        // Act
        bool result = video.IsValid();

        // Assert
        Assert.True(result);
    }

    // ... other test methods
}
```

To run only this test from the command line:
```
dotnet test --filter "FullyQualifiedName~IsValid_AllRequiredFieldsPresent_ReturnsTrue"
```

### Example 2: Writing a new test that follows the same pattern

```csharp
[Fact]
public void GetProgressPercent_HalfDone_ReturnsFifty()
{
    // Arrange
    var video = new CoubVideo();
    video.AddTasks(4); // assume 4 tasks
    video.CompleteTasks(2); // complete 2

    // Act
    double percent = video.GetProgressPercent();

    // Assert
    Assert.Equal(50.0, percent, 1);
}
```

## Notes

- **Edge cases:** The tests cover boundary conditions such as missing identifiers, zero durations, empty task lists, and extreme view counts. When using `CoubVideo` in production, ensure that `Id` is never null or empty and that `Duration` is positive. The `CalculateRequiredAudioDuration` method assumes that audio duration is a positive value when present; a zero or negative audio duration may produce unexpected results.
- **Thread safety:** The `CoubVideo` class is not guaranteed to be thread-safe. These tests are designed to run sequentially and should not be executed concurrently on the same instance. If `CoubVideo` is used in a multi-threaded context, external synchronization (e.g., locks) is recommended, especially for state-modifying methods like `CanStart`, `IsCompleted`, and progress tracking.
- **Test isolation:** Each test method creates its own instance of `CoubVideo` (or relies on a fresh state). No shared state exists between tests, so they can be run in any order.
