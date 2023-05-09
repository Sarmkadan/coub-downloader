# CoubDownloadService

A service class responsible for downloading Coub videos and their associated metadata from the Coub platform. It handles fetching video information, validating download sources, and saving video files locally.

## API

### `public CoubDownloadService()`

Initializes a new instance of the `CoubDownloadService` with default configuration. The service is designed to be reused for multiple download operations.

### `public async Task<CoubVideo> DownloadVideoAsync()`

Downloads a Coub video including its metadata and source files. Combines metadata fetching and video source extraction into a single operation.

- **Returns**: A `Task<CoubVideo>` representing the asynchronous operation. The completed task yields a `CoubVideo` object containing the video's metadata and the local path to the downloaded file.
- **Throws**: `ArgumentException` if the provided Coub identifier is invalid or empty.
- **Throws**: `HttpRequestException` if the network request to Coub fails.
- **Throws**: `InvalidOperationException` if the video source cannot be extracted or the download fails verification.

### `public async Task<CoubVideo> FetchMetadataAsync()`

Fetches metadata for a specified Coub video without downloading the video file itself.

- **Returns**: A `Task<CoubVideo>` representing the asynchronous operation. The completed task yields a `CoubVideo` object populated with metadata such as title, author, tags, and thumbnail URLs.
- **Throws**: `ArgumentException` if the Coub identifier is invalid or empty.
- **Throws**: `HttpRequestException` if the network request to Coub fails.
- **Throws**: `InvalidOperationException` if the metadata cannot be parsed from the response.

### `public async Task<string> ExtractVideoSourceAsync()`

Extracts the direct video source URL for a Coub video. Requires prior metadata fetching to determine the video source location.

- **Returns**: A `Task<string>` representing the asynchronous operation. The completed task yields the direct URL to the video file.
- **Throws**: `InvalidOperationException` if the video source URL cannot be determined from the metadata.
- **Throws**: `HttpRequestException` if the network request to fetch the video source fails.

### `public async Task<bool> VerifyDownloadAsync()`

Verifies that a previously downloaded video file matches the expected Coub video metadata and integrity. Used to confirm successful downloads.

- **Returns**: A `Task<bool>` representing the asynchronous operation. The completed task yields `true` if the file is valid and matches the metadata; otherwise, `false`.
- **Throws**: `FileNotFoundException` if the local file does not exist.
- **Throws**: `InvalidOperationException` if the file size or hash does not match expected values.

### `public async Task<string> DownloadVideoFileAsync()`

Downloads the video file from the provided source URL and saves it to a local file. Does not perform metadata fetching or verification.

- **Parameters**: Accepts a source URL string pointing to the video file.
- **Returns**: A `Task<string>` representing the asynchronous operation. The completed task yields the local file path where the video was saved.
- **Throws**: `ArgumentException` if the source URL is null or invalid.
- **Throws**: `HttpRequestException` if the download from the source URL fails.
- **Throws**: `IOException` if the file cannot be written to disk.

## Usage

### Example 1: Download a Coub video with metadata and verification
