#nullable enable
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

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadStage"/> class.
    /// </summary>
    /// <param name="downloadService">The download service to use for downloading videos.</param>
    public DownloadStage(ICoubDownloadService downloadService)
    {
        _downloadService = downloadService;
    }

    /// <summary>
    /// Executes the download stage asynchronously.
    /// </summary>
    /// <param name="url">The URL to download.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A download task representing the downloaded video.</returns>
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

    /// <summary>
    /// Executes the validation stage asynchronously.
    /// </summary>
    /// <param name="task">The download task to validate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The validated download task.</returns>
    public Task<DownloadTask> ExecuteAsync(DownloadTask task, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(task.Url))
            throw new InvalidOperationException("URL is required");

        if (task.OutputPath is not null && Path.GetInvalidPathChars().Any(c => task.OutputPath.Contains(c)))
            throw new InvalidOperationException("Invalid output path");

        return Task.FromResult(task);
    }
}

/// <summary>Conversion stage</summary>
public class ConversionStage : IPipelineStage<DownloadTask, ConversionResult>
{
    private readonly IVideoConversionService _conversionService;

    public string Name => "Convert";

    /// <summary>
    /// Initializes a new instance of the <see cref="ConversionStage"/> class.
    /// </summary>
    /// <param name="conversionService">The video conversion service to use.</param>
    public ConversionStage(IVideoConversionService conversionService)
    {
        _conversionService = conversionService;
    }

    /// <summary>
    /// Executes the conversion stage asynchronously.
    /// </summary>
    /// <param name="task">The download task containing video to convert.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The conversion result.</returns>
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

    /// <summary>
    /// Executes the cleanup stage asynchronously.
    /// </summary>
    /// <param name="result">The conversion result to clean up after.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cleaned-up conversion result.</returns>
    public Task<ConversionResult> ExecuteAsync(ConversionResult result, CancellationToken cancellationToken = default)
    {
        // Remove temporary files
        return Task.FromResult(result);
    }
}

/// <summary>Conversion result</summary>
public class ConversionResult
{
    /// <summary>
    /// Gets or sets the task identifier.
    /// </summary>
    public string TaskId { get; set; } = "";

    /// <summary>
    /// Gets or sets a value indicating whether the conversion was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the output path of the converted file.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Gets or sets the error message if conversion failed.
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>Pipeline builder for fluent configuration</summary>
public class PipelineBuilder
{
    private readonly ConversionPipeline _pipeline;
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineBuilder"/> class.
    /// </summary>
    /// <param name="logger">The logging service to use.</param>
    public PipelineBuilder(ILoggingService logger)
    {
        _logger = logger;
        _pipeline = new ConversionPipeline(logger);
    }

    /// <summary>
    /// Adds a download stage to the pipeline.
    /// </summary>
    /// <param name="downloadService">The download service to use.</param>
    /// <returns>The pipeline builder for chaining.</returns>
    public PipelineBuilder WithDownload(ICoubDownloadService downloadService)
    {
        _pipeline.AddStage(new DownloadStage(downloadService));
        return this;
    }

    /// <summary>
    /// Adds a validation stage to the pipeline.
    /// </summary>
    /// <returns>The pipeline builder for chaining.</returns>
    public PipelineBuilder WithValidation()
    {
        _pipeline.AddStage(new ValidationStage());
        return this;
    }

    /// <summary>
    /// Adds a conversion stage to the pipeline.
    /// </summary>
    /// <param name="conversionService">The video conversion service to use.</param>
    /// <returns>The pipeline builder for chaining.</returns>
    public PipelineBuilder WithConversion(IVideoConversionService conversionService)
    {
        _pipeline.AddStage(new ConversionStage(conversionService));
        return this;
    }

    /// <summary>
    /// Adds a cleanup stage to the pipeline.
    /// </summary>
    /// <returns>The pipeline builder for chaining.</returns>
    public PipelineBuilder WithCleanup()
    {
        _pipeline.AddStage(new CleanupStage());
        return this;
    }

    /// <summary>
    /// Builds the conversion pipeline.
    /// </summary>
    /// <returns>The configured conversion pipeline.</returns>
    public ConversionPipeline Build()
    {
        return _pipeline;
    }
}
