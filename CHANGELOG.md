# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-08-04

### Added
- Stable public API with full backward-compatibility guarantees
- NuGet packaging configuration and publish workflow
- CodeQL security scanning and Dependabot dependency management
- Comprehensive documentation: getting-started, api-reference, architecture, deployment, FAQ
- Cross-references and integration examples across all service interfaces
- `PROJECT_STRUCTURE.md` describing layer responsibilities
- `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, and `SECURITY.md`

### Changed
- Promoted all interfaces from internal to public surface
- Finalized `ConversionSettings` property names (no further breaking changes)
- Pinned all `Microsoft.Extensions.*` dependencies to stable 10.0.0 releases

### Fixed
- Edge case in `AudioProcessingService` where zero-duration tracks caused division by zero
- `InMemoryBatchJobRepository` thread-safety under high-concurrency batch starts

## [0.9.0] - 2025-07-07

### Added
- `CredentialManager` for secure storage of API tokens
- `RateLimitingService` with per-minute quota enforcement
- Background job processing via `BackgroundWorker`
- `PerformanceMonitor` collecting throughput and latency metrics
- `ExportService` supporting JSON, CSV, and XML result export

### Changed
- Retry logic moved from inline code into dedicated `RetryHelper` with exponential backoff
- `ErrorHandlingMiddleware` now enriches exceptions with operation context before re-throwing

### Fixed
- Rate limiter did not reset its window on clock skew
- Background worker leaked `CancellationTokenSource` on early cancellation

## [0.8.0] - 2025-06-16

### Added
- In-memory repository implementations: `InMemoryBatchJobRepository`, `InMemoryCoubVideoRepository`, `InMemoryDownloadResultRepository`, `InMemoryDownloadTaskRepository`
- `IRepository<T>` generic base interface for all data stores
- `DependencyInjectionExtended` with convenience extension methods for optional services
- `ObjectPool<T>` utility for reusing expensive objects across conversions

### Changed
- `DependencyInjection.cs` split into core registrations and extended registrations
- `CacheService` now enforces a configurable maximum entry count

### Fixed
- `InMemoryCoubVideoRepository.FindByUrl` returned stale entries after update

## [0.7.0] - 2025-05-26

### Added
- `DiagnosticsService` with health-check methods for FFmpeg, storage, and connectivity
- `diagnostic` CLI command (`--all`, `--check-ffmpeg`, `--metrics`, `--report`)
- `VersionHelper` exposing assembly version at runtime
- `DateTimeExtensions` with UTC-normalisation helpers used across services

### Changed
- CLI `--verbose` flag now propagates log level into all middleware components
- `TableFormatter` column widths auto-size based on content

### Fixed
- `DiagnosticsService.CheckFfmpeg` timed out on slow PATH lookups
- `TableFormatter` threw on empty result sets

## [0.6.0] - 2025-05-05

### Added
- Docker support with multi-stage `Dockerfile` and `docker-compose.yml`
- Shorts format (9:16 aspect ratio) for TikTok and Instagram Reels output
- `WebhookManager` for registering and dispatching download-completion webhooks
- `ConversionPipeline` orchestrating pre-process, encode, and post-process stages
- Hardware GPU acceleration support: NVIDIA NVENC, AMD VCE, Intel Quick Sync

### Changed
- `VideoConversionService` delegates encode step to `ConversionPipeline`
- `appsettings.json` extended with `Conversion.EnableHardwareAcceleration` and `Conversion.FFmpegPath`

### Fixed
- Aspect-ratio calculation was inverted for portrait-orientation source videos
- `docker-compose.yml` volume mounts used wrong container path

## [0.5.0] - 2025-04-14

### Added
- In-memory `CacheService` with configurable TTL and eviction
- `EventBus` with typed publish/subscribe for download lifecycle events (`DownloadStartedEvent`, `DownloadCompletedEvent`, `DownloadFailedEvent`)
- `LoggingService` middleware wrapping all service calls with structured timing logs
- `StringExtensions` and `ValidationHelper` utilities
- `appsettings.json` with full configuration schema

### Changed
- `CoubDownloadService` now publishes events at each lifecycle stage
- `BatchProcessingService` honours `CancellationToken` across all async calls

### Fixed
- `CacheService` returned expired entries when system clock rolled back
- `ValidationHelper.IsValidUrl` rejected valid URLs containing query strings

## [0.4.0] - 2025-03-24

### Added
- `CommandLineInterface` with `download`, `batch`, and `diagnostic` top-level commands
- `CommandParser` with typed option binding and help generation
- `JsonFormatter`, `CsvFormatter`, and `TableFormatter` for result output
- `FileUtilities` helpers for safe temp-file creation and cleanup

### Changed
- `Program.cs` wired to `CommandLineInterface` instead of direct service calls
- Error messages now include suggested remediation steps

### Fixed
- `CommandParser` crashed on unknown flags instead of printing usage
- `CsvFormatter` did not escape fields containing commas

## [0.3.0] - 2025-03-03

### Added
- `AudioProcessingService` with `Repeat`, `Fade`, and `Silent` loop strategies
- `PlaylistProcessingService` for downloading all videos from a Coub playlist URL
- `BatchProcessingService` with parallel worker pool and progress tracking
- Domain models: `AudioTrack`, `VideoSection`, `BatchJob`, `DownloadTask`, `CoubPlaylist`
- `CoubDownloaderException` base exception hierarchy

### Changed
- `VideoConversionService` now delegates audio merging to `AudioProcessingService`
- `BatchJob.GetProgressPercent()` extension method moved to `ModelExtensions`

### Fixed
- Audio loop produced audible click at splice point when using `Repeat` strategy
- `BatchJob` state machine allowed invalid `Pending → Completed` transitions

## [0.2.0] - 2025-02-10

### Added
- `CoubApiClient` for fetching video metadata from the Coub REST API
- `FFmpegWrapper` executing FFmpeg as a child process with argument escaping
- `VideoConversionService` converting downloaded streams to MP4
- `CoubDownloadService` orchestrating metadata fetch, stream download, and conversion
- Quality presets: Low (360p), Medium (480p), High (720p), Ultra (1080p), 4K
- `RetryHelper` with configurable attempt count and fixed delay
- `ConfigurationManager` loading `appsettings.json` and environment variable overrides

### Changed
- Project structure reorganised into Domain / Application / Infrastructure / Presentation layers

### Fixed
- `FFmpegWrapper` did not forward stderr output, making encoding failures silent

## [0.1.0] - 2025-01-20

### Added
- Initial project scaffold targeting .NET 10
- Solution file (`coub-downloader.sln`) and `CoubDownloader.csproj`
- Domain layer: `CoubVideo`, `DownloadResult`, `ConversionSettings`, `VideoFormat` enum
- `ApplicationConstants` and `InfrastructureConstants`
- Dependency injection bootstrap via `ApplicationStartup`
- xUnit + Moq + FluentAssertions test project with placeholder structure
- MIT `LICENSE`, `.gitignore`, `.editorconfig`
- GitHub Actions `build.yml` for CI on push and pull request

---

## Migration Guide

### Upgrading from 0.9.0 to 1.0.0

No breaking changes. All public interfaces are stable from this release onward:

```bash
dotnet tool update --global coub-downloader
```

### Upgrading from 0.5.0 to 0.6.0

The `VideoConversionService` now routes encoding through `ConversionPipeline`. If you subclassed `VideoConversionService` directly, override `ExecutePipelineAsync` instead of `ConvertAsync`.

---

## Known Issues

### 1.0.0
- GPU acceleration requires driver version 520+ on Linux (NVIDIA) and 23H2+ on Windows (AMD)
- Some older Coub videos serve audio in `opus` containers that FFmpeg 5.x cannot remux; upgrade to FFmpeg 6.0+

---

## Support

- **Current Version**: 1.0.0
- **Active Support**: 1.0.x
- **Maintenance**: 0.9.x (security fixes only)
- **End of Life**: 0.1.x – 0.8.x

For support, see [GitHub Issues](https://github.com/sarmkadan/coub-downloader/issues)
