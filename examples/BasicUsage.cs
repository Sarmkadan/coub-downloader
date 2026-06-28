using Microsoft.Extensions.DependencyInjection;
using CoubDownloader.Application.Services;

namespace CoubDownloader.Examples;

/// <summary>
/// Demonstrates the most basic usage of the CoubDownloader library:
/// setting up DI and performing a single video download.
/// </summary>
public class BasicUsage
{
    public static async Task RunAsync()
    {
        // 1. Setup DI container
        var services = new ServiceCollection();
        services.AddCoubDownloaderServices(); // Extension method to register library services
        var serviceProvider = services.BuildServiceProvider();

        // 2. Resolve the download service
        var downloadService = serviceProvider.GetRequiredService<ICoubDownloadService>();

        // 3. Perform the download
        Console.WriteLine("Starting download...");
        var result = await downloadService.DownloadAsync("https://coub.com/view/2a3b4c5d");

        // 4. Handle result
        if (result.Success)
        {
            Console.WriteLine($"Successfully downloaded to: {result.OutputPath}");
        }
        else
        {
            Console.WriteLine($"Download failed: {result.ErrorMessage}");
        }
    }
}
