# CoubVideoProcessingExtensions

`CoubVideoProcessingExtensions` is a data transfer object that encapsulates all configuration and metadata required for processing a Coub video. It is used throughout the application to pass processing parameters from the UI or API layer to the batch processing and conversion services. The type is intentionally simple, exposing only the properties that influence the conversion pipeline.

## API

| Property | Purpose | Return Type | Throws |
|----------|---------|-------------|--------|
| `Id` | Unique identifier for the processing configuration. | `string` | None |
| `VideoId` | Identifier of the source Coub video. | `string` | None |
| `Priority` | Numerical priority used by the scheduler to order jobs. Lower values indicate higher priority. | `int` | None |
| `EnableHardwareAcceleration` | Flag indicating whether FFmpeg should use hardware acceleration. | `bool` | None |
| `VideoCodec` | Name of the video codec to use (e.g., `"h264"`). | `string` | None |
| `AudioCodec` | Name of the audio codec to use (e.g., `"aac"`). | `string` | None |
| `VideoBitrate` | Target video bitrate in kilobits per second. | `int` | None |
| `AudioBitrate` | Target audio bitrate in kilobits per second. | `int` | None |
| `TargetWidth` | Desired output width in pixels. | `int` | None |
| `TargetHeight` | Desired output height in pixels. | `int` | None |
| `PreserveAspectRatio` | Flag indicating whether the original aspect ratio should be maintained when resizing. | `bool` | None |
| `MaxDuration` | Maximum allowed duration for the output video in seconds. | `double` | None |
| `Tags` | Collection of tags associated with the processing job. | `List<string>` | None |
| `ProfileName` | Optional name of a predefined processing profile. | `string?` | None |
| `CreatedAt` | Timestamp when the configuration was created. | `DateTime` | None |
| `UpdatedAt` | Timestamp when the configuration was last modified. | `DateTime` | None |
| `IsValid` | Indicates whether the configuration passes basic validation rules. | `bool` | None |

> **Note**: All properties are read/write. The type does not perform validation internally; callers should check `IsValid` before submitting a job.

## Usage

