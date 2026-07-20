#nullable enable
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
        ArgumentNullException.ThrowIfNull(batchRepository);
        ArgumentNullException.ThrowIfNull(taskRepository);
        ArgumentNullException.ThrowIfNull(downloadService);

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

        try
        {
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
        catch (Exception ex) when (ex is not ValidationException and not FileOperationException)
        {
            throw new BatchProcessingException(
                name,
                0,
                $"Failed to create batch job '{name}'")
            {
                BatchJobId = name
            };
        }
    }

    public async Task AddTasksAsync(
        string batchJobId,
        IEnumerable<DownloadTask> tasks,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchJobId);
        ArgumentNullException.ThrowIfNull(tasks);

        try
        {
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
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not ResourceNotFoundException and not InvalidOperationException)
        {
            throw new BatchProcessingException(
                batchJobId,
                0,
                $"Failed to add tasks to batch '{batchJobId}'")
            {
                BatchJobId = batchJobId
            };
        }
    }

    public async Task<BatchJob> StartBatchAsync(string batchJobId, IProgress<BatchProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchJobId);

        try
        {
            var batch = await _batchRepository.GetByIdAsync(batchJobId);
            if (batch is null)
                throw new ResourceNotFoundException(nameof(BatchJob), batchJobId);

            if (!batch.CanStart())
                throw new InvalidOperationException($"Batch '{batch.Id}' cannot be started. It must be pending and have tasks.");

            batch.State = ProcessingState.Downloading;
            batch.StartedAt = DateTime.UtcNow;
            await _batchRepository.UpdateAsync(batch);

            // Report initial progress
            progress?.Report(new BatchProgress
            {
                Total = batch.TotalTasks,
                Completed = 0,
                Failed = 0,
                CurrentItem = 0,
                CurrentTaskUrl = null
            });

            // Process tasks with parallelization limit
            var pendingTasks = batch.Tasks.Where(t => t.State == ProcessingState.Pending).ToList();
            var semaphore = new SemaphoreSlim(batch.MaxParallelTasks);
            var processingTasks = new List<Task>();

            for (int i = 0; i < pendingTasks.Count; i++)
            {
                var task = pendingTasks[i];
                processingTasks.Add(ProcessTaskAsync(task, batch, semaphore, i, pendingTasks.Count, progress, cancellationToken));
            }

            await Task.WhenAll(processingTasks);

            // Update batch completion status
            batch.State = batch.FailedTasks == 0 ? ProcessingState.Completed : ProcessingState.Failed;
            batch.CompletedAt = DateTime.UtcNow;
            await _batchRepository.UpdateAsync(batch);

            // Report final progress
            progress?.Report(new BatchProgress
            {
                Total = batch.TotalTasks,
                Completed = batch.CompletedTasks,
                Failed = batch.FailedTasks,
                CurrentItem = batch.TotalTasks,
                CurrentTaskUrl = null
            });

            return batch;
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not ResourceNotFoundException and not InvalidOperationException)
        {
            throw new BatchProcessingException(
                batchJobId,
                0,
                $"Failed to start batch '{batchJobId}'")
            {
                BatchJobId = batchJobId
            };
        }
    }

    public async Task CancelBatchAsync(string batchJobId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchJobId);

        try
        {
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
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not ResourceNotFoundException)
        {
            throw new BatchProcessingException(
                batchJobId,
                0,
                $"Failed to cancel batch '{batchJobId}'")
            {
                BatchJobId = batchJobId
            };
        }
    }

    public async Task<BatchJob> GetBatchStatusAsync(string batchJobId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchJobId);

        try
        {
            var batch = await _batchRepository.GetByIdAsync(batchJobId);
            if (batch is null)
                throw new ResourceNotFoundException(nameof(BatchJob), batchJobId);

            return batch;
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not ResourceNotFoundException)
        {
            throw new BatchProcessingException(
                batchJobId,
                0,
                $"Failed to get batch status for '{batchJobId}'")
            {
                BatchJobId = batchJobId
            };
        }
    }

    public async Task<IEnumerable<BatchJob>> GetAllBatchesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _batchRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            throw new BatchProcessingException(
                "all_batches",
                0,
                "Failed to get all batches")
            {
                BatchJobId = "all_batches"
            };
        }
    }

    public async Task<IEnumerable<BatchJob>> GetActiveBatchesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var all = await _batchRepository.GetAllAsync();
            return all.Where(b => b.State is ProcessingState.Downloading or ProcessingState.Converting or ProcessingState.ProcessingAudio or ProcessingState.Pending);
        }
        catch (Exception ex)
        {
            throw new BatchProcessingException(
                "active_batches",
                0,
                "Failed to get active batches")
            {
                BatchJobId = "active_batches"
            };
        }
    }

    public async Task<bool> DeleteBatchAsync(string batchJobId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchJobId);

        try
        {
            var batch = await _batchRepository.GetByIdAsync(batchJobId);
            if (batch is null)
                return false;

            if (batch.State is ProcessingState.Downloading or ProcessingState.Converting or ProcessingState.ProcessingAudio)
                throw new InvalidOperationException("Cannot delete a batch that is currently processing");

            return await _batchRepository.DeleteAsync(batchJobId);
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not ResourceNotFoundException and not InvalidOperationException)
        {
            throw new BatchProcessingException(
                batchJobId,
                0,
                $"Failed to delete batch '{batchJobId}'")
            {
                BatchJobId = batchJobId
            };
        }
    }

    /// <summary>Process a single task within a batch</summary>
    private async Task ProcessTaskAsync(
        DownloadTask task,
        BatchJob batch,
        SemaphoreSlim semaphore,
        int taskIndex,
        int totalTasks,
        IProgress<BatchProgress>? progress,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(batch);

        await semaphore.WaitAsync(cancellationToken);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _taskRepository.UpdateStateAsync(task.Id, ProcessingState.Downloading);

            // Report task started progress
            progress?.Report(new BatchProgress
            {
                Total = totalTasks,
                Completed = batch.CompletedTasks,
                Failed = batch.FailedTasks,
                CurrentItem = taskIndex,
                CurrentTaskUrl = task.Url
            });

            var video = await _downloadService.DownloadVideoAsync(task.Url, cancellationToken);

            task.State = ProcessingState.Completed;
            task.ProgressPercent = 100;
            task.CompletedAt = DateTime.UtcNow;

            await _taskRepository.UpdateAsync(task);

            // Update batch progress
            batch.CompletedTasks++;
            await _batchRepository.UpdateProgressAsync(batch.Id, batch.CompletedTasks, batch.FailedTasks);

            // Report task completed progress
            progress?.Report(new BatchProgress
            {
                Total = totalTasks,
                Completed = batch.CompletedTasks,
                Failed = batch.FailedTasks,
                CurrentItem = taskIndex + 1,
                CurrentTaskUrl = task.Url
            });
        }
        catch (OperationCanceledException)
        {
            task.State = ProcessingState.Cancelled;
            task.CompletedAt = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task);
            throw;
        }
        catch (CoubDownloaderException ex)
        {
            task.State = ProcessingState.Failed;
            task.ErrorMessage = ex.Message;
            task.CompletedAt = DateTime.UtcNow;
            batch.FailedTasks++;

            await _taskRepository.UpdateAsync(task);
            await _batchRepository.UpdateAsync(batch);

            // Report task failed progress
            progress?.Report(new BatchProgress
            {
                Total = totalTasks,
                Completed = batch.CompletedTasks,
                Failed = batch.FailedTasks,
                CurrentItem = taskIndex + 1,
                CurrentTaskUrl = task.Url
            });

            if (!batch.ContinueOnError)
                throw new BatchProcessingException(batch.Id, batch.FailedTasks, $"Batch job failed: {ex.Message}")
                {
                    BatchJobId = batch.Id,
                    FailedTaskCount = batch.FailedTasks
                };
        }
        catch (Exception ex)
        {
            task.State = ProcessingState.Failed;
            task.ErrorMessage = ex.Message;
            task.CompletedAt = DateTime.UtcNow;
            batch.FailedTasks++;

            await _taskRepository.UpdateAsync(task);
            await _batchRepository.UpdateAsync(batch);

            // Report task failed progress
            progress?.Report(new BatchProgress
            {
                Total = totalTasks,
                Completed = batch.CompletedTasks,
                Failed = batch.FailedTasks,
                CurrentItem = taskIndex + 1,
                CurrentTaskUrl = task.Url
            });

            if (!batch.ContinueOnError)
                throw new BatchProcessingException(batch.Id, batch.FailedTasks, $"Batch job failed: {ex.Message}")
                {
                    BatchJobId = batch.Id,
                    FailedTaskCount = batch.FailedTasks
                };
        }
        finally
        {
            semaphore.Release();
        }
    }
}