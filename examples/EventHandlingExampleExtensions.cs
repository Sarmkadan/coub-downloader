using System;
using System.IO;
using System.Threading.Tasks;

namespace CoubDownloader.Examples
{
	/// <summary>
	/// Provides extension methods for <see cref="EventHandlingExample"/> to format and display event data.
	/// </summary>
	public static class EventHandlingExampleExtensions
	{
		/// <summary>
		/// Gets a formatted progress string for the download operation.
		/// </summary>
		/// <param name="example">The event handling example instance containing progress data.</param>
		/// <returns>A formatted progress string.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="example"/> is <see langword="null"/>.</exception>
		public static string GetProgressStatus(this EventHandlingExample example)
		{
			ArgumentNullException.ThrowIfNull(example);

			if (example.TotalBytes <= 0)
			{
				return "Waiting to start...";
			}

			if (example.ProgressPercent >= 100)
			{
				return "Download complete!";
			}

			var downloadedMb = Math.Round(example.DownloadedBytes / (1024.0 * 1024.0), 2);
			var totalMb = Math.Round(example.TotalBytes / (1024.0 * 1024.0), 2);
			var speed = example.ProgressPercent > 0 && example.Timestamp > DateTime.MinValue
				? $"({Math.Round((example.DownloadedBytes / 1024.0 / 1024.0) / ((DateTime.UtcNow - example.Timestamp).TotalSeconds + 1), 2)} MB/s)"
				: string.Empty;

			return $"Downloading: {example.ProgressPercent}% - {downloadedMb}/{totalMb} MB {speed}";
		}

		/// <summary>
		/// Creates a formatted filename based on the video title and quality.
		/// </summary>
		/// <param name="example">The event handling example instance containing video metadata.</param>
		/// <returns>A sanitized filename with timestamp.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="example"/> is <see langword="null"/>.</exception>
		public static string GetOutputFilename(this EventHandlingExample example)
		{
			ArgumentNullException.ThrowIfNull(example);

			var timestamp = DateTime.Now;
			return string.IsNullOrWhiteSpace(example.VideoTitle)
				? $"video_{example.Quality}_{timestamp:yyyyMMdd_HHmmss}.mp4"
				: $"{SanitizeFilename(example.VideoTitle)}_{example.Quality}_{timestamp:yyyyMMdd_HHmmss}.mp4";
		}

		/// <summary>
		/// Sanitizes a string to be safe for use as a filename.
		/// </summary>
		/// <param name="input">The input string to sanitize.</param>
		/// <returns>A sanitized string with invalid characters replaced.</returns>
		private static string SanitizeFilename(string input)
		{
			ArgumentException.ThrowIfNullOrEmpty(input);

			return Path.GetInvalidFileNameChars()
				.Aggregate(input, (current, c) => current.Replace(c, '_'));
		}

		/// <summary>
		/// Checks if the download operation has encountered an error.
		/// </summary>
		/// <param name="example">The event handling example instance to check.</param>
		/// <returns><see langword="true"/> if an error exists; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="example"/> is <see langword="null"/>.</exception>
		public static bool HasError(this EventHandlingExample example) =>
			ArgumentNullException.ThrowIfNull(example),
			example.Error is not null and not "";

		/// <summary>
		/// Gets a formatted duration string from the duration in milliseconds.
		/// </summary>
		/// <param name="example">The event handling example instance containing duration data.</param>
		/// <returns>A formatted duration string in HH:mm:ss or mm:ss format.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="example"/> is <see langword="null"/>.</exception>
		public static string GetFormattedDuration(this EventHandlingExample example)
		{
			ArgumentNullException.ThrowIfNull(example);

			return example.DurationMs <= 0
				? "Unknown duration"
				: TimeSpan.FromMilliseconds(example.DurationMs) is var timeSpan
					&& timeSpan.TotalHours >= 1
					? $"{timeSpan:hh\:mm\:ss}"
					: $"{timeSpan:mm\:ss}";
		}

		/// <summary>
		/// Gets a formatted size string for the downloaded file.
		/// </summary>
		/// <param name="example">The event handling example instance containing file size data.</param>
		/// <returns>A formatted size string with appropriate unit (GB, MB, or KB).</returns>
		/// <exception cref="ArgumentNullException"><paramref name="example"/> is <see langword="null"/>.</exception>
		public static string GetFormattedFileSize(this EventHandlingExample example)
		{
			ArgumentNullException.ThrowIfNull(example);

			if (example.FileSizeBytes <= 0)
			{
				return "Unknown size";
			}

			var sizeKb = example.FileSizeBytes / 1024.0;
			var sizeMb = sizeKb / 1024.0;
			var sizeGb = sizeMb / 1024.0;

			return sizeGb >= 1
				? $"{Math.Round(sizeGb, 2)} GB"
				: sizeMb >= 1
					? $"{Math.Round(sizeMb, 2)} MB"
					: $"{Math.Round(sizeKb, 2)} KB";
		}

		/// <summary>
		/// Gets a retry status message indicating how many retry attempts have been made.
		/// </summary>
		/// <param name="example">The event handling example instance containing retry data.</param>
		/// <returns>A status message indicating retry attempt or first attempt.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="example"/> is <see langword="null"/>.</exception>
		public static string GetRetryStatus(this EventHandlingExample example) =>
			ArgumentNullException.ThrowIfNull(example),
			example.RetryAttempt > 0
				? $"Retry #{example.RetryAttempt} at {example.Timestamp:HH:mm:ss}"
				: "First attempt";
	}
}
