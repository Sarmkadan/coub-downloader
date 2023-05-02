# CoubVideo
The `CoubVideo` type represents a video from the Coub platform, encapsulating its metadata and properties. It is designed to provide a structured and accessible way to work with Coub video data, allowing for easy retrieval and manipulation of video information.

## API
The `CoubVideo` type exposes the following public members:
* `Id`: A unique identifier for the video, represented as a `string`.
* `Title`: The title of the video, represented as a `string`.
* `Url`: The URL of the video, represented as a `string`.
* `Duration`: The duration of the video in seconds, represented as a `double`.
* `Width` and `Height`: The width and height of the video in pixels, represented as `int` values.
* `SourceUrl`: The URL of the video's source, represented as a nullable `string`.
* `ThumbnailUrl`: The URL of the video's thumbnail, represented as a nullable `string`.
* `CreatorName`: The name of the video's creator, represented as a nullable `string`.
* `Description`: The description of the video, represented as a nullable `string`.
* `UploadedDate`: The date the video was uploaded, represented as a nullable `DateTime`.
* `ViewCount`: The number of views the video has, represented as a `long`.
* `HasAudio`: A boolean indicating whether the video has audio.
* `AudioTrack`: The audio track of the video, represented as a nullable `AudioTrack`.
* `Sections`: A list of video sections, represented as a `List<VideoSection>`.
* `CreatedAt` and `UpdatedAt`: The dates the video object was created and last updated, represented as `DateTime` values.
* `IsValid`: A boolean indicating whether the video object is valid.
* `GetAspectRatio`: A decimal representing the aspect ratio of the video.
* `IsVerticalFormat`: A boolean indicating whether the video is in vertical format.

## Usage
Here are two examples of using the `CoubVideo` type:
```csharp
// Example 1: Creating a new CoubVideo object
CoubVideo video = new CoubVideo
{
    Id = "12345",
    Title = "Example Video",
    Url = "https://coub.com/view/12345",
    Duration = 10.5,
    Width = 1280,
    Height = 720,
    SourceUrl = "https://example.com/source",
    ThumbnailUrl = "https://example.com/thumbnail",
    CreatorName = "John Doe",
    Description = "This is an example video.",
    UploadedDate = DateTime.Parse("2022-01-01"),
    ViewCount = 100,
    HasAudio = true,
    AudioTrack = new AudioTrack(),
    Sections = new List<VideoSection>(),
    CreatedAt = DateTime.Now,
    UpdatedAt = DateTime.Now,
    IsValid = true
};

// Example 2: Retrieving video metadata
CoubVideo video2 = GetCoubVideo("12345"); // Assume GetCoubVideo is a method that retrieves a CoubVideo object
Console.WriteLine($"Title: {video2.Title}, Duration: {video2.Duration}, View Count: {video2.ViewCount}");
```

## Notes
When working with `CoubVideo` objects, consider the following:
* The `SourceUrl`, `ThumbnailUrl`, `CreatorName`, and `Description` properties may be null if the corresponding data is not available.
* The `UploadedDate` property may be null if the video's upload date is not available.
* The `IsValid` property should be checked before using a `CoubVideo` object to ensure it is valid.
* The `GetAspectRatio` property returns a decimal value representing the aspect ratio of the video.
* The `IsVerticalFormat` property returns a boolean indicating whether the video is in vertical format.
* `CoubVideo` objects are not thread-safe by default. If you need to access or modify `CoubVideo` objects from multiple threads, you should implement appropriate synchronization mechanisms to ensure thread safety.
