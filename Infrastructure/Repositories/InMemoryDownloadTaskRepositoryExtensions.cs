#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Infrastructure.Repositories;

/// <summary>
/// Extension methods for InMemoryDownloadTaskRepository providing additional
/// query and batch operations for download task management.
/// </summary>
public static class InMemoryDownloadTaskRepositoryExtensions
{
    /// <summary>
    /// Gets all tasks that match any of the specified video IDs.
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="videoIds">Collection of video IDs to filter by</param>
    /// <returns>Collection of matching download tasks</returns>
    public static async Task<IEnumerable<DownloadTask>> GetByVideoIdsAsync(
        this IDownloadTaskRepository repository,
        IEnumerable<string> videoIds)
    {
        if (videoIds == null || !videoIds.Any())
            return Enumerable.Empty<DownloadTask>();

        var tasks = new List<DownloadTask>();
        foreach (var videoId in videoIds)
        {
            var task = await repository.GetByVideoIdAsync(videoId);
            if (task != null)
                tasks.AddRange(task);
        }

        return tasks;
    }

    /// <summary>
    /// Gets all tasks that match any of the specified batch job IDs.
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="batchJobIds">Collection of batch job IDs to filter by</param>
    /// <returns>Collection of matching download tasks</returns>
    public static async Task<IEnumerable<DownloadTask>> GetByBatchJobIdsAsync(
        this IDownloadTaskRepository repository,
        IEnumerable<string> batchJobIds)
    {
        if (batchJobIds == null || !batchJobIds.Any())
            return Enumerable.Empty<DownloadTask>();

        var tasks = new List<DownloadTask>();
        foreach (var batchJobId in batchJobIds)
        {
            var task = await repository.GetByBatchIdAsync(batchJobId);
            if (task != null)
                tasks.AddRange(task);
        }

        return tasks;
    }

    /// <summary>
    /// Gets all tasks that match any of the specified states.
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="states">Collection of states to filter by</param>
    /// <returns>Collection of matching download tasks</returns>
    public static async Task<IEnumerable<DownloadTask>> GetByStatesAsync(
        this IDownloadTaskRepository repository,
        IEnumerable<ProcessingState> states)
    {
        if (states == null || !states.Any())
            return Enumerable.Empty<DownloadTask>();

        var allTasks = await repository.GetAllAsync();
        return allTasks.Where(t => states.Contains(t.State));
    }

    /// <summary>
    /// Updates the state of multiple tasks atomically.
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="taskIds">Collection of task IDs to update</param>
    /// <param name="newState">New state to set</param>
    /// <param name="updateProgress">Whether to update progress to 100% for completed states</param>
    /// <returns>Number of successfully updated tasks</returns>
    public static async Task<int> UpdateStatesAsync(
        this IDownloadTaskRepository repository,
        IEnumerable<string> taskIds,
        ProcessingState newState,
        bool updateProgress = false)
    {
        if (taskIds == null || !taskIds.Any())
            return 0;

        var updatedCount = 0;
        foreach (var taskId in taskIds)
        {
            var exists = await repository.ExistsAsync(taskId);
            if (exists)
            {
                await repository.UpdateStateAsync(taskId, newState);
                if (updateProgress &&
                    (newState == ProcessingState.Completed ||
                     newState == ProcessingState.Failed ||
                     newState == ProcessingState.Cancelled))
                {
                    await repository.UpdateProgressAsync(taskId, 100);
                }
                updatedCount++;
            }
        }

        return updatedCount;
    }
}