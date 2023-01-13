#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using CoubDownloader.Domain.Enums;

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Represents an active, non-destructive video editing session. All mutations return
/// new instances; the original source file is never modified.
/// </summary>
public sealed record VideoEditSession
{
    /// <summary>Unique session identifier (32-char hex)</summary>
    public required string SessionId { get; init; }

    /// <summary>Absolute path to the source video file that all operations target</summary>
    public required string SourceFilePath { get; init; }

    /// <summary>
    /// Ordered queue of edit operations to apply when the session is rendered.
    /// Operations are applied sequentially: output of step N is input to step N+1.
    /// </summary>
    public IReadOnlyList<EditOperation> Operations { get; init; } = [];

    /// <summary>Session creation timestamp (UTC)</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>True when the session contains at least one unrendered operation</summary>
    public bool IsDirty { get; init; }

    /// <summary>Creates a blank session for <paramref name="sourceFilePath"/> with a generated identifier</summary>
    public static VideoEditSession Create(string sourceFilePath) => new()
    {
        SessionId = Guid.NewGuid().ToString("N"),
        SourceFilePath = sourceFilePath,
        IsDirty = false
    };

    /// <summary>
    /// Returns a new session with <paramref name="operation"/> appended to the queue
    /// and <see cref="IsDirty"/> set to <c>true</c>.
    /// </summary>
    public VideoEditSession WithOperation(EditOperation operation) => this with
    {
        Operations = [..Operations, operation],
        IsDirty = true
    };
}

/// <summary>
/// Abstract base for all editable operations that can be queued inside a
/// <see cref="VideoEditSession"/>. Concrete subtypes are matched via pattern matching.
/// </summary>
public abstract record EditOperation
{
    /// <summary>Human-readable label shown in the edit history log</summary>
    public required string Label { get; init; }

    /// <summary>UTC timestamp when the operation was added to the session</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Trims the current video stream to a specific time window.
/// </summary>
public sealed record TrimOperation : EditOperation
{
    /// <summary>Start position of the desired output segment</summary>
    public required TimeSpan StartTime { get; init; }

    /// <summary>
    /// End position of the desired output segment.
    /// When <c>null</c> the remainder of the file is kept.
    /// </summary>
    public TimeSpan? EndTime { get; init; }

    /// <summary>Controls how the cut boundary is resolved against the encoded stream</summary>
    public TrimMode Mode { get; init; } = TrimMode.KeyframeAligned;
}

/// <summary>
/// Merges additional video clips into the primary stream in order.
/// The current working file is always prepended as the first input.
/// </summary>
public sealed record MergeOperation : EditOperation
{
    /// <summary>Absolute paths to the clips to concatenate, in playback order</summary>
    public required IReadOnlyList<string> ClipPaths { get; init; }

    /// <summary>Inter-clip transition strategy</summary>
    public MergeStrategy Strategy { get; init; } = MergeStrategy.HardCut;

    /// <summary>
    /// Overlap/crossfade duration when using <see cref="MergeStrategy.Crossfade"/>
    /// or <see cref="MergeStrategy.FadeThrough"/>.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:00", "00:00:05")]
    public TimeSpan TransitionDuration { get; init; } = TimeSpan.FromMilliseconds(500);
}

/// <summary>
/// Applies an ordered chain of visual or audio effects to the current video stream.
/// </summary>
public sealed record EffectsOperation : EditOperation
{
    /// <summary>Effect descriptors applied left-to-right in the FFmpeg filter graph</summary>
    public required IReadOnlyList<VideoEffect> Effects { get; init; }
}

/// <summary>
/// Descriptor for a single visual or audio effect in a filter chain.
/// </summary>
public sealed record VideoEffect
{
    /// <summary>Effect category identifying the FFmpeg filter to apply</summary>
    public required VideoEffectType Type { get; init; }

    /// <summary>
    /// Normalised intensity in the range [0.0, 1.0].
    /// 0.5 typically corresponds to the neutral/default value of the underlying filter.
    /// </summary>
    [Range(0.0, 1.0)]
    public double Intensity { get; init; } = 0.5;

    /// <summary>
    /// Optional key-value overrides forwarded directly to the underlying FFmpeg filter.
    /// Use these for parameters not covered by <see cref="Intensity"/>.
    /// </summary>
    public IReadOnlyDictionary<string, string> Parameters { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Immutable result returned after any video editing operation completes successfully.
/// </summary>
public sealed record VideoEditResult
{
    /// <summary>Absolute path to the produced output file</summary>
    public required string OutputFilePath { get; init; }

    /// <summary>Duration of the output video, if known at the time of result creation</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Output file size in bytes; 0 when the file has not yet been flushed to disk</summary>
    public long FileSizeBytes { get; init; }

    /// <summary>Number of discrete operations that were applied to produce this output</summary>
    public int OperationsApplied { get; init; }

    /// <summary>UTC timestamp of when the operation completed</summary>
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// <c>true</c> when the output was rendered at reduced quality as a preview;
    /// <c>false</c> for a full-quality final render.
    /// </summary>
    public bool IsPreview { get; init; }
}

/// <summary>
/// Options that govern preview clip or thumbnail generation.
/// </summary>
public sealed record PreviewOptions
{
    /// <summary>Render quality preset controlling speed vs. fidelity</summary>
    public PreviewQuality Quality { get; init; } = PreviewQuality.Draft;

    /// <summary>Position in the source stream at which the preview clip starts</summary>
    public TimeSpan StartOffset { get; init; } = TimeSpan.Zero;

    /// <summary>
    /// Maximum length of the preview clip.
    /// Set to <c>null</c> to capture a single representative frame as a JPEG thumbnail.
    /// </summary>
    public TimeSpan? ClipDuration { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Uniform scale factor applied to both spatial dimensions (0.1–1.0).
    /// Values below 1.0 reduce resolution for faster preview rendering.
    /// </summary>
    [Range(0.1, 1.0)]
    public double ScaleFactor { get; init; } = 0.5;
}
