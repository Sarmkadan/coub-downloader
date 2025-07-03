// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Infrastructure.BackgroundJobs;
using CoubDownloader.Infrastructure.Caching;
using CoubDownloader.Infrastructure.Configuration;
using CoubDownloader.Infrastructure.Events;
using CoubDownloader.Infrastructure.Integration;
using CoubDownloader.Infrastructure.Middleware;
using CoubDownloader.Infrastructure.Pipeline;
using CoubDownloader.Infrastructure.Reporting;
using CoubDownloader.Infrastructure.Security;
using CoubDownloader.Infrastructure.Statistics;
using CoubDownloader.Infrastructure.Utilities;
using CoubDownloader.Presentation.CLI;
using CoubDownloader.Presentation.Formatters;
using Microsoft.Extensions.DependencyInjection;

namespace CoubDownloader.Infrastructure;

/// <summary>Extended DI configuration for Phase 2 features</summary>
public static class DependencyInjectionExtended
{
    /// <summary>Add all Phase 2 middleware and utilities</summary>
    public static IServiceCollection AddPhase2Services(this IServiceCollection services)
    {
        // Logging & Middleware
        services.AddSingleton<ILoggingService>(new FileLoggingService("./logs"));
        services.AddSingleton<ErrorHandlingMiddleware>();
        services.AddSingleton(new RateLimitingService(100, 60));
        services.AddSingleton(new ThrottlingService(10, 20));

        // Caching
        services.AddSingleton<ICacheService>(new MemoryCacheService(3600));

        // Configuration
        services.AddSingleton(new ConfigurationManager("appsettings.json"));

        // CLI & Formatters
        services.AddSingleton<CommandLineInterface>();

        // API Integration
        services.AddHttpClient<ICoubApiClient, CoubApiClient>();

        services.AddSingleton<FFmpegWrapper>();
        services.AddSingleton<WebhookManager>();

        // Security
        services.AddSingleton<ICredentialManager>(new InMemoryCredentialManager());
        services.AddSingleton<RequestContextAccessor>();

        // Performance & Reporting
        services.AddSingleton<PerformanceMonitor>();
        services.AddSingleton<ExportService>();
        services.AddSingleton<TableFormatter>();

        // Events
        services.AddSingleton<IEventBus>(new InProcessEventBus());

        // Background Jobs
        services.AddSingleton(new MonitoringWorker());
        services.AddSingleton(new CleanupWorker("./downloads", 30));

        // Utilities
        services.AddSingleton(new FeatureFlags());

        return services;
    }

}


/// <summary>Service locator extension</summary>
public static class ServiceLocatorExtensions
{
    private static IServiceProvider? _serviceProvider;

    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static T? GetService<T>() where T : class
    {
        return _serviceProvider?.GetService(typeof(T)) as T;
    }
}
