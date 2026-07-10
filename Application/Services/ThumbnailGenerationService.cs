#nullable enable

using CoubDownloader.Infrastructure.Integration;

namespace CoubDownloader.Application.Services;

/// <summary>Generates thumbnails and contact sheets from video files using FFmpeg.</summary>
public class ThumbnailGenerationService
{
    private readonly IFFmpegWrapper _ffmpeg;

    public ThumbnailGenerationService(IFFmpegWrapper ffmpeg)
    {
        ArgumentNullException.ThrowIfNull(ffmpeg);
        _ffmpeg = ffmpeg;
    }

    /// <summary>Extracts one frame at the given timestamp to outputPath (.jpg or .png inferred from extension). Throws FileNotFoundException if input missing; ArgumentOutOfRangeException if timestamp is negative.</summary>
    public async Task<string> GenerateThumbnailAsync(
        string videoPath,
        string outputPath,
        TimeSpan timestamp,
        int? maxWidth = null,
        CancellationToken cancellationToken = default)
    {
        if (timestamp < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timestamp), "Timestamp cannot be negative");
        }

        if (!File.Exists(videoPath))
        {
            throw new FileNotFoundException("Video file not found", videoPath);
        }

        var args = new List<string>
        {
            "-ss", timestamp.TotalSeconds.ToString("F3"),
            "-i", videoPath,
            "-frames:v", "1"
        };

        if (maxWidth.HasValue)
        {
            args.AddRange(new[] { "-vf", $"scale={maxWidth.Value}:-2" });
        }

        args.Add("-y");
        args.Add(outputPath);

        var result = await _ffmpeg.ExecuteAsync(args.ToArray());

        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed to generate thumbnail: {result.Error}");
        }

        return outputPath;
    }

    /// <summary>Extracts one frame at percent (0-100) of the video duration, using GetMediaInfoAsync for duration.</summary>
    public async Task<string> GenerateThumbnailAtPercentAsync(
        string videoPath,
        string outputPath,
        double percent,
        int? maxWidth = null,
        CancellationToken cancellationToken = default)
    {
        if (percent < 0 || percent > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percent), "Percent must be between 0 and 100");
        }

        if (!File.Exists(videoPath))
        {
            throw new FileNotFoundException("Video file not found", videoPath);
        }

        var mediaInfo = await _ffmpeg.GetMediaInfoAsync(videoPath);
        if (mediaInfo?.DurationInSeconds == null)
        {
            throw new InvalidOperationException("Could not determine video duration");
        }

        var duration = TimeSpan.FromSeconds(mediaInfo.DurationInSeconds.Value);
        var timestamp = TimeSpan.FromSeconds(duration.TotalSeconds * (percent / 100.0));

        return await GenerateThumbnailAsync(videoPath, outputPath, timestamp, maxWidth);
    }

    /// <summary>Generates a columns x rows contact sheet with frames evenly sampled across the whole video using select+tile filters. tileWidth is the per-tile pixel width.</summary>
    public async Task<string> GenerateContactSheetAsync(
        string videoPath,
        string outputPath,
        int columns = 4,
        int rows = 4,
        int tileWidth = 320,
        CancellationToken cancellationToken = default)
    {
        if (columns <= 0 || rows <= 0)
        {
            throw new ArgumentOutOfRangeException($"columns={columns}, rows={rows}", "Columns and rows must be positive");
        }

        if (tileWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tileWidth), "Tile width must be positive");
        }

        if (!File.Exists(videoPath))
        {
            throw new FileNotFoundException("Video file not found", videoPath);
        }

        var mediaInfo = await _ffmpeg.GetMediaInfoAsync(videoPath);
        if (mediaInfo?.DurationInSeconds == null)
        {
            throw new InvalidOperationException("Could not determine video duration");
        }

        var duration = TimeSpan.FromSeconds(mediaInfo.DurationInSeconds.Value);
        var totalFrames = columns * rows;
        var frameInterval = duration.TotalSeconds / Math.Max(totalFrames - 1, 1);

        var filterComplexParts = new List<string>();
        var inputStreams = new List<string>();

        for (var i = 0; i < totalFrames; i++)
        {
            var frameTime = TimeSpan.FromSeconds(i * frameInterval);
            var streamIndex = i;
            inputStreams.Add($"[{streamIndex}:v]");
            filterComplexParts.Add($"[{streamIndex}:v]select='eq(n\\,{i})',scale={tileWidth}:-2[v{streamIndex}];");
        }

        var tileInputs = string.Join("", inputStreams);
        var tileOutputs = string.Join("", Enumerable.Range(0, totalFrames).Select(i => $"[v{i}]"));
        var tileFilter = $"tile=layout={columns}x{rows}[outv]";

        filterComplexParts.Add($"{tileInputs}{tileOutputs}{tileFilter}");

        var args = new List<string>
        {
            "-i", videoPath,
            "-filter_complex", string.Join("", filterComplexParts),
            "-map", "[outv]"
        };

        args.Add("-y");
        args.Add(outputPath);

        var result = await _ffmpeg.ExecuteAsync(args.ToArray());

        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed to generate contact sheet: {result.Error}");
        }

        return outputPath;
    }
}
