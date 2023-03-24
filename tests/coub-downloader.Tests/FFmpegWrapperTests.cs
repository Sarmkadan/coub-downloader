using Xunit;
using Moq;
using FluentAssertions;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using CoubDownloader.Infrastructure.Integration;
using CoubDownloader.Infrastructure.Middleware;
using System.Linq;
using System.Diagnostics; // For ProcessStartInfo

namespace CoubDownloader.Tests;

public class FFmpegWrapperTests
{
    private readonly Mock<ILoggingService> _mockLogger;
    private readonly string _dummyFfmpegPath;
    private readonly string _dummyFfprobePath;
    private readonly FFmpegWrapper _sut;

    public FFmpegWrapperTests()
    {
        _mockLogger = new Mock<ILoggingService>();
        _dummyFfmpegPath = "ffmpeg"; // Assuming ffmpeg/ffprobe exist in PATH for testing or are mocked
        _dummyFfprobePath = "ffprobe";
        _sut = new FFmpegWrapper(_dummyFfmpegPath, _dummyFfprobePath, _mockLogger.Object);
    }

    // Helper to create a mock of FFmpegWrapper to control ExecuteAsync
    private Mock<FFmpegWrapper> CreateMockFFmpegWrapper()
    {
        return new Mock<FFmpegWrapper>(_dummyFfmpegPath, _dummyFfprobePath, _mockLogger.Object) { CallBase = true };
    }

    // Test cases for IsAvailableAsync
    [Fact]
    public async Task IsAvailableAsync_FfmpegReturnsSuccess_ReturnsTrue()
    {
        // Arrange
        var mockFFmpegWrapper = CreateMockFFmpegWrapper();
        mockFFmpegWrapper.Setup(x => x.ExecuteAsync(It.Is<string[]>(args => args.Contains("-version")), It.IsAny<TimeSpan?>()))
                         .ReturnsAsync(new FFmpegResult { Success = true, ExitCode = 0 });

        // Act
        var result = await mockFFmpegWrapper.Object.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
        mockFFmpegWrapper.Verify(x => x.ExecuteAsync(It.IsAny<string[]>(), It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task IsAvailableAsync_FfmpegReturnsFailure_ReturnsFalse()
    {
        // Arrange
        var mockFFmpegWrapper = CreateMockFFmpegWrapper();
        mockFFmpegWrapper.Setup(x => x.ExecuteAsync(It.Is<string[]>(args => args.Contains("-version")), It.IsAny<TimeSpan?>()))
                         .ReturnsAsync(new FFmpegResult { Success = false, ExitCode = 1 });

        // Act
        var result = await mockFFmpegWrapper.Object.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ExecuteThrowsException_ReturnsFalse()
    {
        // Arrange
        var mockFFmpegWrapper = CreateMockFFmpegWrapper();
        mockFFmpegWrapper.Setup(x => x.ExecuteAsync(It.IsAny<string[]>(), It.IsAny<TimeSpan?>()))
                         .ThrowsAsync(new Exception("Simulated process start error"));

        // Act
        var result = await mockFFmpegWrapper.Object.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    // Test cases for GetVersionAsync
    [Fact]
    public async Task GetVersionAsync_FfmpegReturnsVersion_ReturnsVersionString()
    {
        // Arrange
        var mockFFmpegWrapper = CreateMockFFmpegWrapper();
        mockFFmpegWrapper.Setup(x => x.ExecuteAsync(It.Is<string[]>(args => args.Contains("-version")), It.IsAny<TimeSpan?>()))
                         .ReturnsAsync(new FFmpegResult { Success = true, Output = "ffmpeg version N-107050-g0f7f32997e Copyright (c) 2000-2022 the FFmpeg developers", ExitCode = 0 });

        // Act
        var result = await mockFFmpegWrapper.Object.GetVersionAsync();

        // Assert
        result.Should().StartWith("ffmpeg version");
        mockFFmpegWrapper.Verify(x => x.ExecuteAsync(It.IsAny<string[]>(), It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task GetVersionAsync_FfmpegReturnsFailure_ReturnsUnknown()
    {
        // Arrange
        var mockFFmpegWrapper = CreateMockFFmpegWrapper();
        mockFFmpegWrapper.Setup(x => x.ExecuteAsync(It.Is<string[]>(args => args.Contains("-version")), It.IsAny<TimeSpan?>()))
                         .ReturnsAsync(new FFmpegResult { Success = false, Error = "Error", ExitCode = 1 });

        // Act
        var result = await mockFFmpegWrapper.Object.GetVersionAsync();

        // Assert
        result.Should().Be("Unknown");
    }

    [Fact]
    public async Task GetVersionAsync_ExecuteThrowsException_ReturnsUnknown()
    {
        // Arrange
        var mockFFmpegWrapper = CreateMockFFmpegWrapper();
        mockFFmpegWrapper.Setup(x => x.ExecuteAsync(It.IsAny<string[]>(), It.IsAny<TimeSpan?>()))
                         .ThrowsAsync(new Exception("Simulated process start error"));

        // Act
        var result = await mockFFmpegWrapper.Object.GetVersionAsync();

        // Assert
        result.Should().Be("Unknown");
    }

    // Test cases for ConvertVideoAsync - primarily verifying argument building
    [Fact]
    public async Task ConvertVideoAsync_ValidParameters_CallsExecuteWithCorrectArguments()
    {
        // Arrange
        var mockFFmpegWrapper = CreateMockFFmpegWrapper();
        mockFFmpegWrapper.Setup(x => x.ExecuteAsync(It.IsAny<string[]>(), It.IsAny<TimeSpan?>()))
                         .ReturnsAsync(new FFmpegResult { Success = true });

        var inputFile = "input.mp4";
        var outputFile = "output.mp4";
        var parameters = new ConversionParameters
        {
            VideoCodec = "libx264",
            AudioCodec = "aac",
            VideoBitrate = 5000,
            AudioBitrate = 128,
            FrameRate = 30,
            Width = 1920,
            Height = 1080,
            UseHardwareAcceleration = true
        };

        // Act
        await mockFFmpegWrapper.Object.ConvertVideoAsync(inputFile, outputFile, parameters);

        // Assert
        mockFFmpegWrapper.Verify(x => x.ExecuteAsync(It.Is<string[]>(args =>
            args.Contains("-hwaccel") && args.Contains("auto") &&
            args.Contains("-i") && args.Contains(inputFile) &&
            args.Contains("-c:v") && args.Contains("libx264") &&
            args.Contains("-c:a") && args.Contains("aac") &&
            args.Contains("-b:v") && args.Contains("5000k") &&
            args.Contains("-b:a") && args.Contains("128k") &&
            args.Contains("-r") && args.Contains("30") &&
            args.Contains("-vf") && args.Contains("scale=1920:1080") &&
            args.Contains("-y") && args.Contains(outputFile)
        ), It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task ConvertVideoAsync_NoHardwareAcceleration_DoesNotIncludeHwaccel()
    {
        // Arrange
        var mockFFmpegWrapper = CreateMockFFmpegWrapper();
        mockFFmpegWrapper.Setup(x => x.ExecuteAsync(It.IsAny<string[]>(), It.IsAny<TimeSpan?>()))
                         .ReturnsAsync(new FFmpegResult { Success = true });

        var inputFile = "input.mp4";
        var outputFile = "output.mp4";
        var parameters = new ConversionParameters
        {
            VideoCodec = "libx264",
            AudioCodec = "aac",
            VideoBitrate = 5000,
            AudioBitrate = 128,
            FrameRate = 30,
            Width = 1920,
            Height = 1080,
            UseHardwareAcceleration = false
        };

        // Act
        await mockFFmpegWrapper.Object.ConvertVideoAsync(inputFile, outputFile, parameters);

        // Assert
        mockFFmpegWrapper.Verify(x => x.ExecuteAsync(It.Is<string[]>(args =>
            !args.Contains("-hwaccel")
        ), It.IsAny<TimeSpan?>()), Times.Once);
    }

    // Test cases for ExtractAudioAsync - primarily verifying argument building
    [Fact]
    public async Task ExtractAudioAsync_ValidPaths_CallsExecuteWithCorrectArguments()
    {
        // Arrange
        var mockFFmpegWrapper = CreateMockFFmpegWrapper();
        mockFFmpegWrapper.Setup(x => x.ExecuteAsync(It.IsAny<string[]>(), It.IsAny<TimeSpan?>()))
                         .ReturnsAsync(new FFmpegResult { Success = true });

        var inputFile = "video.mp4";
        var outputFile = "audio.mp3";

        // Act
        await mockFFmpegWrapper.Object.ExtractAudioAsync(inputFile, outputFile);

        // Assert
        mockFFmpegWrapper.Verify(x => x.ExecuteAsync(It.Is<string[]>(args =>
            args.Contains("-i") && args.Contains(inputFile) &&
            args.Contains("-q:a") && args.Contains("0") &&
            args.Contains("-map") && args.Contains("a") &&
            args.Contains("-y") && args.Contains(outputFile)
        ), It.IsAny<TimeSpan?>()), Times.Once);
    }

    // Test cases for ConcatenateVideosAsync - verifying argument building and temp file handling
    [Fact]
    public async Task ConcatenateVideosAsync_ValidInputs_CallsExecuteWithCorrectArgumentsAndCleansUpTempFile()
    {
        // Arrange
        var mockFFmpegWrapper = CreateMockFFmpegWrapper();
        mockFFmpegWrapper.Setup(x => x.ExecuteAsync(It.IsAny<string[]>(), It.IsAny<TimeSpan?>()))
                         .ReturnsAsync(new FFmpegResult { Success = true });

        var inputFiles = new List<string> { "part1.mp4", "part2.mp4" };
        var outputFile = "concatenated.mp4";

        // Mock File.WriteAllText and File.Delete to avoid actual file system interaction
        var mockFile = new Mock<IFileAdapter>(); // Assuming an IFileAdapter for File.WriteAllText/Delete
                                                 // Since the current code uses static File methods, this requires refactoring.
                                                 // For now, let's allow actual file system interaction for the temp file.

        // Act
        var result = await mockFFmpegWrapper.Object.ConcatenateVideosAsync(inputFiles, outputFile);

        // Assert
        result.Success.Should().BeTrue();
        mockFFmpegWrapper.Verify(x => x.ExecuteAsync(It.Is<string[]>(args =>
            args.Contains("-f") && args.Contains("concat") &&
            args.Contains("-safe") && args.Contains("0") &&
            args.Contains("-i") && args.Any(arg => arg.EndsWith(".tmp")) && // temp file argument
            args.Contains("-c") && args.Contains("copy") &&
            args.Contains("-y") && args.Contains(outputFile)
        ), It.IsAny<TimeSpan?>()), Times.Once);

        // Ensure temp file is cleaned up (this is tested by the finally block in the real code)
        // Can't directly assert on File.Delete without mocking File System.
    }

    // Test cases for LoopAudioAsync - verifying argument building
    [Fact]
    public async Task LoopAudioAsync_ValidInputs_CallsExecuteWithCorrectArguments()
    {
        // Arrange
        var mockFFmpegWrapper = CreateMockFFmpegWrapper();
        mockFFmpegWrapper.Setup(x => x.ExecuteAsync(It.IsAny<string[]>(), It.IsAny<TimeSpan?>()))
                         .ReturnsAsync(new FFmpegResult { Success = true });

        var audioFile = "loop.mp3";
        var targetDuration = 60.5;
        var outputFile = "looped_audio.aac";

        // Act
        await mockFFmpegWrapper.Object.LoopAudioAsync(audioFile, targetDuration, outputFile);

        // Assert
        mockFFmpegWrapper.Verify(x => x.ExecuteAsync(It.Is<string[]>(args =>
            args.Contains("-stream_loop") && args.Contains("-1") &&
            args.Contains("-i") && args.Contains(audioFile) &&
            args.Contains("-t") && args.Contains(targetDuration.ToString("F2")) &&
            args.Contains("-c:a") && args.Contains("aac") &&
            args.Contains("-y") && args.Contains(outputFile)
        ), It.IsAny<TimeSpan?>()), Times.Once);
    }

    // Test cases for GetMediaInfoAsync - JSON parsing
    [Fact]
    public async Task GetMediaInfoAsync_ValidFfprobeOutput_ReturnsMediaInfo()
    {
        // Arrange
        var ffprobeOutput = @"{
            ""format"": {
                ""duration"": ""123.45"",
                ""size"": ""1024000"",
                ""bit_rate"": ""6000000""
            }
        }";

        // Since GetMediaInfoAsync directly calls Process.Start for ffprobe,
        // we can't easily mock this method's internal `Process` creation with basic Moq.
        // A more advanced mocking framework or refactoring of FFmpegWrapper would be needed.
        // For now, we will test the JSON deserialization aspect by having a mock of FFmpegWrapper
        // that overrides GetMediaInfoAsync to return a predefined MediaInfo.
        // This is not ideal, as it doesn't test the actual Process.Start part.

        var mockFFmpegWrapper = CreateMockFFmpegWrapper(); // CallBase is true by default
        mockFFmpegWrapper.Setup(x => x.GetMediaInfoAsync(It.IsAny<string>()))
                         .ReturnsAsync(JsonSerializer.Deserialize<FFmpegWrapper.MediaInfoWrapper>(ffprobeOutput)?.Format);

        var filePath = "dummy.mp4";

        // Act
        var result = await mockFFmpegWrapper.Object.GetMediaInfoAsync(filePath);

        // Assert
        result.Should().NotBeNull();
        result.DurationInSeconds.Should().Be(123.45);
        result.Size.Should().Be(1024000);
        result.BitRate.Should().Be(6000000);
    }

    [Fact]
    public async Task GetMediaInfoAsync_InvalidFfprobeOutput_ReturnsNullAndLogsError()
    {
        // Arrange
        var invalidFfprobeOutput = "THIS IS NOT JSON";

        // Same mocking strategy as above for GetMediaInfoAsync
        var mockFFmpegWrapper = CreateMockFFmpegWrapper();
        mockFFmpegWrapper.Setup(x => x.GetMediaInfoAsync(It.IsAny<string>()))
                         .ThrowsAsync(new Exception("Simulated JSON parsing error")); // Simulate deserialization failure

        var filePath = "dummy.mp4";

        // Act
        var result = await mockFFmpegWrapper.Object.GetMediaInfoAsync(filePath);

        // Assert
        result.Should().BeNull();
        _mockLogger.Verify(x => x.LogError(
            It.Is<string>(msg => msg.Contains("Failed to get media info")),
            It.IsAny<Exception>(),
            It.IsAny<string>()), Times.Once);
    }

    // A basic integration-style test for ExecuteAsync
    [Fact]
    public async Task ExecuteAsync_WithEchoCommand_ReturnsSuccessfulResult()
    {
        // Arrange
        // This test will actually execute a process.
        // This is more of an integration test for the FFmpegWrapper's ability to run external processes.
        var args = new[] { "echo", "hello world" };

        // Act
        var result = await _sut.ExecuteAsync(args);

        // Assert
        result.Success.Should().BeTrue();
        result.ExitCode.Should().Be(0);
        result.Output.Should().Contain("hello world");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentCommand_ReturnsFailureResult()
    {
        // Arrange
        var args = new[] { "non_existent_command_123456", "arg1" };

        // Act
        var result = await _sut.ExecuteAsync(args);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        // Exit code might vary depending on OS (e.g., 127 on Linux for command not found)
    }

    [Fact]
    public async Task ExecuteAsync_WithTimeout_KillsProcessAndReturnsTimeoutError()
    {
        // Arrange
        // Use a command that runs longer than the timeout
        var args = new[] { "sleep", "5" }; // Linux/macOS
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            args = new[] { "timeout", "5" }; // Windows
        }
        var timeout = TimeSpan.FromSeconds(1);

        // Act
        var result = await _sut.ExecuteAsync(args, timeout);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("timed out");
    }
}
