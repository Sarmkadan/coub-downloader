#nullable enable
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
    // Primary storage indexed by video Id
    private readonly Dictionary<string, CoubVideo> _videos = new();

    // Secondary indexes
    private readonly Dictionary<string, string> _urlToId = new();                     // url -> id
    private readonly Dictionary<string, HashSet<string>> _creatorToIds = new();       // creator name (lowercase) -> set of ids
    private readonly Dictionary<string, HashSet<string>> _titleToIds = new();         // title (lowercase) -> set of ids

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
            IndexEntity(entity);
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

            // Remove old indexes
            var old = _videos[entity.Id];
            DeindexEntity(old);

            // Store updated entity and re‑index
            _videos[entity.Id] = entity;
            IndexEntity(entity);
        }

        return Task.FromResult(entity);
    }

    public Task<bool> DeleteAsync(string id)
    {
        lock (_lock)
        {
            if (_videos.TryGetValue(id, out var video))
            {
                DeindexEntity(video);
                _videos.Remove(id);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
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
            if (_urlToId.TryGetValue(url, out var id) && _videos.TryGetValue(id, out var video))
                return Task.FromResult<CoubVideo?>(video);

            return Task.FromResult<CoubVideo?>(null);
        }
    }

    public Task<IEnumerable<CoubVideo>> GetByCreatorAsync(string creatorName)
    {
        lock (_lock)
        {
            var key = creatorName?.ToLowerInvariant() ?? string.Empty;
            if (_creatorToIds.TryGetValue(key, out var ids))
            {
                var videos = ids.Select(id => _videos[id]);
                return Task.FromResult(videos);
            }

            return Task.FromResult(Enumerable.Empty<CoubVideo>());
        }
    }

    public Task<IEnumerable<CoubVideo>> SearchByTitleAsync(string searchTerm)
    {
        lock (_lock)
        {
            var term = searchTerm?.ToLowerInvariant() ?? string.Empty;
            var matchingIds = _titleToIds
                .Where(kvp => kvp.Key.Contains(term, StringComparison.OrdinalIgnoreCase))
                .SelectMany(kvp => kvp.Value)
                .Distinct();

            var videos = matchingIds.Select(id => _videos[id]);
            return Task.FromResult(videos);
        }
    }

    public Task<IEnumerable<CoubVideo>> GetByViewCountRangeAsync(long minViews, long maxViews)
    {
        lock (_lock)
        {
            var videos = _videos.Values
                .Where(v => v.ViewCount >= minViews && v.ViewCount <= maxViews);
            return Task.FromResult(videos);
        }
    }

    #region Index management

    private void IndexEntity(CoubVideo video)
    {
        // Index by URL
        if (!string.IsNullOrWhiteSpace(video.Url))
            _urlToId[video.Url] = video.Id;

        // Index by creator name (case‑insensitive)
        if (!string.IsNullOrWhiteSpace(video.CreatorName))
        {
            var key = video.CreatorName.ToLowerInvariant();
            if (!_creatorToIds.TryGetValue(key, out var set))
            {
                set = new HashSet<string>();
                _creatorToIds[key] = set;
            }
            set.Add(video.Id);
        }

        // Index by title (case‑insensitive)
        if (!string.IsNullOrWhiteSpace(video.Title))
        {
            var key = video.Title.ToLowerInvariant();
            if (!_titleToIds.TryGetValue(key, out var set))
            {
                set = new HashSet<string>();
                _titleToIds[key] = set;
            }
            set.Add(video.Id);
        }
    }

    private void DeindexEntity(CoubVideo video)
    {
        // Remove URL index
        if (!string.IsNullOrWhiteSpace(video.Url))
            _urlToId.Remove(video.Url);

        // Remove creator index
        if (!string.IsNullOrWhiteSpace(video.CreatorName))
        {
            var key = video.CreatorName.ToLowerInvariant();
            if (_creatorToIds.TryGetValue(key, out var set))
            {
                set.Remove(video.Id);
                if (set.Count == 0)
                    _creatorToIds.Remove(key);
            }
        }

        // Remove title index
        if (!string.IsNullOrWhiteSpace(video.Title))
        {
            var key = video.Title.ToLowerInvariant();
            if (_titleToIds.TryGetValue(key, out var set))
            {
                set.Remove(video.Id);
                if (set.Count == 0)
                    _titleToIds.Remove(key);
            }
        }
    }

    #endregion
}
