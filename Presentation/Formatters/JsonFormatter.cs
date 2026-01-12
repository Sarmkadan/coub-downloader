// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Presentation.Formatters;

/// <summary>Formats domain models to JSON output</summary>
public class JsonFormatter
{
    private readonly JsonSerializerOptions _options;

    public JsonFormatter()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>Format a Coub video to JSON</summary>
    public string FormatVideo(CoubVideo video)
    {
        var dto = new
        {
            video.Id,
            video.Title,
            video.Url,
            video.Duration,
            video.Width,
            video.Height,
            video.CreatorName,
            video.ViewCount,
            video.HasAudio,
            video.Description,
            video.UploadedDate
        };

        return JsonSerializer.Serialize(dto, _options);
    }

    /// <summary>Format multiple Coub videos to JSON</summary>
    public string FormatVideos(IEnumerable<CoubVideo> videos)
    {
        var dtos = videos.Select(v => new
        {
            v.Id,
            v.Title,
            v.Url,
            v.Duration,
            v.Width,
            v.Height,
            v.CreatorName,
            v.ViewCount,
            v.HasAudio
        }).ToList();

        return JsonSerializer.Serialize(dtos, _options);
    }

    /// <summary>Format a batch job to JSON</summary>
    public string FormatBatchJob(BatchJob batch)
    {
        var dto = new
        {
            batch.Id,
            batch.Name,
            batch.OutputDirectory,
            batch.State,
            batch.CreatedAt,
            batch.CompletedAt,
            TaskCount = batch.Tasks.Count,
            Status = new
            {
                TotalTasks = batch.Tasks.Count,
                CompletedTasks = batch.Tasks.Count(t => t.State == ProcessingState.Completed),
                FailedTasks = batch.Tasks.Count(t => t.State == ProcessingState.Failed),
                PendingTasks = batch.Tasks.Count(t => t.State == ProcessingState.Pending)
            }
        };

        return JsonSerializer.Serialize(dto, _options);
    }

    /// <summary>Format conversion settings to JSON</summary>
    public string FormatSettings(ConversionSettings settings)
    {
        var dto = new
        {
            settings.Format,
            settings.Quality,
            settings.VideoBitrate,
            settings.AudioBitrate,
            settings.FrameRate,
            settings.Width,
            settings.Height,
            settings.PreserveAspectRatio,
            settings.VideoCodec,
            settings.AudioCodec,
            settings.EnableHardwareAcceleration
        };

        return JsonSerializer.Serialize(dto, _options);
    }
}
