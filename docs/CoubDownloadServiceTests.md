# CoubDownloadServiceTests

`CoubDownloadServiceTests` is the test class for the `CoubDownloadService` component within the `coub-downloader` project. It contains unit tests that validate the behavior of the core download pipeline—metadata fetching, video source extraction, file downloading, and download verification—under both normal and exceptional conditions. The class ensures that valid Coub URLs produce correct results, invalid inputs are rejected with appropriate exceptions, and failure modes in external dependencies are handled gracefully.

## API

### `public CoubDownloadServiceTests`

Default constructor. Initializes a new instance of the test class, setting up any shared test infrastructure required by the individual test methods.

### `public async Task DownloadVideoAsync_ValidCoubUrl_ReturnsCoubVideo`

Tests the end-to-end download workflow when given a well-formed Coub URL. Verifies that the returned object contains the expected video data and metadata.

- **Parameters:** none (inputs are arranged within the test).
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if the returned value is not a valid `CoubVideo`.

### `public async Task DownloadVideoAsync_InvalidCoubUrl_ThrowsArgumentException`

Ensures that the download workflow throws an `ArgumentException` when the provided URL is malformed or does not point to a valid Coub resource.

- **Parameters:** none.
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if `ArgumentException` is not raised.

### `public async Task DownloadVideoAsync_MetadataFetchingFails_ThrowsMetadataExtractionException`

Validates that a failure during the metadata-fetching stage causes a `MetadataExtractionException` to propagate, rather than being silently swallowed.

- **Parameters:** none.
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if `MetadataExtractionException` is not raised.

### `public async Task DownloadVideoAsync_SourceExtractionFails_ThrowsMetadataExtractionException`

Confirms that when video source extraction fails (e.g., due to missing or corrupt API data), the download workflow throws a `MetadataExtractionException`.

- **Parameters:** none.
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if `MetadataExtractionException` is not raised.

### `public async Task FetchMetadataAsync_ValidCoubUrl_ReturnsCoubVideoWithMetadata`

Tests the metadata-fetching step in isolation. A valid Coub URL should yield a `CoubVideo` object whose metadata fields are populated.

- **Parameters:** none.
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if metadata is missing or incorrect.

### `public async Task FetchMetadataAsync_InvalidCoubUrl_ThrowsArgumentException`

Verifies that `FetchMetadataAsync` throws an `ArgumentException` when called with an invalid Coub URL.

- **Parameters:** none.
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if `ArgumentException` is not raised.

### `public async Task FetchMetadataAsync_ApiReturnsNull_ThrowsMetadataExtractionException`

Simulates a scenario where the Coub API returns a null response. The test asserts that a `MetadataExtractionException` is thrown to signal the unrecoverable metadata failure.

- **Parameters:** none.
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if `MetadataExtractionException` is not raised.

### `public async Task ExtractVideoSourceAsync_ValidCoubUrl_ReturnsExpectedSourceUrl`

Tests the video-source extraction logic with a valid Coub URL. The returned source URL must match the expected value derived from the Coub’s video metadata.

- **Parameters:** none.
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if the extracted URL does not match expectations.

### `public async Task ExtractVideoSourceAsync_InvalidCoubUrl_ThrowsArgumentException`

Ensures that `ExtractVideoSourceAsync` throws an `ArgumentException` when the supplied URL is invalid.

- **Parameters:** none.
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if `ArgumentException` is not raised.

### `public async Task ExtractVideoSourceAsync_ApiReturnsNull_ThrowsMetadataExtractionException`

Validates that a null API response during source extraction results in a `MetadataExtractionException`.

- **Parameters:** none.
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if `MetadataExtractionException` is not raised.

### `public async Task ExtractVideoSourceAsync_ApiReturnsVideoInfoWithNullId_ThrowsMetadataExtractionException`

Covers the edge case where the API returns a video info object but its identifier field is null. The test asserts that this is treated as a metadata extraction failure and throws `MetadataExtractionException`.

- **Parameters:** none.
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if `MetadataExtractionException` is not raised.

### `public async Task VerifyDownloadAsync_FileExistsAndIsNotEmpty_ReturnsTrue`

Tests the download verification logic when the target file exists on disk and contains data. Expects a `true` result.

- **Parameters:** none.
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if the result is not `true`.

### `public async Task VerifyDownloadAsync_FileDoesNotExist_ReturnsFalse`

Tests verification when the specified file path does not exist. Expects a `false` result.

- **Parameters:** none.
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if the result is not `false`.

### `public async Task VerifyDownloadAsync_FileExistsButIsEmpty_ReturnsFalse`

Tests verification when the file exists but has zero length. Expects a `false` result, treating an empty file as an unsuccessful download.

- **Parameters:** none.
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if the result is not `false`.

### `public async Task VerifyDownloadAsync_InvalidFilePath_ThrowsArgumentException`

Ensures that an invalid file path (e.g., containing illegal characters) causes `VerifyDownloadAsync` to throw an `ArgumentException`.

- **Parameters:** none.
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if `ArgumentException` is not raised.

### `public async Task DownloadVideoFileAsync_ValidInputs_AttemptsFileDownload`

Tests the file-download step with valid source URL and destination path. Verifies that the download is initiated and completes without throwing.

- **Parameters:** none.
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if the download does not proceed as expected.

### `public async Task DownloadVideoFileAsync_InvalidInputs_ThrowsArgumentException`

Confirms that invalid inputs (e.g., null or malformed URL/path) cause `DownloadVideoFileAsync` to throw an `ArgumentException`.

- **Parameters:** none.
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if `ArgumentException` is not raised.

### `public async Task DownloadVideoFileAsync_HttpRequestFails_ThrowsHttpRequestException`

Simulates an HTTP failure during the file download and asserts that an `HttpRequestException` is thrown to signal the network-level error.

- **Parameters:** none.
- **Returns:** a `Task` representing the asynchronous test operation.
- **Throws:** test assertion failures if `HttpRequestException` is not raised.

## Usage

### Example 1: Running all tests with a standard test runner

```csharp
using Xunit;

public class TestSuiteRunner
{
    private readonly CoubDownloadServiceTests _tests = new CoubDownloadServiceTests();

    [Fact]
    public async Task RunAllDownloadServiceTests()
    {
        // These calls would typically be invoked by the test framework,
        // but they can also be orchestrated manually in a composite scenario.
        await _tests.DownloadVideoAsync_ValidCoubUrl_ReturnsCoubVideo();
        await _tests.FetchMetadataAsync_ValidCoubUrl_ReturnsCoubVideoWithMetadata();
        await _tests.ExtractVideoSourceAsync_ValidCoubUrl_ReturnsExpectedSourceUrl();
        await _tests.VerifyDownloadAsync_FileExistsAndIsNotEmpty_ReturnsTrue();
        await _tests.DownloadVideoFileAsync_ValidInputs_AttemptsFileDownload();
    }
}
```

### Example 2: Selective execution focusing on failure scenarios

```csharp
using Xunit;

public class FailureScenarioRunner
{
    private readonly CoubDownloadServiceTests _tests = new CoubDownloadServiceTests();

    [Fact]
    public async Task ValidateExceptionHandling()
    {
        // Verify that invalid URLs are consistently rejected.
        await _tests.DownloadVideoAsync_InvalidCoubUrl_ThrowsArgumentException();
        await _tests.FetchMetadataAsync_InvalidCoubUrl_ThrowsArgumentException();
        await _tests.ExtractVideoSourceAsync_InvalidCoubUrl_ThrowsArgumentException();

        // Verify that null or incomplete API responses trigger metadata exceptions.
        await _tests.FetchMetadataAsync_ApiReturnsNull_ThrowsMetadataExtractionException();
        await _tests.ExtractVideoSourceAsync_ApiReturnsNull_ThrowsMetadataExtractionException();
        await _tests.ExtractVideoSourceAsync_ApiReturnsVideoInfoWithNullId_ThrowsMetadataExtractionException();

        // Verify network failure propagation.
        await _tests.DownloadVideoFileAsync_HttpRequestFails_ThrowsHttpRequestException();
    }
}
```

## Notes

- **Edge cases covered:** The test suite explicitly handles null API responses, null identifier fields within otherwise valid-looking video info objects, empty files that exist on disk, and completely missing files. These ensure the service does not treat partial or corrupt data as success.
- **Exception consistency:** `ArgumentException` is used uniformly for invalid input URLs and file paths. `MetadataExtractionException` covers all failures related to incomplete or unparseable Coub metadata. `HttpRequestException` is reserved for transport-level failures during file download.
- **Thread safety:** The test methods themselves are asynchronous and designed to be run by a test framework that may execute them in parallel. No shared mutable state is exposed by the test class, so concurrent execution of different test methods is safe. Individual tests should not be assumed thread-safe if external resources (e.g., temporary files) are shared across parallel runs without isolation.
- **Test isolation:** Each method arranges its own inputs and expected outcomes. There is no dependency ordering enforced between tests; they can be executed independently or in arbitrary sequences.
