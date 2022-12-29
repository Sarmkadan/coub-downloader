// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Runtime.InteropServices;
using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Presentation.CLI;

/// <summary>Main command-line interface handler for the application</summary>
public class CommandLineInterface
{
    private readonly ICoubDownloadService _downloadService;
    private readonly IVideoConversionService _conversionService;
    private readonly IBatchProcessingService _batchService;
    private readonly CommandParser _commandParser;

    public CommandLineInterface(
        ICoubDownloadService downloadService,
        IVideoConversionService conversionService,
        IBatchProcessingService batchService)
    {
        _downloadService = downloadService;
        _conversionService = conversionService;
        _batchService = batchService;
        _commandParser = new CommandParser();
    }

    /// <summary>Execute CLI with provided arguments</summary>
    public async Task<int> ExecuteAsync(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                DisplayHelp();
                return 0;
            }

            var command = args[0].ToLowerInvariant();

            return command switch
            {
                "download" => await HandleDownloadCommand(args),
                "convert" => await HandleConvertCommand(args),
                "batch" => await HandleBatchCommand(args),
                "info" => await HandleInfoCommand(args),
                "version" => HandleVersionCommand(),
                "help" => HandleHelpCommand(),
                _ => HandleUnknownCommand(command)
            };
        }
        catch (Exception ex)
        {
            DisplayError($"Fatal error: {ex.Message}");
            return 1;
        }
    }

    private async Task<int> HandleDownloadCommand(string[] args)
    {
        var options = _commandParser.ParseDownloadOptions(args);

        if (string.IsNullOrEmpty(options.Url))
        {
            DisplayError("URL is required. Use: coub-downloader download --url <url>");
            return 1;
        }

        Console.WriteLine($"📥 Downloading from: {options.Url}");
        Console.WriteLine($"   Output: {options.OutputPath ?? "default"}");

        var video = await _downloadService.DownloadVideoAsync(options.Url);

        if (video != null)
        {
            Console.WriteLine($"✓ Download completed: {video.Title}");
            Console.WriteLine($"  Duration: {video.Duration}s");
            Console.WriteLine($"  Resolution: {video.Width}x{video.Height}");
            return 0;
        }

        DisplayError($"Download failed");
        return 1;
    }

    private async Task<int> HandleConvertCommand(string[] args)
    {
        var options = _commandParser.ParseConvertOptions(args);

        if (string.IsNullOrEmpty(options.InputFile))
        {
            DisplayError("Input file is required. Use: coub-downloader convert --input <file>");
            return 1;
        }

        Console.WriteLine($"🎬 Converting: {options.InputFile}");
        Console.WriteLine($"   Format: {options.Format}");
        Console.WriteLine($"   Quality: {options.Quality}");

        var settings = new ConversionSettings
        {
            Format = options.Format,
            Quality = options.Quality,
            Width = options.Width ?? 1920,
            Height = options.Height ?? 1080,
            FrameRate = options.FrameRate ?? 30
        };

        settings.ApplyQualityPreset();

        var outputPath = await _conversionService.ConvertVideoAsync(
            options.InputFile,
            options.OutputFile ?? "output.mp4",
            settings);

        if (!string.IsNullOrEmpty(outputPath))
        {
            Console.WriteLine($"✓ Conversion completed: {outputPath}");
            return 0;
        }

        DisplayError("Conversion failed");
        return 1;
    }

    private async Task<int> HandleBatchCommand(string[] args)
    {
        var options = _commandParser.ParseBatchOptions(args);

        if (options.Files?.Count == 0)
        {
            DisplayError("At least one file is required");
            return 1;
        }

        Console.WriteLine($"📦 Processing {options.Files?.Count ?? 0} files");

        var job = await _batchService.CreateBatchJobAsync(
            options.Name ?? "Batch Job",
            options.OutputDirectory ?? "./output",
            new ConversionSettings { Format = options.Format, Quality = options.Quality });

        Console.WriteLine($"✓ Batch job created: {job.Id}");
        return 0;
    }

    private async Task<int> HandleInfoCommand(string[] args)
    {
        var options = _commandParser.ParseInfoOptions(args);

        if (string.IsNullOrEmpty(options.Url))
        {
            DisplayError("URL is required");
            return 1;
        }

        Console.WriteLine($"🔍 Analyzing: {options.Url}");

        try
        {
            var ffmpegAvailable = await _conversionService.IsFfmpegAvailableAsync();
            var version = ffmpegAvailable ? await _conversionService.GetFfmpegVersionAsync() : "Not found";

            Console.WriteLine($"\nℹ️  System Information:");
            Console.WriteLine($"   FFmpeg available: {(ffmpegAvailable ? "✓ Yes" : "✗ No")}");
            Console.WriteLine($"   FFmpeg version: {version}");
            Console.WriteLine($"   .NET version: {RuntimeInformation.FrameworkDescription}");
            Console.WriteLine($"   OS: {RuntimeInformation.OSDescription}");

            return 0;
        }
        catch (Exception ex)
        {
            DisplayError(ex.Message);
            return 1;
        }
    }

    private int HandleVersionCommand()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        Console.WriteLine($"Coub Downloader v{version}");
        Console.WriteLine("Author: Vladyslav Zaiets (https://sarmkadan.com)");
        return 0;
    }

    private int HandleHelpCommand()
    {
        DisplayHelp();
        return 0;
    }

    private int HandleUnknownCommand(string command)
    {
        DisplayError($"Unknown command: {command}");
        Console.WriteLine("Use 'coub-downloader help' for available commands");
        return 1;
    }

    private void DisplayHelp()
    {
        Console.WriteLine(@"
╔══════════════════════════════════════════════════════════════════════════╗
║                     COUB DOWNLOADER - Command Reference                 ║
╚══════════════════════════════════════════════════════════════════════════╝

USAGE:
  coub-downloader <command> [options]

COMMANDS:

  download            Download a Coub video
    Options:
      --url <url>             Coub video URL (required)
      --output <path>         Output file path
      --format <fmt>          MP4 or WEBM (default: MP4)
      --quality <level>       Low, Medium, High, Ultra (default: High)

  convert             Convert video file
    Options:
      --input <file>          Input file path (required)
      --output <file>         Output file path
      --format <fmt>          MP4 or WEBM
      --quality <level>       Low, Medium, High, Ultra
      --width <px>            Video width
      --height <px>           Video height
      --fps <num>             Frame rate (default: 30)

  batch               Process multiple videos
    Options:
      --files <f1,f2,...>     Comma-separated file list
      --output <dir>          Output directory
      --name <name>           Batch job name
      --format <fmt>          Output format

  info                Display system information
    Options:
      --url <url>             Coub video URL

  version             Display version information

  help                Show this help message

EXAMPLES:
  coub-downloader download --url https://coub.com/view/abc123
  coub-downloader convert --input video.mp4 --quality Ultra --width 1080
  coub-downloader batch --files file1.mp4,file2.mp4 --output ./converted
  coub-downloader info --url https://coub.com/view/abc123

For more information, visit: https://github.com/vladyslav-zaiets/coub-downloader
");
    }

    private void DisplayError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"✗ ERROR: {message}");
        Console.ResetColor();
    }
}
