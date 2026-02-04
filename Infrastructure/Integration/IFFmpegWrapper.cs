#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Infrastructure.Integration;
using CoubDownloader.Infrastructure.Middleware;

namespace CoubDownloader.Infrastructure.Integration;

public interface IFFmpegWrapper
{
    Task<bool> IsAvailableAsync();
    Task<string> GetVersionAsync();
    Task<FFmpegResult> ExecuteAsync(string[] arguments, TimeSpan? timeout = null);
    Task<FFmpegResult> ConvertVideoAsync(string inputFile, string outputFile, ConversionParameters parameters, IProgress<int>? progress = null);
    Task<FFmpegResult> ExtractAudioAsync(string inputFile, string outputFile);
    Task<FFmpegResult> ConcatenateVideosAsync(List<string> inputFiles, string outputFile);
    Task<FFmpegResult> LoopAudioAsync(string audioFile, double targetDuration, string outputFile);
    Task<MediaInfo?> GetMediaInfoAsync(string filePath);
}
