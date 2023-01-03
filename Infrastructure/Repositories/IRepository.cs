// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Infrastructure.Repositories;

/// <summary>
/// Generic repository interface for CRUD operations.
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>Get entity by unique identifier</summary>
    Task<T?> GetByIdAsync(string id);

    /// <summary>Get all entities</summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>Create new entity</summary>
    Task<T> CreateAsync(T entity);

    /// <summary>Update existing entity</summary>
    Task<T> UpdateAsync(T entity);

    /// <summary>Delete entity by ID</summary>
    Task<bool> DeleteAsync(string id);

    /// <summary>Check if entity exists</summary>
    Task<bool> ExistsAsync(string id);
}

/// <summary>
/// Repository for DownloadTask entities with specialized queries.
/// </summary>
public interface IDownloadTaskRepository : IRepository<DownloadTask>
{
    /// <summary>Get tasks by video ID</summary>
    Task<IEnumerable<DownloadTask>> GetByVideoIdAsync(string videoId);

    /// <summary>Get tasks by processing state</summary>
    Task<IEnumerable<DownloadTask>> GetByStateAsync(ProcessingState state);

    /// <summary>Get tasks for batch job</summary>
    Task<IEnumerable<DownloadTask>> GetByBatchIdAsync(string batchJobId);

    /// <summary>Get pending tasks (not yet started)</summary>
    Task<IEnumerable<DownloadTask>> GetPendingTasksAsync();

    /// <summary>Get failed tasks that can be retried</summary>
    Task<IEnumerable<DownloadTask>> GetRetryableTasksAsync();

    /// <summary>Update task progress</summary>
    Task UpdateProgressAsync(string taskId, int progressPercent);

    /// <summary>Update task state</summary>
    Task UpdateStateAsync(string taskId, ProcessingState state);
}

/// <summary>
/// Repository for CoubVideo entities.
/// </summary>
public interface ICoubVideoRepository : IRepository<CoubVideo>
{
    /// <summary>Get video by Coub URL</summary>
    Task<CoubVideo?> GetByUrlAsync(string url);

    /// <summary>Get videos by creator name</summary>
    Task<IEnumerable<CoubVideo>> GetByCreatorAsync(string creatorName);

    /// <summary>Search videos by title</summary>
    Task<IEnumerable<CoubVideo>> SearchByTitleAsync(string searchTerm);

    /// <summary>Get videos within view count range</summary>
    Task<IEnumerable<CoubVideo>> GetByViewCountRangeAsync(long minViews, long maxViews);
}

/// <summary>
/// Repository for BatchJob entities.
/// </summary>
public interface IBatchJobRepository : IRepository<BatchJob>
{
    /// <summary>Get batch jobs by processing state</summary>
    Task<IEnumerable<BatchJob>> GetByStateAsync(ProcessingState state);

    /// <summary>Get recent batch jobs</summary>
    Task<IEnumerable<BatchJob>> GetRecentAsync(int count = 10);

    /// <summary>Get batch jobs by name pattern</summary>
    Task<IEnumerable<BatchJob>> SearchByNameAsync(string namePattern);

    /// <summary>Update batch completion status</summary>
    Task UpdateProgressAsync(string batchId, int completed, int failed);
}

/// <summary>
/// Repository for DownloadResult entities.
/// </summary>
public interface IDownloadResultRepository : IRepository<DownloadResult>
{
    /// <summary>Get results by task ID</summary>
    Task<DownloadResult?> GetByTaskIdAsync(string taskId);

    /// <summary>Get successful results</summary>
    Task<IEnumerable<DownloadResult>> GetSuccessfulResultsAsync();

    /// <summary>Get failed results</summary>
    Task<IEnumerable<DownloadResult>> GetFailedResultsAsync();

    /// <summary>Get results by processing time range</summary>
    Task<IEnumerable<DownloadResult>> GetByProcessingTimeRangeAsync(long minMs, long maxMs);
}
