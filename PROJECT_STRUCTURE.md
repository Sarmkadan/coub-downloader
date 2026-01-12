# Project Structure Overview

This document provides a complete overview of the Coub Downloader project structure and all included files.

## Directory Structure

```
coub-downloader/
│
├── 📄 README.md                          # Main project documentation (2000+ words)
├── 📄 LICENSE                            # MIT License
├── 📄 .gitignore                         # Git ignore rules
├── 📄 CHANGELOG.md                       # Version history and release notes
├── 📄 CONTRIBUTING.md                    # Contribution guidelines
├── 📄 DEVELOPMENT.md                     # Development setup and workflow
├── 📄 PROJECT_STRUCTURE.md               # This file
│
├── 📄 CoubDownloader.csproj              # .NET 10 project file
├── 📄 Program.cs                         # Application entry point
│
├── 📄 Dockerfile                         # Multi-stage Docker build
├── 📄 docker-compose.yml                 # Docker Compose configuration
├── 📄 Makefile                           # Build automation (Linux/macOS)
├── 📄 build.sh                           # Build script (Linux/macOS)
├── 📄 build.cmd                          # Build script (Windows)
│
├── 📄 appsettings.json                   # Default configuration
├── 📄 .env.example                       # Environment variables template
├── 📄 .editorconfig                      # Code style rules
│
├── 📁 Application/                       # Business logic layer
│   └── Services/
│       ├── CoubDownloadService.cs
│       ├── VideoConversionService.cs
│       ├── AudioProcessingService.cs
│       ├── BatchProcessingService.cs
│       └── (Service interfaces)
│
├── 📁 Domain/                            # Core domain models
│   ├── Constants/
│   │   └── ApplicationConstants.cs
│   ├── Enums/
│   │   └── VideoFormat.cs
│   ├── Models/
│   │   ├── CoubVideo.cs
│   │   ├── DownloadTask.cs
│   │   ├── BatchJob.cs
│   │   ├── AudioTrack.cs
│   │   ├── ConversionSettings.cs
│   │   └── (Other models)
│   ├── Exceptions/
│   │   └── CoubDownloaderException.cs
│   └── Extensions/
│       └── ModelExtensions.cs
│
├── 📁 Infrastructure/                    # External integration
│   ├── Integration/
│   │   ├── CoubApiClient.cs
│   │   ├── FFmpegWrapper.cs
│   │   └── WebhookManager.cs
│   ├── Repositories/
│   │   ├── IRepository.cs
│   │   ├── InMemoryCoubVideoRepository.cs
│   │   ├── InMemoryDownloadTaskRepository.cs
│   │   ├── InMemoryDownloadResultRepository.cs
│   │   └── InMemoryBatchJobRepository.cs
│   ├── Caching/
│   │   └── CacheService.cs
│   ├── Events/
│   │   └── EventBus.cs
│   ├── Pipeline/
│   │   └── ConversionPipeline.cs
│   ├── Middleware/
│   │   ├── ErrorHandlingMiddleware.cs
│   │   ├── LoggingService.cs
│   │   └── RateLimitingService.cs
│   ├── Configuration/
│   │   └── ConfigurationManager.cs
│   ├── Diagnostics/
│   │   └── DiagnosticsService.cs
│   ├── Statistics/
│   │   └── PerformanceMonitor.cs
│   ├── Utilities/
│   │   ├── FileUtilities.cs
│   │   ├── RetryHelper.cs
│   │   ├── ValidationHelper.cs
│   │   └── (Other utilities)
│   ├── BackgroundJobs/
│   │   └── BackgroundWorker.cs
│   ├── Reporting/
│   │   └── ExportService.cs
│   ├── Security/
│   │   └── CredentialManager.cs
│   └── DependencyInjection.cs
│
├── 📁 Presentation/                      # CLI layer
│   ├── CLI/
│   │   ├── CommandLineInterface.cs
│   │   └── CommandParser.cs
│   └── Formatters/
│       ├── JsonFormatter.cs
│       ├── CsvFormatter.cs
│       ├── TableFormatter.cs
│       └── TableFormatter.cs
│
├── 📁 examples/                          # Complete example programs
│   ├── BasicDownload.cs                  # Simple single video download
│   ├── BatchDownload.cs                  # Batch processing with progress
│   ├── CustomConversion.cs               # Custom quality and GPU settings
│   ├── AudioProcessing.cs                # Audio looping and sync
│   ├── EventHandling.cs                  # Event subscriptions
│   ├── RepositoryUsage.cs                # Data persistence
│   ├── DiagnosticsAndMonitoring.cs       # System health checks
│   └── ShortsConversion.cs               # Shorts format conversion
│
├── 📁 docs/                              # Comprehensive documentation
│   ├── getting-started.md                # Installation and quick start
│   ├── architecture.md                   # System design and patterns
│   ├── api-reference.md                  # Complete API documentation
│   ├── deployment.md                     # Deployment guides (Docker, Cloud)
│   └── faq.md                            # Frequently asked questions
│
├── 📁 .github/                           # GitHub configuration
│   └── workflows/
│       └── build.yml                     # CI/CD pipeline
│
├── 📁 bin/                               # Compiled output (generated)
│   └── Debug/net10.0/
│       └── CoubDownloader.dll
│
├── 📁 obj/                               # Build artifacts (generated)
│
└── 📁 Tests/                             # Unit tests (optional)
    └── (Test files)
```

## File Descriptions

### Root Configuration Files

| File | Purpose |
|------|---------|
| `README.md` | Main documentation with features, installation, usage examples, and API reference |
| `LICENSE` | MIT License (Copyright © 2026 Vladyslav Zaiets) |
| `.gitignore` | Git ignore patterns for build artifacts, IDE files, secrets |
| `CHANGELOG.md` | Version history from v0.1.0 through v1.2.0 with migration guides |
| `CONTRIBUTING.md` | Guidelines for contributing (code style, testing, PR process) |
| `DEVELOPMENT.md` | Development setup, debugging, profiling, and testing guides |
| `PROJECT_STRUCTURE.md` | This file - complete project overview |

### Build & Deployment

| File | Purpose |
|------|---------|
| `CoubDownloader.csproj` | .NET 10 project configuration with NuGet dependencies |
| `Dockerfile` | Multi-stage Docker build for production deployment |
| `docker-compose.yml` | Docker Compose with volumes, environment, health checks |
| `Makefile` | Cross-platform build automation (Linux/macOS) |
| `build.sh` | Bash build script for Linux/macOS |
| `build.cmd` | Batch build script for Windows |

### Configuration

| File | Purpose |
|------|---------|
| `appsettings.json` | Default application configuration |
| `.env.example` | Environment variables template for custom setup |
| `.editorconfig` | IDE-independent code style rules |

### Source Code Layers

**Application Layer** (`Application/Services/`):
- Business logic and service implementations
- Orchestrates workflows across domain and infrastructure layers
- Service interfaces for dependency injection

**Domain Layer** (`Domain/`):
- Core business models (CoubVideo, DownloadTask, etc.)
- Enums and constants
- Domain exceptions
- Business rule validation and extensions

**Infrastructure Layer** (`Infrastructure/`):
- External API clients (Coub API, FFmpeg)
- Repository implementations
- Caching, event system, middleware
- Utilities and helpers
- Background job processing

**Presentation Layer** (`Presentation/CLI/`):
- Command-line interface
- Command parsing and routing
- Output formatters (JSON, CSV, Table)

### Examples (8 Complete Programs)

Located in `examples/`:

1. **BasicDownload.cs** - Simple video download with default settings
2. **BatchDownload.cs** - Multiple videos with progress tracking
3. **CustomConversion.cs** - Custom quality, codec, GPU acceleration
4. **AudioProcessing.cs** - Audio looping strategies and synchronization
5. **EventHandling.cs** - Event subscriptions and reactive programming
6. **RepositoryUsage.cs** - Data persistence and querying
7. **DiagnosticsAndMonitoring.cs** - System health checks and performance
8. **ShortsConversion.cs** - Vertical format for TikTok/Instagram Reels

### Documentation (5 Guides)

Located in `docs/`:

1. **getting-started.md** - Installation methods, quick start, first download
2. **architecture.md** - Layered architecture, design patterns, extensibility
3. **api-reference.md** - Complete service and model documentation
4. **deployment.md** - Docker, Linux, Windows, cloud deployment guides
5. **faq.md** - Common questions and troubleshooting

### CI/CD

Located in `.github/workflows/`:

- **build.yml** - GitHub Actions workflow for:
  - Building on Linux, Windows, macOS
  - Running tests with coverage
  - Security scanning
  - Docker image building and pushing
  - Release artifact creation

## Code Statistics

- **Total C# Files**: 30+
- **Example Programs**: 8
- **Documentation Files**: 5+
- **Lines of Documentation**: 2000+
- **Code Style**: EditorConfig enforced
- **Testing**: xUnit.net ready
- **CI/CD**: GitHub Actions included

## Key Features by File

### Video Download (`Application/Services/CoubDownloadService.cs`)
- Download from Coub URL
- Metadata extraction
- Concurrent multi-video downloads
- Quality preset selection
- Retry logic with exponential backoff

### Video Conversion (`Application/Services/VideoConversionService.cs`)
- FFmpeg integration
- Multiple format support (MP4, WebM, MOV, Shorts)
- Quality presets (Low, Medium, High, Ultra, 4K)
- GPU acceleration support (NVIDIA, AMD, Intel)
- Codec selection and bitrate configuration

### Audio Processing (`Application/Services/AudioProcessingService.cs`)
- Audio looping synchronization
- Multiple strategies (Repeat, Fade, Silent)
- Sample rate and channel configuration
- Volume adjustment
- Audio-video merging

### Batch Processing (`Application/Services/BatchProcessingService.cs`)
- Multiple video job management
- Parallel download with configurable concurrency
- Progress tracking and status reporting
- Error handling and retry
- Export to JSON, CSV, XML

### Data Persistence (`Infrastructure/Repositories/`)
- In-memory repository implementations
- Video, task, result, and batch job storage
- Query by state, creator, and other filters
- Extensible for database backends

## Technology Stack

- **Framework**: .NET 10.0
- **Language**: C# 13 (latest features)
- **HTTP**: System.Net.Http with retry policies
- **JSON**: Newtonsoft.Json
- **Dependency Injection**: Microsoft.Extensions
- **Configuration**: Microsoft.Extensions.Configuration
- **Logging**: Microsoft.Extensions.Logging
- **Testing**: xUnit.net (ready to use)
- **Containerization**: Docker & Docker Compose
- **CI/CD**: GitHub Actions

## Code Quality Features

- ✓ Layered architecture with clear separation of concerns
- ✓ SOLID principles (Single Responsibility, Open/Closed, etc.)
- ✓ Dependency injection for loose coupling
- ✓ XML documentation on public APIs
- ✓ EditorConfig for consistent formatting
- ✓ Error handling with custom exceptions
- ✓ Async/await throughout for I/O operations
- ✓ Comprehensive logging infrastructure
- ✓ Rate limiting and retry policies
- ✓ Security best practices

## Getting Started

### Quick Development Start

```bash
# Clone and setup
git clone https://github.com/vladyslav-zaiets/coub-downloader.git
cd coub-downloader

# Restore and build
dotnet restore
dotnet build -c Release

# Run
dotnet run
```

### Using Docker

```bash
docker-compose up
```

### Production Deployment

See `docs/deployment.md` for comprehensive guides.

## Contributing

See `CONTRIBUTING.md` for:
- Development setup
- Code style guidelines
- Testing requirements
- Pull request process

## Support Resources

- **Documentation**: See `docs/` directory
- **Examples**: See `examples/` directory
- **GitHub Issues**: [Report bugs and request features](https://github.com/vladyslav-zaiets/coub-downloader/issues)
- **FAQ**: See `docs/faq.md`

## License

MIT License - Copyright © 2026 Vladyslav Zaiets

See `LICENSE` file for full details.

---

**Project**: Coub Downloader v1.2.0
**Author**: Vladyslav Zaiets
**Website**: https://sarmkadan.com
**Repository**: https://github.com/vladyslav-zaiets/coub-downloader
