# Getting Started with Coub Downloader

This guide will help you install and start using Coub Downloader in minutes.

## Installation

### Quick Install with Docker

```bash
docker run -it \
  -v /path/to/downloads:/downloads \
  vladyslav-zaiets/coub-downloader:latest \
  download --url https://coub.com/view/2a3b4c5d --output /downloads/video.mp4
```

### Install from Source

**Prerequisites:**
- .NET SDK 10.0 or later
- Git
- FFmpeg 6.0+

**Steps:**

```bash
# Clone repository
git clone https://github.com/vladyslav-zaiets/coub-downloader.git
cd coub-downloader

# Restore NuGet packages
dotnet restore

# Build project
dotnet build -c Release

# Run
dotnet run --project CoubDownloader.csproj
```

### Install Standalone Binary

Download from [Releases](https://github.com/vladyslav-zaiets/coub-downloader/releases):

```bash
# Extract and set executable permission
tar xzf coub-downloader-linux-x64.tar.gz
chmod +x coub-downloader

# Run
./coub-downloader --version
```

## Verify Installation

```bash
# Check version
coub-downloader --version

# Verify FFmpeg
coub-downloader diagnostic --check-ffmpeg
```

## First Download

### Command Line

```bash
# Simple download
coub-downloader download \
  --url https://coub.com/view/2a3b4c5d \
  --output ~/Downloads/video.mp4
```

### Using .NET Code

```csharp
using CoubDownloader.Application.Services;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddCoubDownloaderServices().AddHttpClient();
var provider = services.BuildServiceProvider();

var downloadService = provider.GetRequiredService<ICoubDownloadService>();
var result = await downloadService.DownloadAsync("https://coub.com/view/2a3b4c5d");

Console.WriteLine($"Downloaded: {result.OutputPath}");
```

## Common Tasks

### Download with Specific Quality

```bash
coub-downloader download \
  --url https://coub.com/view/2a3b4c5d \
  --output ~/Downloads/video.mp4 \
  --quality ultra
```

Quality options: `low`, `medium`, `high`, `ultra`, `4k`

### Convert to Shorts Format

```bash
coub-downloader download \
  --url https://coub.com/view/2a3b4c5d \
  --output ~/Downloads/shorts.mp4 \
  --format shorts
```

### Batch Download Multiple Videos

```bash
# Create urls.txt with one URL per line
coub-downloader batch \
  --input urls.txt \
  --output ~/Downloads/coubs \
  --parallel 4
```

### Check System Requirements

```bash
coub-downloader diagnostic --all
```

## Troubleshooting

### FFmpeg Not Found

**Error:** `FFmpeg not found in PATH`

**Solution:**
```bash
# Ubuntu/Debian
sudo apt-get install ffmpeg

# macOS
brew install ffmpeg

# Windows
choco install ffmpeg
```

### Network Issues

**Error:** `Unable to connect to Coub API`

**Solution:**
- Check internet connection: `ping coub.com`
- Try with `--timeout 60` flag for slow connections
- Disable SSL verification if needed: `--no-verify-ssl`

### Disk Space Issues

**Error:** `Not enough disk space`

**Solution:**
- Check available space: `df -h`
- Change output directory: `--output /path/with/more/space`
- Reduce quality: `--quality low`

## Next Steps

- Read [Architecture Guide](./architecture.md)
- Check [API Reference](./api-reference.md)
- Review [Examples](../examples/)
- See [Configuration Guide](../README.md#⚙️-configuration)
