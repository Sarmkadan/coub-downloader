// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;
using CoubDownloader.Infrastructure.Middleware;

namespace CoubDownloader.Infrastructure.Pipeline;

/// <summary>Pipeline stage for processing</summary>
public interface IPipelineStage<TInput, TOutput>
{
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken = default);
    string Name { get; }
}

/// <summary>Pipeline for chaining conversion operations</summary>
public class ConversionPipeline
{
    private readonly List<IPipelineStage<object, object>> _stages = [];
    private readonly ILoggingService _logger;

    public ConversionPipeline(ILoggingService logger)
    {
        _logger = logger;
    }

    /// <summary>Add a stage to the pipeline</summary>
    public ConversionPipeline AddStage<TInput, TOutput>(IPipelineStage<TInput, TOutput> stage)
    {
        _stages.Add(new StageAdapter<TInput, TOutput>(stage));
        return this;
    }

    /// <summary>Execute the pipeline</summary>
    public async Task<object> ExecuteAsync(object input, CancellationToken cancellationToken = default)
    {
        var current = input;

        foreach (var stage in _stages)
        {
            try
            {
                _logger.LogInfo($"Executing pipeline stage: {stage.Name}", "Pipeline");
                current = await stage.ExecuteAsync(current, cancellationToken);
                _logger.LogInfo($"Stage completed: {stage.Name}", "Pipeline");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Pipeline stage failed: {stage.Name}", ex, "Pipeline");
                throw;
            }
        }

        return current;
    }

    private class StageAdapter<TInput, TOutput> : IPipelineStage<object, object>
    {
        private readonly IPipelineStage<TInput, TOutput> _stage;

        public string Name => _stage.Name;

        public StageAdapter(IPipelineStage<TInput, TOutput> stage)
        {
            _stage = stage;
        }

        public async Task<object> ExecuteAsync(object input, CancellationToken cancellationToken = default)
        {
            var typedInput = (TInput)input;
            var result = await _stage.ExecuteAsync(typedInput, cancellationToken);
            return result!;
        }
    }
}

/// <summary>Download stage in conversion pipeline</summary>
public class DownloadStage : IPipelineStage<string, DownloadTask>
{
    private readonly ICoubDownloadService _downloadService;

    public string Name => "Download";

    public DownloadStage(ICoubDownloadService downloadService)
    {
        _downloadService = downloadService;
    }

    public async Task<DownloadTask> ExecuteAsync(string url, CancellationToken cancellationToken = default)
    {
        var task = new DownloadTask
        {
            Id = Guid.NewGuid().ToString(),
            Url = url,
            State = ProcessingState.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var video = await _downloadService.DownloadVideoAsync(url, cancellationToken);

        return task;
    }
}

/// <summary>Validation stage</summary>
public class ValidationStage : IPipelineStage<DownloadTask, DownloadTask>
{
    public string Name => "Validate";

    public Task<DownloadTask> ExecuteAsync(DownloadTask task, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(task.Url))
            throw new InvalidOperationException("URL is required");

        if (task.OutputPath != null && Path.GetInvalidPathChars().Any(c => task.OutputPath.Contains(c)))
            throw new InvalidOperationException("Invalid output path");

        return Task.FromResult(task);
    }
}

/// <summary>Conversion stage</summary>
public class ConversionStage : IPipelineStage<DownloadTask, ConversionResult>
{
    private readonly IVideoConversionService _conversionService;

    public string Name => "Convert";

    public ConversionStage(IVideoConversionService conversionService)
    {
        _conversionService = conversionService;
    }

    public async Task<ConversionResult> ExecuteAsync(DownloadTask task, CancellationToken cancellationToken = default)
    {
        var settings = new ConversionSettings { Format = task.Format, Quality = task.Quality };
        settings.ApplyQualityPreset();

        var outputPath = await _conversionService.ConvertVideoAsync(
            task.OutputPath ?? "input.mp4",
            "output.mp4",
            settings,
            null,
            cancellationToken);

        var success = !string.IsNullOrEmpty(outputPath);

        return new ConversionResult
        {
            TaskId = task.Id,
            Success = success,
            OutputPath = outputPath
        };
    }
}

/// <summary>Cleanup stage</summary>
public class CleanupStage : IPipelineStage<ConversionResult, ConversionResult>
{
    public string Name => "Cleanup";

    public Task<ConversionResult> ExecuteAsync(ConversionResult result, CancellationToken cancellationToken = default)
    {
        // Remove temporary files
        return Task.FromResult(result);
    }
}

/// <summary>Conversion result</summary>
public class ConversionResult
{
    public string TaskId { get; set; } = "";
    public bool Success { get; set; }
    public string? OutputPath { get; set; }
    public string? Error { get; set; }
}

/// <summary>Pipeline builder for fluent configuration</summary>
public class PipelineBuilder
{
    private readonly ConversionPipeline _pipeline;
    private readonly ILoggingService _logger;

    public PipelineBuilder(ILoggingService logger)
    {
        _logger = logger;
        _pipeline = new ConversionPipeline(logger);
    }

    public PipelineBuilder WithDownload(ICoubDownloadService downloadService)
    {
        _pipeline.AddStage(new DownloadStage(downloadService));
        return this;
    }

    public PipelineBuilder WithValidation()
    {
        _pipeline.AddStage(new ValidationStage());
        return this;
    }

    public PipelineBuilder WithConversion(IVideoConversionService conversionService)
    {
        _pipeline.AddStage(new ConversionStage(conversionService));
        return this;
    }

    public PipelineBuilder WithCleanup()
    {
        _pipeline.AddStage(new CleanupStage());
        return this;
    }

    public ConversionPipeline Build()
    {
        return _pipeline;
    }
}
