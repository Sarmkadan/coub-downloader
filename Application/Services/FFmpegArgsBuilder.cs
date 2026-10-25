#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text;
using CoubDownloader.Domain.Constants;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Builds FFmpeg and ffprobe argument lists. Every added value is exactly one argument:
/// use <see cref="ToArray"/> for argv-style APIs and <see cref="ToCommandLine"/> for APIs that take a single string.
/// </summary>
public sealed class FFmpegArgsBuilder
{
    private readonly List<Argument> _arguments = new();

    /// <summary>Adds arguments. In a command line they are quoted only when they are empty or contain whitespace or quotes.</summary>
    public FFmpegArgsBuilder Add(params string[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        foreach (var value in values)
        {
            ArgumentNullException.ThrowIfNull(value);
            _arguments.Add(new Argument(value, AlwaysQuote: false));
        }

        return this;
    }

    /// <summary>Adds an argument that is always double-quoted in a command line (file paths, filter graphs).</summary>
    public FFmpegArgsBuilder AddQuoted(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _arguments.Add(new Argument(value, AlwaysQuote: true));
        return this;
    }

    /// <summary>Returns the arguments as they should be passed to a process argument list (no quoting).</summary>
    public string[] ToArray() => _arguments.Select(argument => argument.Value).ToArray();

    /// <summary>Returns the arguments joined into a single command line with Windows (MSVCRT) quoting rules.</summary>
    public string ToCommandLine()
    {
        return string.Join(' ', _arguments.Select(argument =>
            argument.AlwaysQuote || NeedsQuoting(argument.Value)
                ? QuoteArgument(argument.Value)
                : argument.Value));
    }

    /// <summary>
    /// Wraps a value in double quotes and escapes it so that a command-line parser following the
    /// MSVCRT rules (as used by <c>ProcessStartInfo.Arguments</c>) reads back the original value.
    /// Backslashes are only special before a quote or at the end of the value.
    /// </summary>
    public static string QuoteArgument(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var builder = new StringBuilder(value.Length + 2);
        builder.Append('"');

        var pendingBackslashes = 0;
        foreach (var c in value)
        {
            if (c == '\\')
            {
                pendingBackslashes++;
                continue;
            }

            if (c == '"')
            {
                // Double the preceding backslashes and escape the quote itself.
                builder.Append('\\', pendingBackslashes * 2 + 1).Append('"');
            }
            else
            {
                builder.Append('\\', pendingBackslashes).Append(c);
            }

            pendingBackslashes = 0;
        }

        // Backslashes before the closing quote must be doubled, otherwise they would escape it.
        builder.Append('\\', pendingBackslashes * 2).Append('"');
        return builder.ToString();
    }

    /// <summary>Returns true when a value cannot be written to a command line without quotes.</summary>
    public static bool NeedsQuoting(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Length == 0 || value.Any(c => char.IsWhiteSpace(c) || c == '"');
    }

    /// <summary>
    /// Formats one line of an FFmpeg concat demuxer list. A single quote inside the path is written as
    /// <c>'\''</c>, which is how the concat demuxer escapes it inside a single-quoted value.
    /// </summary>
    public static string ConcatListEntry(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return $"file '{path.Replace("'", @"'\''", StringComparison.Ordinal)}'";
    }

    /// <summary>Builds the ffprobe arguments that extract stream and format metadata as JSON (without the executable).</summary>
    public static string MetadataProbeCommand(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        return new FFmpegArgsBuilder()
            .Add("-v", "error")
            .Add("-select_streams", "v:0")
            .Add("-show_entries", "stream=width,height,duration,codec_name,r_frame_rate,bit_rate")
            .Add("-show_entries", "format=size,duration,bit_rate,format_name")
            .Add("-of", "json")
            .AddQuoted(filePath)
            .ToCommandLine();
    }

    /// <summary>Builds the ffprobe arguments that read media duration and size as JSON (without the executable).</summary>
    public static string MediaInfoProbeCommand(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        return new FFmpegArgsBuilder()
            .Add("-v", "error")
            .Add("-show_entries", "format=duration,size,bit_rate")
            .Add("-of", "json")
            .AddQuoted(filePath)
            .ToCommandLine();
    }

    /// <summary>Builds the concat-demuxer arguments that copy streams from a list file into an output file.</summary>
    public static string[] ConcatArguments(string concatListFile, string outputFile)
    {
        return new FFmpegArgsBuilder()
            .Add("-f", "concat")
            .Add("-safe", "0")
            .Add("-i", concatListFile)
            .Add("-c", "copy")
            .Add("-y", outputFile)
            .ToArray();
    }

    /// <summary>Builds the conversion command: scale (optionally with fades), codecs, bitrates and preset.</summary>
    public static string ConversionCommand(string inputPath, string outputPath, ConversionSettings settings)
    {
        ArgumentNullException.ThrowIfNull(inputPath);
        ArgumentNullException.ThrowIfNull(outputPath);
        ArgumentNullException.ThrowIfNull(settings);

        var scaleFilter = settings.PreserveAspectRatio
            ? $"scale={settings.Width}:{settings.Height}:force_original_aspect_ratio=decrease"
            : $"scale={settings.Width}:{settings.Height}";

        var fades = string.Empty;
        if (settings.ApplyFades)
        {
            var fadeIn = settings.FadeInMs / 1000.0;
            var fadeOut = settings.FadeOutMs / 1000.0;
            fades = FormattableString.Invariant($",fade=t=in:st=0:d={fadeIn},fade=t=out:st=10:d={fadeOut}");
        }

        return new FFmpegArgsBuilder()
            .Add("-i").AddQuoted(inputPath)
            .Add("-vf").AddQuoted($"{scaleFilter}{fades}")
            .Add("-r", settings.FrameRate.ToString(CultureInfo.InvariantCulture))
            .Add("-c:v", settings.VideoCodec)
            .Add("-b:v", $"{settings.VideoBitrate}k")
            .Add("-c:a", settings.AudioCodec)
            .Add("-b:a", $"{settings.AudioBitrate}k")
            .Add("-preset", VideoProcessingConstants.FFmpegPreset)
            .AddQuoted(outputPath)
            .Add("-y")
            .ToCommandLine();
    }

    /// <summary>Builds the command that copies the video stream and replaces its audio with another file.</summary>
    public static string AudioTrackCommand(string videoPath, string audioPath, string outputPath, ConversionSettings settings)
    {
        ArgumentNullException.ThrowIfNull(videoPath);
        ArgumentNullException.ThrowIfNull(audioPath);
        ArgumentNullException.ThrowIfNull(outputPath);
        ArgumentNullException.ThrowIfNull(settings);

        return new FFmpegArgsBuilder()
            .Add("-i").AddQuoted(videoPath)
            .Add("-i").AddQuoted(audioPath)
            .Add("-c:v", "copy")
            .Add("-c:a", settings.AudioCodec)
            .Add("-b:a", $"{settings.AudioBitrate}k")
            .Add("-shortest")
            .AddQuoted(outputPath)
            .Add("-y")
            .ToCommandLine();
    }

    /// <summary>Builds the command that rescales a video to the given size with H.264/AAC.</summary>
    public static string RescaleCommand(string inputPath, string outputPath, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(inputPath);
        ArgumentNullException.ThrowIfNull(outputPath);

        return new FFmpegArgsBuilder()
            .Add("-i").AddQuoted(inputPath)
            .Add("-vf", $"scale={width}:{height}")
            .Add("-c:v", "h264")
            .Add("-crf", "23")
            .Add("-c:a", "aac")
            .Add("-b:a", "128k")
            .AddQuoted(outputPath)
            .Add("-y")
            .ToCommandLine();
    }

    /// <summary>
    /// Builds the command for a 1080x1920 vertical video: the source is blurred to fill the frame and
    /// the scaled source is overlaid centred on top.
    /// </summary>
    public static string ShortsCommand(string inputPath, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(inputPath);
        ArgumentNullException.ThrowIfNull(outputPath);

        const string shortsFilter =
            "[0:v]split[a][b];" +
            "[a]scale=1080:1920:force_original_aspect_ratio=increase,crop=1080:1920,boxblur=20:5[blurred];" +
            "[b]scale=1080:1920:force_original_aspect_ratio=decrease[fg];" +
            "[blurred][fg]overlay=(W-w)/2:(H-h)/2";

        return new FFmpegArgsBuilder()
            .Add("-i").AddQuoted(inputPath)
            .Add("-vf").AddQuoted(shortsFilter)
            .Add("-c:v", "h264")
            .Add("-crf", VideoProcessingConstants.FFmpegCRF.ToString(CultureInfo.InvariantCulture))
            .Add("-preset", VideoProcessingConstants.FFmpegPreset)
            .Add("-c:a", "aac")
            .Add("-b:a", $"{VideoProcessingConstants.DefaultAudioBitrate}k")
            .AddQuoted(outputPath)
            .Add("-y")
            .ToCommandLine();
    }

    private readonly record struct Argument(string Value, bool AlwaysQuote);
}
