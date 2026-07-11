# AudioTrackExtensions
The `AudioTrackExtensions` class provides a set of static methods for extending the functionality of audio tracks in the `coub-downloader` project. These methods allow for the calculation of various audio track properties, such as duration, fade effects, and file size, as well as checks for volume normalization and recent creation.

## API
* `public static double GetTotalDuration`: Calculates the total duration of an audio track. Returns the total duration in seconds. Throws no exceptions.
* `public static double GetSingleLoopDuration`: Calculates the duration of a single loop of an audio track. Returns the single loop duration in seconds. Throws no exceptions.
* `public static double GetFadeInRatio`: Calculates the fade-in ratio of an audio track. Returns the fade-in ratio as a double value between 0 and 1. Throws no exceptions.
* `public static double GetFadeOutRatio`: Calculates the fade-out ratio of an audio track. Returns the fade-out ratio as a double value between 0 and 1. Throws no exceptions.
* `public static bool HasFadeEffects`: Checks if an audio track has fade effects. Returns `true` if the track has fade effects, `false` otherwise. Throws no exceptions.
* `public static string GetAudioFormat`: Retrieves the audio format of an audio track. Returns the audio format as a string. Throws no exceptions.
* `public static double GetEstimatedFileSizeMb`: Estimates the file size of an audio track in megabytes. Returns the estimated file size in megabytes. Throws no exceptions.
* `public static bool NeedsVolumeNormalization`: Checks if an audio track needs volume normalization. Returns `true` if the track needs volume normalization, `false` otherwise. Throws no exceptions.
* `public static double GetAgeInDays`: Calculates the age of an audio track in days. Returns the age in days. Throws no exceptions.
* `public static bool IsRecentlyCreated`: Checks if an audio track was recently created. Returns `true` if the track was recently created, `false` otherwise. Throws no exceptions.

## Usage
The following examples demonstrate how to use the `AudioTrackExtensions` class:
```csharp
// Example 1: Calculate total duration and check for fade effects
AudioTrack track = new AudioTrack();
double totalDuration = AudioTrackExtensions.GetTotalDuration(track);
bool hasFadeEffects = AudioTrackExtensions.HasFadeEffects(track);
Console.WriteLine($"Total duration: {totalDuration} seconds, Has fade effects: {hasFadeEffects}");

// Example 2: Estimate file size and check for volume normalization
AudioTrack anotherTrack = new AudioTrack();
double estimatedFileSizeMb = AudioTrackExtensions.GetEstimatedFileSizeMb(anotherTrack);
bool needsVolumeNormalization = AudioTrackExtensions.NeedsVolumeNormalization(anotherTrack);
Console.WriteLine($"Estimated file size: {estimatedFileSizeMb} MB, Needs volume normalization: {needsVolumeNormalization}");
```

## Notes
The `AudioTrackExtensions` class provides a set of static methods that can be used to extend the functionality of audio tracks. These methods are designed to be thread-safe, as they do not modify any shared state. However, the underlying audio track objects may still be modified by other threads, which could affect the results of these methods. Additionally, the estimated file size calculation may not be exact, as it is based on various factors such as audio format and compression. The `IsRecentlyCreated` method uses a heuristic to determine if an audio track was recently created, and may not always produce accurate results.
