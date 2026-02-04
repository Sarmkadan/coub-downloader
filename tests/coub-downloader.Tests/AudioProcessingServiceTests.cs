#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Exceptions;
using CoubDownloader.Infrastructure.Integration;
using FluentAssertions;
using System.IO;
using System.Text.RegularExpressions;

namespace coub_downloader.Tests;

public class AudioProcessingServiceTests
{
    private readonly Mock<IFFmpegWrapper> _mockFFmpegWrapper;
    private readonly AudioProcessingService _audioProcessingService;

    public AudioProcessingServiceTests()
    {
        _mockFFmpegWrapper = new Mock<IFFmpegWrapper>();
        _audioProcessingService = new AudioProcessingService(_mockFFmpegWrapper.Object);

        // Setup common mock behaviors
        _mockFFmpegWrapper.Setup(w => w.ExecuteAsync(It.IsAny<string[]>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(new FFmpegResult { Success = true });
        _mockFFmpegWrapper.Setup(w => w.ExtractAudioAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new FFmpegResult { Success = true });
        _mockFFmpegWrapper.Setup(w => w.LoopAudioAsync(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<string>()))
            .ReturnsAsync(new FFmpegResult { Success = true });

        // Ensure temp files can be created
        _mockFFmpegWrapper.Setup(w => w.GetMediaInfoAsync(It.IsAny<string>()))
            .ReturnsAsync((string path) =>
            {
                if (File.Exists(path))
                {
                    return new MediaInfo { DurationInSeconds = 10.0 }; // Default duration for existing files
                }
                return null;
            });
    }

    [Fact]
    public async Task GetAudioDurationAsync_ReturnsCorrectDuration_WhenFileExists()
    {
        // Arrange
        var tempAudioFile = Path.GetTempFileName();
        File.WriteAllText(tempAudioFile, "dummy audio content");
        var expectedDuration = 123.45;
        _mockFFmpegWrapper.Setup(w => w.GetMediaInfoAsync(tempAudioFile))
            .ReturnsAsync(new MediaInfo { DurationInSeconds = expectedDuration });

        // Act
        var result = await _audioProcessingService.GetAudioDurationAsync(tempAudioFile);

        // Assert
        result.Should().Be(expectedDuration);
        File.Delete(tempAudioFile);
    }

    [Fact]
    public async Task LoopAudioAsync_RepeatStrategy_CallsFFmpegWrapperLoopAudioAsync()
    {
        // Arrange
        var audioPath = Path.GetTempFileName();
        var outputPath = Path.GetTempFileName();
        var targetDuration = 15.0;
        File.WriteAllText(audioPath, "dummy content"); // Ensure file exists for validation

        _mockFFmpegWrapper.Setup(w => w.GetMediaInfoAsync(audioPath))
            .ReturnsAsync(new MediaInfo { DurationInSeconds = 5.0 }); // Set a dummy duration

        // Act
        await _audioProcessingService.LoopAudioAsync(audioPath, targetDuration, outputPath, AudioLoopStrategy.Repeat);

        // Assert
        _mockFFmpegWrapper.Verify(w => w.LoopAudioAsync(audioPath, targetDuration, outputPath), Times.Once);
        File.Delete(audioPath);
        File.Delete(outputPath);
    }

    [Fact]
    public async Task LoopAudioAsync_CrossfadeStrategy_CallsFFmpegWrapperExecuteAsyncWithCorrectArgs()
    {
        // Arrange
        var audioPath = Path.GetTempFileName();
        var outputPath = Path.GetTempFileName();
        var targetDuration = 25.0;
        var audioDuration = 5.0;
        File.WriteAllText(audioPath, "dummy content");

        _mockFFmpegWrapper.Setup(w => w.GetMediaInfoAsync(audioPath))
            .ReturnsAsync(new MediaInfo { DurationInSeconds = audioDuration });

        // Act
        await _audioProcessingService.LoopAudioAsync(audioPath, targetDuration, outputPath, AudioLoopStrategy.Crossfade);

        // Assert
        var numLoops = (int)Math.Ceiling(targetDuration / audioDuration);
        var expectedFilterComplex = new List<string>();
        for (int i = 0; i < numLoops; i++)
        {
            expectedFilterComplex.Add($"[{i}:0]adelay={i * audioDuration * 1000}|{i * audioDuration * 1000}[a{i}]");
        }
        expectedFilterComplex.Add(string.Join("", Enumerable.Range(0, numLoops).Select(i => $"[a{i}]")) + $"amix=inputs={numLoops}:duration=longest:dropout_transition=0,apad[aout]");
        var expectedFilterComplexString = string.Join(";", expectedFilterComplex);

        _mockFFmpegWrapper.Verify(w => w.ExecuteAsync(It.Is<string[]>(args =>
                args.Contains("-filter_complex") &&
                args.Contains(expectedFilterComplexString) &&
                args.Contains(outputPath)),
                It.IsAny<TimeSpan?>()), Times.Once);
        File.Delete(audioPath);
        File.Delete(outputPath);
    }

    [Fact]
    public async Task SyncAudioWithVideoAsync_CallsLoopAudioAsyncWithVideoDuration()
    {
        // Arrange
        var audioPath = Path.GetTempFileName();
        var videoPath = Path.GetTempFileName();
        var outputPath = Path.GetTempFileName();
        var videoDuration = 30.5;
        File.WriteAllText(audioPath, "dummy audio content");
        File.WriteAllText(videoPath, "dummy video content");

        _mockFFmpegWrapper.Setup(w => w.GetMediaInfoAsync(videoPath))
            .ReturnsAsync(new MediaInfo { DurationInSeconds = videoDuration });
        _mockFFmpegWrapper.Setup(w => w.GetMediaInfoAsync(audioPath))
            .ReturnsAsync(new MediaInfo { DurationInSeconds = 5.0 }); // Dummy audio duration

        // Act
        await _audioProcessingService.SyncAudioWithVideoAsync(audioPath, videoPath, outputPath, AudioLoopStrategy.Repeat);

        // Assert
        _mockFFmpegWrapper.Verify(w => w.LoopAudioAsync(
            audioPath,
            videoDuration,
            It.IsAny<string>()), Times.Once);
        File.Delete(audioPath);
        File.Delete(videoPath);
        File.Delete(outputPath);
    }
}
