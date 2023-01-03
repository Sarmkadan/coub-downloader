#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Contract for session-based, non-destructive video editing: trim, merge,
/// effect application, and real-time preview generation.
/// </summary>
/// <remarks>
/// All write operations accept an optional <see cref="IProgress{T}"/> reporter
/// that delivers integer values in the range [0, 100] as the operation proceeds.
/// </remarks>
public interface IVideoEditorService
{
    /// <summary>
    /// Opens a new editing session for the given source file.
    /// The session starts with an empty operation queue; use
    /// <see cref="VideoEditSession.WithOperation"/> to enqueue edits before
    /// calling <see cref="ApplySessionAsync"/>.
    /// </summary>
    /// <param name="sourceFilePath">Absolute path to the source video.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>A new <see cref="VideoEditSession"/> with no queued operations.</returns>
    /// <exception cref="FileNotFoundException">
    /// Thrown when <paramref name="sourceFilePath"/> does not exist on disk.
    /// </exception>
    Task<VideoEditSession> CreateSessionAsync(
        string sourceFilePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Trims the video at <paramref name="inputPath"/> to the given time window
    /// and writes the result to <paramref name="outputPath"/>.
    /// </summary>
    /// <param name="inputPath">Absolute path to the video to trim.</param>
    /// <param name="outputPath">Desired path for the trimmed output file.</param>
    /// <param name="startTime">Start position of the desired segment (must be ≥ 0).</param>
    /// <param name="endTime">
    /// End position of the desired segment; when <c>null</c> the remainder of the file is kept.
    /// Must be strictly greater than <paramref name="startTime"/> when supplied.
    /// </param>
    /// <param name="mode">Trim boundary alignment strategy (default: <see cref="TrimMode.KeyframeAligned"/>).</param>
    /// <param name="progress">Optional progress reporter delivering 0–100.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>A <see cref="VideoEditResult"/> describing the produced file.</returns>
    Task<VideoEditResult> TrimVideoAsync(
        string inputPath,
        string outputPath,
        TimeSpan startTime,
        TimeSpan? endTime = null,
        TrimMode mode = TrimMode.KeyframeAligned,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Concatenates two or more video clips into a single output file.
    /// </summary>
    /// <param name="inputPaths">
    /// Ordered list of absolute clip paths to join (minimum 2 entries).
    /// </param>
    /// <param name="outputPath">Desired output path.</param>
    /// <param name="operation">
    /// Merge configuration carrying the strategy and transition parameters.
    /// </param>
    /// <param name="progress">Optional progress reporter delivering 0–100.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>A <see cref="VideoEditResult"/> describing the produced file.</returns>
    Task<VideoEditResult> MergeVideosAsync(
        IReadOnlyList<string> inputPaths,
        string outputPath,
        MergeOperation operation,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a chain of visual or audio effects to the video at
    /// <paramref name="inputPath"/> and writes the result to <paramref name="outputPath"/>.
    /// Effects are composed in the order they appear in <paramref name="effects"/>.
    /// </summary>
    /// <param name="inputPath">Absolute path to the source video.</param>
    /// <param name="outputPath">Desired output path.</param>
    /// <param name="effects">
    /// Ordered effect descriptors; must contain at least one entry.
    /// </param>
    /// <param name="progress">Optional progress reporter delivering 0–100.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>A <see cref="VideoEditResult"/> describing the produced file.</returns>
    Task<VideoEditResult> ApplyEffectsAsync(
        string inputPath,
        string outputPath,
        IReadOnlyList<VideoEffect> effects,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders a low-latency preview clip or thumbnail reflecting all operations
    /// currently queued in <paramref name="session"/>.
    /// </summary>
    /// <param name="session">The active edit session to preview.</param>
    /// <param name="outputPath">Destination path for the preview file.</param>
    /// <param name="options">Quality, region, and duration settings for the preview.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>
    /// A <see cref="VideoEditResult"/> with <see cref="VideoEditResult.IsPreview"/> set to
    /// <c>true</c>.
    /// </returns>
    Task<VideoEditResult> GeneratePreviewAsync(
        VideoEditSession session,
        string outputPath,
        PreviewOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies all queued operations in <paramref name="session"/> in order and writes
    /// the final output to <paramref name="outputPath"/>.
    /// </summary>
    /// <param name="session">
    /// Session whose operation queue should be rendered.
    /// Must contain at least one operation.
    /// </param>
    /// <param name="outputPath">Destination path for the finished video.</param>
    /// <param name="settings">
    /// Conversion settings governing codec, bitrate, and output dimensions for the
    /// final render pass.
    /// </param>
    /// <param name="progress">Optional progress reporter delivering 0–100 across all operations.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>A <see cref="VideoEditResult"/> describing the finished file.</returns>
    Task<VideoEditResult> ApplySessionAsync(
        VideoEditSession session,
        string outputPath,
        ConversionSettings settings,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the operation queue of <paramref name="session"/> as a human-readable
    /// history log suitable for display in a CLI or UI.
    /// </summary>
    /// <param name="session">Session to inspect.</param>
    /// <returns>
    /// An ordered list of strings in the format
    /// <c>"[HH:mm:ss] {OperationLabel}"</c>.
    /// </returns>
    IReadOnlyList<string> GetEditHistory(VideoEditSession session);
}
