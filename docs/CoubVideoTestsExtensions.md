# CoubVideoTestsExtensions

Provides a set of static helper methods used in unit tests to generate `CoubVideo` instances with specific characteristics and to query common properties of those instances. The methods are intended to simplify test setup and assertions by encapsulating repetitive construction and evaluation logic.

## API

### CreateVerticalVideo
**Purpose:** Returns a `CoubVideo` object configured to represent a vertical‑oriented video suitable for test scenarios.  
**Parameters:** None.  
**Return value:** A fully populated `CoubVideo` instance whose `Resolution` indicates a portrait aspect ratio (e.g., 720×1280).  
**Exceptions:** None under normal operation; if internal data initialization fails, an `InvalidOperationException` may be thrown.

### CreateHdLandscapeVideo
**Purpose:** Returns a `CoubVideo` object configured to represent a high‑definition landscape video.  
**Parameters:** None.  
**Return value:** A `CoubVideo` instance with a landscape resolution such as 1280×720 or 1920×1080.  
**Exceptions:** None; throws `InvalidOperationException` only if the underlying test data cannot be constructed.

### Create4kVideo
**Purpose:** Returns a `CoubVideo` object configured to represent a 4K ultra‑high‑definition video.  
**Parameters:** None.  
**Return value:** A `CoubVideo` instance whose resolution meets or exceeds 3840×2160.  
**Exceptions:** None; may throw `InvalidOperationException` on failure to build the test video.

### CreateShortDurationVideo
**Purpose:** Returns a `CoubVideo` object configured to represent a video with a short playback duration (typically under 5 seconds).  
**Parameters:** None.  
**Return value:** A `CoubVideo` instance with a `Duration` property set to a short timespan.  
**Exceptions:** None; throws `InvalidOperationException` if the short‑duration preset cannot be instantiated.

### IsPopular
**Purpose:** Determines whether a given `CoubVideo` meets the criteria for being considered “popular” (e.g., view count above a threshold).  
**Parameters:** `video` – the `CoubVideo` instance to evaluate.  
**Return value:** `true` if the video is popular; otherwise `false`.  
**Exceptions:** Throws `ArgumentNullException` if `video` is `null`.

### GetResolutionCategory
**Purpose:** Returns a string label describing the resolution category of the supplied video (e.g., “SD”, “HD”, “Full HD”, “4K”).  
**Parameters:** `video` – the `CoubVideo` instance to assess.  
**Return value:** A category string; returns an empty string if the resolution does not match any known category.  
**Exceptions:** Throws `ArgumentNullException` when `video` is `null`.

### GetTotalDurationWithAudio
**Purpose:** Calculates the combined duration of the video and its audio tracks, expressed in seconds as a `double`.  
**Parameters:** `video` – the `CoubVideo` instance whose total duration is required.  
**Return value:** The sum of the video track duration and any audio track durations.  
**Exceptions:** Throws `ArgumentNullException` if `video` is `null`; may throw `InvalidOperationException` if the video lacks a duration property.

### IsProcessable
**Purpose:** Indicates whether the video can be processed by the downloader (based on factors such as duration, resolution, and format support).  
**Parameters:** `video` – the `CoubVideo` instance to test.  
**Return value:** `true` if the video satisfies all processing requirements; otherwise `false`.  
**Exceptions:** Throws `ArgumentNullException` when `video` is `null`.

## Usage

```csharp
using CoubDownloader.Tests; // namespace containing CoubVideoTestsExtensions
using NUnit.Framework;

[TestFixture]
public class CoubVideoProcessorTests
{
    [Test]
    public void ProcessVerticalVideo_ReturnsExpectedResult()
    {
        // Arrange: create a test video with vertical orientation
        var video = CoubVideoTestsExtensions.CreateVerticalVideo();

        // Act: evaluate popularity and processability
        bool popular = CoubVideoTestsExtensions.IsPopular(video);
        bool processable = CoubVideoTestsExtensions.IsProcessable(video);

        // Assert: verify test-specific expectations
        Assert.IsFalse(popular, "Vertical test video should not be marked as popular.");
        Assert.IsTrue(processable, "Vertical test video should be processable.");
    }
}
```

```csharp
using CoubDownloader.Tests;
using System;

public class Demo
{
    public static void Main()
    {
        // Arrange: generate a 4K test video
        var video4k = CoubVideoTestsExtensions.Create4kVideo();

        // Act: obtain resolution category and total duration with audio
        string category = CoubVideoTestsExtensions.GetResolutionCategory(video4k);
        double totalDuration = CoubVideoTestsExtensions.GetTotalDurationWithAudio(video4k);

        // Output results for demonstration
        Console.WriteLine($"Resolution category: {category}");
        Console.WriteLine($"Total duration (sec): {totalDuration:F2}");
    }
}
```

## Notes

- All extension methods that accept a `CoubVideo` parameter will throw an `ArgumentNullException` if the argument is `null`. Callers should ensure the instance is non‑null before invoking these helpers.
- The factory methods (`Create*Video`) generate fresh instances on each call and do not rely on mutable static state; therefore they are thread‑safe and can be invoked concurrently from multiple threads without additional synchronization.
- The property‑query methods (`IsPopular`, `GetResolutionCategory`, `GetTotalDurationWithAudio`, `IsProcessable`) are pure functions of the supplied video object; they do not modify the video or any external state, making them safe to use in parallel test execution.
- If the internal test data used by the factory methods becomes corrupted or unavailable, the methods may throw an `InvalidOperationException`. This scenario is unlikely under normal test runs but should be considered when extending the helper with new presets.
