# Frequently Asked Questions

## Installation & Setup

**Q: How do I install Coub Downloader?**

A: Choose one of these methods:
- Docker: `docker run vladyslav-zaiets/coub-downloader:latest`
- From source: Clone repo, run `dotnet build -c Release`
- Standalone binary: Download from [Releases](https://github.com/vladyslav-zaiets/coub-downloader/releases)

**Q: Do I need FFmpeg installed?**

A: Yes. Install it:
- Ubuntu/Debian: `sudo apt-get install ffmpeg`
- macOS: `brew install ffmpeg`
- Windows: `choco install ffmpeg`

**Q: What .NET versions are supported?**

A: Only .NET 10.0 and later. Older versions (6, 7, 8, 9) are not supported.

**Q: Can I run this on Windows?**

A: Yes. Download the Windows binary or use Docker. The application is fully cross-platform.

## Usage

**Q: How do I download a single video?**

A: ```bash
coub-downloader download --url https://coub.com/view/2a3b4c5d --output ~/Downloads/video.mp4
```

**Q: What quality options are available?**

A: Five options:
- `low` (240p)
- `medium` (480p)
- `high` (720p) - default
- `ultra` (1080p)
- `4k` (2160p)

**Q: How do I download multiple videos?**

A: Create `urls.txt` with one URL per line, then:
```bash
coub-downloader batch --input urls.txt --output ~/Downloads/coubs
```

**Q: Can I convert to Shorts format?**

A: Yes: `coub-downloader download --url <url> --format shorts`

**Q: How do I handle audio synchronization?**

A: Use the `--audio-loop` flag:
```bash
coub-downloader download --url <url> --audio-loop repeat
```

## Video Conversion

**Q: What video formats are supported?**

A: MP4, WebM, MOV, and Shorts format.

**Q: Can I use custom bitrates?**

A: Yes:
```bash
coub-downloader download --url <url> --bitrate-video 8000 --bitrate-audio 192
```

**Q: How do I preserve aspect ratio?**

A: This is the default. Aspect ratio is preserved automatically unless you specify exact dimensions.

**Q: Can I use GPU acceleration?**

A: Yes, with NVIDIA, AMD, or Intel GPUs:
```bash
export COUB_GPU_CODEC=h264_nvenc
coub-downloader download --url <url> --output video.mp4
```

## Performance

**Q: How fast are downloads?**

A: Speed depends on:
- Video length (typically 8-30 seconds)
- Internet speed
- CPU/GPU performance
- Target quality

Typical: 30-60 seconds for 720p, 1-2 minutes for 4K.

**Q: Can I download multiple videos in parallel?**

A: Yes: `coub-downloader batch --input urls.txt --parallel 8`

**Q: How much disk space do I need?**

A: Depends on quality:
- Low (240p): ~10-20 MB
- Medium (480p): ~30-50 MB
- High (720p): ~50-100 MB
- Ultra (1080p): ~100-200 MB
- 4K: ~200-400 MB

**Q: How can I improve performance?**

A: Try:
- Lower quality setting
- Increase parallel downloads
- Use hardware acceleration
- Use SSD for temporary files
- Close other applications

## Troubleshooting

**Q: "FFmpeg not found" error**

A: Install FFmpeg:
```bash
# Linux
sudo apt-get install ffmpeg

# macOS
brew install ffmpeg

# Windows
choco install ffmpeg
```

**Q: Download timeout**

A: Increase timeout:
```bash
coub-downloader download --url <url> --timeout 600
```

Or check network:
```bash
ping coub.com
```

**Q: "Not enough disk space" error**

A: Check available space:
```bash
df -h
```

Use a different output directory or lower quality.

**Q: Audio not syncing properly**

A: Try different loop strategy:
```bash
coub-downloader download --url <url> --audio-loop fade
```

**Q: Video conversion very slow**

A: Check system resources:
```bash
coub-downloader diagnostic --metrics
```

Try lower quality or enable GPU acceleration.

**Q: SSL certificate error**

A: Update certificates or disable verification (not recommended):
```bash
coub-downloader download --url <url> --no-verify-ssl
```

## Advanced Usage

**Q: Can I use this in a CI/CD pipeline?**

A: Yes. Example for GitHub Actions:
```yaml
- name: Download Coub video
  run: |
    coub-downloader download \
      --url ${{ secrets.COUB_URL }} \
      --output video.mp4
```

**Q: Can I integrate this with my application?**

A: Yes, use as a library:
```csharp
var downloadService = serviceProvider.GetRequiredService<ICoubDownloadService>();
var result = await downloadService.DownloadAsync(url);
```

**Q: How do I handle errors programmatically?**

A: Catch `CoubDownloaderException`:
```csharp
try
{
    var result = await downloadService.DownloadAsync(url);
}
catch (CoubDownloaderException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Code: {ex.ErrorCode}");
}
```

**Q: Can I customize the output naming?**

A: Yes, using naming patterns:
```bash
coub-downloader download \
  --url <url> \
  --output ~/Downloads/{title}_{timestamp}.mp4
```

**Q: How do I subscribe to download events?**

A: ```csharp
eventBus.Subscribe<DownloadCompletedEvent>(async @event =>
{
    Console.WriteLine($"Download completed: {@event.OutputPath}");
});
```

## Legal & Licensing

**Q: Is it legal to download Coub videos?**

A: Check Coub's Terms of Service. This tool is for personal use only.

**Q: Can I redistribute videos?**

A: Only with proper permissions. Always respect copyright.

**Q: What's the license?**

A: MIT License. See LICENSE file.

## Support

**Q: How do I report a bug?**

A: Open an issue on [GitHub Issues](https://github.com/vladyslav-zaiets/coub-downloader/issues)

**Q: How do I request a feature?**

A: Create a feature request in [GitHub Issues](https://github.com/vladyslav-zaiets/coub-downloader/issues)

**Q: Where can I get help?**

A: Check these resources:
- [Documentation](../README.md)
- [Getting Started Guide](./getting-started.md)
- [Examples](../examples/)
- [GitHub Discussions](https://github.com/vladyslav-zaiets/coub-downloader/discussions)
