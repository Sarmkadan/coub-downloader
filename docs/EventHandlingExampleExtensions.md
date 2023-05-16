# EventHandlingExampleExtensions

Static extension methods that simplify formatting and inspection of event data raised by the `EventHandlingExample` class in the coub‑downloader project. These helpers convert raw event arguments into user‑friendly strings or boolean flags, making it easier to update UI components or log progress without repeating formatting logic.

## API

### GetProgressStatus
- **Purpose:** Produces a human‑readable string that reflects the current download progress (e.g., “Downloading: 57 %”).
- **Parameters:** `progress` – the progress event data supplied by `EventHandlingExample`.
- **Return Value:** A formatted status string; returns an empty string if no meaningful progress can be derived.
- **Exceptions:** Throws `ArgumentNullException` when `progress` is `null`.

### GetOutputFilename
- **Purpose:** Retrieves the file name that will be used for the downloaded output.
- **Parameters:** `fileReady` – the file‑ready event data supplied by `EventHandlingExample`.
- **Return Value:** The output file name as a string; returns `null` if the file name is not available.
- **Exceptions:** Throws `ArgumentNullException` when `fileReady` is `null`.

### HasError
- **Purpose:** Indicates whether an error occurred during the operation associated with the event.
- **Parameters:** `error` – the error event data supplied by `EventHandlingExample`.
- **Return Value:** `true` if the event contains error information; otherwise `false`.
- **Exceptions:** Throws `ArgumentNullException` when `error` is `null`.

### GetFormattedDuration
- **Purpose:** Formats a time span into a concise string (e.g., “02:15:07” for hours:minutes:seconds).
- **Parameters:** `duration` – a `System.TimeSpan` representing the elapsed or remaining time.
- **Return Value:** A formatted duration string.
- **Exceptions:** Throws `ArgumentOutOfRangeException` if `duration` is negative.

### GetFormattedFileSize
- **Purpose:** Converts a raw byte count into a readable size string with appropriate units (B, KB, MB, GB).
- **Parameters:** `bytes` – the file size in bytes.
- **Return Value:** A formatted file size string.
- **Exceptions:** Throws `ArgumentOutOfRangeException` if `bytes` is less than zero.

### GetRetryStatus
- **Purpose:** Generates a string describing the current retry attempt (e.g., “Retrying (3/5)”).
- **Parameters:** 
  - `retryCount` – the number of retries already performed.
  - `maxRetries` – the maximum number of retries allowed.
- **Return Value:** A descriptive retry status string; returns an empty string when no retry is in progress.
- **Exceptions:** Throws `ArgumentOutOfRangeException` if `retryCount` is negative or `maxRetries` is zero or negative.

## Usage

```csharp
using CoubDownloader.Events; // namespace containing EventHandlingExample and its args

public class DownloadViewModel
{
    public void Subscribe(EventHandlingExample example)
    {
        example.ProgressChanged += (s, e) =>
        {
            // Update a UI label with a friendly progress string.
            ProgressLabel.Text = EventHandlingExampleExtensions.GetProgressStatus(e);
        };

        example.FileReady += (s, e) =>
        {
            // Show the eventual output file name.
            OutputFileName.Text = EventHandlingExampleExtensions.GetOutputFilename(e) ?? "(unknown)";
        };

        example.ErrorOccurred += (s, e) =>
        {
            if (EventHandlingExampleExtensions.HasError(e))
            {
                ErrorMessage.Text = "An error occurred during download.";
            }
        };
    }
}
```

```csharp
using System;
using System.Threading.Tasks;
using CoubDownloader.Events;

public class ConsoleDownloader
{
    public async Task RunAsync(EventHandlingExample example)
    {
        example.ProgressChanged += (s, e) =>
        {
            var status = EventHandlingExampleExtensions.GetProgressStatus(e);
            var elapsed = EventHandlingExampleExtensions.GetFormattedDuration(e.Elapsed);
            Console.Write($"\r{status} | Elapsed: {elapsed}");
        };

        example.FileReady += (s, e) =>
        {
            var fileName = EventHandlingExampleExtensions.GetOutputFilename(e);
            var size = EventHandlingExampleExtensions.GetFormattedFileSize(e.FileSize);
            Console.WriteLine($"\nSaved to {fileName} ({size})");
        };

        example.Retrying += (s, e) =>
        {
            var retryInfo = EventHandlingExampleExtensions.GetRetryStatus(e.RetryCount, e.MaxRetries);
            Console.WriteLine($"\n{RetryInfo}");
        };

        await example.StartAsync();
    }
}
```

## Notes

- All extension methods are **pure**: they depend solely on their input parameters and do not access any static or instance state. Consequently, they are thread‑safe with respect to concurrent calls, provided the arguments themselves are not mutated by other threads.
- Passing `null` for any event‑argument parameter results in an `ArgumentNullException`. Callers should guard against this when subscribing to events that may be raised with incomplete data.
- The formatting methods (`GetFormattedDuration`, `GetFormattedFileSize`, `GetRetryStatus`) return sensible default strings (empty or placeholder) when the input values are outside the expected range, rather than throwing, except for explicitly invalid negatives which trigger `ArgumentOutOfRangeException`.
- These helpers do not perform any I/O, logging, or UI updates themselves; the caller remains responsible for marshaling results to the appropriate thread (e.g., invoking UI updates on the UI thread in WPF or WinForms applications).
