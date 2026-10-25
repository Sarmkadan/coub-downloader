# ExportService

`ExportService` (namespace `CoubDownloader.Infrastructure.Reporting`, file `Infrastructure/Reporting/ExportService.cs`) writes batch job reports and download results to files in JSON, CSV, or XML, and builds HTML and Markdown report strings in memory. It is registered as a singleton in `Infrastructure/DependencyInjectionExtended.cs`.

The same file also contains the `ExportFormat` enum and the `ReportBuilder` class.

## Supported formats

| Format | Batch report (`ExportBatchReportAsync`) | Download results (`ExportDownloadResultsAsync`) | Notes |
|---|---|---|---|
| `ExportFormat.Json` | Yes | Yes | Batch uses camelCase via `JsonFormatter`; results use default PascalCase |
| `ExportFormat.Csv` | Yes | Yes | Batch and results use different column sets (see below) |
| `ExportFormat.Xml` | Yes | Yes | Hand-built XML string, no escaping |
| `ExportFormat.Html` | No | No | Export methods throw internally and return `false`; use `GenerateHtmlReport` instead |

`ExportFormat` enum members:

| Member | Description |
|---|---|
| `Json` | JavaScript Object Notation |
| `Csv` | Comma-Separated Values |
| `Xml` | eXtensible Markup Language |
| `Html` | HyperText Markup Language (only through `GenerateHtmlReport`) |

## Public API

### Constructor

```csharp
public ExportService(ILoggingService logger)
```

Creates a `JsonFormatter` and a `CsvFormatter` internally. `logger` receives info and error messages with the category `"ExportService"`.

### ExportBatchReportAsync

```csharp
public async Task<bool> ExportBatchReportAsync(BatchJob batch, string outputPath, ExportFormat format)
```

Writes a report for `batch` to `outputPath`, overwriting any existing file. The content depends on `format`:

- **Json**: `JsonFormatter.FormatBatchJob`. Fields: `id`, `name`, `outputDirectory`, `state`, `createdAt`, `completedAt`, `taskCount`, and `status` (`totalTasks`, `completedTasks`, `failedTasks`, `pendingTasks`).
- **Csv**: `CsvFormatter.FormatBatchTasks`. Header: `TaskID,VideoID,URL,OutputPath,State,Format,Quality,CreatedAt`, one row per task.
- **Xml**: a `<BatchJob>` document with `Id`, `Name`, `State`, `CreatedAt` (ISO 8601 `"O"`), `TaskCount`, and a `<Tasks>` list of `<Task>` elements (`Id`, `State`, `Format`, `Quality`).

Returns `true` after a successful write and logs at info level. Returns `false` for any exception, including an unsupported format, a null argument, or an I/O error. The exception is logged at error level and not rethrown.

### ExportDownloadResultsAsync

```csharp
public async Task<bool> ExportDownloadResultsAsync(
    List<DownloadResult> results,
    string outputPath,
    ExportFormat format)
```

Writes one record per `DownloadResult` to `outputPath`, overwriting any existing file. The content depends on `format`:

- **Json**: a JSON array, indented, PascalCase. Each item has `TaskId`, `Success`, `OutputFilePath`, `OutputFileSizeBytes`, `ProcessingTimeMs`, `Format`, `Quality`, and `ErrorMessage`. `Format` and `Quality` are serialized as their enum values.
- **Csv**: header `id,title,status,size,duration,path`. `status` is `completed` or `failed`. Text fields are quoted per RFC 4180 (see [CSV escaping](#csv-escaping)). A null `Title` or `OutputFilePath` becomes an empty field.
- **Xml**: a `<Results>` document with one `<Result>` per item (`TaskId`, `Success`, `OutputPath`, `FileSize`, `ProcessingTime`, `ErrorMessage`).

Return value and logging behave as in `ExportBatchReportAsync`. A null `results` list returns `false`.

### GenerateHtmlReport

```csharp
public string GenerateHtmlReport(BatchJob batch)
```

Returns a complete HTML5 page as a string. It does not write a file, and it is not wrapped in try/catch, so a null `batch` throws `NullReferenceException`.

The page contains:

- a header with the batch name, ID, and creation time;
- four stat boxes: total, completed, failed, and progress percentage (completed tasks divided by total tasks, integer division; `0` when there are no tasks);
- a table of tasks with `Task ID`, `State`, `Format`, `Quality`, and `Created`.

Inline CSS is included. The batch name and task values are inserted without HTML encoding.

### ReportBuilder

```csharp
public class ReportBuilder
public ReportBuilder AddSection(string title, string content)
public ReportBuilder AddTable(string title, Dictionary<string, string> data)
public string Build()
```

A fluent builder that produces Markdown. It has no connection to `ExportService`.

- `AddSection` appends `## {title}` followed by a newline and `content`.
- `AddTable` appends `## {title}`, a blank line, and a two-column `| Key | Value |` table in insertion order. Values are not escaped, so a `|` in a value breaks the table.
- `Build` joins all sections with a blank line (`\n\n`). An empty builder returns an empty string.

## Internal helper

```csharp
internal static string EscapeCsvField(string? value)
```

Implements RFC 4180 for the results CSV. A null or empty value returns `""`. A value containing `,`, `"`, `\r`, or `\n` is wrapped in double quotes, with embedded quotes doubled. Other values are returned unchanged. It is `internal`, so the tests in `tests/coub-downloader.Tests/ExportServiceTests.cs` can call it directly.

## Usage

```csharp
var exporter = new ExportService(logger);

bool ok = await exporter.ExportDownloadResultsAsync(
    results,
    Path.Combine(outputDir, "results.csv"),
    ExportFormat.Csv);

string html = exporter.GenerateHtmlReport(batch);
await File.WriteAllTextAsync(Path.Combine(outputDir, "batch.html"), html);
```

Export methods return `false` rather than throwing, so check the result. `GenerateHtmlReport` returns a string, so the caller is responsible for writing it.

## Known limitations

- **XML is not escaped.** Values such as batch names, task IDs, paths, and error messages are inserted as-is. A `<`, `&`, or `]]>` in a value produces malformed XML.
- **HTML is not escaped.** `GenerateHtmlReport` inserts the batch name and task fields directly, so a name containing markup is rendered as markup.
- **`ExportFormat.Html` is not valid for export methods.** Passing it to `ExportBatchReportAsync` or `ExportDownloadResultsAsync` logs an error and returns `false`.
- **The two CSV layouts differ.** The results CSV uses `id,title,status,size,duration,path`. `CsvFormatter.FormatDownloadResults` uses a different header (`TaskID,Success,...`). Consumers must know which one they are reading.
- **Files are overwritten without a check.** There is no append mode and no temp-file write, so an interrupted write can leave a partial file.
- **`ReportBuilder` output is Markdown,** not HTML.

## Tests

`tests/coub-downloader.Tests/ExportServiceTests.cs` covers the CSV path of `ExportDownloadResultsAsync` (header, row values, null fields, commas, quotes, newlines, empty list, unwritable path) and `EscapeCsvField`. JSON, XML, batch reports, and `GenerateHtmlReport` have no tests.

## Related files

- `Infrastructure/Reporting/ExportServiceValidation.cs`: validation helpers. `ExportService` has no public properties, so `Validate` returns no problems.
- `Presentation/Formatters/JsonFormatter.cs`, `Presentation/Formatters/CsvFormatter.cs`: formatters used for batch reports.
- `Infrastructure/Middleware/LoggingService.cs`: `ILoggingService`.
- `Domain/Models/DownloadResult.cs`, `Domain/Models/BatchJob.cs`: models exported by this service.
