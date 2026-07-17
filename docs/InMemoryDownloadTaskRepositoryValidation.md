# InMemoryDownloadTaskRepositoryValidation

`InMemoryDownloadTaskRepositoryValidation` is a static utility class responsible for validating the state and configuration of an in-memory download task repository. It provides methods to check validity, retrieve validation errors, and enforce validation constraints, ensuring that the repository adheres to expected operational requirements before use.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate()
```

Returns a read-only list of validation error messages describing why the repository is invalid. If the repository is valid, an empty list is returned. This method performs a comprehensive validation of the repository's current state.

**Parameters:** None.

**Return Value:** `IReadOnlyList<string>` containing validation errors, or an empty list if valid.

**Exceptions:** None.

---

```csharp
public static IReadOnlyList<string> Validate(InMemoryDownloadTaskRepository repository)
```

Validates a specific instance of `InMemoryDownloadTaskRepository` and returns a list of error messages. This overload allows validation of a provided repository instance rather than the default or current state.

**Parameters:**
- `repository`: The `InMemoryDownloadTaskRepository` instance to validate.

**Return Value:** `IReadOnlyList<string>` containing validation errors, or an empty list if valid.

**Exceptions:** None.

---

### IsValid

```csharp
public static bool IsValid()
```

Indicates whether the repository is currently in a valid state. Returns `true` if no validation errors exist, `false` otherwise.

**Parameters:** None.

**Return Value:** `bool` indicating validity.

**Exceptions:** None.

---

```csharp
public static bool IsValid(InMemoryDownloadTaskRepository repository)
```

Determines the validity of a provided `InMemoryDownloadTaskRepository` instance. Returns `true` if the instance is valid, `false` otherwise.

**Parameters:**
- `repository`: The `InMemoryDownloadTaskRepository` instance to check.

**Return Value:** `bool` indicating validity.

**Exceptions:** None.

---

### EnsureValid

```csharp
public static void EnsureValid()
```

Throws an exception if the repository is invalid. This method is useful for precondition checks where invalid state should halt execution immediately.

**Parameters:** None.

**Return Value:** `void`.

**Exceptions:** Throws `InvalidOperationException` if the repository is invalid.

---

```csharp
public static void EnsureValid(InMemoryDownloadTaskRepository repository)
```

Throws an exception if the provided `InMemoryDownloadTaskRepository` instance is invalid. Ensures that a specific repository meets validation criteria before proceeding.

**Parameters:**
- `repository`: The `InMemoryDownloadTaskRepository` instance to validate.

**Return Value:** `void`.

**Exceptions:** Throws `InvalidOperationException` if the repository is invalid.

## Usage

### Basic Validation Check

```csharp
if (InMemoryDownloadTaskRepositoryValidation.IsValid())
{
    Console.WriteLine("Repository is valid.");
}
else
{
    var errors = InMemoryDownloadTaskRepositoryValidation.Validate();
    Console.WriteLine($"Validation errors: {string.Join(", ", errors)}");
}
```

### Enforcing Validity Before Operation

```csharp
try
{
    InMemoryDownloadTaskRepositoryValidation.EnsureValid();
    // Proceed with repository operations
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Repository validation failed: {ex.Message}");
}
```

## Notes

- **Thread Safety:** The static methods access shared state and are not inherently thread-safe. Concurrent modifications to the underlying repository during validation may result in inconsistent validation outcomes. External synchronization is required when validating repositories in multi-threaded contexts.
- **Edge Cases:** Calling `Validate()` or `IsValid()` on an uninitialized or disposed repository may return unexpected results. Ensure the repository is properly initialized before validation.
- **Performance Considerations:** Repeated calls to `Validate()` may incur performance overhead if the repository contains a large number of tasks. Cache results when possible to avoid redundant validation checks.
