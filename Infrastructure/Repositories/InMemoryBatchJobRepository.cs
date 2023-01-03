// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of IBatchJobRepository.
/// </summary>
public class InMemoryBatchJobRepository : IBatchJobRepository
{
    private readonly Dictionary<string, BatchJob> _batches = new();
    private readonly object _lock = new object();

    public Task<BatchJob?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_batches.TryGetValue(id, out var batch) ? batch : null);
        }
    }

    public Task<IEnumerable<BatchJob>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_batches.Values.AsEnumerable());
        }
    }

    public Task<BatchJob> CreateAsync(BatchJob entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Id))
            entity.Id = Guid.NewGuid().ToString();

        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        lock (_lock)
        {
            _batches[entity.Id] = entity;
        }

        return Task.FromResult(entity);
    }

    public Task<BatchJob> UpdateAsync(BatchJob entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;

        lock (_lock)
        {
            if (!_batches.ContainsKey(entity.Id))
                throw new KeyNotFoundException($"Batch with ID {entity.Id} not found");

            _batches[entity.Id] = entity;
        }

        return Task.FromResult(entity);
    }

    public Task<bool> DeleteAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_batches.Remove(id));
        }
    }

    public Task<bool> ExistsAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_batches.ContainsKey(id));
        }
    }

    public Task<IEnumerable<BatchJob>> GetByStateAsync(ProcessingState state)
    {
        lock (_lock)
        {
            var batches = _batches.Values.Where(b => b.State == state).AsEnumerable();
            return Task.FromResult(batches);
        }
    }

    public Task<IEnumerable<BatchJob>> GetRecentAsync(int count = 10)
    {
        lock (_lock)
        {
            var batches = _batches.Values
                .OrderByDescending(b => b.CreatedAt)
                .Take(count)
                .AsEnumerable();
            return Task.FromResult(batches);
        }
    }

    public Task<IEnumerable<BatchJob>> SearchByNameAsync(string namePattern)
    {
        lock (_lock)
        {
            var batches = _batches.Values
                .Where(b => b.Name.Contains(namePattern, StringComparison.OrdinalIgnoreCase))
                .AsEnumerable();
            return Task.FromResult(batches);
        }
    }

    public Task UpdateProgressAsync(string batchId, int completed, int failed)
    {
        lock (_lock)
        {
            if (_batches.TryGetValue(batchId, out var batch))
            {
                batch.CompletedTasks = completed;
                batch.FailedTasks = failed;
                batch.UpdatedAt = DateTime.UtcNow;

                if (batch.IsCompleted())
                {
                    batch.State = batch.FailedTasks == 0 ? ProcessingState.Completed : ProcessingState.Failed;
                    batch.CompletedAt = DateTime.UtcNow;
                }
            }
        }
        return Task.CompletedTask;
    }
}
