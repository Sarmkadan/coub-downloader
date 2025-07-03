// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Enums;

namespace CoubDownloader.Presentation.CLI;

/// <summary>Parses command-line arguments into strongly-typed option objects</summary>
public class CommandParser
{
    /// <summary>Parse download command arguments</summary>
    public DownloadCommandOptions ParseDownloadOptions(string[] args)
    {
        var options = new DownloadCommandOptions();

        for (int i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--")) continue;

            var key = arg[2..].ToLowerInvariant();
            var value = i + 1 < args.Length ? args[i + 1] : null;

            switch (key)
            {
                case "url":
                    options.Url = value;
                    i++;
                    break;
                case "output":
                    options.OutputPath = value;
                    i++;
                    break;
                case "format":
                    options.Format = Enum.TryParse<VideoFormat>(value, true, out var fmt) ? fmt : VideoFormat.MP4;
                    i++;
                    break;
                case "quality":
                    options.Quality = Enum.TryParse<VideoQuality>(value, true, out var q) ? q : VideoQuality.High;
                    i++;
                    break;
            }
        }

        return options;
    }

    /// <summary>Parse convert command arguments</summary>
    public ConvertCommandOptions ParseConvertOptions(string[] args)
    {
        var options = new ConvertCommandOptions();

        for (int i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--")) continue;

            var key = arg[2..].ToLowerInvariant();
            var value = i + 1 < args.Length ? args[i + 1] : null;

            switch (key)
            {
                case "input":
                    options.InputFile = value;
                    i++;
                    break;
                case "output":
                    options.OutputFile = value;
                    i++;
                    break;
                case "format":
                    options.Format = Enum.TryParse<VideoFormat>(value, true, out var fmt) ? fmt : VideoFormat.MP4;
                    i++;
                    break;
                case "quality":
                    options.Quality = Enum.TryParse<VideoQuality>(value, true, out var q) ? q : VideoQuality.High;
                    i++;
                    break;
                case "width":
                    if (int.TryParse(value, out var w)) options.Width = w;
                    i++;
                    break;
                case "height":
                    if (int.TryParse(value, out var h)) options.Height = h;
                    i++;
                    break;
                case "fps":
                    if (int.TryParse(value, out var fps)) options.FrameRate = fps;
                    i++;
                    break;
            }
        }

        return options;
    }

    /// <summary>Parse batch command arguments</summary>
    public BatchCommandOptions ParseBatchOptions(string[] args)
    {
        var options = new BatchCommandOptions();

        for (int i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--")) continue;

            var key = arg[2..].ToLowerInvariant();
            var value = i + 1 < args.Length ? args[i + 1] : null;

            switch (key)
            {
                case "files":
                    options.Files = value?.Split(',').Select(f => f.Trim()).ToList();
                    i++;
                    break;
                case "output":
                    options.OutputDirectory = value;
                    i++;
                    break;
                case "name":
                    options.Name = value;
                    i++;
                    break;
                case "format":
                    options.Format = Enum.TryParse<VideoFormat>(value, true, out var fmt) ? fmt : VideoFormat.MP4;
                    i++;
                    break;
                case "quality":
                    options.Quality = Enum.TryParse<VideoQuality>(value, true, out var q) ? q : VideoQuality.High;
                    i++;
                    break;
            }
        }

        return options;
    }

    /// <summary>Parse info command arguments</summary>
    public InfoCommandOptions ParseInfoOptions(string[] args)
    {
        var options = new InfoCommandOptions();

        for (int i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--")) continue;

            var key = arg[2..].ToLowerInvariant();
            var value = i + 1 < args.Length ? args[i + 1] : null;

            if (key == "url")
            {
                options.Url = value;
                i++;
            }
        }

        return options;
    }
}

/// <summary>Download command options</summary>
public class DownloadCommandOptions
{
    public string? Url { get; set; }
    public string? OutputPath { get; set; }
    public VideoFormat Format { get; set; } = VideoFormat.MP4;
    public VideoQuality Quality { get; set; } = VideoQuality.High;
}

/// <summary>Convert command options</summary>
public class ConvertCommandOptions
{
    public string? InputFile { get; set; }
    public string? OutputFile { get; set; }
    public VideoFormat Format { get; set; } = VideoFormat.MP4;
    public VideoQuality Quality { get; set; } = VideoQuality.High;
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? FrameRate { get; set; }
}

/// <summary>Batch command options</summary>
public class BatchCommandOptions
{
    public List<string> Files { get; set; } = [];
    public string? OutputDirectory { get; set; }
    public string? Name { get; set; }
    public VideoFormat Format { get; set; } = VideoFormat.MP4;
    public VideoQuality Quality { get; set; } = VideoQuality.High;
}

/// <summary>Info command options</summary>
public class InfoCommandOptions
{
    public string? Url { get; set; }
}
