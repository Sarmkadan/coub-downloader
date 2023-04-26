# ProcessExecutionException

Exception that represents a failure during the execution of an external process, capturing the process name, command‑line arguments, exit code, and standard error output for diagnostic purposes.

## API

### Properties

#### ProcessName
- **Purpose:** Gets the name or file path of the process that failed.  
- **Return value:** `string?` containing the process name, or `null` if not supplied.  
- **Throws:** None.

#### Arguments
- **Purpose:** Gets the command‑line arguments that were passed to the process.  
- **Return value:** `string?` containing the arguments, or `null` if not supplied.  
- **Throws:** None.

#### ExitCode
- **Purpose:** Gets the exit code returned by the process.  
- **Return value:** `int` representing the process exit code.  
- **Throws:** None.

#### StandardError
- **Purpose:** Gets the standard error output produced by the process.  
- **Return value:** `string?` containing the error stream, or `null` if not supplied or if the stream was empty.  
- **Throws:** None.

### Constructors

#### ProcessExecutionException(string message)
- **Purpose:** Initializes a new instance with a specified error message.  
- **Parameters:**  
  - `message`: The error message that explains the reason for the exception.  
- **Throws:** `ArgumentNullException` if `message` is `null`.

#### ProcessExecutionException(string message, string processName, int exitCode)
- **Purpose:** Initializes a new instance with a message, the name of the process, and its exit code.  
- **Parameters:**  
  - `message`: The error message.  
  - `processName`: The name or file path of the process.  
  - `exitCode`: The exit code returned by the process.  
- **Throws:** `ArgumentNullException` if `message` is `null`.

#### ProcessExecutionException(string message, string processName, string arguments, int exitCode, string standardError)
- **Purpose:** Initializes a new instance with detailed process execution information.  
- **Parameters:**  
  - `message`: The error message.  
  - `processName`: The name or file path of the process.  
  - `arguments`: The command‑line arguments used to start the process.  
  - `exitCode`: The exit code returned by the process.  
  - `standardError`: The standard error output from the process.  
- **Throws:** `ArgumentNullException` if `message` is `null`.

#### ProcessExecutionException(string message, Exception inner)
- **Purpose:** Initializes a new instance with a specified error message and a reference to the inner exception that caused this exception.  
- **Parameters:**  
  - `message`: The error message.  
  - `inner`: The exception that is the cause of the current exception.  
- **Throws:** `ArgumentNullException` if `message` is `null`.

### Methods

#### ToString
- **Purpose:** Returns a string representation of the exception that includes the base message and, when available, the process name, arguments, exit code, and standard error.  
- **Return value:** `string` containing the formatted exception information.  
- **Throws:** None.

## Usage

### Example 1: Catching and inspecting the exception
```csharp
try
{
    var result = ProcessRunner.Execute("ffmpeg", "-i input.mp4 output.mkv");
}
catch (ProcessExecutionException ex)
{
    Console.WriteLine($"Process '{ex.ProcessName}' failed.");
    Console.WriteLine($"Arguments: {ex.Arguments}");
    Console.WriteLine($"Exit code: {ex.ExitCode}");
    Console.WriteLine($"Standard error: {ex.StandardError}");
    // Optionally log or rethrow
    throw;
}
```

### Example 2: Throwing the exception from a process wrapper
```csharp
public static int RunProcess(string fileName, string arguments)
{
    using var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }
    };

    process.Start();
    process.WaitForExit();

    if (process.ExitCode != 0)
    {
        var error = process.StandardError.ReadToEnd();
        throw new ProcessExecutionException(
            $"Process exited with code {process.ExitCode}.",
            fileName,
            arguments,
            process.ExitCode,
            error);
    }

    return process.ExitCode;
}
```

## Notes
- The `ProcessName`, `Arguments`, and `StandardError` properties may be `null` if the corresponding information was not available when the exception was constructed; callers should handle null values appropriately.
- Once instantiated, the exception object is immutable; its properties are safe to read from multiple threads without additional synchronization.
- Throwing a `ProcessExecutionException` is thread‑safe, but care should be taken to avoid throwing exceptions from finally blocks that could mask more critical errors.
