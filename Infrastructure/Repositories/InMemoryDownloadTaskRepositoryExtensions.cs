#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Infrastructure.Repositories;

/// <summary>
/// Extension methods for <see cref="IDownloadTaskRepository"/> providing additional
/// query and batch operations for download task management.
/// </summary>
public static class InMemoryDownloadTaskRepositoryExtensions
{
    /// <summary>
    /// Gets all tasks that match any of the specified video IDs.
    /// </summary>
    /// <param name="repository">The repository instance. Cannot be <see langword="null"/></param>
    /// <param name="videoIds">Collection of video IDs to filter by. Cannot be <see langword="null"/></param>
    /// <returns>Collection of matching download tasks</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="videoIds"/> is <see langword="null"/></exception>
    public static async Task<IEnumerable<DownloadTask>> GetByVideoIdsAsync(
        this IDownloadTaskRepository repository,
        IEnumerable<string> videoIds)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(videoIds);

        if (!videoIds.Any())
            return Enumerable.Empty<DownloadTask>();

        var tasks = new List<DownloadTask>();
        foreach (var videoId in videoIds)
        {
            var task = await repository.GetByVideoIdAsync(videoId);
            if (task is not null)
                tasks.AddRange(task);
        }

        return tasks;
    }

    /// <summary>
    /// Gets all tasks that match any of the specified batch job IDs.
    /// </summary>
    /// <param name="repository">The repository instance. Cannot be <see langword="null"/></param>
    /// <param name="batchJobIds">Collection of batch job IDs to filter by. Cannot be <see langword="null"/></param>
    /// <returns>Collection of matching download tasks</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="batchJobIds"/> is <see langword="null"/></exception>
    public static async Task<IEnumerable<DownloadTask>> GetByBatchJobIdsAsync(
        this IDownloadTaskRepository repository,
        IEnumerable<string> batchJobIds)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(batchJobIds);

        if (!batchJobIds.Any())
            return Enumerable.Empty<DownloadTask>();

        var tasks = new List<DownloadTask>();
        foreach (var batchJobId in batchJobIds)
        {
            var task = await repository.GetByBatchIdAsync(batchJobId);
            if (task is not null)
                tasks.AddRange(task);
        }

        return tasks;
    }

    /// <summary>
    /// Gets all tasks that match any of the specified states.
    /// </summary>
    /// <param name="repository">The repository instance. Cannot be <see langword="null"/></param>
    /// <param name="states">Collection of states to filter by. Cannot be <see langword="null"/></param>
    /// <returns>Collection of matching download tasks</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="states"/> is <see langword="null"/></exception>
    public static async Task<IEnumerable<DownloadTask>> GetByStatesAsync(
        this IDownloadTaskRepository repository,
        IEnumerable<ProcessingState> states)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(states);

        if (!states.Any())
            return Enumerable.Empty<DownloadTask>();

        var allTasks = await repository.GetAllAsync();
        return allTasks.Where(t => states.Contains(t.State));
    }

    /// <summary>
    /// Updates the state of multiple tasks atomically.
    /// </summary>
    /// <param name="repository">The repository instance. Cannot be <see langword="null"/></param>
    /// <param name="taskIds">Collection of task IDs to update. Cannot be <see langword="null"/></param>
    /// <param name="newState">New state to set</param>
    /// <param name="updateProgress">Whether to update progress to 100% for completed states</param>
    /// <returns>Number of successfully updated tasks</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="taskIds"/> is <see langword="null"/></exception>
    public static async Task<int> UpdateStatesAsync(
        this IDownloadTaskRepository repository,
        IEnumerable<string> taskIds,
        ProcessingState newState,
        bool updateProgress = false)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(taskIds);

        if (!taskIds.Any())
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