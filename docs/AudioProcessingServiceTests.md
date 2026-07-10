# AudioProcessingServiceTests

Unit test suite for the `AudioProcessingService` class, verifying its core audio manipulation workflows—duration extraction, looping with repeat and crossfade strategies, and synchronization with video timelines. The tests rely on mocked `IFFmpegWrapper` and `ILogger<AudioProcessingService>` dependencies to isolate service logic from external FFmpeg processes.

## API

### `public AudioProcessingServiceTests`

Default parameterless constructor. Initializes the test class instance. No setup logic is performed here; mocks and the system under test are typically configured within individual test methods or shared setup helpers.

### `public async Task GetAudioDurationAsync_ReturnsCorrectDuration_WhenFileExists`

**Purpose:** Validates that `AudioProcessingService.GetAudioDurationAsync` returns the expected duration when invoked with a valid audio file path.

**Parameters:** None (test method).

**Return value:** `Task` representing the asynchronous test operation. The test asserts that the returned duration matches a preconfigured value from the mocked FFmpeg wrapper.

**Throws:** Test assertion failures if the returned duration does not match the expected value or if the mocked FFmpeg wrapper is not invoked correctly.

### `public async Task LoopAudioAsync_RepeatStrategy_CallsFFmpegWrapperLoopAudioAsync`

**Purpose:** Confirms that when `LoopAudioAsync` is called with a repeat strategy, the service delegates to `IFFmpegWrapper.LoopAudioAsync` with the correct input path, output path, and iteration count.

**Parameters:** None (test method).

**Return value:** `Task` representing the asynchronous test operation. The test verifies the correct method call and argument values on the mock.

**Throws:** Test assertion failures if the mock is not called, is called with mismatched arguments, or is called an unexpected number of times.

### `public async Task LoopAudioAsync_CrossfadeStrategy_CallsFFmpegWrapperExecuteAsyncWithCorrectArgs`

**Purpose:** Ensures that when `LoopAudioAsync` is invoked with a crossfade strategy, the service calls `IFFmpegWrapper.ExecuteAsync` with an argument string containing the expected crossfade-specific FFmpeg filter parameters.

**Parameters:** None (test method).

**Return value:** `Task` representing the asynchronous test operation. The test asserts that the argument string passed to `ExecuteAsync` includes the correct crossfade duration and audio stream references.

**Throws:** Test assertion failures if `ExecuteAsync` is not invoked or the argument string does not match the expected crossfade filter specification.

### `public async Task SyncAudioWithVideoAsync_CallsLoopAudioAsyncWithVideoDuration`

**Purpose:** Verifies that `SyncAudioWithVideoAsync` internally calls `LoopAudioAsync` using the video’s duration as the target length, ensuring the audio is extended or trimmed to match the video timeline.

**Parameters:** None (test method).

**Return value:** `Task` representing the asynchronous test operation. The test confirms that `LoopAudioAsync` receives a duration argument derived from the video file.

**Throws:** Test assertion failures if `LoopAudioAsync` is not called or is called with a duration that does not correspond to the mocked video duration.

## Usage

```csharp
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class AudioProcessingServiceTestRunner
{
    [Fact]
    public async Task VerifyDurationExtraction_Example()
    {
        // Arrange
        var ffmpegMock = new Mock<IFFmpegWrapper>();
        ffmpegMock.Setup(f => f.GetAudioDurationAsync("input.mp3"))
                  .ReturnsAsync(TimeSpan.FromSeconds(42.5));

        var loggerMock = new Mock<ILogger<AudioProcessingService>>();
        var service = new AudioProcessingService(ffmpegMock.Object, loggerMock.Object);

        // Act
        var duration = await service.GetAudioDurationAsync("input.mp3");

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(42.5), duration);
    }

    [Fact]
    public async Task VerifySyncAudioWithVideo_Example()
    {
        // Arrange
        var ffmpegMock = new Mock<IFFmpegWrapper>();
        ffmpegMock.Setup(f => f.GetVideoDurationAsync("video.mp4"))
                  .ReturnsAsync(TimeSpan.FromMinutes(2));
        ffmpegMock.Setup(f => f.LoopAudioAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                  .Returns(Task.CompletedTask);

        var loggerMock = new Mock<ILogger<AudioProcessingService>>();
        var service = new AudioProcessingService(ffmpegMock.Object, loggerMock.Object);

        // Act
        await service.SyncAudioWithVideoAsync("audio.wav", "video.mp4", "output.wav");

        // Assert
        ffmpegMock.Verify(f => f.LoopAudioAsync(
            "audio.wav",
            "output.wav",
            It.Is<int>(count => count > 0)
        ), Times.Once);
    }
}
```

## Notes

- **Test isolation:** Each test method operates on independently configured mocks. There is no shared state between tests, making them safe for parallel execution by test runners such as xUnit.
- **Mock verification:** Tests rely on strict mock setups and `Verify` calls. If the service implementation changes its internal delegation pattern (e.g., switching from `LoopAudioAsync` to `ExecuteAsync` for repeat logic), the corresponding test will fail, serving as a contract guard.
- **Crossfade argument validation:** The crossfade test inspects the string argument passed to `ExecuteAsync`. Changes to FFmpeg filter syntax (e.g., parameter ordering, label naming) will break this test, which is intentional to catch regressions in the generated command line.
- **No real I/O:** All file paths are arbitrary strings; no actual filesystem access occurs. The mocked `IFFmpegWrapper` returns predefined values without invoking FFmpeg, keeping tests fast and deterministic.
- **Thread safety:** The test class itself is stateless. Thread safety concerns are not applicable at the test level; they are a concern of the `AudioProcessingService` implementation being tested.
