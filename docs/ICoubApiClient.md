# ICoubApiClient

`ICoubApiClient` is an interface defining the contract for interacting with the Coub API to retrieve video information and perform searches. It provides methods to fetch details about individual coubs, verify their existence, and search for coubs based on various criteria.

## API

### `public CoubApiClient`
The default implementation of `ICoubApiClient`. This class is responsible for making HTTP requests to the Coub API endpoints and parsing the responses into structured data.

### `public async Task<CoubVideoInfo?> GetVideoInfoAsync`
Fetches detailed information about a specific coub by its ID.

- **Parameters**:
  - `id` (string): The unique identifier of the coub to retrieve.
- **Return value**:
  - `Task<CoubVideoInfo?>`: A task that resolves to a `CoubVideoInfo` object containing the coub's details, or `null` if the coub does not exist or an error occurs.
- **Exceptions**:
  - Throws `HttpRequestException` if the network request fails.
  - Throws `CoubApiException` if the API returns an error response (e.g., invalid ID format).

---

### `public async Task<bool> VerifyVideoExistsAsync`
Checks whether a coub with the given ID exists on the Coub platform.

- **Parameters**:
  - `id` (string): The unique identifier of the coub to verify.
- **Return value**:
  - `Task<bool>`: A task that resolves to `true` if the coub exists, `false` otherwise.
- **Exceptions**:
  - Throws `HttpRequestException` if the network request fails.

---

### `public async Task<List<CoubVideoInfo>> SearchVideosAsync`
Searches for coubs matching the provided query and optional filters.

- **Parameters**:
  - `query` (string): The search term to use.
  - `page` (int, optional): The page number for pagination (default: 1).
  - `perPage` (int, optional): The number of results per page (default: 20, max: 100).
- **Return value**:
  - `Task<List<CoubVideoInfo>>`: A task that resolves to a list of `CoubVideoInfo` objects matching the search criteria.
- **Exceptions**:
  - Throws `HttpRequestException` if the network request fails.
  - Throws `ArgumentOutOfRangeException` if `page` or `perPage` are outside valid ranges.

---

### `public string Id`
The unique identifier of the coub. This is the same identifier used in API requests.

### `public string Title`
The title of the coub, as provided by the uploader.

### `public string? Description`
The description of the coub, if provided by the uploader. May be `null` if no description exists.

### `public int ViewCount`
The total number of views the coub has received.

### `public double Duration`
The duration of the coub in seconds.

### `public string? ChannelUrl`
The URL of the channel that uploaded the coub, if available. May be `null` if the channel information is not provided.

### `public bool HasAudio`
Indicates whether the coub contains audio. `true` if audio is present, `false` otherwise.

## Usage

### Example 1: Fetching a single coub's details
