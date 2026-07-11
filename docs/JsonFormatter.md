# JsonFormatter

The `JsonFormatter` class serves as a dedicated utility within the `coub-downloader` project for serializing internal data structures into JSON strings. It provides specific methods to format video metadata, collections of videos, batch job configurations, and application settings, ensuring consistent data representation for storage or transmission without relying on external serialization libraries for these specific domains.

## API

### `public JsonFormatter`
Initializes a new instance of the `JsonFormatter` class. This constructor requires no parameters and prepares the formatter for immediate use. It does not perform any I/O operations or throw exceptions under normal circumstances.

### `public string FormatVideo`
Generates a JSON string representation of a single video entity.
*   **Purpose**: Serializes the properties of a specific video object (such as ID, title, and URL) into a JSON format.
*   **Parameters**: This member acts as a delegate or method expecting a video object input (signature details inferred from usage context imply a single video domain object).
*   **Return Value**: Returns a `string` containing the JSON representation of the video.
*   **Exceptions**: May throw a serialization exception if the input video object is null or contains circular references that cannot be resolved.

### `public string FormatVideos`
Generates a JSON string representation of a collection of video entities.
*   **Purpose**: Serializes a list or array of video objects into a single JSON array string.
*   **Parameters**: Expects a collection (e.g., `IEnumerable` or `List`) of video objects.
*   **Return Value**: Returns a `string` containing the JSON array of videos.
*   **Exceptions**: Throws if the provided collection is null. Individual item serialization errors may propagate if a specific video in the list is invalid.

### `public string FormatBatchJob`
Generates a JSON string representation of a batch download job.
*   **Purpose**: Serializes the state and configuration of a batch job, including the queue of videos and job metadata, for persistence or resumption.
*   **Parameters**: Expects a batch job object containing the relevant job data.
*   **Return Value**: Returns a `string` containing the JSON representation of the batch job.
*   **Exceptions**: Throws if the batch job object is null or if required job identifiers are missing.

### `public string FormatSettings`
Generates a JSON string representation of the application settings.
*   **Purpose**: Serializes the current user or application configuration settings into a JSON format suitable for writing to a configuration file.
*   **Parameters**: Expects a settings object containing configuration properties.
*   **Return Value**: Returns a `string` containing the JSON representation of the settings.
*   **Exceptions**: Throws if the settings object is null.

## Usage

The following examples demonstrate how to instantiate the formatter and serialize different data types typical in the `coub-downloader` workflow.

**Example 1: Serializing a single video and saving to a log**

```csharp
using CoubDownloader.Core;

// Assume 'video' is a populated Video object retrieved from the API
var video = new Video 
{ 
    Id = "abc123", 
    Title = "Sample Coub", 
    Url = "https://coub.com/view/abc123" 
};

var formatter = new JsonFormatter();

try 
{
    string jsonOutput = formatter.FormatVideo(video);
    System.IO.File.WriteAllText("latest_video.json", jsonOutput);
}
catch (Exception ex)
{
    System.Console.WriteLine($"Failed to format video: {ex.Message}");
}
```

**Example 2: Persisting application settings and a batch job**

```csharp
using CoubDownloader.Core;
using System.Collections.Generic;

// Assume 'settings' and 'job' are populated configuration and job objects
var settings = new AppSettings { DownloadPath = "/downloads", MaxRetries = 3 };
var job = new BatchJob { Id = "job_01", Videos = new List<Video>() };

var formatter = new JsonFormatter();

// Serialize settings for config file
string settingsJson = formatter.FormatSettings(settings);
System.IO.File.WriteAllText("config.json", settingsJson);

// Serialize batch job for resume capability
string jobJson = formatter.FormatBatchJob(job);
System.IO.File.WriteAllText("active_job.json", jobJson);
```

## Notes

*   **Null Handling**: All formatting methods (`FormatVideo`, `FormatVideos`, `FormatBatchJob`, `FormatSettings`) expect non-null input objects. Passing `null` will likely result in a runtime exception; callers should validate inputs before invocation.
*   **Thread Safety**: The `JsonFormatter` class exposes stateless formatting methods. While the instance itself does not appear to maintain mutable internal state that would cause race conditions during read-only formatting operations, creating a new instance per thread or ensuring external synchronization is recommended if the underlying serialization implementation utilizes shared static resources or mutable buffers.
*   **Encoding**: The returned strings are standard .NET `string` types (UTF-16). When writing these results to disk, ensure the file stream uses UTF-8 encoding to maintain compatibility with standard JSON parsers outside the .NET ecosystem.
*   **Data Integrity**: These methods perform serialization only. They do not validate the logical correctness of the data (e.g., whether a video URL is actually reachable); they only ensure the object structure can be represented as JSON.
