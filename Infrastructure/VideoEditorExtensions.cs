#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoubDownloader.Infrastructure;

/// <summary>
/// Provides extension methods for registering video editor services in the dependency injection container.
/// </summary>
public static class VideoEditorExtensions
{
    /// <summary>
    /// Registers <see cref="IVideoEditorService"/> and <see cref="VideoEditorService"/>
    /// as scoped dependencies in the dependency injection container.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method depends on <see cref="CoubDownloader.Infrastructure.Integration.FFmpegWrapper"/>
    /// and <see cref="CoubDownloader.Infrastructure.Middleware.ILoggingService"/> already being
    /// registered. Call <see cref="DependencyInjectionExtended.AddPhase2Services"/> before
    /// invoking this extension, or use the convenience method
    /// <see cref="AddCoubDownloaderWithEditor"/> which satisfies all prerequisites automatically.
    /// </para>
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    /// <returns>The same <paramref name="services"/> instance for method chaining.</returns>
    public static IServiceCollection AddVideoEditorServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IVideoEditorService, VideoEditorService>();
        return services;
    }

    /// <summary>
    /// Convenience bootstrap method that registers the complete coub-downloader v2.0 stack:
    /// core services, repositories, Phase 2 infrastructure, and the video editor feature.
    /// </summary>
    /// <remarks>
    /// Equivalent to calling <see cref="DependencyInjection.AddCoubDownloaderServices"/>,
    /// <see cref="DependencyInjectionExtended.AddPhase2Services"/>, and
    /// <see cref="AddVideoEditorServices"/> in sequence.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> containing application configuration.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>.
    /// </exception>
    /// <returns>The same <paramref name="services"/> instance for method chaining.</returns>
    public static IServiceCollection AddCoubDownloaderWithEditor(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        return services
            .AddCoubDownloaderServices(configuration)
            .AddPhase2Services()
            .AddVideoEditorServices();
    }
}