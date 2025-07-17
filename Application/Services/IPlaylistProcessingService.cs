// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Models;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Service for fetching Coub playlists from channel or tag feeds and enqueuing
/// their videos as batch processing jobs.
/// </summary>
public interface IPlaylistProcessingService
{
    /// <summary>
    /// Fetch playlist metadata and discover all video URLs from a Coub channel or tag page.
    /// </summary>
    /// <param name="playlistUrl">
    /// Absolute URL of a Coub channel (e.g., <c>https://coub.com/funny</c>) or tag page
    /// (e.g., <c>https://coub.com/tags/cats</c>).
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Populated <see cref="CoubPlaylist"/> containing the discovered video URLs.</returns>
    Task<CoubPlaylist> FetchPlaylistAsync(string playlistUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetch a playlist from its URL and enqueue all videos as a single <see cref="BatchJob"/>.
    /// The returned batch is in <c>Pending</c> state — call
    /// <see cref="IBatchProcessingService.StartBatchAsync"/> when ready to begin downloading.
    /// </summary>
    /// <param name="playlistUrl">Absolute URL of a Coub channel or tag page.</param>
    /// <param name="outputDirectory">Directory where converted video files will be written.</param>
    /// <param name="settings">Optional shared conversion settings applied to every task in the batch.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Created <see cref="BatchJob"/> in Pending state.</returns>
    Task<BatchJob> QueuePlaylistAsync(
        string playlistUrl,
        string outputDirectory,
        ConversionSettings? settings = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enqueue all videos from a pre-fetched <see cref="CoubPlaylist"/> as a single <see cref="BatchJob"/>.
    /// The returned batch is in <c>Pending</c> state — call
    /// <see cref="IBatchProcessingService.StartBatchAsync"/> when ready to begin downloading.
    /// </summary>
    /// <param name="playlist">Pre-fetched playlist that must satisfy <see cref="CoubPlaylist.IsValid"/>.</param>
    /// <param name="outputDirectory">Directory where converted video files will be written.</param>
    /// <param name="settings">Optional shared conversion settings applied to every task in the batch.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Created <see cref="BatchJob"/> in Pending state.</returns>
    Task<BatchJob> QueuePlaylistAsync(
        CoubPlaylist playlist,
        string outputDirectory,
        ConversionSettings? settings = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve all currently active batch jobs, including those that originated from playlist processing.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Sequence of active <see cref="BatchJob"/> instances.</returns>
    Task<IEnumerable<BatchJob>> GetActivePlaylistJobsAsync(CancellationToken cancellationToken = default);
}
