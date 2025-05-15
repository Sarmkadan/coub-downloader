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
    /// <summary>Create a new batch job</summary>
    Task<BatchJob> CreateBatchJobAsync(string name, string outputDirectory, ConversionSettings? sharedSettings = null, CancellationToken cancellationToken = default);

    /// <summary>Add tasks to a batch job</summary>
    Task AddTasksAsync(string batchJobId, IEnumerable<DownloadTask> tasks, CancellationToken cancellationToken = default);

    /// <summary>Start processing a batch job</summary>
    Task<BatchJob> StartBatchAsync(string batchJobId, CancellationToken cancellationToken = default);

    /// <summary>Cancel a batch job</summary>
    Task CancelBatchAsync(string batchJobId, CancellationToken cancellationToken = default);

    /// <summary>Get batch job status</summary>
    Task<BatchJob> GetBatchStatusAsync(string batchJobId, CancellationToken cancellationToken = default);

    /// <summary>Get all batch jobs</summary>
    Task<IEnumerable<BatchJob>> GetAllBatchesAsync(CancellationToken cancellationToken = default);

    /// <summary>Get active batch jobs</summary>
    Task<IEnumerable<BatchJob>> GetActiveBatchesAsync(CancellationToken cancellationToken = default);

    /// <summary>Delete a completed batch job</summary>
    Task<bool> DeleteBatchAsync(string batchJobId, CancellationToken cancellationToken = default);
}
