# ValidationException

Represents a validation failure that occurred while checking a method argument or property value. It carries the name of the offending parameter and the invalid value that caused the check to fail, allowing callers to diagnose validation problems without parsing the exception message.

## API

### ParameterName property
- **Purpose:** Gets the name of the parameter that failed validation, if it was supplied when the exception was created.  
- **Return value:** `string?` containing the parameter name, or `null` when no parameter name was provided.  
- **Throws:** None.

### InvalidValue property
- **Purpose:** Gets the value that failed validation, if it was supplied when the exception was created.  
- **Return value:** `object?` containing the invalid value, or `null` when no invalid value was supplied.  
- **Throws:** None.

### ValidationException(string message)
- **Purpose:** Initializes a new instance with a specified error message.  
- **Parameters:**  
  - `message` – The message that describes the error.  
- **Return value:** A new `ValidationException` instance.  
- **Throws:** `ArgumentNullException` if `message` is `null`.

### ValidationException(string message, string parameterName, object? invalidValue)
- **Purpose:** Initializes a new instance with a specified error message, the name of the invalid parameter, and the invalid value.  
- **Parameters:**  
  - `message` – The message that describes the error.  
  - `parameterName` – The name of the parameter that caused the validation failure.  
  - `invalidValue` – The value that failed validation (may be `null`).  
- **Return value:** A new `ValidationException` instance.  
- **Throws:** `ArgumentNullException` if `message` is `null`.

### ValidationException(string message, string parameterName, object? invalidValue, Exception inner)
- **Purpose:** Initializes a new instance with a specified error message, parameter name, invalid value, and a reference to the inner exception that caused this exception.  
- **Parameters:**  
  - `message` – The message that describes the error.  
  - `parameterName` – The name of the parameter that caused the validation failure.  
  - `invalidValue` – The value that failed validation (may be `null`).  
  - `inner` – The exception that is the cause of the current exception (may be `null`).  
- **Return value:** A new `ValidationException` instance.  
- **Throws:** `ArgumentNullException` if `message` is `null`.

### ValidationException(string message, Exception inner)
- **Purpose:** Initializes a new instance with a specified error message and a reference to the inner exception that caused this exception.  
- **Parameters:**  
  - `message` – The message that describes the error.  
  - `inner` – The exception that is the cause of the current exception (may be `null`).  
- **Return value:** A new `ValidationException` instance.  
- **Throws:** `ArgumentNullException` if `message` is `null`.

### ToString()
- **Purpose:** Returns a string representation of the exception, including the message and, when available, the parameter name and invalid value.  
- **Parameters:** None.  
- **Return value:** A `string` that represents the current exception.  
- **Throws:** None.

## Usage

```csharp
public void SetAge(int age)
{
    if (age < 0)
    {
        // Inform the caller which argument is invalid and what value was supplied.
        throw new ValidationException(
            "Age must be non‑negative.",
            nameof(age),
            age);
    }

    _age = age;
}
```

```csharp
try
{
    user.SetAge(-5);
}
catch (ValidationException vex)
{
    // Log detailed information for debugging.
    Console.WriteLine($"Validation failed for parameter '{vex.ParameterName}': {vex.InvalidValue}");
    Console.WriteLine(vex.Message);
}
```

## Notes

- If a constructor that does not accept `parameterName` or `invalidValue` is used, the corresponding properties will return `null`.  
- The `ToString` implementation includes the base message; when `ParameterName` or `InvalidValue` are non‑null they are appended to the output to aid diagnostics.  
- The exception object construction is complete, the instance is immutable; reading its properties from multiple threads is safe and requires no additional synchronization.  
- Deriving from `System.Exception` ensures that `ValidationException` behaves correctly with standard exception‑handling mechanisms (e.g., `catch (Exception)`).
