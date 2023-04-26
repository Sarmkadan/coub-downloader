# NetworkException

A specialized exception type used in the coub-downloader project to represent network-related failures during HTTP requests or other network operations, providing additional context such as the target URL, HTTP status code, and timeout status.

## API

### Fields

- **`public string? Url`**
  The URL associated with the failed network operation. May be `null` if the URL was not available when the exception was created.

- **`public int? HttpStatusCode`**
  The HTTP status code returned by the server, if applicable. May be `null` if the request did not complete with a status code (e.g., due to a timeout or connection failure).

- **`public bool IsTimeout`**
  Indicates whether the exception was caused by a network timeout. Useful for distinguishing between transient and non-transient failures.

### Constructors

- **`public NetworkException(string message)`**
  Initializes a new instance with a descriptive error message. All other fields (`Url`, `HttpStatusCode`, `IsTimeout`) are left as `null`.

- **`public NetworkException(string message, string url)`**
  Initializes a new instance with a message and the URL that was being accessed when the failure occurred. `HttpStatusCode` and `IsTimeout` are left as `null`.

- **`public NetworkException(string message, string url, int statusCode)`**
  Initializes a new instance with a message, the target URL, and the HTTP status code returned by the server. `IsTimeout` is left as `false`.

- **`public NetworkException(string message, Exception inner)`**
  Initializes a new instance with a message and an inner exception that caused this exception. `Url`, `HttpStatusCode`, and `IsTimeout` are left as `null`.

- **`public NetworkException(string message, string url, Exception inner)`**
  Initializes a new instance with a message, the target URL, and an inner exception. `HttpStatusCode` and `IsTimeout` are left as `null`.

### Methods

- **`public override string ToString()`**
  Returns a string representation of the exception, including the message, URL (if available), HTTP status code (if available), and timeout status. The output is formatted for readability and debugging purposes.

## Usage
