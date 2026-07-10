# ConfigurationException

Represents an error that occurs while processing configuration settings in the coub-downloader application. The exception carries an optional `ConfigurationKey` to identify which setting caused the failure, enabling callers to correlate the exception with specific configuration entries.

## API

### `public string? ConfigurationKey { get; set; }`

- **Purpose:** Gets or sets the configuration key associated with the exception.  
- **Parameters:** None.  
- **Return value:** The configuration key as a nullable string; returns `null` if no key is associated.  
- **When it throws:** The property itself does not throw exceptions. Assigning `null` is permitted and indicates an unspecified key.

### `public ConfigurationException(string message) : base(message)`

- **Purpose:** Initializes a new instance with a specified error message.  
- **Parameters:**  
  - `message`: The error message that explains the reason for the exception.  
- **Return value:** (Constructor) Returns the newly created `ConfigurationException` instance.  
- **When it throws:** May throw `ArgumentNullException` if `message` is `null`.

### `public ConfigurationException(string message, string configurationKey) : base(message)`

- **Purpose:** Initializes a new instance with a specified error message and configuration key.  
- **Parameters:**  
  - `message`: The error message.  
  - `configurationKey`: The configuration key related to the error; may be `null`.  
- **Return value:** (Constructor) Returns the newly created `ConfigurationException` instance.  
- **When it throws:** May throw `ArgumentNullException` if `message` is `null`.

### `public ConfigurationException(string message, Exception inner) : base(message, inner)`

- **Purpose:** Initializes a new instance with a specified error message and a reference to the inner exception that caused this exception.  
- **Parameters:**  
  - `message`: The error message.  
  - `inner`: The exception that is the cause of the current exception; may be `null`.  
- **Return value:** (Constructor) Returns the newly created `ConfigurationException` instance.  
- **When it throws:** May throw `ArgumentNullException` if `message` is `null`.

### `public ConfigurationException(string message, string configurationKey, Exception inner) : base(message, inner)`

- **Purpose:** Initializes a new instance with a specified error message, configuration key, and inner exception.  
- **Parameters:**  
  - `message`: The error message.  
  - `configurationKey`: The configuration key related to the error; may be `null`.  
  - `inner`: The exception that is the cause of the current exception; may be `null`.  
- **Return value:** (Constructor) Returns the newly created `ConfigurationException` instance.  
- **When it throws:** May throw `ArgumentNullException` if `message` is `null`.

## Usage

```csharp
// Example 1: Throwing a ConfigurationException with only a message.
if (!TryGetSetting("timeout", out var timeout))
{
    throw new ConfigurationException("The 'timeout' setting is missing or invalid.");
}
```

```csharp
// Example 2: Throwing a ConfigurationException that includes a configuration key and an inner exception.
try
{
    var value = ParseInt(config["maxRetries"]);
}
catch (FormatException ex)
{
    throw new ConfigurationException(
        "The 'maxRetries' setting must be an integer.",
        "maxRetries",
        ex);
}
```

## Notes

- The `ConfigurationKey` property can be read or set after construction, but once the exception instance has been thrown, treating it as immutable is recommended to avoid inconsistent state if accessed concurrently.
- Setting `ConfigurationKey` to `null` is allowed and indicates that no specific configuration key is associated with the error.
- Because `ConfigurationException` derives from `System.Exception`, it inherits the standard thread‑safety guarantees of exception objects: they are safe to read concurrently, but modifications should not be performed after the exception has been observed by other threads.
- None of the constructors perform validation beyond what the base `Exception` class does; callers should ensure that `message` is non‑null to avoid `ArgumentNullException` from the base class.
