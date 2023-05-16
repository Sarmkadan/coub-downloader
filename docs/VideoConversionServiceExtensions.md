# VideoConversionServiceExtensions

The `VideoConversionServiceExtensions` class provides a set of extension methods for video processing tasks within the `coub-downloader` project. These methods facilitate common operations such as converting videos to square aspect ratios, extracting audio, generating thumbnails, and retrieving formatted video durations. The methods are designed to work asynchronously and integrate with the project's existing video handling infrastructure.

## API

### `ConvertToSquareAsync`
Converts a source video file to a square aspect ratio (1:1) by cropping or padding the video as necessary.

**Parameters:**
- `sourcePath` (`string`): The file path of the source video to be converted.
- `outputDirectory` (`string`, optional): The directory where the converted video will be saved. If not specified, the output is saved in the same directory as the source file.
- `outputFileName` (`string`, optional): The name of the output file. If not specified, a default name is generated based on the source file.

**Returns:**
- `Task<string>`: A task that resolves to the full path of the converted video file upon completion.

**Throws:**
- `ArgumentException`: Thrown if `sourcePath` is null, empty, or does not point to a valid file.
- `InvalidOperationException`: Thrown if the conversion process fails (e.g., due to unsupported codec, corrupt file, or insufficient permissions).
- `IOException`: Thrown if there are issues accessing or writing files.

---

### `ExtractAudioAsync`
Extracts the audio track from a source video file and saves it as a separate audio file.

**Parameters:**
- `sourcePath` (`string`): The file path of the source video from which audio will be extracted.
- `outputDirectory` (`string`, optional): The directory where the extracted audio file will be saved. If not specified, the output is saved in the same directory as the source file.
- `outputFileName` (`string`, optional): The name of the output file. If not specified, a default name is generated based on the source file.

**Returns:**
- `Task<string>`: A task that resolves to the full path of the extracted audio file upon completion.

**Throws:**
- `ArgumentException`: Thrown if `sourcePath` is null, empty, or does not point to a valid file.
- `InvalidOperationException`: Thrown if the extraction process fails (e.g., due to unsupported codec, corrupt file, or insufficient permissions).
- `IOException`: Thrown if there are issues accessing or writing files.

---

### `CreateThumbnailAsync`
Generates a thumbnail image from a specified frame of the source video.

**Parameters:**
- `sourcePath` (`string`): The file path of the source video from which the thumbnail will be generated.
- `outputDirectory` (`string`, optional): The directory where the thumbnail will be saved. If not specified, the output is saved in the same directory as the source file.
- `outputFileName` (`string`, optional): The name of the output file. If not specified, a default name is generated based on the source file.
- `timeOffset` (`TimeSpan`, optional): The time offset within the video from which the thumbnail will be generated. Defaults to `TimeSpan.Zero` (beginning of the video).

**Returns:**
- `Task<string>`: A task that resolves to the full path of the generated thumbnail file upon completion.

**Throws:**
- `ArgumentException`: Thrown if `sourcePath` is null, empty, or does not point to a valid file.
- `InvalidOperationException`: Thrown if the thumbnail generation fails (e.g., due to unsupported codec, corrupt file, or insufficient permissions).
- `IOException`: Thrown if there are issues accessing or writing files.

---

### `GetVideoDurationFormattedAsync`
Retrieves the duration of a video file and returns it as a formatted string (e.g., "00:01:23").

**Parameters:**
- `sourcePath` (`string`): The file path of the source video whose duration will be retrieved.

**Returns:**
- `Task<string>`: A task that resolves to the formatted duration string (e.g., "HH:MM:SS").

**Throws:**
- `ArgumentException`: Thrown if `sourcePath` is null, empty, or does not point to a valid file.
- `InvalidOperationException`: Thrown if the duration cannot be retrieved (e.g., due to unsupported codec or corrupt file).

## Usage

### Example 1: Convert a Video to Square and Extract Audio
