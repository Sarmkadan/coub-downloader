# CommandParser

The `CommandParser` type encapsulates the state required to translate user‑provided command line arguments into strongly‑typed options objects for the various sub‑commands of the coub‑downloader application. It exposes a set of properties that represent the parsed inputs and a family of parameter‑less methods that perform the actual conversion, validating the internal state and returning the appropriate options instance or throwing when the data is insufficient or malformed.

## API

### ParseDownloadOptions  
**Purpose:** Constructs a `DownloadCommandOptions` instance from the current property values.  
**Parameters:** None.  
**Return value:** A fully populated `DownloadCommandOptions` object ready for use by the download workflow.  
**When it throws:**  
- `InvalidOperationException` if `Url` is null or empty.  
- `FormatException` if `Format` or `Quality` have values that cannot be mapped to the underlying video format/enumeration.  

### ParseConvertOptions  
**Purpose:** Constructs a `ConvertCommandOptions` instance from the current property values.  
**Parameters:** None.  
**Return value:** A `ConvertCommandOptions` object containing conversion settings.  
**When it throws:**  
- `InvalidOperationException` if `InputFile` is null, empty, or does not point to an existing file.  
- `InvalidOperationException` if `OutputFile` is null or empty.  
- `FormatException` if `Format`, `Quality`, `Width`, `Height`, or `FrameRate` contain invalid combinations.  

### ParseBatchOptions  
**Purpose:** Constructs a `BatchCommandOptions` instance from the current property values.  
**Parameters:** None.  
**Return value:** A `BatchCommandOptions` object describing a batch operation.  
**When it throws:**  
- `InvalidOperationException` if `Files` is null or contains no entries.  
- `InvalidOperationException` if `OutputDirectory` is null, empty, or points to a non‑existent directory.  
- `FormatException` if any of `Format`, `Quality`, `Width`, `Height`, or `FrameRate` are invalid for the batch conversion.  

### ParseInfoOptions  
**Purpose:** Constructs an `InfoCommandOptions` instance from the current property values.  
**Parameters:** None.  
**Return value:** An `InfoCommandOptions` object used to request media information.  
**When it throws:**  
- `InvalidOperationException` if `Url` is null or empty and `InputFile` is null or empty (at least one source must be supplied).  

### Url  
**Purpose:** Holds the target Coub URL for download or info operations.  
**Return value:** The URL string, or `null` if not set.  
**When it throws:** The property itself does not throw; accessing it returns the stored value (which may be `null`).  

### OutputPath  
**Purpose:** Specifies the destination folder or file path for downloaded content.  
**Return value:** The path string, or `null` if not set.  
**When it throws:** The property itself does not throw.  

### Format (first occurrence)  
**Purpose:** Stores the desired video format for download or conversion operations.  
**Return value:** A `VideoFormat` enumeration value.  
**When it throws:** The property itself does not throw.  

### Quality (first occurrence)  
**Purpose:** Stores the desired video quality for download or conversion operations.  
**Return value:** A `VideoQuality` enumeration value.  
**When it throws:** The property itself does not throw.  

### InputFile  
**Purpose:** Indicates the source file path for conversion or info operations.  
**Return value:** The file path string, or `null` if not set.  
**When it throws:** The property itself does not throw.  

### OutputFile  
**Purpose:** Indicates the destination file path for conversion operations.  
**Return value:** The file path string, or `null` if not set.  
**When it throws:** The property itself does not throw.  

### Format (second occurrence)  
**Purpose:** Stores the video format used specifically for batch conversion settings.  
**Return value:** A `VideoFormat` enumeration value.  
**When it throws:** The property itself does not throw.  

### Quality (second occurrence)  
**Purpose:** Stores the video quality used specifically for batch conversion settings.  
**Return value:** A `VideoQuality` enumeration value.  
**When it throws:** The property itself does not throw.  

### Width  
**Purpose:** Desired output width in pixels for conversion or batch operations.  
**Return value:** Nullable integer; `null` indicates no explicit width constraint.  
**When it throws:** The property itself does not throw.  

### Height  
**Purpose:** Desired output height in pixels for conversion or batch operations.  
**Return value:** Nullable integer; `null` indicates no explicit height constraint.  
**When it throws:** The property itself does not throw.  

### FrameRate  
**Purpose:** Desired output frame rate (frames per second) for conversion or batch operations.  
**Return value:** Nullable integer; `null` indicates no explicit frame‑rate constraint.  
**When it throws:** The property itself does not throw.  

### Files  
**Purpose:** Collection of input file paths for batch operations.  
**Return value:** A `List<string>` containing the file paths; may be empty but never `null` after initialization.  
**When it throws:** The property itself does not throw.  

### OutputDirectory  
**Purpose:** Destination directory where batch‑processed files are written.  
**Return value:** The directory path string, or `null` if not set.  
**When it throws:** The property itself does not throw.  

### Name  
**Purpose:** Optional name or title assigned to the processed media item (e.g., for metadata).  
**Return value:** The name string, or `null` if not set.  
**When it throws:** The property itself does not throw.  

### Format (third occurrence)  
**Purpose:** Stores the video format used for info or other miscellaneous operations.  
**Return value:** A `VideoFormat` enumeration value.  
**When it throws:** The property itself does not throw.  

### Quality (third occurrence)  
**Purpose:** Stores the video quality used for info or other miscellaneous operations.  
**Return value:** A `VideoQuality` enumeration value.  
**When it throws:** The property itself does not throw.  

## Usage

### Example 1: Preparing a download operation
```csharp
using CoubDownloader.Models; // assumes relevant namespaces

var parser = new CommandParser
{
    Url = "https://coub.com/view/abcdefg",
    OutputPath = @"C:\Downloads\Coubs",
    Format = VideoFormat.Mp4,
    Quality = VideoQuality.High
};

DownloadCommandOptions options = parser.ParseDownloadOptions();
// options can now be passed to the download service
```

### Example 2: Configuring a batch conversion
```csharp
var parser = new CommandParser
{
    Files = new List<string>
    {
        @"C:\Media\input1.mov",
        @"C:\Media\input2.mov"
    },
    OutputDirectory = @"C:\Media\Converted",
    Format = VideoFormat.Webm,
    Quality = VideoQuality.Medium,
    Width = 1280,
    Height = 720,
    FrameRate = 30
};

BatchCommandOptions batchOptions = parser.ParseBatchOptions();
// batchOptions feeds the batch processing pipeline
```

## Notes

- All property getters are simple field accesses; they never throw exceptions. Validation occurs only when the corresponding `Parse*Options` method is invoked.  
- The class is **not thread‑safe**. Concurrent reads or writes to its properties from multiple threads may lead to inconsistent state; if shared access is required, external synchronization must be applied.  
- Duplicate `Format` and `Quality` properties exist to allow distinct values for different command contexts (e.g., download vs. batch conversion). Setting one does not affect the others.  
- Passing `null` or empty strings for required fields such as `Url`, `InputFile`, `OutputFile`, or `Files` will cause the relevant `Parse*Options` method to throw an `InvalidOperationException`.  
- Numeric properties (`Width`, `Height`, `FrameRate`) accept `null` to indicate “use default”; supplying a value outside the supported range for the selected `Format` triggers a `FormatException`.  
- The `Files` list is initialized internally to an empty list; assigning `null` will result in a `NullReferenceException` when `ParseBatchOptions` attempts to read the collection.  
- No members are marked `static`; each instance maintains its own state, and options objects produced by the parse methods are independent snapshots of that state at the moment of invocation.
