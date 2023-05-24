#nullable enable

using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Enums;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Extension methods for <see cref="VideoEditorService"/> providing convenient utility operations
/// for common video editing workflows.
/// </summary>
public static class VideoEditorServiceExtensions
{
    /// <summary>
    /// Creates a trimmed copy of a video with a specified duration starting from the beginning.
    /// </summary>
    /// <param name="service">The video editor service instance</param>
    /// <param name="inputPath">Path to the source video file</param>
    /// <param name="outputPath">Path where the trimmed video will be saved</param>
    /// <param name="duration">The duration of the trimmed clip</param>
    /// <param name="mode">The trimming mode to use</param>
    /// <param name="progress">Optional progress reporter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation with the edit result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="inputPath"/> is <see langword="null"/>, empty, or whitespace</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is <see langword="null"/>, empty, or whitespace</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="duration"/> is less than or equal to <see cref="TimeSpan.Zero"/></exception>
    public static async Task<VideoEditResult> TrimFirstSecondsAsync(
        this VideoEditorService service,
        string inputPath,
        string outputPath,
        TimeSpan duration,
        TrimMode mode = TrimMode.KeyframeAligned,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(duration, TimeSpan.Zero);

        return await service.TrimVideoAsync(inputPath, outputPath, TimeSpan.Zero, duration, mode, progress, cancellationToken);
    }

    /// <summary>
    /// Creates a session, applies a single trim operation, and generates the final output in one call.
    /// </summary>
    /// <param name="service">The video editor service instance</param>
    /// <param name="sourceFilePath">Path to the source video file</param>
    /// <param name="outputPath">Path where the final video will be saved</param>
    /// <param name="startTime">Start time for the trim operation</param>
    /// <param name="endTime">Optional end time for the trim operation</param>
    /// <param name="mode">The trimming mode to use</param>
    /// <param name="settings">Conversion settings to apply</param>
    /// <param name="progress">Optional progress reporter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation with the edit result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="sourceFilePath"/> is <see langword="null"/>, empty, or whitespace</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is <see langword="null"/>, empty, or whitespace</exception>
    public static async Task<VideoEditResult> TrimAndRenderAsync(
        this VideoEditorService service,
        string sourceFilePath,
        string outputPath,
        TimeSpan startTime,
        TimeSpan? endTime = null,
        TrimMode mode = TrimMode.KeyframeAligned,
        ConversionSettings? settings = null,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFilePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        var session = await service.CreateSessionAsync(sourceFilePath, cancellationToken);
        var trimOp = new TrimOperation
        {
            Label = $"Trim [{startTime:g} - {(endTime?.ToString("g") ?? "end")}]",
            StartTime = startTime,
            EndTime = endTime,
            Mode = mode
        };
        var sessionWithOp = session.WithOperation(trimOp);

        settings ??= new ConversionSettings();
        return await service.ApplySessionAsync(sessionWithOp, outputPath, settings, progress, cancellationToken);
    }

    /// <summary>
    /// Creates a preview video with standardized settings for quick review.
    /// </summary>
    /// <param name="service">The video editor service instance</param>
    /// <param name="session">The video edit session to generate preview from</param>
    /// <param name="outputPath">Path where the preview will be saved</param>
    /// <param name="quality">The preview quality level</param>
    /// <param name="startOffset">Optional start offset for the preview</param>
    /// <param name="clipDuration">Optional duration of the clip to include in preview</param>
    /// <param name="scaleFactor">Scaling factor for the preview (1.0 = original size)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation with the edit result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="session"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is <see langword="null"/>, empty, or whitespace</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="scaleFactor"/> is less than or equal to 0.0</exception>
    public static async Task<VideoEditResult> GenerateStandardPreviewAsync(
        this VideoEditorService service,
        VideoEditSession session,
        string outputPath,
        PreviewQuality quality = PreviewQuality.Standard,
        TimeSpan? startOffset = null,
        TimeSpan? clipDuration = null,
        double scaleFactor = 0.5,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(scaleFactor, 0.0);

        var options = new PreviewOptions
        {
            Quality = quality,
            StartOffset = startOffset ?? TimeSpan.Zero,
            ClipDuration = clipDuration ?? TimeSpan.FromSeconds(10),
            ScaleFactor = Math.Clamp(scaleFactor, 0.1, 1.0)
        };

        return await service.GeneratePreviewAsync(session, outputPath, options, cancellationToken);
    }

    /// <summary>
    /// Applies multiple effects to a video in a single operation using a fluent-like syntax.
    /// </summary>
    /// <param name="service">The video editor service instance</param>
    /// <param name="inputPath">Path to the source video file</param>
    /// <param name="outputPath">Path where the processed video will be saved</param>
    /// <param name="effects">List of effects to apply</param>
    /// <param name="progress">Optional progress reporter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation with the edit result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="effects"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="inputPath"/> is <see langword="null"/>, empty, or whitespace</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is <see langword="null"/>, empty, or whitespace</exception>
    public static async Task<VideoEditResult> ApplyEffectsAsync(
        this VideoEditorService service,
        string inputPath,
        string outputPath,
        IReadOnlyList<VideoEffect> effects,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(effects);

        return await service.ApplyEffectsAsync(inputPath, outputPath, effects, progress, cancellationToken);
    }

    /// <summary>
    /// Gets the edit history as a formatted string list with timestamps and operation labels.
    /// </summary>
    /// <param name="session">The video edit session</param>
    /// <param name="includeTimestamps">Whether to include timestamps in the output</param>
    /// <param name="maxLength">Optional maximum number of history entries to return</param>
    /// <returns>Formatted list of edit operations</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="session"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> GetEditHistory(
        this VideoEditorService service,
        VideoEditSession session,
        bool includeTimestamps = true,
        int? maxLength = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(session);

        var history = service.GetEditHistory(session);

        return includeTimestamps
            ? maxLength.HasValue
                ? history.Take(maxLength.Value).ToList()
                : history
            : maxLength.HasValue
                ? history.Select(h => h[(h.IndexOf(']') + 2)..]).Take(maxLength.Value).ToList()
                : history.Select(h => h[(h.IndexOf(']') + 2)..]).ToList();
    }

    /// <summary>
    /// Creates a new session from an existing one with additional operations appended.
    /// </summary>
    /// <param name="service">The video editor service instance</param>
    /// <param name="baseSession">The base session to extend</param>
    /// <param name="operations">Operations to add to the session</param>
    /// <returns>A new session with the additional operations</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="baseSession"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="operations"/> is <see langword="null"/></exception>
    public static VideoEditSession WithOperations(
        this VideoEditorService service,
        VideoEditSession baseSession,
        params EditOperation[] operations)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(baseSession);
        ArgumentNullException.ThrowIfNull(operations);

        VideoEditSession result = baseSession;
        foreach (var operation in operations)
        {
            result = result.WithOperation(operation);
        }
        return result;
    }
}