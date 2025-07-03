// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of IDownloadTaskRepository for development and testing.
/// </summary>
public class InMemoryDownloadTaskRepository : IDownloadTaskRepository
{
    private readonly Dictionary<string, DownloadTask> _tasks = new();
    private readonly object _lock = new object();

    public Task<DownloadTask?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_tasks.TryGetValue(id, out var task) ? task : null);
        }
    }

    public Task<IEnumerable<DownloadTask>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_tasks.Values.AsEnumerable());
        }
    }

    public Task<DownloadTask> CreateAsync(DownloadTask entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Id))
            entity.Id = Guid.NewGuid().ToString();

        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        lock (_lock)
        {
            _tasks[entity.Id] = entity;
        }

        return Task.FromResult(entity);
    }

    public Task<DownloadTask> UpdateAsync(DownloadTask entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;

        lock (_lock)
        {
            if (!_tasks.ContainsKey(entity.Id))
                throw new KeyNotFoundException($"Task with ID {entity.Id} not found");

            _tasks[entity.Id] = entity;
        }

        return Task.FromResult(entity);
    }

    public Task<bool> DeleteAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_tasks.Remove(id));
        }
    }

    public Task<bool> ExistsAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_tasks.ContainsKey(id));
        }
    }

    public Task<IEnumerable<DownloadTask>> GetByVideoIdAsync(string videoId)
    {
        lock (_lock)
        {
            var tasks = _tasks.Values.Where(t => t.VideoId == videoId).AsEnumerable();
            return Task.FromResult(tasks);
        }
    }

    public Task<IEnumerable<DownloadTask>> GetByStateAsync(ProcessingState state)
    {
        lock (_lock)
        {
            var tasks = _tasks.Values.Where(t => t.State == state).AsEnumerable();
            return Task.FromResult(tasks);
        }
    }

    public Task<IEnumerable<DownloadTask>> GetByBatchIdAsync(string batchJobId)
    {
        lock (_lock)
        {
            var tasks = _tasks.Values.Where(t => t.BatchJobId == batchJobId).AsEnumerable();
            return Task.FromResult(tasks);
        }
    }

    public Task<IEnumerable<DownloadTask>> GetPendingTasksAsync()
    {
        lock (_lock)
        {
            var tasks = _tasks.Values.Where(t => t.State == ProcessingState.Pending).AsEnumerable();
            return Task.FromResult(tasks);
        }
    }

    public Task<IEnumerable<DownloadTask>> GetRetryableTasksAsync()
    {
        lock (_lock)
        {
            var tasks = _tasks.Values.Where(t => t.CanRetry()).AsEnumerable();
            return Task.FromResult(tasks);
        }
    }

    public Task UpdateProgressAsync(string taskId, int progressPercent)
    {
        lock (_lock)
        {
            if (_tasks.TryGetValue(taskId, out var task))
            {
                task.ProgressPercent = Math.Clamp(progressPercent, 0, 100);
                task.UpdatedAt = DateTime.UtcNow;
            }
        }
        return Task.CompletedTask;
    }

    public Task UpdateStateAsync(string taskId, ProcessingState state)
    {
        lock (_lock)
        {
            if (_tasks.TryGetValue(taskId, out var task))
            {
                task.State = state;
                task.UpdatedAt = DateTime.UtcNow;

                if (state == ProcessingState.Downloading && task.StartedAt is null)
                    task.StartedAt = DateTime.UtcNow;

                if (state is ProcessingState.Completed or ProcessingState.Failed or ProcessingState.Cancelled)
                    task.CompletedAt = DateTime.UtcNow;
            }
        }
        return Task.CompletedTask;
    }
}
