# ExportService

The `ExportService` class provides functionality for generating and exporting download reports and batch statistics within the `coub-downloader` application. It facilitates the construction of structured reports through a fluent builder pattern and supports asynchronous operations for persisting these reports as files, handling both HTML generation and direct export of download results or batch summaries.

## API

### `public ExportService`
Initializes a new instance of the `ExportService` class. This constructor prepares the internal state required for report building and file export operations.

### `public async Task<bool> ExportBatchReportAsync`
Asynchronously generates and saves a summary report for a specific batch of download operations.
*   **Parameters**: Accepts parameters defining the batch context and the target file path (specific signature details depend on the invocation context, typically involving batch ID and output path).
*   **Return Value**: Returns a `Task<bool>` which resolves to `true` if the file was successfully written to disk, or `false` if the operation failed (e.g., due to I/O errors or cancellation).
*   **Exceptions**: May throw `IOException` or `UnauthorizedAccessException` if the file system is inaccessible or permissions are insufficient.

### `public async Task<bool> ExportDownloadResultsAsync`
Asynchronously exports the detailed results of individual download tasks to a specified format and location.
*   **Parameters**: Accepts a collection of download result objects and the destination file path.
*   **Return Value**: Returns a `Task<bool>` indicating the success (`true`) or failure (`false`) of the export operation.
*   **Exceptions**: Throws `ArgumentNullException` if the provided result collection is null; may throw `IOException` during file writing.

### `public string GenerateHtmlReport`
Synchronously generates an HTML-formatted string representation of the current report data accumulated in the builder.
*   **Parameters**: Takes no explicit parameters; operates on the internal state built via `AddSection` and `AddTable`.
*   **Return Value**: Returns a `string` containing the complete HTML markup.
*   **Exceptions**: Throws an `InvalidOperationException` if called before any sections or tables have been added to the report.

### `public ReportBuilder AddSection`
Adds a new named section to the report being constructed. This method initiates a fluent chain for configuring the section.
*   **Parameters**: Accepts a `string` representing the section title or header.
*   **Return Value**: Returns the `ReportBuilder` instance to allow method chaining.
*   **Exceptions**: Throws `ArgumentNullException` if the section title is null or empty.

### `public ReportBuilder AddTable`
Appends a data table to the current section of the report.
*   **Parameters**: Accepts the data source (typically a collection of objects or a DataTable) and optional column headers.
*   **Return Value**: Returns the `ReportBuilder` instance to allow method chaining.
*   **Exceptions**: Throws `ArgumentException` if the data source is empty or malformed.

### `public string Build`
Finalizes the report construction process and returns the compiled content.
*   **Parameters**: No parameters.
*   **Return Value**: Returns a `string` representing the final formatted report content (format depends on the configured builder state).
*   **Exceptions**: Throws `InvalidOperationException` if the report structure is incomplete (e.g., an open section was not closed properly).

## Usage

### Example 1: Generating an HTML Report Manually
This example demonstrates constructing a custom report with multiple sections and tables, then generating the HTML string.

```csharp
var service = new ExportService();
var builder = service.AddSection("Download Summary");

var downloadData = new[] 
{ 
    new { Id = 1, Status = "Success", Size = "15MB" },
    new { Id = 2, Status = "Failed", Size = "0MB" }
};

builder.AddTable(downloadData, new[] { "ID", "Status", "Size" });
builder.AddSection("Error Logs");
// Add error table logic here...

string htmlContent = service.GenerateHtmlReport();
Console.WriteLine($"Report generated: {htmlContent.Length} characters");
```

### Example 2: Asynchronously Exporting Batch Results
This example shows how to export the results of a completed download batch directly to a file.

```csharp
public async Task SaveBatchResultsAsync(int batchId, IEnumerable<DownloadResult> results)
{
    var service = new ExportService();
    string filePath = $"reports/batch_{batchId}_results.csv";
    
    bool success = await service.ExportDownloadResultsAsync(results, filePath);
    
    if (success)
    {
        Console.WriteLine($"Batch {batchId} exported successfully to {filePath}");
    }
    else
    {
        Console.Error.WriteLine($"Failed to export batch {batchId}. Check file permissions.");
    }
}
```

## Notes

*   **Thread Safety**: The `ExportService` and its associated `ReportBuilder` are not thread-safe. Concurrent calls to `AddSection`, `AddTable`, or `Build` from multiple threads without external synchronization may result in corrupted report structures or race conditions.
*   **Resource Management**: The asynchronous export methods (`ExportBatchReportAsync`, `ExportDownloadResultsAsync`) handle file stream disposal internally. However, callers should ensure that the target directories exist before invocation to avoid unnecessary exceptions.
*   **State Dependency**: Methods like `GenerateHtmlReport` and `Build` rely on the internal state populated by `AddSection` and `AddTable`. Calling these generation methods on a fresh instance without adding content will result in an `InvalidOperationException`.
*   **Return Values**: A return value of `false` from the async export methods indicates a logical failure (such as a locked file or transient I/O issue) rather than a thrown exception, allowing for simpler retry logic in calling code.
