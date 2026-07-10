using System;
using System.IO;
using System.Threading.Tasks;

namespace CoubDownloader.Examples
{
    public static class EventHandlingExampleExtensions
    {
        /// <summary>
        /// Gets a formatted progress string for the download operation.
        /// </summary>
        public static string GetProgressStatus(this EventHandlingExample example)
        {
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
                : "";

            return $"Downloading: {example.ProgressPercent}% - {downloadedMb}/{totalMb} MB {speed}";
        }

        /// <summary>
        /// Creates a formatted filename based on the video title and quality.
        /// </summary>
        public static string GetOutputFilename(this EventHandlingExample example)
        {
            if (string.IsNullOrWhiteSpace(example.VideoTitle))
            {
                return $"video_{example.Quality}_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
            }

            var safeTitle = Path.GetInvalidFileNameChars()
                .Aggregate(example.VideoTitle, (current, c) => current.Replace(c, '_'));

            return $"{safeTitle}_{example.Quality}_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
        }

        /// <summary>
        /// Checks if the download operation has encountered an error.
        /// </summary>
        public static bool HasError(this EventHandlingExample example)
        {
            return !string.IsNullOrEmpty(example.Error);
        }

        /// <summary>
        /// Gets a formatted duration string from the duration in milliseconds.
        /// </summary>
        public static string GetFormattedDuration(this EventHandlingExample example)
        {
            if (example.DurationMs <= 0)
            {
                return "Unknown duration";
            }

            var timeSpan = TimeSpan.FromMilliseconds(example.DurationMs);
            return timeSpan.TotalHours >= 1
                ? $"{timeSpan:hh\\:mm\\:ss}"
                : $"{timeSpan:mm\\:ss}";
        }

        /// <summary>
        /// Gets a formatted size string for the downloaded file.
        /// </summary>
        public static string GetFormattedFileSize(this EventHandlingExample example)
        {
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
        public static string GetRetryStatus(this EventHandlingExample example)
        {
            return example.RetryAttempt > 0
                ? $"Retry #{example.RetryAttempt} at {example.Timestamp:HH:mm:ss}"
                : "First attempt";
        }
    }
}