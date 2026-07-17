#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Infrastructure.Repositories;

/// <summary>
/// Provides validation helpers for <see cref="InMemoryDownloadTaskRepository"/> instances.
/// </summary>
public static class InMemoryDownloadTaskRepositoryValidation
{
    /// <summary>
    /// Validates the repository instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The repository instance to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this InMemoryDownloadTaskRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // In-memory repositories don't have persistent state to validate,
        // but we can verify the internal dictionary isn't corrupted
        try
        {
            _ = value.GetAllAsync().Result;
        }
        catch (Exception ex)
        {
            problems.Add($"Repository internal state is corrupted: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the repository instance and returns whether it is valid.
    /// </summary>
    /// <param name="value">The repository instance to validate</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static bool IsValid(this InMemoryDownloadTaskRepository value) => Validate(value).Count == 0;

    /// <summary>
    /// Validates the repository instance and throws an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if invalid.
    /// </summary>
    /// <param name="value">The repository instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails with problem details</exception>
    public static void EnsureValid(this InMemoryDownloadTaskRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Repository validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Validates a <see cref="DownloadTask"/> entity for repository operations.
    /// </summary>
    /// <param name="entity">The entity to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if entity is null</exception>
    public static IReadOnlyList<string> Validate(this DownloadTask entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var problems = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(entity.Id))
        {
            problems.Add("Id is null or whitespace");
        }
        else if (entity.Id == Guid.Empty.ToString())
        {
            problems.Add("Id is empty Guid string");
        }

        if (string.IsNullOrWhiteSpace(entity.VideoId))
        {
            problems.Add("VideoId is null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(entity.Url))
        {
            problems.Add("Url is null or whitespace");
        }
        else if (!Uri.TryCreate(entity.Url, UriKind.Absolute, out _))
        {
            problems.Add("Url is not a valid absolute URI");
        }

        if (string.IsNullOrWhiteSpace(entity.OutputPath))
        {
            problems.Add("OutputPath is null or whitespace");
        }

        // Validate numeric ranges
        if (entity.ProgressPercent < 0 || entity.ProgressPercent > 100)
        {
            problems.Add("ProgressPercent must be between 0 and 100");
        }

        if (entity.FileSizeBytes < 0)
        {
            problems.Add("FileSizeBytes must be non-negative");
        }

        if (entity.RetryCount < 0)
        {
            problems.Add("RetryCount must be non-negative");
        }

        if (entity.MaxRetries < 1 || entity.MaxRetries > 10)
        {
            problems.Add("MaxRetries must be between 1 and 10");
        }

        if (entity.RetryCount > entity.MaxRetries)
        {
            problems.Add("RetryCount cannot exceed MaxRetries");
        }

        // Validate timestamps
        if (entity.CreatedAt == default)
        {
            problems.Add("CreatedAt is default DateTime");
        }
        else if (entity.CreatedAt.Kind != DateTimeKind.Utc)
        {
            problems.Add("CreatedAt must be in UTC");
        }

        if (entity.UpdatedAt == default)
        {
            problems.Add("UpdatedAt is default DateTime");
        }
        else if (entity.UpdatedAt.Kind != DateTimeKind.Utc)
        {
            problems.Add("UpdatedAt must be in UTC");
        }

        if (entity.StartedAt.HasValue)
        {
            if (entity.StartedAt.Value == default)
            {
                problems.Add("StartedAt is default DateTime");
            }
            else if (entity.StartedAt.Value.Kind != DateTimeKind.Utc)
            {
                problems.Add("StartedAt must be in UTC");
            }
            else if (entity.StartedAt.Value < entity.CreatedAt)
            {
                problems.Add("StartedAt cannot be before CreatedAt");
            }
        }

        if (entity.CompletedAt.HasValue)
        {
            if (entity.CompletedAt.Value == default)
            {
                problems.Add("CompletedAt is default DateTime");
            }
            else if (entity.CompletedAt.Value.Kind != DateTimeKind.Utc)
            {
                problems.Add("CompletedAt must be in UTC");
            }
            else if (entity.StartedAt.HasValue && entity.CompletedAt.Value < entity.StartedAt.Value)
            {
                problems.Add("CompletedAt cannot be before StartedAt");
            }
        }

        // Validate state transitions
        if (entity.CompletedAt.HasValue && entity.State != ProcessingState.Completed
            && entity.State != ProcessingState.Failed && entity.State != ProcessingState.Cancelled)
        {
            problems.Add("CompletedAt is set but State is not Completed, Failed, or Cancelled");
        }

        if (entity.StartedAt.HasValue && entity.State != ProcessingState.Downloading
            && entity.State != ProcessingState.Converting && entity.State != ProcessingState.ProcessingAudio
            && entity.State != ProcessingState.Completed)
        {
            problems.Add("StartedAt is set but State is not a running state");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="DownloadTask"/> entity and returns whether it is valid.
    /// </summary>
    /// <param name="entity">The entity to validate</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if entity is null</exception>
    public static bool IsValid(this DownloadTask entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return Validate(entity).Count == 0;
    }

    /// <summary>
    /// Validates a <see cref="DownloadTask"/> entity and throws an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if invalid.
    /// </summary>
    /// <param name="entity">The entity to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if entity is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails with problem details</exception>
    public static void EnsureValid(this DownloadTask entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var problems = Validate(entity);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DownloadTask validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}