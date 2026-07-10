# AudioTrack

Represents an audio track extracted or processed from a video, including metadata and playback configuration for looping and fading.

## API

### `public string Id`
Unique identifier for the audio track. Must not be null or empty.

### `public string VideoId`
Identifier of the video from which this audio track was derived. Must not be null or empty.

### `public double Duration`
Duration of the audio track in seconds. Must be non-negative.

### `public int SampleRate`
Audio sample rate in Hz. Must be a positive integer.

### `public int Channels`
Number of audio channels (e.g., 1 for mono, 2 for stereo). Must be a positive integer.

### `public int Bitrate`
Audio bitrate in kilobits per second (kbps). Must be a positive integer.

### `public string Codec`
Audio codec used (e.g., "aac", "mp3"). Must not be null or empty.

### `public string? FilePath`
Optional filesystem path where the audio file is stored. May be null if not persisted.

### `public AudioLoopStrategy LoopStrategy`
Strategy used for looping the audio track. Defaults to `AudioLoopStrategy.None`.

### `public int LoopCount`
Number of times the audio track should loop. Ignored if `LoopStrategy` is `None`. Must be non-negative.

### `public int FadeInMs`
Duration of fade-in effect in milliseconds. Must be non-negative.

### `public int FadeOutMs`
Duration of fade-out effect in milliseconds. Must be non-negative.

### `public double VolumeLevel`
Playback volume level, where 1.0 represents full volume. Must be between 0.0 and 1.0.

### `public double SyncDuration`
Duration in seconds used for synchronization purposes. Must be non-negative.

### `public DateTime CreatedAt`
Timestamp indicating when the audio track was created or processed.

### `public bool IsValid`
Indicates whether the audio track is valid and ready for use. Derived from internal state and file existence if applicable.

### `public double CalculateLoopedDuration()`
Calculates the total duration of the audio track after applying looping and fade effects.

- **Returns**: Total duration in seconds.
- **Throws**: `InvalidOperationException` if `Duration` is negative or `LoopCount` is negative.

### `public string GetAudioSpec()`
Generates a string representation of the audio track's technical specifications.

- **Returns**: A string in the format `"[Codec] [SampleRate]Hz [Channels]ch [Bitrate]kbps"`.
- **Throws**: `InvalidOperationException` if `Codec`, `SampleRate`, `Channels`, or `Bitrate` are invalid.

## Usage

### Example 1: Creating and validating an audio track
