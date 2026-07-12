// ... (rest of the README.md content remains the same)

## FileUtilitiesTestsExtensions

`FileUtilitiesTestsExtensions` provides a set of helper methods for creating temporary files and directories, as well as verifying their contents. These extensions simplify the process of testing file-related functionality.

### Usage Example

```csharp
using CoubDownloader.Tests;

// Create a temporary file
var (filePath, cleanup) = FileUtilitiesTestsExtensions.CreateTempFile();
using (cleanup)
{
    // Use the temporary file
    File.WriteAllText(filePath, "Hello, World!");
}

// Create a temporary directory
var (directoryPath, cleanup) = FileUtilitiesTestsExtensions.CreateTempDirectory();
using (cleanup)
{
    // Use the temporary directory
    Directory.CreateDirectory($"{directoryPath}/subdir");
}

// Verify the contents of a file
FileUtilitiesTestsExtensions.ShouldContainSameContentAs("expected.txt", "actual.txt");

// Create a temporary file with random content
var (filePath, cleanup) = FileUtilitiesTestsExtensions.CreateTempFileWithRandomContent();
using (cleanup)
{
    // Use the temporary file
    var randomContent = new byte[1024];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomContent);
    File.WriteAllBytes(filePath, randomContent);
}
```

// ... (rest of the README.md content remains the same)
```