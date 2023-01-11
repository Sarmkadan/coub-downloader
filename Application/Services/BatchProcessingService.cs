// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Exceptions;
using CoubDownloader.Domain.Models;
using CoubDownloader.Infrastructure.Repositories;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Service for managing and processing batch jobs.
/// </summary>
public class BatchProcessingService : IBatchProcessingService
{
    private readonly IBatchJobRepository _batchRepository;
    private readonly IDownloadTaskRepository _taskRepository;
    private readonly ICoubDownloadService _downloadService;

    public BatchProcessingService(
        IBatchJobRepository batchRepository,
        IDownloadTaskRepository taskRepository,
        ICoubDownloadService downloadService)
    {
        _batchRepository = batchRepository;
        _taskRepository = taskRepository;
        _downloadService = downloadService;
    }

    public async Task<BatchJob> CreateBatchJobAsync(
        string name,
        string outputDirectory,
        ConversionSettings? sharedSettings = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        var batch = new BatchJob
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            OutputDirectory = outputDirectory,
            SharedSettings = sharedSettings,
            State = ProcessingState.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _batchRepository.CreateAsync(batch);
    }

    public async Task AddTasksAsync(
        string batchJobId,
        IEnumerable<DownloadTask> tasks,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchJobId);

        var batch = await _batchRepository.GetByIdAsync(batchJobId);
        if (batch is null)
            throw new ResourceNotFoundException(nameof(BatchJob), batchJobId);

        if (batch.State != ProcessingState.Pending)
            throw new InvalidOperationException("Cannot add tasks to a batch that is already processing");

        var taskList = tasks.ToList();
        foreach (var task in taskList)
        {
            task.BatchJobId = batchJobId;
            task.OutputPath = Path.Combine(batch.OutputDirectory, $"{Path.GetFileNameWithoutExtension(task.Url)}.mp4");
            await _taskRepository.CreateAsync(task);
        }

        batch.TotalTasks = taskList.Count;
        batch.Tasks.AddRange(taskList);
        await _batchRepository.UpdateAsync(batch);
    }

    public async Task<BatchJob> StartBatchAsync(string batchJobId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchJobId);

        var batch = await _batchRepository.GetByIdAsync(batchJobId);
        if (batch is null)
            throw new ResourceNotFoundException(nameof(BatchJob), batchJobId);

        if (!batch.CanStart())
            throw new InvalidOperationException($"Batch '{batch.Id}' cannot be started. It must be pending and have tasks.");

        batch.State = ProcessingState.Downloading;
        batch.StartedAt = DateTime.UtcNow;
        await _batchRepository.UpdateAsync(batch);

        // Process tasks with parallelization limit
        var pendingTasks = batch.Tasks.Where(t => t.State == ProcessingState.Pending).ToList();
        var semaphore = new SemaphoreSlim(batch.MaxParallelTasks);
        var processingTasks = new List<Task>();

        foreach (var task in pendingTasks)
        {
            processingTasks.Add(ProcessTaskAsync(task, batch, semaphore, cancellationToken));
        }

        await Task.WhenAll(processingTasks);

        // Update batch completion status
        batch.State = batch.FailedTasks == 0 ? ProcessingState.Completed : ProcessingState.Failed;
        batch.CompletedAt = DateTime.UtcNow;
        await _batchRepository.UpdateAsync(batch);

        return batch;
    }

    public async Task CancelBatchAsync(string batchJobId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchJobId);

        var batch = await _batchRepository.GetByIdAsync(batchJobId);
        if (batch is null)
            throw new ResourceNotFoundException(nameof(BatchJob), batchJobId);

        batch.State = ProcessingState.Cancelled;
        batch.CompletedAt = DateTime.UtcNow;
        await _batchRepository.UpdateAsync(batch);

        // Cancel all running tasks
        foreach (var task in batch.Tasks.Where(t => t.IsRunning()))
        {
            task.State = ProcessingState.Cancelled;
            await _taskRepository.UpdateAsync(task);
        }
    }

    public async Task<BatchJob> GetBatchStatusAsync(string batchJobId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchJobId);

        var batch = await _batchRepository.GetByIdAsync(batchJobId);
        if (batch is null)
            throw new ResourceNotFoundException(nameof(BatchJob), batchJobId);

        return batch;
    }

    public async Task<IEnumerable<BatchJob>> GetAllBatchesAsync(CancellationToken cancellationToken = default)
    {
        return await _batchRepository.GetAllAsync();
    }

    public async Task<IEnumerable<BatchJob>> GetActiveBatchesAsync(CancellationToken cancellationToken = default)
    {
        var all = await _batchRepository.GetAllAsync();
        return all.Where(b => b.State is ProcessingState.Downloading or ProcessingState.Converting or ProcessingState.ProcessingAudio or ProcessingState.Pending);
    }

    public async Task<bool> DeleteBatchAsync(string batchJobId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchJobId);

        var batch = await _batchRepository.GetByIdAsync(batchJobId);
        if (batch is null)
            return false;

        if (batch.State is ProcessingState.Downloading or ProcessingState.Converting or ProcessingState.ProcessingAudio)
            throw new InvalidOperationException("Cannot delete a batch that is currently processing");

        return await _batchRepository.DeleteAsync(batchJobId);
    }


    /// <summary>Process a single task within a batch</summary>
    private async Task ProcessTaskAsync(
        DownloadTask task,
        BatchJob batch,
        SemaphoreSlim semaphore,
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            await _taskRepository.UpdateStateAsync(task.Id, ProcessingState.Downloading);

            // In a real implementation, would perform actual download/conversion
            // For now, simulate with delays
            await Task.Delay(100, cancellationToken);

            task.State = ProcessingState.Completed;
            task.FileSizeBytes = 5_000_000; // Simulated file size
            task.ProgressPercent = 100;
            task.CompletedAt = DateTime.UtcNow;

            await _taskRepository.UpdateAsync(task);

            var completed = batch.Tasks.Count(t => t.State == ProcessingState.Completed);
            var failed = batch.Tasks.Count(t => t.State == ProcessingState.Failed);

            await _batchRepository.UpdateProgressAsync(batch.Id, completed, failed);
        }
        catch (Exception ex)
        {
            task.State = ProcessingState.Failed;
            task.ErrorMessage = ex.Message;
            task.CompletedAt = DateTime.UtcNow;
            batch.FailedTasks++;

            await _taskRepository.UpdateAsync(task);
            await _batchRepository.UpdateAsync(batch);

            if (!batch.ContinueOnError)
                throw;
        }
        finally
        {
            semaphore.Release();
        }
    }
}
