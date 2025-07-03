// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Models;

namespace CoubDownloader.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of IDownloadResultRepository.
/// </summary>
public class InMemoryDownloadResultRepository : IDownloadResultRepository
{
    private readonly Dictionary<string, DownloadResult> _results = new();
    private readonly object _lock = new object();

    public Task<DownloadResult?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_results.TryGetValue(id, out var result) ? result : null);
        }
    }

    public Task<IEnumerable<DownloadResult>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_results.Values.AsEnumerable());
        }
    }

    public Task<DownloadResult> CreateAsync(DownloadResult entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Id))
            entity.Id = Guid.NewGuid().ToString();

        entity.CompletedAt = DateTime.UtcNow;

        lock (_lock)
        {
            _results[entity.Id] = entity;
        }

        return Task.FromResult(entity);
    }

    public Task<DownloadResult> UpdateAsync(DownloadResult entity)
    {
        lock (_lock)
        {
            if (!_results.ContainsKey(entity.Id))
                throw new KeyNotFoundException($"Result with ID {entity.Id} not found");

            _results[entity.Id] = entity;
        }

        return Task.FromResult(entity);
    }

    public Task<bool> DeleteAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_results.Remove(id));
        }
    }

    public Task<bool> ExistsAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_results.ContainsKey(id));
        }
    }

    public Task<DownloadResult?> GetByTaskIdAsync(string taskId)
    {
        lock (_lock)
        {
            var result = _results.Values.FirstOrDefault(r => r.TaskId == taskId);
            return Task.FromResult(result);
        }
    }

    public Task<IEnumerable<DownloadResult>> GetSuccessfulResultsAsync()
    {
        lock (_lock)
        {
            var results = _results.Values.Where(r => r.Success).AsEnumerable();
            return Task.FromResult(results);
        }
    }

    public Task<IEnumerable<DownloadResult>> GetFailedResultsAsync()
    {
        lock (_lock)
        {
            var results = _results.Values.Where(r => !r.Success).AsEnumerable();
            return Task.FromResult(results);
        }
    }

    public Task<IEnumerable<DownloadResult>> GetByProcessingTimeRangeAsync(long minMs, long maxMs)
    {
        lock (_lock)
        {
            var results = _results.Values
                .Where(r => r.ProcessingTimeMs >= minMs && r.ProcessingTimeMs <= maxMs)
                .AsEnumerable();
            return Task.FromResult(results);
        }
    }
}
