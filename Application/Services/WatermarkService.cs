#nullable enable
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CoubDownloader.Infrastructure.Integration;

namespace CoubDownloader.Application.Services;

/// <summary>Anchor position for overlays.</summary>
public enum WatermarkPosition
{
    TopLeft, TopCenter, TopRight,
    MiddleLeft, Center, MiddleRight,
    BottomLeft, BottomCenter, BottomRight
}

/// <summary>Burns image or text watermarks into videos via FFmpeg.</summary>
public class WatermarkService
{
    private readonly IFFmpegWrapper _ffmpeg;

    public WatermarkService(IFFmpegWrapper ffmpeg)
    {
        if (ffmpeg is null)
            throw new ArgumentNullException(nameof(ffmpeg));

        _ffmpeg = ffmpeg;
    }

    /// <summary>
    /// Overlays <paramref name="watermarkImagePath"/> onto the video at the given position with margin (px) and opacity (0.0-1.0),
    /// re-encoding video and copying audio. Returns <paramref name="outputPath"/>.
    /// </summary>
    public async Task<string> ApplyImageWatermarkAsync(
        string videoPath,
        string watermarkImagePath,
        string outputPath,
        WatermarkPosition position = WatermarkPosition.BottomRight,
        int marginPx = 16,
        double opacity = 0.8,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        var (xExpr, yExpr) = GetOverlayExpressions(position, marginPx);
        var opacityStr = opacity.ToString(CultureInfo.InvariantCulture);
        var filter = $"[1]format=rgba,colorchannelmixer=aa={opacityStr}[wm];[0][wm]overlay={xExpr}:{yExpr}";

        var args = new[]
        {
            "-i", videoPath,
            "-i", watermarkImagePath,
            "-filter_complex", filter,
            "-c:a", "copy",
            "-y", outputPath
        };

        await _ffmpeg.ExecuteAsync(args);
        return outputPath;
    }

    /// <summary>
    /// Draws text (e.g. "coub.com/creator") with <paramref name="fontSize"/> and hex <paramref name="fontColor"/> like "white" or "#FFFFFF",
    /// with a semi‑transparent box behind it. Escapes FFmpeg drawtext special characters in <paramref name="text"/>.
    /// </summary>
    public async Task<string> ApplyTextWatermarkAsync(
        string videoPath,
        string text,
        string outputPath,
        WatermarkPosition position = WatermarkPosition.BottomLeft,
        int fontSize = 24,
        string fontColor = "white",
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        var (xExpr, yExpr) = GetOverlayExpressions(position, marginPx: 16);
        var escapedText = EscapeDrawText(text);
        var filter = $"drawtext=text='{escapedText}':fontcolor={fontColor}:fontsize={fontSize}:box=1:boxcolor=black@0.5:boxborderw=5:x={xExpr}:y={yExpr}";

        var args = new[]
        {
            "-i", videoPath,
            "-filter_complex", filter,
            "-c:a", "copy",
            "-y", outputPath
        };

        await _ffmpeg.ExecuteAsync(args);
        return outputPath;
    }

    /// <summary>
    /// Maps a <see cref="WatermarkPosition"/> to FFmpeg overlay x/y expressions given a margin.
    /// Public for testability.
    /// </summary>
    public (string X, string Y) GetOverlayExpressions(WatermarkPosition position, int marginPx)
    {
        string x = position switch
        {
            WatermarkPosition.TopLeft or
            WatermarkPosition.MiddleLeft or
            WatermarkPosition.BottomLeft => marginPx.ToString(CultureInfo.InvariantCulture),

            WatermarkPosition.TopCenter or
            WatermarkPosition.Center or
            WatermarkPosition.BottomCenter => "(main_w-overlay_w)/2",

            WatermarkPosition.TopRight or
            WatermarkPosition.MiddleRight or
            WatermarkPosition.BottomRight => $"main_w-overlay_w-{marginPx}",

            _ => throw new ArgumentOutOfRangeException(nameof(position), position, null)
        };

        string y = position switch
        {
            WatermarkPosition.TopLeft or
            WatermarkPosition.TopCenter or
            WatermarkPosition.TopRight => marginPx.ToString(CultureInfo.InvariantCulture),

            WatermarkPosition.MiddleLeft or
            WatermarkPosition.Center or
            WatermarkPosition.MiddleRight => "(main_h-overlay_h)/2",

            WatermarkPosition.BottomLeft or
            WatermarkPosition.BottomCenter or
            WatermarkPosition.BottomRight => $"main_h-overlay_h-{marginPx}",

            _ => throw new ArgumentOutOfRangeException(nameof(position), position, null)
        };

        return (x, y);
    }

    private static string EscapeDrawText(string text)
    {
        // Escape backslashes first, then single quotes and colons which have special meaning in FFmpeg drawtext.
        return text
            .Replace("\\", "\\\\")
            .Replace("'", "\\'")
            .Replace(":", "\\:");
    }
}
