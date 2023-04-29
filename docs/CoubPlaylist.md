# CoubPlaylist

Represents a playlist of Coub videos retrieved from the Coub service. This type encapsulates metadata about a playlist and provides methods to access its constituent video URLs.

## API

### `public string Id`
Gets the unique identifier of the playlist. This value is never `null` and is set when the playlist is created.

### `public string Title`
Gets the title of the playlist. This value is never `null` and is set when the playlist is created.

### `public string? Description`
Gets the optional description of the playlist. May be `null` if the playlist has no description.

### `public string PlaylistUrl`
Gets the URL of the playlist on Coub. This value is never `null` and is set when the playlist is created.

### `public List<string> VideoUrls`
Gets the list of video URLs in the playlist. This list is never `null` but may be empty. Modifications to this list affect the internal state of the playlist.

### `public int? MaxVideos`
Gets the maximum number of videos allowed in the playlist, if any. May be `null` if no limit is enforced. This value is set when the playlist is created and does not change.

### `public DateTime CreatedAt`
Gets the timestamp when the playlist was created. This value is never `null` and is set when the playlist is created.

### `public DateTime? FetchedAt`
Gets the timestamp when the playlist was last fetched from Coub, if applicable. May be `null` if the playlist has not been fetched.

### `public bool IsValid`
Gets a value indicating whether the playlist is valid (e.g., required fields are present and non-empty). This value is computed based on the playlist's state and does not change unless the playlist is modified.

### `public bool IsEmpty`
Gets a value indicating whether the playlist contains no videos. This value is computed based on the `VideoUrls` property and does not change unless the playlist is modified.

### `public IEnumerable<string> GetEffectiveVideoUrls()`
Returns an enumerable sequence of video URLs that are considered valid for playback. This method filters out any `null` or empty strings from `VideoUrls` and returns them in their original order. The returned sequence is a new enumeration and does not reflect subsequent changes to `VideoUrls`.

## Usage

### Example 1: Fetching and inspecting a playlist
