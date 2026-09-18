# CLAUDE.md

## Overview
CoubDownloader - .NET 10 console CLI for downloading Coub videos, with FFmpeg-based conversion/editing, batch processing, caching and webhooks. Clean Architecture layout (Domain / Application / Infrastructure / Presentation) in a single executable project.

## Build
```bash
dotnet restore
dotnet build -c Release                # or: make build
dotnet run --project CoubDownloader.csproj -- <args>   # or: make run
dotnet publish -c Release -o publish   # or: make publish
docker build -t coub-downloader .      # or: make docker-build
```
SDK pinned in `global.json` (10.0.100, rollForward latestMinor). Solution: `coub-downloader.sln` (main project, tests, benchmarks).

## Test
```bash
dotnet test                            # or: make test
dotnet test --filter "FullyQualifiedName~CoubApiClientTests"
```
Tests: `tests/coub-downloader.Tests/` - xUnit + FluentAssertions + Moq. Benchmarks: `benchmarks/` (BenchmarkDotNet).

## Lint / Format
```bash
dotnet format                          # or: make format
dotnet format --verify-no-changes      # CI check
make lint                              # = build with analyzers
make ci                                # clean restore build lint test
```
Style rules in `.editorconfig` (4 spaces, C# rules). Nullable and ImplicitUsings enabled via `Directory.Build.props`; warnings are not errors.

## Key directories and entry points
- `Program.cs` - entry point; host/DI setup, then hands off to CLI
- `Application/Startup/ApplicationStartup.cs` - app bootstrap
- `Application/Services/` - use-case services (`CoubDownloadService`, `BatchProcessingService`, `VideoConversionService`, `AudioProcessingService`, `PlaylistProcessingService`, `VideoEditorService`, `WatermarkService`, `ThumbnailGenerationService`) with `I*` interfaces alongside
- `Domain/` - `Models`, `Enums`, `Constants`, `Exceptions`, `Extensions`; no external deps
- `Infrastructure/` - `Integration/` (`CoubApiClient`, `FFmpegWrapper`, `WebhookManager`), `Caching`, `Repositories`, `Pipeline`, `BackgroundJobs`, `Security`, `Statistics`, `Reporting`, `Middleware`, `VideoEditor`; DI wiring in `DependencyInjection.cs` / `DependencyInjectionExtended.cs`
- `Presentation/CLI/` - `CommandLineInterface`, `CommandParser`; `Presentation/Formatters/` - output formatters (table, JSON)
- `appsettings.json` / `appsettings.example.json` / `.env.example` - configuration
- `.github/workflows/` - ci, build, release, docker, nuget-publish, codeql
- `docs/`, `DEVELOPMENT.md`, `PROJECT_STRUCTURE.md` - extended docs

## Conventions
- Layering: Presentation -> Application -> Domain; Infrastructure implements Application/Domain interfaces. Domain must not reference other layers.
- Interfaces prefixed `I`, one per service, in the same folder as the implementation.
- Private fields `_camelCase`; constructor injection via `Microsoft.Extensions.DependencyInjection`; settings via `IOptions<T>` with DataAnnotations validation.
- Extra behavior for a class goes in a partial/extension file named `<Class>Extensions.cs`, `<Class>JsonExtensions.cs` or `<Class>Validation.cs`.
- Tests named `<Class>Tests.cs`, mirroring the class under test; use Moq for `IFFmpegWrapper` / HTTP dependencies.
- Logging via Serilog; JSON via `System.Text.Json`.
- Stray files in repo root (`Console.WriteLine(...)`, `sharedSettings: config);`, `*.csx`, `*.backup`) are leftovers, not part of the build - do not extend them.
