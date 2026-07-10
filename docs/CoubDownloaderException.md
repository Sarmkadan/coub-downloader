# CoubDownloaderException

The `CoubDownloaderException` serves as the base exception type for all error conditions encountered within the `coub-downloader` library. It extends the standard .NET `Exception` class to provide specialized context regarding failed operations, including HTTP status codes, file paths, tool names, and nested exceptions specific to video downloading, conversion, audio processing, and metadata extraction. This type allows consumers to distinguish between different failure modes in the pipeline while retaining access to the original inner exception and relevant operational data.

## API

### Constructors

#### `public CoubDownloaderException(string message)`
Initializes a new instance of the `CoubDownloaderException` class with a specified error message.
*   **Parameters**:
    *   `message` (`string`): The message that describes the error.
*   **Remarks**: The `InnerException` property is null.

#### `public CoubDownloaderException(string message, Exception inner)`
Initializes a new instance of the `CoubDownloaderException` class with a specified error message and a reference to the inner exception that is the cause of this exception.
*   **Parameters**:
    *   `message` (`string`): The message that describes the error.
    *   `inner` (`Exception`): The exception that is the cause of the current exception. If the `inner` parameter is not null, the current exception is raised in a `catch` block that handles the inner exception.
*   **Remarks**: Preserves the stack trace of the original error for debugging complex failure chains.

### Properties

#### `public string VideoUrl`
Gets the URL of the video that was being processed when the exception occurred. This property is populated when the error relates to a specific video resource request.

#### `public int? HttpStatusCode`
Gets the HTTP status code returned by the server if the exception was triggered by a failed network request. Returns `null` if the exception did not originate from an HTTP response or if the status code is unavailable.

#### `public VideoDownloadException VideoDownloadException`
Gets the specific `VideoDownloadException` instance associated with this error, if the failure occurred during the video download phase. This property allows direct access to download-specific details without casting the base exception.

#### `public string InputPath`
Gets the file system path of the input file involved in the operation. This is typically populated when errors occur during file reading, conversion, or processing stages.

#### `public string OutputPath`
Gets the intended or partially created file system path for the output file. Useful for identifying where a write operation failed or where a temporary file resides.

#### `public VideoConversionException VideoConversionException`
Gets the specific `VideoConversionException` instance associated with this error, if the failure occurred during the video format conversion process.

#### `public string AudioFilePath`
Gets the file system path to the audio file involved in the operation. This is populated when errors occur specifically during audio extraction or processing.

#### `public AudioProcessingException AudioProcessingException`
Gets the specific `AudioProcessingException` instance associated with this error, if the failure occurred during audio manipulation or encoding.

#### `public string ToolName`
Gets the name of the external tool or binary (e.g., "ffmpeg", "yt-dlp") that caused the exception. This is primarily used when a `ToolNotFoundException` or tool execution error occurs.

#### `public ToolNotFoundException ToolNotFoundException`
Gets the specific `ToolNotFoundException` instance associated with this error, indicating that a required external dependency was missing or could not be executed.

#### `public string SourceUrl`
Gets the original source URL from which metadata or content was being fetched. This may differ from `VideoUrl` if the error occurred during the initial page scrape or metadata resolution phase.

#### `public MetadataExtractionException MetadataExtractionException`
Gets the specific `MetadataExtractionException` instance associated with this error, if the failure occurred while parsing or extracting metadata from the source page.

## Usage

### Example 1: Handling HTTP Errors and Video Download Failures
This example demonstrates catching the exception when a video download fails due to a server error, utilizing the `HttpStatusCode` and `VideoUrl` properties to log specific diagnostic information.

```csharp
try
{
    await downloader.DownloadVideoAsync("https://coub.com/view/example123");
}
catch (CoubDownloaderException ex)
{
    if (ex.HttpStatusCode.HasValue)
    {
        Console.WriteLine($"Failed to download {ex.VideoUrl}. Server returned status: {ex.HttpStatusCode.Value}");
        
        // Access specific download exception details if available
        if (ex.VideoDownloadException != null)
        {
            Console.WriteLine($"Download specific error: {ex.VideoDownloadException.Message}");
        }
    }
    else
    {
        Console.WriteLine($"General error for {ex.VideoUrl}: {ex.Message}");
    }
}
```

### Example 2: Diagnosing Tool and Conversion Issues
This example illustrates handling errors related to missing external tools or video conversion failures, leveraging the `ToolName` and `VideoConversionException` properties.

```csharp
try
{
    await converter.ConvertAsync(inputPath: "./temp/video.webm", outputPath: "./output/video.mp4");
}
catch (CoubDownloaderException ex)
{
    if (ex.ToolNotFoundException != null)
    {
        Console.WriteLine($"Required tool '{ex.ToolName}' was not found. Please ensure it is installed and in the PATH.");
    }
    else if (ex.VideoConversionException != null)
    {
        Console.WriteLine($"Conversion failed for input: {ex.InputPath}");
        Console.WriteLine($"Intended output: {ex.OutputPath}");
        Console.WriteLine($"Details: {ex.VideoConversionException.Message}");
    }
    else
    {
        Console.WriteLine($"Unexpected processing error: {ex.Message}");
    }
}
```

## Notes

*   **Property Availability**: Not all properties are populated for every instance of `CoubDownloaderException`. Properties such as `HttpStatusCode`, `VideoUrl`, or specific nested exception types (e.g., `AudioProcessingException`) are only set when the error context specifically relates to those domains. Consumers should check for `null` values before accessing these members.
*   **Nested Exceptions**: The specific exception properties (e.g., `VideoDownloadException`, `ToolNotFoundException`) provide strongly-typed access to inner errors. However, the standard `InnerException` property on the base `Exception` class is also populated when the constructor `CoubDownloaderException(string message, Exception inner)` is used, preserving the full stack trace chain.
*   **Thread Safety**: Instances of `CoubDownloaderException` are immutable after construction regarding their core state (message, inner exception, and populated context properties). They are safe to pass between threads. However, if mutable objects (such as custom objects stored within an inner exception) are referenced, thread safety depends on those specific objects.
*   **Path Sensitivity**: The `InputPath`, `OutputPath`, and `AudioFilePath` properties reflect the paths provided at the time of the operation. These paths are not verified for existence when the exception is thrown; they represent the state of the operation at the moment of failure.
