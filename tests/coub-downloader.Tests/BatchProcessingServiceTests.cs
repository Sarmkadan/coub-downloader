using Xunit;
using Moq;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Enums;
using CoubDownloader.Infrastructure.Repositories;
using CoubDownloader.Domain.Exceptions;
using System.IO;

namespace CoubDownloader.Tests;

/// <summary>
/// Contains unit tests for the <see cref="BatchProcessingService"/> class.
/// Tests various scenarios for batch job creation, task management, batch processing,
/// cancellation, status retrieval, and deletion.
/// </summary>
public class BatchProcessingServiceTests
{
    private readonly Mock<IBatchJobRepository> _mockBatchRepository;
    private readonly Mock<IDownloadTaskRepository> _mockTaskRepository;
    private readonly Mock<ICoubDownloadService> _mockDownloadService;
    private readonly BatchProcessingService _sut;

    public BatchProcessingServiceTests()
    {
        _mockBatchRepository = new Mock<IBatchJobRepository>();
        _mockTaskRepository = new Mock<IDownloadTaskRepository>();
        _mockDownloadService = new Mock<ICoubDownloadService>();
        _sut = new BatchProcessingService(
            _mockBatchRepository.Object,
            _mockTaskRepository.Object,
            _mockDownloadService.Object);
    }

    // Test cases for CreateBatchJobAsync
/// <summary>
/// Tests that creating a batch job with valid inputs returns a new batch job with correct properties.
/// Verifies that the batch job is created with Pending state and stored via repository.
/// </summary>
    [Fact]
    public async Task CreateBatchJobAsync_ValidInputs_ReturnsNewBatchJob()
    {
        // Arrange
        var name = "Test Batch";
        var outputDir = "/path/to/output";
        _mockBatchRepository.Setup(repo => repo.CreateAsync(It.IsAny<BatchJob>()))
            .ReturnsAsync((BatchJob b) => { b.Id = Guid.NewGuid().ToString(); return b; });

        // Act
        var result = await _sut.CreateBatchJobAsync(name, outputDir);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(name);
        result.OutputDirectory.Should().Be(outputDir);
        result.State.Should().Be(ProcessingState.Pending);
        _mockBatchRepository.Verify(repo => repo.CreateAsync(It.IsAny<BatchJob>()), Times.Once);
    }

    [Theory]
    [InlineData(null, "output")]
    [InlineData("name", null)]
    [InlineData("", "output")]
    [InlineData("name", "")]
    public async Task CreateBatchJobAsync_InvalidInputs_ThrowsArgumentException(string name, string outputDir)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateBatchJobAsync(name, outputDir));
    }

    // Test cases for AddTasksAsync
/// <summary>
/// Tests that adding tasks to a valid batch updates the batch with the new tasks.
/// Verifies that tasks are created, assigned to batch, and output paths are generated.
/// </summary>
    [Fact]
    public async Task AddTasksAsync_ValidInputs_AddsTasksToBatch()
    {
        // Arrange
        var batchId = "batch123";
        var batch = new BatchJob
        {
            Id = batchId, Name = "Batch", OutputDirectory = "/path/to/output", State = ProcessingState.Pending
        };
        var tasks = new List<DownloadTask>
        {
            new() { Url = "http://coub.com/1", State = ProcessingState.Pending },
            new() { Url = "http://coub.com/2", State = ProcessingState.Pending }
        };

        _mockBatchRepository.Setup(repo => repo.GetByIdAsync(batchId)).ReturnsAsync(batch);
        _mockTaskRepository.Setup(repo => repo.CreateAsync(It.IsAny<DownloadTask>()))
            .ReturnsAsync((DownloadTask t) => { t.Id = Guid.NewGuid().ToString(); return t; });
        _mockBatchRepository.Setup(repo => repo.UpdateAsync(It.IsAny<BatchJob>())).ReturnsAsync(batch);

        // Act
        await _sut.AddTasksAsync(batchId, tasks);

        // Assert
        batch.Tasks.Should().HaveCount(2);
        batch.TotalTasks.Should().Be(2);
        tasks.All(t => t.BatchJobId == batchId).Should().BeTrue();
        tasks.All(t => !string.IsNullOrEmpty(t.OutputPath)).Should().BeTrue();
        _mockBatchRepository.Verify(repo => repo.GetByIdAsync(batchId), Times.Once);
        _mockTaskRepository.Verify(repo => repo.CreateAsync(It.IsAny<DownloadTask>()), Times.Exactly(2));
        _mockBatchRepository.Verify(repo => repo.UpdateAsync(batch), Times.Once);
    }

/// <summary>
/// Tests that adding tasks to a non-existent batch throws ResourceNotFoundException.
/// Verifies error handling when batch does not exist.
/// </summary>
    [Fact]
    public async Task AddTasksAsync_BatchNotFound_ThrowsResourceNotFoundException()
    {
        // Arrange
        var batchId = "nonexistent";
        _mockBatchRepository.Setup(repo => repo.GetByIdAsync(batchId)).ReturnsAsync((BatchJob)null!);
        var tasks = new List<DownloadTask> { new() { Url = "http://coub.com/1" } };

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(() => _sut.AddTasksAsync(batchId, tasks));
    }

/// <summary>
/// Tests that adding tasks to a batch that is not in Pending state throws InvalidOperationException.
/// Verifies that tasks cannot be added to batches that are already processing.
/// </summary>
    [Fact]
    public async Task AddTasksAsync_BatchNotPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var batchId = "batch123";
        var batch = new BatchJob { Id = batchId, State = ProcessingState.Downloading };
        _mockBatchRepository.Setup(repo => repo.GetByIdAsync(batchId)).ReturnsAsync(batch);
        var tasks = new List<DownloadTask> { new() { Url = "http://coub.com/1" } };

        // Act & Assert
        await FluentActions.Awaiting(() => _sut.AddTasksAsync(batchId, tasks))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot add tasks to a batch that is already processing");
    }

    // Test cases for StartBatchAsync
/// <summary>
/// Tests that starting a batch with valid tasks processes all tasks successfully.
/// Verifies that batch state transitions to Completed, all tasks complete, and progress is updated.
/// </summary>
    [Fact]
    public async Task StartBatchAsync_SuccessfulProcessing_UpdatesBatchStateToCompleted()
    {
        // Arrange
        var batchId = "batch123";
        var batch = new BatchJob
        {
            Id = batchId,
            Name = "Batch",
            OutputDirectory = "/path/to/output",
            State = ProcessingState.Pending,
            TotalTasks = 2,
            Tasks = new List<DownloadTask>
            {
                new() { Id = "task1", Url = "http://coub.com/1", OutputPath = "/path/to/output/1.mp4", State = ProcessingState.Pending },
                new() { Id = "task2", Url = "http://coub.com/2", OutputPath = "/path/to/output/2.mp4", State = ProcessingState.Pending }
            }
        };
        var coubVideo = new CoubVideo { Id = "vid1", Url = "http://coub.com/1" };

        _mockBatchRepository.Setup(repo => repo.GetByIdAsync(batchId)).ReturnsAsync(batch);
        _mockBatchRepository.Setup(repo => repo.UpdateAsync(It.IsAny<BatchJob>())).ReturnsAsync(batch);
        _mockBatchRepository.Setup(repo => repo.UpdateProgressAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);
        _mockDownloadService.Setup(ds => ds.DownloadVideoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(coubVideo);
        _mockTaskRepository.Setup(repo => repo.UpdateStateAsync(It.IsAny<string>(), It.IsAny<ProcessingState>()))
            .Returns(Task.CompletedTask);
        _mockTaskRepository.Setup(repo => repo.UpdateAsync(It.IsAny<DownloadTask>())).ReturnsAsync((DownloadTask t) => t);

        // Act
        var result = await _sut.StartBatchAsync(batchId, null);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(batchId);
        result.State.Should().Be(ProcessingState.Completed);
        result.CompletedAt.Should().NotBeNull();
        result.Tasks.Should().AllSatisfy(t => t.State.Should().Be(ProcessingState.Completed));
        result.Tasks.Should().AllSatisfy(t => t.ProgressPercent.Should().Be(100));

        _mockBatchRepository.Verify(repo => repo.UpdateAsync(It.Is<BatchJob>(b => b.State == ProcessingState.Downloading)), Times.Once);
        _mockBatchRepository.Verify(repo => repo.UpdateAsync(It.Is<BatchJob>(b => b.State == ProcessingState.Completed)), Times.Once);
        _mockDownloadService.Verify(ds => ds.DownloadVideoAsync("http://coub.com/1", It.IsAny<CancellationToken>()), Times.Once);
        _mockDownloadService.Verify(ds => ds.DownloadVideoAsync("http://coub.com/2", It.IsAny<CancellationToken>()), Times.Once);
        _mockTaskRepository.Verify(repo => repo.UpdateStateAsync("task1", ProcessingState.Downloading), Times.Once);
        _mockTaskRepository.Verify(repo => repo.UpdateStateAsync("task2", ProcessingState.Downloading), Times.Once);
        _mockTaskRepository.Verify(repo => repo.UpdateAsync(It.IsAny<DownloadTask>()), Times.Exactly(2));
        _mockBatchRepository.Verify(repo => repo.UpdateProgressAsync(batchId, 1, 0), Times.Once);
        _mockBatchRepository.Verify(repo => repo.UpdateProgressAsync(batchId, 2, 0), Times.Once);
    }

/// <summary>
/// Tests that starting a batch where some tasks fail updates batch state to Failed when ContinueOnError is true.
/// Verifies that successful tasks complete while failed tasks are marked as Failed.
/// </summary>
    [Fact]
    public async Task StartBatchAsync_SomeTasksFail_UpdatesBatchStateToFailed()
    {
        // Arrange
        var batchId = "batch123";
        var batch = new BatchJob
        {
            Id = batchId,
            Name = "Batch",
            OutputDirectory = "/path/to/output",
            State = ProcessingState.Pending,
            TotalTasks = 2,
            Tasks = new List<DownloadTask>
            {
                new() { Id = "task1", Url = "http://coub.com/1", OutputPath = "/path/to/output/1.mp4", State = ProcessingState.Pending },
                new() { Id = "task2", Url = "http://coub.com/2", OutputPath = "/path/to/output/2.mp4", State = ProcessingState.Pending }
            },
            ContinueOnError = true // Continue processing even if one task fails
        };
        var coubVideo = new CoubVideo { Id = "vid1", Url = "http://coub.com/1" };

        _mockBatchRepository.Setup(repo => repo.GetByIdAsync(batchId)).ReturnsAsync(batch);
        _mockBatchRepository.Setup(repo => repo.UpdateAsync(It.IsAny<BatchJob>())).ReturnsAsync(batch);
        _mockBatchRepository.Setup(repo => repo.UpdateProgressAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _mockDownloadService.SetupSequence(ds => ds.DownloadVideoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(coubVideo) // task1 succeeds
            .ThrowsAsync(new Exception("Download failed for task2")); // task2 fails

        _mockTaskRepository.Setup(repo => repo.UpdateStateAsync(It.IsAny<string>(), It.IsAny<ProcessingState>()))
            .Returns(Task.CompletedTask);
        _mockTaskRepository.Setup(repo => repo.UpdateAsync(It.IsAny<DownloadTask>())).ReturnsAsync((DownloadTask t) => t);

        // Act
        var result = await _sut.StartBatchAsync(batchId, null);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(batchId);
        result.State.Should().Be(ProcessingState.Failed); // Because one task failed
        result.FailedTasks.Should().Be(1);
        result.Tasks.First(t => t.Id == "task1").State.Should().Be(ProcessingState.Completed);
        result.Tasks.First(t => t.Id == "task2").State.Should().Be(ProcessingState.Failed);
        result.Tasks.First(t => t.Id == "task2").ErrorMessage.Should().Contain("Download failed");
    }

/// <summary>
/// Tests that starting a non-existent batch throws ResourceNotFoundException.
/// Verifies error handling when batch does not exist.
/// </summary>
    [Fact]
    public async Task StartBatchAsync_BatchNotFound_ThrowsResourceNotFoundException()
    {
        // Arrange
        var batchId = "nonexistent";
        _mockBatchRepository.Setup(repo => repo.GetByIdAsync(batchId)).ReturnsAsync((BatchJob)null!);

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(() => _sut.StartBatchAsync(batchId, null));
    }

/// <summary>
/// Tests that starting a batch with no tasks throws InvalidOperationException.
/// Verifies that batches must have tasks to be started.
/// </summary>
    [Fact]
    public async Task StartBatchAsync_BatchCannotStart_ThrowsInvalidOperationException()
    {
        // Arrange
        var batchId = "batch123";
        var batch = new BatchJob
        {
            Id = batchId,
            Name = "Batch",
            OutputDirectory = "/path/to/output",
            State = ProcessingState.Pending,
            TotalTasks = 0, // No tasks, so cannot start
            Tasks = new List<DownloadTask>()
        };
        _mockBatchRepository.Setup(repo => repo.GetByIdAsync(batchId)).ReturnsAsync(batch);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.StartBatchAsync(batchId, null));
    }

/// <summary>
/// Tests that starting a batch can be cancelled via CancellationToken.
/// Verifies that both batch and tasks are marked as Cancelled when cancellation is requested.
/// </summary>
    [Fact]
    public async Task StartBatchAsync_CancellationRequested_CancelsBatchAndTasks()
    {
        // Arrange
        var batchId = "batch123";
        var batch = new BatchJob
        {
            Id = batchId,
            Name = "Batch",
            OutputDirectory = "/path/to/output",
            State = ProcessingState.Pending,
            TotalTasks = 1,
            Tasks = new List<DownloadTask>
            {
                new() { Id = "task1", Url = "http://coub.com/1", OutputPath = "/path/to/output/1.mp4", State = ProcessingState.Pending }
            }
        };

        _mockBatchRepository.Setup(repo => repo.GetByIdAsync(batchId)).ReturnsAsync(batch);
        _mockBatchRepository.Setup(repo => repo.UpdateAsync(It.IsAny<BatchJob>())).ReturnsAsync(batch);
        _mockTaskRepository.Setup(repo => repo.UpdateStateAsync(It.IsAny<string>(), It.IsAny<ProcessingState>()))
            .Returns(Task.CompletedTask);
        _mockTaskRepository.Setup(repo => repo.UpdateAsync(It.IsAny<DownloadTask>())).ReturnsAsync((DownloadTask t) => t);

        var cts = new CancellationTokenSource();
        _mockDownloadService.Setup(ds => ds.DownloadVideoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns((string url, CancellationToken ct) =>
            {
                cts.Cancel(); // Simulate cancellation during download
                ct.ThrowIfCancellationRequested();
                return Task.FromResult(new CoubVideo());
            });

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => _sut.StartBatchAsync(batchId, null, cts.Token));

        // Ensure task and batch are updated as cancelled
        batch.State.Should().Be(ProcessingState.Cancelled); // Final state after cancellation
        batch.Tasks.First().State.Should().Be(ProcessingState.Cancelled);
        _mockBatchRepository.Verify(repo => repo.UpdateAsync(It.Is<BatchJob>(b => b.State == ProcessingState.Cancelled)), Times.Once);
        _mockTaskRepository.Verify(repo => repo.UpdateAsync(It.Is<DownloadTask>(t => t.State == ProcessingState.Cancelled)), Times.Once);
    }

    // Test cases for CancelBatchAsync
/// <summary>
/// Tests that cancelling a valid batch updates batch and running tasks to Cancelled state.
/// Verifies that only running tasks are cancelled while pending tasks remain unchanged.
/// </summary>
    [Fact]
    public async Task CancelBatchAsync_ValidBatch_UpdatesBatchAndTasksToCancelled()
    {
        // Arrange
        var batchId = "batch123";
        var batch = new BatchJob
        {
            Id = batchId,
            Name = "Batch",
            OutputDirectory = "/path/to/output",
            State = ProcessingState.Downloading,
            Tasks = new List<DownloadTask>
            {
                new() { Id = "task1", State = ProcessingState.Downloading },
                new() { Id = "task2", State = ProcessingState.Pending }
            }
        };

        _mockBatchRepository.Setup(repo => repo.GetByIdAsync(batchId)).ReturnsAsync(batch);
        _mockBatchRepository.Setup(repo => repo.UpdateAsync(It.IsAny<BatchJob>())).ReturnsAsync((BatchJob b) => b);
        _mockTaskRepository.Setup(repo => repo.UpdateAsync(It.IsAny<DownloadTask>())).ReturnsAsync((DownloadTask t) => t);

        // Act
        await _sut.CancelBatchAsync(batchId);

        // Assert
        batch.State.Should().Be(ProcessingState.Cancelled);
        batch.Tasks.First(t => t.Id == "task1").State.Should().Be(ProcessingState.Cancelled); // Running task
        batch.Tasks.First(t => t.Id == "task2").State.Should().Be(ProcessingState.Pending); // Pending tasks are not touched by this method
        _mockBatchRepository.Verify(repo => repo.UpdateAsync(batch), Times.Once);
        _mockTaskRepository.Verify(repo => repo.UpdateAsync(It.Is<DownloadTask>(t => t.Id == "task1")), Times.Once);
        _mockTaskRepository.Verify(repo => repo.UpdateAsync(It.Is<DownloadTask>(t => t.Id == "task2")), Times.Never);
    }

/// <summary>
/// Tests that cancelling a non-existent batch does nothing.
/// Verifies that the method handles non-existent batches gracefully.
/// </summary>
    [Fact]
    public async Task CancelBatchAsync_BatchNotFound_DoesNothing()
    {
        // Arrange
        var batchId = "nonexistent";
        _mockBatchRepository.Setup(repo => repo.GetByIdAsync(batchId)).ReturnsAsync((BatchJob)null!);

        // Act
        await _sut.CancelBatchAsync(batchId);

        // Assert
        _mockBatchRepository.Verify(repo => repo.UpdateAsync(It.IsAny<BatchJob>()), Times.Never);
        _mockTaskRepository.Verify(repo => repo.UpdateAsync(It.IsAny<DownloadTask>()), Times.Never);
    }

    // Test cases for GetBatchStatusAsync
/// <summary>
/// Tests that retrieving batch status with a valid batch ID returns the batch job.
/// Verifies that the repository is called and the correct batch is returned.
/// </summary>
    [Fact]
    public async Task GetBatchStatusAsync_ValidBatchId_ReturnsBatchJob()
    {
        // Arrange
        var batchId = "batch123";
        var expectedBatch = new BatchJob { Id = batchId, Name = "Test Batch" };
        _mockBatchRepository.Setup(repo => repo.GetByIdAsync(batchId)).ReturnsAsync(expectedBatch);

        // Act
        var result = await _sut.GetBatchStatusAsync(batchId);

        // Assert
        result.Should().Be(expectedBatch);
    }

/// <summary>
/// Tests that retrieving batch status for a non-existent batch throws ResourceNotFoundException.
/// Verifies error handling when batch does not exist.
/// </summary>
    [Fact]
    public async Task GetBatchStatusAsync_BatchNotFound_ThrowsResourceNotFoundException()
    {
        // Arrange
        var batchId = "nonexistent";
        _mockBatchRepository.Setup(repo => repo.GetByIdAsync(batchId)).ReturnsAsync((BatchJob)null!);

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(() => _sut.GetBatchStatusAsync(batchId));
    }

    // Test cases for GetAllBatchesAsync
/// <summary>
/// Tests that retrieving all batches returns the complete list of batch jobs.
/// Verifies that the repository's GetAllAsync method is called and results are returned.
/// </summary>
    [Fact]
    public async Task GetAllBatchesAsync_ReturnsAllBatches()
    {
        // Arrange
        var batches = new List<BatchJob>
        {
            new() { Id = "b1" }, new() { Id = "b2" }
        };
        _mockBatchRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(batches);

        // Act
        var result = await _sut.GetAllBatchesAsync();

        // Assert
        result.Should().BeEquivalentTo(batches);
    }

    // Test cases for GetActiveBatchesAsync
/// <summary>
/// Tests that retrieving active batches returns only batches that are not completed.
/// Verifies that Completed batches are filtered out from the results.
/// </summary>
    [Fact]
    public async Task GetActiveBatchesAsync_ReturnsOnlyActiveBatches()
    {
        // Arrange
        var batches = new List<BatchJob>
        {
            new() { Id = "b1", State = ProcessingState.Downloading },
            new() { Id = "b2", State = ProcessingState.Completed },
            new() { Id = "b3", State = ProcessingState.Pending },
            new() { Id = "b4", State = ProcessingState.Converting }
        };
        _mockBatchRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(batches);

        // Act
        var result = await _sut.GetActiveBatchesAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(b => b.Id == "b1");
        result.Should().Contain(b => b.Id == "b3");
        result.Should().Contain(b => b.Id == "b4");
        result.Should().NotContain(b => b.Id == "b2");
    }

    // Test cases for DeleteBatchAsync
/// <summary>
/// Tests that deleting a completed batch successfully deletes it and returns true.
/// Verifies that the repository's DeleteAsync method is called and returns true.
/// </summary>
    [Fact]
    public async Task DeleteBatchAsync_ValidBatch_ReturnsTrueAndDeletes()
    {
        // Arrange
        var batchId = "batch123";
        var batch = new BatchJob { Id = batchId, State = ProcessingState.Completed };
        _mockBatchRepository.Setup(repo => repo.GetByIdAsync(batchId)).ReturnsAsync(batch);
        _mockBatchRepository.Setup(repo => repo.DeleteAsync(batchId)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteBatchAsync(batchId);

        // Assert
        result.Should().BeTrue();
        _mockBatchRepository.Verify(repo => repo.DeleteAsync(batchId), Times.Once);
    }

/// <summary>
/// Tests that deleting a non-existent batch returns false.
/// Verifies that the method handles non-existent batches gracefully.
/// </summary>
    [Fact]
    public async Task DeleteBatchAsync_BatchNotFound_ReturnsFalse()
    {
        // Arrange
        var batchId = "nonexistent";
        _mockBatchRepository.Setup(repo => repo.GetByIdAsync(batchId)).ReturnsAsync((BatchJob)null!);

        // Act
        var result = await _sut.DeleteBatchAsync(batchId);

        // Assert
        result.Should().BeFalse();
        _mockBatchRepository.Verify(repo => repo.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

/// <summary>
/// Tests that deleting a batch that is currently processing throws InvalidOperationException.
/// Verifies that batches in processing states cannot be deleted.
/// </summary>
    [Fact]
    public async Task DeleteBatchAsync_ProcessingBatch_ThrowsInvalidOperationException()
    {
        // Arrange
        var batchId = "batch123";
        var batch = new BatchJob { Id = batchId, State = ProcessingState.Downloading };
        _mockBatchRepository.Setup(repo => repo.GetByIdAsync(batchId)).ReturnsAsync(batch);

        // Act & Assert
        await FluentActions.Awaiting(() => _sut.DeleteBatchAsync(batchId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot delete a batch that is currently processing");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task DeleteBatchAsync_InvalidBatchId_ThrowsArgumentException(string batchId)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteBatchAsync(batchId));
    }
}
