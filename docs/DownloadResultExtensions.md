# DownloadResultExtensions

Extension methods for `DownloadResult` that provide common operations for inspecting, formatting, and validating download results.

## API

### `bool IsSuccessfulWithFile(DownloadResult result)`

Determines whether the download operation completed successfully and produced a local file.

- **Parameters**
  - `result` – The `DownloadResult` instance to evaluate.
- **Return value**
  - `true` if `result.Status` is `DownloadStatus.Success` and `result.FilePath` is not `null` or empty; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `result` is `null`.

---

### `string GetFormattedFileInfo(DownloadResult result)`

Returns a human-readable string summarizing the downloaded file’s path and size.

- **Parameters**
  - `result` – The `DownloadResult` instance to format.
- **Return value**
  - A string in the format `"<filePath> (<fileSize>)"`, where `<fileSize>` is formatted in bytes, KB, MB, or GB as appropriate.
- **Exceptions**
  - Throws `ArgumentNullException` if `result` is `null`.
  - Throws `InvalidOperationException` if `result.FilePath` is `null` or empty.

---

### `DownloadResult Clone(DownloadResult result)`

Creates a deep copy of the given `DownloadResult` instance.

- **Parameters**
  - `result` – The `DownloadResult` instance to clone.
- **Return value**
  - A new `DownloadResult` with all properties copied from `result`.
- **Exceptions**
  - Throws `ArgumentNullException` if `result` is `null`.

---

### `bool ExceededProcessingTime(DownloadResult result, TimeSpan threshold)`

Checks whether the download processing time exceeded the specified threshold.

- **Parameters**
  - `result` – The `DownloadResult` instance to evaluate.
  - `threshold` – The maximum allowed processing duration.
- **Return value**
  - `true` if `result.ProcessingTime` is greater than `threshold`; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `result` is `null`.

---

### `string FormatProcessingTime(DownloadResult result)`

Formats the processing time of the download into a human-readable string.

- **Parameters**
  - `result` – The `DownloadResult` instance to format.
- **Return value**
  - A string representing the processing time in the format `"HH:mm:ss.fff"`.
- **Exceptions**
  - Throws `ArgumentNullException` if `result` is `null`.
  - Throws `InvalidOperationException` if `result.ProcessingTime` is negative.

---
### `bool HasCriticalError(DownloadResult result)`

Determines whether the download encountered a critical error.

- **Parameters**
  - `result` – The `DownloadResult` instance to evaluate.
- **Return value**
  - `true` if `result.Status` is `DownloadStatus.CriticalError`; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `result` is `null`.

---
### `string GetWarningsSummary(DownloadResult result)`

Returns a summary of all non-critical warnings associated with the download.

- **Parameters**
  - `result` – The `DownloadResult` instance to evaluate.
- **Return value**
  - A string containing all warnings joined by semicolons, or an empty string if there are no warnings.
- **Exceptions**
  - Throws `ArgumentNullException` if `result` is `null`.

---
### `bool IsFileSizeWithinBounds(DownloadResult result, long minSize, long maxSize)`

Checks whether the downloaded file’s size falls within the specified bounds.

- **Parameters**
  - `result` – The `DownloadResult` instance to evaluate.
  - `minSize` – The minimum allowed file size in bytes.
  - `maxSize` – The maximum allowed file size in bytes.
- **Return value**
  - `true` if `result.FileSize` is between `minSize` and `maxSize` (inclusive); otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `result` is `null`.
  - Throws `ArgumentOutOfRangeException` if `minSize` is negative or `maxSize` is less than `minSize`.

---
### `string GetStatusEmoji(DownloadResult result)`

Returns an emoji representing the status of the download.

- **Parameters**
  - `result` – The `DownloadResult` instance to evaluate.
- **Return value**
  - A string emoji based on `result.Status`:
    - `"✅"` for `DownloadStatus.Success`
    - `"⚠️"` for `DownloadStatus.Warning`
    - `"❌"` for `DownloadStatus.CriticalError`
    - `"⏳"` for `DownloadStatus.InProgress`
- **Exceptions**
  - Throws `ArgumentNullException` if `result` is `null`.

## Usage
