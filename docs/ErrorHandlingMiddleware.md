# ErrorHandlingMiddleware

A reusable middleware component for ASP.NET Core applications that provides structured error handling, automatic retry logic for transient faults, and detailed error reporting. It transforms unhandled exceptions into consistent `ErrorResponse` objects, supports custom exception handlers, and enables retry policies with exponential backoff for idempotent operations.

## API

### `public ErrorHandlingMiddleware`

Initializes a new instance of the `ErrorHandlingMiddleware` with default retry configuration and no exception handlers registered.

### `public ErrorResponse HandleError(Exception exception)`

Transforms the given exception into a structured `ErrorResponse` containing standardized error details such as status code, message, error type, category, timestamp, and optional technical details.

- **Parameters**
  - `exception` – The caught exception to be processed.
- **Return Value**
  - An `ErrorResponse` object populated with error metadata.
- **Exceptions**
  - Throws `ArgumentNullException` if `exception` is `null`.

### `public void RegisterHandler<TException>(Func<TException, ErrorResponse> handler) where TException : Exception`

Registers a custom error handler for a specific exception type. The handler function receives the exception and returns a tailored `ErrorResponse`.

- **Type Parameters**
  - `TException` – The exception type to handle (e.g., `HttpRequestException`).
- **Parameters**
  - `handler` – A function that maps the exception to an `ErrorResponse`.
- **Exceptions**
  - Throws `ArgumentNullException` if `handler` is `null`.

### `public int StatusCode`

Gets or sets the HTTP status code to be returned in the error response. Defaults to `500` (Internal Server Error).

### `public string Message`

Gets or sets the human-readable error message. If not set, defaults to the exception message.

### `public string ErrorType`

Gets or sets the technical error type identifier (e.g., `"TimeoutException"`, `"ValidationError"`). Used for logging and client-side error classification.

### `public string Category`

Gets or sets the error category (e.g., `"Network"`, `"Validation"`, `"Database"`). Helps group related errors for monitoring and alerting.

### `public DateTime Timestamp`

Gets or sets the timestamp when the error occurred. Defaults to `DateTime.UtcNow` at construction.

### `public string? Details`

Gets or sets optional technical details such as stack traces, inner exception messages, or request context. May be `null`.

### `public Dictionary<string, object>? Metadata`

Gets or sets additional custom metadata to include in the error response. May be `null`.

### `public int MaxRetries`

Gets or sets the maximum number of retry attempts for transient operations. Defaults to `3`.

### `public int InitialDelayMs`

Gets or sets the initial delay in milliseconds before the first retry. Defaults to `200`.

### `public double BackoffMultiplier`

Gets or sets the multiplier applied to the delay after each retry attempt (e.g., `2.0` for exponential backoff). Defaults to `2.0`.

### `public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)`

Executes the provided asynchronous operation with automatic retry logic for transient faults. Retries are applied only if the operation throws an exception derived from `System.Net.Http.HttpRequestException` or `System.IO.IOException`.

- **Type Parameters**
  - `T` – The return type of the operation.
- **Parameters**
  - `operation` – The asynchronous function to execute.
- **Return Value**
  - The result of `operation` if successful.
- **Exceptions**
  - Propagates the last exception if all retry attempts fail.
  - Throws `ArgumentNullException` if `operation` is `null`.

### `public T Execute<T>(Func<T> operation)`

Synchronous variant of `ExecuteAsync<T>`. Executes the provided operation with the same retry logic and transient fault detection.

- **Type Parameters**
  - `T` – The return type of the operation.
- **Parameters**
  - `operation` – The synchronous function to execute.
- **Return Value**
  - The result of `operation` if successful.
- **Exceptions**
  - Propagates the last exception if all retry attempts fail.
  - Throws `ArgumentNullException` if `operation` is `null`.

## Usage

### Example 1: Basic Error Handling in ASP.NET Core
