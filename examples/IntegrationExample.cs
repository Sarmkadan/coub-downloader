using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CoubDownloader.Application.Services;

namespace CoubDownloader.Examples;

/// <summary>
/// Shows how to integrate CoubDownloader services into a standard .NET host (e.g., ASP.NET Core or Worker Service).
/// </summary>
public class IntegrationExample
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Register library services
        services.AddCoubDownloaderServices();
        
        // Register your own service that consumes the library
        services.AddSingleton<MediaIngestionService>();
    }
}

public class MediaIngestionService
{
    private readonly ICoubDownloadService _downloadService;

    // Use constructor injection
    public MediaIngestionService(ICoubDownloadService downloadService)
    {
        _downloadService = downloadService;
    }

    public async Task<string> ProcessUrlAsync(string url)
    {
        var result = await _downloadService.DownloadAsync(url);
        if (!result.Success) throw new Exception(result.ErrorMessage);
        return result.OutputPath;
    }
}
