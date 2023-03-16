// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Extensions;
using CoubDownloader.Domain.Models;
using FluentAssertions;
using Xunit;

namespace CoubDownloader.Tests;

public class CoubVideoTests
{
    private static CoubVideo ValidVideo() => new()
    {
        Id = "abc123",
        Title = "Test Video",
        Url = "https://coub.com/view/abc123",
        Duration = 15.0,
        Width = 1280,
        Height = 720
    };

    [Fact]
    public void IsValid_AllRequiredFieldsPresent_ReturnsTrue()
    {
        var video = ValidVideo();
        video.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_MissingId_ReturnsFalse()
    {
        var video = ValidVideo();
        video.Id = "";
        video.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_ZeroDuration_ReturnsFalse()
    {
        var video = ValidVideo();
        video.Duration = 0;
        video.IsValid().Should().BeFalse();
    }

    [Fact]
    public void GetAspectRatio_LandscapeVideo_ReturnsRatioGreaterThanOne()
    {
        var video = ValidVideo(); // 1280x720
        video.GetAspectRatio().Should().BeGreaterThan(1m);
    }

    [Fact]
    public void IsVerticalFormat_PortraitDimensions_ReturnsTrue()
    {
        var video = ValidVideo();
        video.Width = 720;
        video.Height = 1280;
        video.IsVerticalFormat().Should().BeTrue();
    }

    [Fact]
    public void IsVerticalFormat_LandscapeDimensions_ReturnsFalse()
    {
        var video = ValidVideo(); // 1280x720
        video.IsVerticalFormat().Should().BeFalse();
    }

    [Theory]
    [InlineData(3.0, "Short")]
    [InlineData(8.0, "Medium")]
    [InlineData(20.0, "Long")]
    [InlineData(60.0, "Extra Long")]
    public void GetDurationCategory_VariousDurations_ReturnsCorrectCategory(double duration, string expected)
    {
        var video = ValidVideo();
        video.Duration = duration;
        video.GetDurationCategory().Should().Be(expected);
    }

    [Theory]
    [InlineData(500, "500")]
    [InlineData(1500, "1K")]
    [InlineData(2_500_000, "2M")]
    public void GetFormattedViewCount_VariousCounts_FormatsCorrectly(long views, string expected)
    {
        var video = ValidVideo();
        video.ViewCount = views;
        video.GetFormattedViewCount().Should().Be(expected);
    }

    [Fact]
    public void IsHdQuality_VideoAbove720p_ReturnsTrue()
    {
        var video = ValidVideo(); // 1280x720
        video.IsHdQuality().Should().BeTrue();
    }

    [Fact]
    public void Is4kQuality_StandardHD_ReturnsFalse()
    {
        var video = ValidVideo(); // 1280x720
        video.Is4kQuality().Should().BeFalse();
    }

    [Fact]
    public void Is4kQuality_UltraHDDimensions_ReturnsTrue()
    {
        var video = ValidVideo();
        video.Width = 3840;
        video.Height = 2160;
        video.Is4kQuality().Should().BeTrue();
    }

    [Fact]
    public void CalculateRequiredAudioDuration_AudioShorterThanVideo_ReturnsNextMultiple()
    {
        var video = ValidVideo();
        video.Duration = 10.0;
        video.AudioTrack = new AudioTrack
        {
            Id = "t1",
            VideoId = video.Id,
            Duration = 4.0
        };

        // ceil(10/4) * 4 = 3 * 4 = 12
        var result = CoubVideoExtensions.CalculateRequiredAudioDuration(video);
        result.Should().Be(12.0);
    }

    [Fact]
    public void CalculateRequiredAudioDuration_NoAudioTrack_ReturnsZero()
    {
        var video = ValidVideo();
        video.AudioTrack = null;

        var result = CoubVideoExtensions.CalculateRequiredAudioDuration(video);
        result.Should().Be(0);
    }
}

public class BatchJobTests
{
    private static BatchJob NewBatchJob(int total, int completed, int failed) => new()
    {
        Id = "batch-1",
        Name = "Test Batch",
        OutputDirectory = "/tmp/output",
        TotalTasks = total,
        CompletedTasks = completed,
        FailedTasks = failed
    };

    [Fact]
    public void GetProgressPercent_NoTasks_ReturnsZero()
    {
        var job = NewBatchJob(0, 0, 0);
        job.GetProgressPercent().Should().Be(0);
    }

    [Fact]
    public void GetProgressPercent_HalfDone_ReturnsFifty()
    {
        var job = NewBatchJob(10, 5, 0);
        job.GetProgressPercent().Should().Be(50);
    }

    [Fact]
    public void GetProgressPercent_AllCompleted_Returns100()
    {
        var job = NewBatchJob(4, 3, 1);
        job.GetProgressPercent().Should().Be(100);
    }

    [Fact]
    public void IsCompleted_AllTasksDone_ReturnsTrue()
    {
        var job = NewBatchJob(5, 4, 1);
        job.IsCompleted().Should().BeTrue();
    }

    [Fact]
    public void IsCompleted_TasksStillPending_ReturnsFalse()
    {
        var job = NewBatchJob(5, 3, 1);
        job.IsCompleted().Should().BeFalse();
    }

    [Fact]
    public void CanStart_PendingStateWithTasks_ReturnsTrue()
    {
        var job = NewBatchJob(3, 0, 0);
        job.State = ProcessingState.Pending;
        job.CanStart().Should().BeTrue();
    }

    [Fact]
    public void CanStart_AlreadyRunning_ReturnsFalse()
    {
        var job = NewBatchJob(3, 0, 0);
        job.State = ProcessingState.Downloading;
        job.CanStart().Should().BeFalse();
    }

    [Fact]
    public void GetPendingTaskCount_PartialProgress_ReturnsRemainder()
    {
        var job = NewBatchJob(10, 3, 2);
        job.GetPendingTaskCount().Should().Be(5);
    }

    [Fact]
    public void IsValid_ValidJob_ReturnsTrue()
    {
        var job = NewBatchJob(1, 0, 0);
        job.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_MissingName_ReturnsFalse()
    {
        var job = NewBatchJob(1, 0, 0);
        job.Name = "";
        job.IsValid().Should().BeFalse();
    }
}

public class AudioTrackTests
{
    private static AudioTrack NewTrack(AudioLoopStrategy strategy = AudioLoopStrategy.Repeat) => new()
    {
        Id = "track-1",
        VideoId = "video-1",
        Duration = 5.0,
        LoopCount = 3,
        LoopStrategy = strategy
    };

    [Fact]
    public void CalculateLoopedDuration_RepeatStrategy_MultipliesByLoopCount()
    {
        var track = NewTrack(AudioLoopStrategy.Repeat);
        track.CalculateLoopedDuration().Should().Be(15.0); // 5 * 3
    }

    [Fact]
    public void CalculateLoopedDuration_NoneStrategy_ReturnsOriginalDuration()
    {
        var track = NewTrack(AudioLoopStrategy.None);
        track.CalculateLoopedDuration().Should().Be(5.0);
    }

    [Fact]
    public void CalculateLoopedDuration_StretchStrategy_ReturnsOriginalDuration()
    {
        var track = NewTrack(AudioLoopStrategy.Stretch);
        track.CalculateLoopedDuration().Should().Be(5.0);
    }

    [Fact]
    public void CalculateLoopedDuration_CrossfadeStrategy_MultipliesByLoopCount()
    {
        var track = NewTrack(AudioLoopStrategy.Crossfade);
        track.CalculateLoopedDuration().Should().Be(15.0); // 5 * 3
    }

    [Fact]
    public void IsLossless_FlacCodec_ReturnsTrue()
    {
        var track = NewTrack();
        track.Codec = "flac";
        track.IsLossless().Should().BeTrue();
    }

    [Fact]
    public void IsLossless_AacCodec_ReturnsFalse()
    {
        var track = NewTrack();
        track.Codec = "aac";
        track.IsLossless().Should().BeFalse();
    }

    [Fact]
    public void IsStereo_TwoChannels_ReturnsTrue()
    {
        var track = NewTrack();
        track.Channels = 2;
        track.IsStereo().Should().BeTrue();
    }

    [Fact]
    public void IsMono_OneChannel_ReturnsTrue()
    {
        var track = NewTrack();
        track.Channels = 1;
        track.IsMono().Should().BeTrue();
    }

    [Fact]
    public void IsValid_AllRequiredFields_ReturnsTrue()
    {
        var track = NewTrack();
        track.SampleRate = 44100;
        track.Channels = 2;
        track.Bitrate = 128;
        track.IsValid().Should().BeTrue();
    }

    [Fact]
    public void GetAudioSpec_FormatsCorrectly()
    {
        var track = new AudioTrack
        {
            Id = "t1",
            VideoId = "v1",
            Duration = 3.0,
            SampleRate = 44100,
            Channels = 2,
            Bitrate = 128,
            Codec = "aac"
        };

        track.GetAudioSpec().Should().Be("44100Hz 2ch 128kbps aac");
    }
}

public class DownloadTaskTests
{
    private static DownloadTask NewTask(ProcessingState state = ProcessingState.Pending) => new()
    {
        Id = "task-1",
        VideoId = "video-1",
        Url = "https://coub.com/view/abc",
        OutputPath = "/tmp/output/video.mp4",
        State = state,
        RetryCount = 0,
        MaxRetries = 3
    };

    [Fact]
    public void IsRunning_DownloadingState_ReturnsTrue()
    {
        var task = NewTask(ProcessingState.Downloading);
        task.IsRunning().Should().BeTrue();
    }

    [Fact]
    public void IsRunning_CompletedState_ReturnsFalse()
    {
        var task = NewTask(ProcessingState.Completed);
        task.IsRunning().Should().BeFalse();
    }

    [Fact]
    public void CanRetry_FailedWithRemainingAttempts_ReturnsTrue()
    {
        var task = NewTask(ProcessingState.Failed);
        task.RetryCount = 1;
        task.MaxRetries = 3;
        task.CanRetry().Should().BeTrue();
    }

    [Fact]
    public void CanRetry_MaxRetriesReached_ReturnsFalse()
    {
        var task = NewTask(ProcessingState.Failed);
        task.RetryCount = 3;
        task.MaxRetries = 3;
        task.CanRetry().Should().BeFalse();
    }

    [Fact]
    public void CanRetry_NotFailed_ReturnsFalse()
    {
        var task = NewTask(ProcessingState.Pending);
        task.RetryCount = 0;
        task.CanRetry().Should().BeFalse();
    }

    [Fact]
    public void IsValid_AllRequiredFieldsSet_ReturnsTrue()
    {
        var task = NewTask();
        task.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_MissingUrl_ReturnsFalse()
    {
        var task = NewTask();
        task.Url = "";
        task.IsValid().Should().BeFalse();
    }
}

public class DownloadResultTests
{
    [Fact]
    public void GetStatusMessage_SuccessResult_ContainsCompletedText()
    {
        var result = new DownloadResult
        {
            Id = "r1",
            TaskId = "t1",
            Success = true,
            ProcessingTimeMs = 1200,
            OutputFileSizeBytes = 1024 * 1024
        };

        result.GetStatusMessage().Should().Contain("Completed");
    }

    [Fact]
    public void GetStatusMessage_FailedResult_ContainsErrorMessage()
    {
        var result = new DownloadResult
        {
            Id = "r1",
            TaskId = "t1",
            Success = false,
            ErrorMessage = "Network timeout"
        };

        result.GetStatusMessage().Should().Contain("Network timeout");
    }

    [Fact]
    public void AddWarning_NewWarning_AddsToList()
    {
        var result = new DownloadResult { Id = "r1", TaskId = "t1" };
        result.AddWarning("Low bitrate detected");

        result.HasWarnings.Should().BeTrue();
        result.Warnings.Should().ContainSingle().Which.Should().Be("Low bitrate detected");
    }

    [Fact]
    public void AddWarning_DuplicateWarning_NotAddedTwice()
    {
        var result = new DownloadResult { Id = "r1", TaskId = "t1" };
        result.AddWarning("Low bitrate detected");
        result.AddWarning("Low bitrate detected");

        result.Warnings.Should().HaveCount(1);
    }

    [Fact]
    public void GetFormattedFileSize_MegabyteRange_DisplaysMB()
    {
        var result = new DownloadResult
        {
            Id = "r1",
            TaskId = "t1",
            OutputFileSizeBytes = 5 * 1024 * 1024
        };

        result.GetFormattedFileSize().Should().Contain("MB");
    }
}
