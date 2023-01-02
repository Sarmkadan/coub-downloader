// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Exceptions;
using CoubDownloader.Domain.Models;
using CoubDownloader.Infrastructure.Integration;
using CoubDownloader.Infrastructure.Middleware;
using CoubDownloader.Infrastructure.VideoEditor;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Session-based, non-destructive video editor that orchestrates trim, merge, effect
/// application, and preview generation through <see cref="FFmpegWrapper"/>.
/// </summary>
/// <remarks>
/// Operations are composable: a <see cref="VideoEditSession"/> accumulates an ordered
/// queue of <see cref="EditOperation"/> instances which are applied sequentially by
/// <see cref="ApplySessionAsync"/>, piping each step's output into the next.
/// Temporary intermediate files are always cleaned up on completion or cancellation.
/// </remarks>
public sealed class VideoEditorService(FFmpegWrapper ffmpeg, ILoggingService logger)
    : IVideoEditorService
{
    private const string LogCategory = nameof(VideoEditorService);

    /// <inheritdoc/>
    public Task<VideoEditSession> CreateSessionAsync(
        string sourceFilePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFilePath);

        if (!File.Exists(sourceFilePath))
            throw new FileNotFoundException("Source video file not found.", sourceFilePath);

        var session = VideoEditSession.Create(sourceFilePath);
        logger.LogInfo(
            $"Edit session {session.SessionId} opened for '{Path.GetFileName(sourceFilePath)}'",
            LogCategory);

        return Task.FromResult(session);
    }

    /// <inheritdoc/>
    public async Task<VideoEditResult> TrimVideoAsync(
        string inputPath,
        string outputPath,
        TimeSpan startTime,
        TimeSpan? endTime = null,
        TrimMode mode = TrimMode.KeyframeAligned,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        if (!File.Exists(inputPath))
            throw new FileNotFoundException("Input video not found.", inputPath);

        if (startTime < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(startTime), "Start time must be non-negative.");

        if (endTime.HasValue && endTime.Value <= startTime)
            throw new ArgumentOutOfRangeException(nameof(endTime), "End time must be greater than start time.");

        EnsureOutputDirectory(outputPath);
        progress?.Report(5);

        var endLabel = endTime?.ToString("g") ?? "end";
        logger.LogInfo(
            $"Trimming '{Path.GetFileName(inputPath)}' [{startTime:g} → {endLabel}] mode={mode}",
            LogCategory);

        var args = BuildTrimArguments(inputPath, outputPath, startTime, endTime, mode);
        progress?.Report(10);

        var result = await ffmpeg.ExecuteAsync(args, TimeSpan.FromMinutes(15));
        progress?.Report(95);

        if (!result.Success)
        {
            logger.LogError($"Trim failed: {result.Error}", null, LogCategory);
            throw new VideoConversionException($"Video trim failed: {result.Error}", inputPath, outputPath);
        }

        progress?.Report(100);
        var duration = endTime.HasValue ? endTime.Value - startTime : TimeSpan.Zero;
        return BuildResult(outputPath, duration);
    }

    /// <inheritdoc/>
    public async Task<VideoEditResult> MergeVideosAsync(
        IReadOnlyList<string> inputPaths,
        string outputPath,
        MergeOperation operation,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputPaths);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(operation);

        if (inputPaths.Count < 2)
            throw new ArgumentException("At least two input clips are required for a merge.", nameof(inputPaths));

        foreach (var path in inputPaths)
            if (!File.Exists(path))
                throw new FileNotFoundException($"Clip not found: {path}");

        EnsureOutputDirectory(outputPath);
        progress?.Report(5);

        logger.LogInfo(
            $"Merging {inputPaths.Count} clips → '{Path.GetFileName(outputPath)}' ({operation.Strategy})",
            LogCategory);

        var result = operation.Strategy switch
        {
            MergeStrategy.Sequential or MergeStrategy.HardCut =>
                await ffmpeg.ConcatenateVideosAsync(inputPaths.ToList(), outputPath),

            MergeStrategy.Crossfade or MergeStrategy.FadeThrough =>
                await ExecuteCrossfadeMergeAsync(inputPaths, outputPath, operation, cancellationToken),

            _ => throw new NotSupportedException($"Merge strategy '{operation.Strategy}' is not supported.")
        };

        progress?.Report(95);

        if (!result.Success)
        {
            logger.LogError($"Merge failed: {result.Error}", null, LogCategory);
            throw new VideoConversionException($"Video merge failed: {result.Error}", inputPaths[0], outputPath);
        }

        progress?.Report(100);
        return BuildResult(outputPath, TimeSpan.Zero, operationsApplied: 1);
    }

    /// <inheritdoc/>
    public async Task<VideoEditResult> ApplyEffectsAsync(
        string inputPath,
        string outputPath,
        IReadOnlyList<VideoEffect> effects,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(effects);

        if (!File.Exists(inputPath))
            throw new FileNotFoundException("Input video not found.", inputPath);

        if (effects.Count == 0)
            throw new ArgumentException("At least one effect must be provided.", nameof(effects));

        EnsureOutputDirectory(outputPath);
        progress?.Report(5);

        var filterGraph = VideoEffectsProcessor.BuildFilterGraph(effects);
        logger.LogInfo(
            $"Applying {effects.Count} effect(s) to '{Path.GetFileName(inputPath)}'",
            LogCategory);
        logger.LogDebug($"Filter graph: {filterGraph}", LogCategory);

        var args = new List<string>
        {
            "-i",       inputPath,
            "-vf",      filterGraph,
            "-c:v",     "libx264",
            "-crf",     "18",
            "-preset",  "medium",
            "-c:a",     "copy",
            "-y",       outputPath
        };

        progress?.Report(15);
        var result = await ffmpeg.ExecuteAsync(args.ToArray(), TimeSpan.FromMinutes(30));
        progress?.Report(95);

        if (!result.Success)
        {
            logger.LogError($"Effects processing failed: {result.Error}", null, LogCategory);
            throw new VideoConversionException($"Effect application failed: {result.Error}", inputPath, outputPath);
        }

        progress?.Report(100);
        return BuildResult(outputPath, TimeSpan.Zero, operationsApplied: effects.Count);
    }

    /// <inheritdoc/>
    public async Task<VideoEditResult> GeneratePreviewAsync(
        VideoEditSession session,
        string outputPath,
        PreviewOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(options);

        if (!File.Exists(session.SourceFilePath))
            throw new FileNotFoundException("Session source file not found.", session.SourceFilePath);

        EnsureOutputDirectory(outputPath);

        var (crf, preset) = options.Quality switch
        {
            PreviewQuality.Draft        => ("30", "ultrafast"),
            PreviewQuality.Standard     => ("24", "fast"),
            PreviewQuality.HighFidelity => ("18", "medium"),
            _                           => ("28", "veryfast")
        };

        var scale = FormattableString.Invariant($"scale=iw*{options.ScaleFactor:F2}:ih*{options.ScaleFactor:F2}");

        var args = new List<string>
        {
            "-ss",      options.StartOffset.TotalSeconds.ToString("F3"),
            "-i",       session.SourceFilePath,
            "-vf",      scale,
            "-c:v",     "libx264",
            "-crf",     crf,
            "-preset",  preset,
            "-an"
        };

        if (options.ClipDuration.HasValue)
            args.AddRange(["-t", options.ClipDuration.Value.TotalSeconds.ToString("F3")]);

        args.AddRange(["-y", outputPath]);

        logger.LogDebug(
            $"Generating {options.Quality} preview for session {session.SessionId}",
            LogCategory);

        var result = await ffmpeg.ExecuteAsync(args.ToArray(), TimeSpan.FromMinutes(5));

        if (!result.Success)
        {
            logger.LogError($"Preview generation failed: {result.Error}", null, LogCategory);
            throw new VideoConversionException(
                $"Preview generation failed: {result.Error}",
                session.SourceFilePath,
                outputPath);
        }

        return BuildResult(outputPath, options.ClipDuration ?? TimeSpan.Zero, isPreview: true);
    }

    /// <inheritdoc/>
    public async Task<VideoEditResult> ApplySessionAsync(
        VideoEditSession session,
        string outputPath,
        ConversionSettings settings,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(settings);

        if (!File.Exists(session.SourceFilePath))
            throw new FileNotFoundException("Session source file not found.", session.SourceFilePath);

        if (session.Operations.Count == 0)
            throw new InvalidOperationException("The session has no operations to apply.");

        EnsureOutputDirectory(outputPath);

        logger.LogInfo(
            $"Rendering {session.Operations.Count} operation(s) from session {session.SessionId}",
            LogCategory);

        var tempFiles = new List<string>();
        try
        {
            var currentInput = session.SourceFilePath;
            var opCount = session.Operations.Count;

            for (var i = 0; i < opCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var op          = session.Operations[i];
                var isLastOp    = i == opCount - 1;
                var stepOutput  = isLastOp ? outputPath : GetTempPath(".mp4");

                if (!isLastOp)
                    tempFiles.Add(stepOutput);

                var capturedIndex = i;
                var stepProgress  = new Progress<int>(p =>
                    progress?.Report((int)((capturedIndex + p / 100.0) / opCount * 100)));

                await ApplyOperationAsync(op, currentInput, stepOutput, settings, stepProgress, cancellationToken);
                currentInput = stepOutput;
            }

            progress?.Report(100);
            return BuildResult(outputPath, TimeSpan.Zero, operationsApplied: opCount);
        }
        finally
        {
            foreach (var tmp in tempFiles)
                TryDeleteFile(tmp);
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetEditHistory(VideoEditSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return session.Operations
            .Select(op => $"[{op.Timestamp:HH:mm:ss}] {op.Label}")
            .ToList();
    }

    // -------------------------------------------------------------------------
    // Private orchestration helpers
    // -------------------------------------------------------------------------

    private async Task ApplyOperationAsync(
        EditOperation operation,
        string inputPath,
        string outputPath,
        ConversionSettings settings,
        IProgress<int> progress,
        CancellationToken cancellationToken)
    {
        switch (operation)
        {
            case TrimOperation trim:
                await TrimVideoAsync(
                    inputPath, outputPath,
                    trim.StartTime, trim.EndTime,
                    trim.Mode, progress, cancellationToken);
                break;

            case EffectsOperation effects:
                await ApplyEffectsAsync(
                    inputPath, outputPath,
                    effects.Effects, progress, cancellationToken);
                break;

            case MergeOperation merge:
                var allPaths = new List<string> { inputPath };
                allPaths.AddRange(merge.ClipPaths);
                await MergeVideosAsync(allPaths, outputPath, merge, progress, cancellationToken);
                break;

            default:
                throw new NotSupportedException(
                    $"Operation type '{operation.GetType().Name}' is not supported by {nameof(VideoEditorService)}.");
        }
    }

    private async Task<FFmpegResult> ExecuteCrossfadeMergeAsync(
        IReadOnlyList<string> inputPaths,
        string outputPath,
        MergeOperation operation,
        CancellationToken cancellationToken)
    {
        var transitionSec = operation.TransitionDuration.TotalSeconds.ToString("F3");
        var inputArgs     = inputPaths.SelectMany(p => new[] { "-i", p }).ToList();

        var filterParts = new List<string>();
        for (var i = 0; i < inputPaths.Count - 1; i++)
            filterParts.Add(
                $"[{i}][{i + 1}]xfade=transition=fade:duration={transitionSec}:offset=0[v{i}]");

        var mapLabel      = filterParts.Count > 0 ? $"[v{filterParts.Count - 1}]" : "[0]";
        var filterComplex = string.Join(";", filterParts);

        var args = new List<string>(inputArgs);

        if (filterParts.Count > 0)
            args.AddRange(["-filter_complex", filterComplex, "-map", mapLabel]);

        args.AddRange(["-c:v", "libx264", "-preset", "medium", "-crf", "20", "-y", outputPath]);

        return await ffmpeg.ExecuteAsync(args.ToArray(), TimeSpan.FromMinutes(30));
    }

    // -------------------------------------------------------------------------
    // Static helpers
    // -------------------------------------------------------------------------

    private static string[] BuildTrimArguments(
        string inputPath,
        string outputPath,
        TimeSpan startTime,
        TimeSpan? endTime,
        TrimMode mode)
    {
        var args = new List<string>();

        // Pre-input seek enables fast keyframe-aligned copy; post-input seek is precise but slower
        if (mode == TrimMode.KeyframeAligned)
            args.AddRange(["-ss", startTime.TotalSeconds.ToString("F3")]);

        args.AddRange(["-i", inputPath]);

        if (mode == TrimMode.Precise)
            args.AddRange(["-ss", startTime.TotalSeconds.ToString("F3")]);

        if (endTime.HasValue)
        {
            var duration = endTime.Value - startTime;
            args.AddRange(["-t", duration.TotalSeconds.ToString("F3")]);
        }

        // Stream copy for keyframe-aligned trimming avoids unnecessary re-encode
        var codec = mode == TrimMode.KeyframeAligned ? "copy" : "libx264";
        args.AddRange(["-c", codec, "-avoid_negative_ts", "make_zero", "-y", outputPath]);

        return args.ToArray();
    }

    private static VideoEditResult BuildResult(
        string outputPath,
        TimeSpan duration,
        bool isPreview = false,
        int operationsApplied = 1)
    {
        var fileInfo = File.Exists(outputPath) ? new FileInfo(outputPath) : null;
        return new VideoEditResult
        {
            OutputFilePath   = outputPath,
            Duration         = duration,
            FileSizeBytes    = fileInfo?.Length ?? 0,
            OperationsApplied = operationsApplied,
            IsPreview        = isPreview
        };
    }

    private static void EnsureOutputDirectory(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
    }

    private static string GetTempPath(string extension) =>
        Path.Combine(Path.GetTempPath(), $"coub_edit_{Guid.NewGuid():N}{extension}");

    private static void TryDeleteFile(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }
}
