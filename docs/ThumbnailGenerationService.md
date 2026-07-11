# ThumbnailGenerationService
The `ThumbnailGenerationService` class is designed to generate thumbnails and contact sheets for videos. It provides methods to create thumbnails at specific percentages of the video duration and to generate contact sheets. This service is part of the `coub-downloader` project, which aims to provide tools for downloading and processing video content from Coub.

## API
The `ThumbnailGenerationService` class has the following public members:
* `public ThumbnailGenerationService`: The constructor for the `ThumbnailGenerationService` class.
* `public async Task<string> GenerateThumbnailAsync`: Generates a thumbnail for a video. The method returns a string representing the thumbnail. It may throw exceptions if there are issues with video processing or if the video is not found.
* `public async Task<string> GenerateThumbnailAtPercentAsync`: Generates a thumbnail for a video at a specified percentage of the video duration. The method returns a string representing the thumbnail. It may throw exceptions if there are issues with video processing, if the video is not found, or if the percentage is invalid.
* `public async Task<string> GenerateContactSheetAsync`: Generates a contact sheet for a video. The method returns a string representing the contact sheet. It may throw exceptions if there are issues with video processing or if the video is not found.

## Usage
Here are two examples of using the `ThumbnailGenerationService` class:
```csharp
// Example 1: Generate a thumbnail
var thumbnailService = new ThumbnailGenerationService();
var thumbnail = await thumbnailService.GenerateThumbnailAsync();
Console.WriteLine(thumbnail);

// Example 2: Generate a thumbnail at 50% of the video duration
var thumbnailService = new ThumbnailGenerationService();
var thumbnail = await thumbnailService.GenerateThumbnailAtPercentAsync(50);
Console.WriteLine(thumbnail);
```

## Notes
When using the `ThumbnailGenerationService` class, note that the `GenerateThumbnailAsync` and `GenerateThumbnailAtPercentAsync` methods may throw exceptions if the video is not found or if there are issues with video processing. The `GenerateContactSheetAsync` method may also throw exceptions if there are issues with video processing. Additionally, the `GenerateThumbnailAtPercentAsync` method may throw an exception if the percentage is invalid (e.g., less than 0 or greater than 100). The `ThumbnailGenerationService` class is designed to be thread-safe, but it is still important to ensure that the class is used in a thread-safe manner to avoid any potential issues.
