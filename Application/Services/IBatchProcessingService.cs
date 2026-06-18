#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Models;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Service for batch job management and processing.
/// </summary>
public interface IBatchProcessingService
{
    /// <summary>
    /// Creates a new batch job.
    /// </summary>
    /// <param name="name">The name of the batch job.</param>
    /// <param name="outputDirectory">The output directory for the batch.</param>
    /// <param name="sharedSettings">Optional shared conversion settings.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="BatchJob"/> object.</returns>
    Task<BatchJob> CreateBatchJobAsync(string name, string outputDirectory, ConversionSettings? sharedSettings = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds tasks to a batch job.
    /// </summary>
    /// <param name="batchJobId">The ID of the batch job.</param>
    /// <param name="tasks">The collection of tasks to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddTasksAsync(string batchJobId, IEnumerable<DownloadTask> tasks, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts processing a batch job.
    /// </summary>
    /// <param name="batchJobId">The ID of the batch job.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated <see cref="BatchJob"/>.</returns>
    Task<BatchJob> StartBatchAsync(string batchJobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a batch job.
    /// </summary>
    /// <param name="batchJobId">The ID of the batch job.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task CancelBatchAsync(string batchJobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a batch job.
    /// </summary>
    /// <param name="batchJobId">The ID of the batch job.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="BatchJob"/> object representing the status.</returns>
    Task<BatchJob> GetBatchStatusAsync(string batchJobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all batch jobs.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of all batch jobs.</returns>
    Task<IEnumerable<BatchJob>> GetAllBatchesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active batch jobs.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of active batch jobs.</returns>
    Task<IEnumerable<BatchJob>> GetActiveBatchesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a completed batch job.
    /// </summary>
    /// <param name="batchJobId">The ID of the batch job.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if the batch job was deleted; otherwise, <c>false</c>.</returns>
    Task<bool> DeleteBatchAsync(string batchJobId, CancellationToken cancellationToken = default);
}
