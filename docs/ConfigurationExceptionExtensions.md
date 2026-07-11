# ConfigurationExceptionExtensions

Provides extension methods for working with `ConfigurationException` instances, enabling detailed error messages, key-based checks, and fluent construction of exceptions with context.

## API

### `GetDetailedMessage`

Extracts a detailed error message from a `ConfigurationException`, including the exception's message and any contextual data attached to it.

- **Parameters**
  - `exception` (`ConfigurationException`): The exception from which to extract the detailed message.
- **Return Value**
  - `string`: The detailed message, or the original message if no context is available.
- **Throws**
  - `ArgumentNullException`: If `exception` is `null`.

---

### `IsForKey`

Determines whether a `ConfigurationException` was raised for a specific configuration key.

- **Parameters**
  - `exception` (`ConfigurationException`): The exception to check.
  - `key` (`string`): The configuration key to compare against.
- **Return Value**
  - `bool`: `true` if the exception's context contains the specified key; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `exception` is `null` or `key` is `null`.

---

### `WithMessage`

Creates a new `ConfigurationException` with an updated error message while preserving any existing context.

- **Parameters**
  - `exception` (`ConfigurationException`): The original exception.
  - `message` (`string`): The new error message to attach.
- **Return Value**
  - `ConfigurationException`: A new exception with the updated message and original context.
- **Throws**
  - `ArgumentNullException`: If `exception` is `null` or `message` is `null`.

---

### `WithContext`

Creates a new `ConfigurationException` with additional contextual data merged into its existing context.

- **Parameters**
  - `exception` (`ConfigurationException`): The original exception.
  - `key` (`string`): The configuration key to add to the context.
  - `value` (`string`): The value associated with the key in the context.
- **Return Value**
  - `ConfigurationException`: A new exception with the merged context.
- **Throws**
  - `ArgumentNullException`: If `exception` is `null`, `key` is `null`, or `value` is `null`.

## Usage

### Validating a configuration key
