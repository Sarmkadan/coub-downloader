// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Models;

namespace CoubDownloader.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of ICoubVideoRepository.
/// </summary>
public class InMemoryCoubVideoRepository : ICoubVideoRepository
{
    private readonly Dictionary<string, CoubVideo> _videos = new();
    private readonly object _lock = new object();

    public Task<CoubVideo?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_videos.TryGetValue(id, out var video) ? video : null);
        }
    }

    public Task<IEnumerable<CoubVideo>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_videos.Values.AsEnumerable());
        }
    }

    public Task<CoubVideo> CreateAsync(CoubVideo entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Id))
            entity.Id = Guid.NewGuid().ToString();

        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        lock (_lock)
        {
            _videos[entity.Id] = entity;
        }

        return Task.FromResult(entity);
    }

    public Task<CoubVideo> UpdateAsync(CoubVideo entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;

        lock (_lock)
        {
            if (!_videos.ContainsKey(entity.Id))
                throw new KeyNotFoundException($"Video with ID {entity.Id} not found");

            _videos[entity.Id] = entity;
        }

        return Task.FromResult(entity);
    }

    public Task<bool> DeleteAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_videos.Remove(id));
        }
    }

    public Task<bool> ExistsAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_videos.ContainsKey(id));
        }
    }

    public Task<CoubVideo?> GetByUrlAsync(string url)
    {
        lock (_lock)
        {
            var video = _videos.Values.FirstOrDefault(v => v.Url == url);
            return Task.FromResult(video);
        }
    }

    public Task<IEnumerable<CoubVideo>> GetByCreatorAsync(string creatorName)
    {
        lock (_lock)
        {
            var videos = _videos.Values
                .Where(v => v.CreatorName != null && v.CreatorName.Contains(creatorName, StringComparison.OrdinalIgnoreCase))
                .AsEnumerable();
            return Task.FromResult(videos);
        }
    }

    public Task<IEnumerable<CoubVideo>> SearchByTitleAsync(string searchTerm)
    {
        lock (_lock)
        {
            var videos = _videos.Values
                .Where(v => v.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .AsEnumerable();
            return Task.FromResult(videos);
        }
    }

    public Task<IEnumerable<CoubVideo>> GetByViewCountRangeAsync(long minViews, long maxViews)
    {
        lock (_lock)
        {
            var videos = _videos.Values
                .Where(v => v.ViewCount >= minViews && v.ViewCount <= maxViews)
                .AsEnumerable();
            return Task.FromResult(videos);
        }
    }
}
