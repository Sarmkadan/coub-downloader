# VideoSection

Represents a segment of a video with timing, metadata, and inclusion status used during the download and assembly of Coub videos.

## API

### `public string Id`
Unique identifier for this video section. Used to reference the segment within a collection.

### `public string VideoId`
Identifier of the parent video from which this section is derived.

### `public int Index`
Zero-based position of this section within the parent video’s sequence of sections.

### `public double StartTime`
Start time of the segment in seconds, relative to the parent video.

### `public double EndTime`
End time of the segment in seconds, relative to the parent video.

### `public string? Description`
Optional human-readable description of the section’s content or purpose.

### `public bool IsIncluded`
Indicates whether this section should be included when assembling the final output.

### `public string? TransitionEffect`
Optional name of a transition effect to apply between this section and the next.

### `public int TransitionDurationMs`
Duration of the transition effect in milliseconds.

### `public double GetDuration()`
Calculates and returns the duration of the section in seconds as `EndTime - StartTime`.

Returns:
- The computed duration in seconds.

### `public bool IsValid()`
Determines whether the section’s timing values and identifiers are logically consistent.

Returns:
- `true` if `StartTime` and `EndTime` are non-negative, `StartTime` ≤ `EndTime`, and `VideoId` is non-empty; otherwise `false`.
Throws:
- No exceptions.
