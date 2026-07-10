# VideoConversionServiceTests

Unit test class for `VideoConversionService`, covering video conversion, metadata extraction, audio track application, and rescaling operations. Tests validate both happy paths and error conditions using mock or real FFmpeg/FFprobe binaries.

## API

### `VideoConversionServiceTests`
Test fixture class containing methods for verifying `VideoConversionService` functionality.

### `ConvertVideoAsync_ValidInputs_ReturnsOutputPath`
Verifies that a valid input video path produces an output path after successful conversion.
- **Parameters**: None (uses test fixture setup).
- **Return value**: `Task` completing when conversion succeeds.
- **Throws**: Never (success case only).

### `ConvertVideoAsync_InvalidPaths_ThrowsArgumentException`
Ensures conversion fails with `ArgumentException` when input or output paths are invalid.
- **Parameters**: None (uses invalid paths).
- **Return value**: `Task` representing the failed operation.
- **Throws**: `ArgumentException`.

### `ConvertVideoAsync_InputFileNotFound_ThrowsFileNotFoundException`
Validates that missing input files raise `FileNotFoundException`.
- **Parameters**: None (uses non-existent input path).
- **Return value**: `Task` representing the failed operation.
- **Throws**: `FileNotFoundException`.

### `ConvertVideoAsync_FfmpegFails_ThrowsVideoConversionException`
Checks that FFmpeg failures are surfaced as `VideoConversionException`.
- **Parameters**: None (simulates FFmpeg error).
- **Return value**: `Task` representing the failed operation.
- **Throws**: `VideoConversionException`.

### `GetVideoMetadataAsync_ValidFile_ReturnsMetadata`
Tests successful extraction of video metadata from a valid file.
- **Parameters**: None (uses valid test file).
- **Return value**: `Task` completing with metadata object.
- **Throws**: Never (success case only).

### `GetVideoMetadataAsync_InvalidFilePath_ThrowsArgumentException`
Ensures invalid file paths raise `ArgumentException` during metadata extraction.
- **Parameters**: None (uses invalid path).
- **Return value**: `Task` representing the failed operation.
- **Throws**: `ArgumentException`.

### `GetVideoMetadataAsync_InputFileNotFound_ThrowsFileNotFoundException`
Validates that missing files raise `FileNotFoundException` during metadata extraction.
- **Parameters**: None (uses non-existent file path).
- **Return value**: `Task` representing the failed operation.
- **Throws**: `FileNotFoundException`.

### `GetVideoMetadataAsync_FfprobeFails_ThrowsVideoConversionException`
Checks that FFprobe failures are surfaced as `VideoConversionException`.
- **Parameters**: None (simulates FFprobe error).
- **Return value**: `Task` representing the failed operation.
- **Throws**: `VideoConversionException`.

### `GetVideoMetadataAsync_InvalidJsonOutput_ThrowsVideoConversionException`
Ensures malformed FFprobe JSON output raises `VideoConversionException`.
- **Parameters**: None (uses invalid JSON output).
- **Return value**: `Task` representing the failed operation.
- **Throws**: `VideoConversionException`.

### `ApplyAudioTrackAsync_ValidInputs_ReturnsOutputPath`
Verifies that valid video and audio paths produce an output path after audio application.
- **Parameters**: None (uses valid test files).
- **Return value**: `Task` completing when operation succeeds.
- **Throws**: Never (success case only).

### `ApplyAudioTrackAsync_InvalidPaths_ThrowsArgumentException`
Ensures invalid video or audio paths raise `ArgumentException`.
- **Parameters**: None (uses invalid paths).
- **Return value**: `Task` representing the failed operation.
- **Throws**: `ArgumentException`.

### `ApplyAudioTrackAsync_VideoNotFound_ThrowsFileNotFoundException`
Validates that missing video files raise `FileNotFoundException`.
- **Parameters**: None (uses non-existent video path).
- **Return value**: `Task` representing the failed operation.
- **Throws**: `FileNotFoundException`.

### `ApplyAudioTrackAsync_AudioNotFound_ThrowsFileNotFoundException`
Validates that missing audio files raise `FileNotFoundException`.
- **Parameters**: None (uses non-existent audio path).
- **Return value**: `Task` representing the failed operation.
- **Throws**: `FileNotFoundException`.

### `ApplyAudioTrackAsync_FfmpegFails_ThrowsVideoConversionException`
Checks that FFmpeg failures during audio application raise `VideoConversionException`.
- **Parameters**: None (simulates FFmpeg error).
- **Return value**: `Task` representing the failed operation.
- **Throws**: `VideoConversionException`.

### `RescaleVideoAsync_ValidInputs_ReturnsOutputPath`
Tests successful rescaling of a video with valid dimensions and paths.
- **Parameters**: None (uses valid test file and dimensions).
- **Return value**: `Task` completing when rescaling succeeds.
- **Throws**: Never (success case only).

### `RescaleVideoAsync_InvalidPaths_ThrowsArgumentException`
Ensures invalid input or output paths raise `ArgumentException`.
- **Parameters**: None (uses invalid paths).
- **Return value**: `Task` representing the failed operation.
- **Throws**: `ArgumentException`.

### `RescaleVideoAsync_InvalidDimensions_ThrowsArgumentException`
Validates that invalid width or height values raise `ArgumentException`.
- **Parameters**: None (uses invalid dimensions).
- **Return value**: `Task` representing the failed operation.
- **Throws**: `ArgumentException`.

### `RescaleVideoAsync_FfmpegFails_ThrowsVideoConversionException`
Checks that FFmpeg failures during rescaling raise `VideoConversionException`.
- **Parameters**: None (simulates FFmpeg error).
- **Return value**: `Task` representing the failed operation.
- **Throws**: `VideoConversionException`.

### `ConvertToShortsAsync_ValidInputs_ReturnsOutputPath`
Verifies that valid input produces an output path after conversion to Shorts format.
- **Parameters**: None (uses valid test file).
- **Return value**: `Task` completing when conversion succeeds.
- **Throws**: Never (success case only).

## Usage
