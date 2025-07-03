// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Application.Services;
using CoubDownloader.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CoubDownloader.Infrastructure;

/// <summary>
/// Extension methods for registering dependencies in the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Add all application services to the DI container</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register HTTP client with default configuration
        services.AddHttpClient<ICoubDownloadService, CoubDownloadService>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

        // Register services
        services.AddScoped<IVideoConversionService, VideoConversionService>();
        services.AddScoped<IAudioProcessingService, AudioProcessingService>();
        services.AddScoped<IBatchProcessingService, BatchProcessingService>();

        return services;
    }

    /// <summary>Add all repositories to the DI container (in-memory for phase 1)</summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Register in-memory repositories
        services.AddSingleton<IDownloadTaskRepository, InMemoryDownloadTaskRepository>();
        services.AddSingleton<ICoubVideoRepository, InMemoryCoubVideoRepository>();
        services.AddSingleton<IBatchJobRepository, InMemoryBatchJobRepository>();
        services.AddSingleton<IDownloadResultRepository, InMemoryDownloadResultRepository>();

        return services;
    }

    /// <summary>Configure all application dependencies</summary>
    public static IServiceCollection AddCoubDownloaderServices(this IServiceCollection services)
    {
        services
            .AddRepositories()
            .AddApplicationServices();

        return services;
    }
}
