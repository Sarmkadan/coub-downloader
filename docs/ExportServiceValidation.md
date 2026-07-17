# ExportServiceValidation

Static utility class that provides validation helpers for the export service in the coub‑downloader project. It offers synchronous and asynchronous validation routines that return collections of error messages, boolean validity checks, and methods that enforce validity by throwing exceptions when the service state is not acceptable.

## API

### Validate()
Returns a read‑only list of validation messages for the default export service configuration.  
- **Parameters:** none  
- **Return value:** `IReadOnlyList<string>` – empty list indicates the service is valid; each string describes a validation problem.  
- **Throws:** none.

### ValidateExportBatchReportAsync()
Validates the data required to generate an export batch report.  
- **Parameters:** (implementation‑specific, not exposed in the public signature)  
- **Return value:** `IReadOnlyList<string>` – empty list indicates the batch report data is valid.  
- **Throws:** none.

### ValidateExportDownloadResultsAsync()
Validates the results of an export download operation.  
- **Parameters:** (implementation‑specific, not exposed in the public signature)  
- **Return value:** `IReadOnlyList<string>` – empty list indicates the download results are valid.  
- **Throws:** none.

### ValidateGenerateHtmlReport()
Validates the inputs needed to generate an HTML report.  
- **Parameters:** (implementation‑specific, not exposed in the public signature)  
- **Return value:** `IReadOnlyList<string>` – empty list indicates the HTML report generation inputs are valid.  
- **Throws:** none.

### ValidateAddSection()
Validates the parameters for adding a section to an export document.  
- **Parameters:** (implementation‑specific, not exposed in the public signature)  
- **Return value:** `IReadOnlyList<string>` – empty list indicates the section addition is valid.  
- **Throws:** none.

### ValidateAddTable()
Validates the parameters for adding a table to an export document.  
- **Parameters:** (implementation‑specific, not exposed in the public signature)  
- **Return value:** `IReadOnlyList<string>` – empty list indicates the table addition is valid.  
- **Throws:** none.

### Validate (overload 2)
Validates a specific aspect of the export service state (different parameter set from the first Validate overload).  
- **Parameters:** (implementation‑specific, not exposed in the public signature)  
- **Return value:** `IReadOnlyList<string>` – empty list indicates validity.  
- **Throws:** none.

### Validate (overload 3)
Validates another facet of the export service state (different parameter set from the previous Validate overloads).  
- **Parameters:** (implementation‑specific, not exposed in the public signature)  
- **Return value:** `IReadOnlyList<string>` – empty list indicates validity.  
- **Throws:** none.

### Validate (overload 4)
Validates yet another configuration of the export service (different parameter set).  
- **Parameters:** (implementation‑specific, not exposed in the public signature)  
- **Return value:** `IReadOnlyList<string>` – empty list indicates validity.  
- **Throws:** none.

### IsValid()
Determines whether the export service is currently in a valid state.  
- **Parameters:** none  
- **Return value:** `bool` – `true` if valid, `false` otherwise.  
- **Throws:** none.

### IsValid (overload 2)
Determines validity for a specific export service configuration (different parameter set).  
- **Parameters:** (implementation‑specific, not exposed in the public signature)  
- **Return value:** `bool` – `true` if valid, `false` otherwise.  
- **Throws:** none.

### IsValid (overload 3)
Determines validity for another export service configuration (different parameter set).  
- **Parameters:** (implementation‑specific, not exposed in the public signature)  
- **Return value:** `bool` – `true` if valid, `false` otherwise.  
- **Throws:** none.

### IsValid (overload 4)
Determines validity for yet another export service configuration (different parameter set).  
- **Parameters:** (implementation‑specific, not exposed in the public signature)  
- **Return value:** `bool` – `true` if valid, `false` otherwise.  
- **Throws:** none.

### EnsureValid()
Throws an exception if the export service is not in a valid state; otherwise does nothing.  
- **Parameters:** none  
- **Return value:** `void`  
- **Throws:** `InvalidOperationException` (or a derived type) when validation fails, containing the validation messages.

### EnsureValid (overload 2)
Throws an exception if a specific export service configuration is invalid.  
- **Parameters:** (implementation‑specific, not exposed in the public signature)  
- **Return value:** `void`  
- **Throws:** `InvalidOperationException` (or a derived type) when validation fails.

### EnsureValid (overload 3)
Throws an exception if another export service configuration is invalid.  
- **Parameters:** (implementation‑specific, not exposed in the public signature)  
- **Return value:** `void`  
- **Throws:** `InvalidOperationException` (or a derived type) when validation fails.

### EnsureValid (overload 4)
Throws an exception if yet another export service configuration is invalid.  
- **Parameters:** (implementation‑specific, not exposed in the public signature)  
- **Return value:** `void`  
- **Throws:** `InvalidOperationException` (or a derived type) when validation fails.

## Usage

```csharp
// Example 1: Simple validation before exporting
var errors = ExportServiceValidation.Validate();
if (errors.Count > 0)
{
    foreach (var err in errors)
    {
        Console.WriteLine($"Validation error: {err}");
    }
    // Handle errors appropriately
}
else
{
    // Proceed with export
    ExportService.RunExport();
}
```

```csharp
// Example 2: Asynchronous validation of download results
var downloadResults = await DownloadService.GetResultsAsync();
var validationErrors = ExportServiceValidation.ValidateExportDownloadResultsAsync();
if (validationErrors.Count == 0)
{
    ExportServiceValidation.EnsureValid(); // throws if something changed unexpectedly
    ReportGenerator.CreateReport(downloadResults);
}
else
{
    Logger.LogWarning("Download results failed validation: {Errors}", string.Join("; ", validationErrors));
}
```

## Notes

- All members are **static**; the class holds no instance state, making it inherently thread‑safe for concurrent calls.  
- Validation methods never return `null`; they always return a valid `IReadOnlyList<string>` instance (possibly empty).  
- The `EnsureValid` overloads throw an exception when the corresponding validation would produce a non‑empty error list; the exception’s message typically aggregates the validation strings.  
- Async‑named validation methods (`ValidateExportBatchReportAsync`, `ValidateExportDownloadResultsAsync`) are currently synchronous in signature; callers should not await them unless the underlying implementation changes to return a `Task<IReadOnlyList<string>>`.  
- Overloads differ only in their parameter lists (not shown in the public signature); developers should consult the source code or IntelliSense to select the appropriate overload for their context.  
- No members perform I/O or modify global state; they are pure validation helpers.
