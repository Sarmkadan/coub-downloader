# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-05-04

### Added
- Hardware GPU acceleration support (NVIDIA NVENC, AMD VCE, Intel Quick Sync)
- Webhook integration for download completion notifications
- Performance monitoring and statistics dashboard
- Advanced audio looping strategies (Fade, Silent modes)
- Diagnostics command with system health checks
- Batch export to CSV and XML formats
- Configuration file validation at startup
- Rate limiting with per-minute quota management

### Changed
- Improved FFmpeg parameter generation for better video quality
- Refactored audio synchronization algorithm for better reliability
- Optimized memory usage in batch processing
- Enhanced error messages with diagnostic context
- Updated to .NET 10 runtime for performance improvements

### Fixed
- Audio sync issues on videos with variable frame rates
- Memory leak in long-running batch operations
- Incorrect aspect ratio calculation for non-standard resolutions
- Race condition in concurrent download operations
- FFmpeg version detection on different platforms

### Deprecated
- Legacy audio looping strategy enum (use new strategies instead)

## [1.1.0] - 2026-04-15

### Added
- Docker support with optimized multi-stage build
- Docker Compose configuration for easy deployment
- Shorts format support for TikTok/Instagram Reels
- Custom naming patterns for output files
- Environment variable configuration
- Background job processing system
- Event bus for reactive programming
- In-memory caching for API responses
- Retry logic with exponential backoff
- Comprehensive logging with adjustable levels

### Changed
- Refactored service architecture for better testability
- Improved CLI argument parsing and validation
- Enhanced progress reporting in batch operations
- Better exception handling with detailed error codes

### Fixed
- Issue with videos having no audio track
- Concurrent download task management
- File path handling on different operating systems
- Quality preset bitrate calculations

## [1.0.0] - 2026-03-20

### Added
- Initial release
- Core video download functionality
- FFmpeg integration for video conversion
- Audio looping synchronization
- Batch processing of multiple videos
- Quality presets (Low, Medium, High, Ultra, 4K)
- CLI interface with comprehensive commands
- Repository pattern for data access
- Dependency injection setup
- Configuration management
- Error handling middleware
- Performance monitoring
- Comprehensive documentation

### Features
- Download Coub videos from URL
- Convert to MP4, WebM, MOV formats
- Automatic audio-video synchronization
- Batch download with progress tracking
- Multiple quality options
- Aspect ratio preservation
- CLI and programmatic API
- Cross-platform support (Linux, Windows, macOS)

---

## Migration Guide

### Upgrading from 1.0.0 to 1.1.0

No breaking changes. Update and enjoy new features:

```bash
dotnet tool update --global coub-downloader
# or
docker pull vladyslav-zaiets/coub-downloader:latest
```

### Upgrading from 1.1.0 to 1.2.0

No breaking changes. The new hardware acceleration is optional:

```bash
# Enable GPU acceleration (optional)
export COUB_ENABLE_GPU_ACCELERATION=true
export COUB_GPU_CODEC=h264_nvenc
```

---

## Known Issues

### 1.2.0
- GPU acceleration requires specific driver versions (see docs)
- Some older Coub videos may have incompatible audio formats
- Rate limiting may affect batch operations with 1000+ videos

### 1.1.0
- Docker on Apple Silicon (M1/M2) requires rosetta emulation

---

## Future Roadmap

### Planned for 1.3.0 (Q3 2026)
- Database backend support (PostgreSQL, MongoDB)
- REST API server mode
- Web UI dashboard
- Webhook retries with exponential backoff
- Custom FFmpeg filter support
- Subtitle download support

### Planned for 2.0.0 (Q4 2026)
- Multi-language support
- Plugin system for custom processors
- Advanced scheduling and automation
- Cloud storage integration (S3, GCS, Azure Blob)
- Advanced analytics and reporting

---

## Deprecation Notices

### In 1.2.0
- `AudioLoopStrategy.Legacy` will be removed in 2.0.0
  - Use `AudioLoopStrategy.Repeat` or `AudioLoopStrategy.Fade` instead

---

## Support

- **Current Version**: 1.2.0
- **Active Support**: 1.2.x, 1.1.x
- **Maintenance**: 1.0.x (security fixes only)
- **End of Life**: 0.x versions

For support, see [GitHub Issues](https://github.com/vladyslav-zaiets/coub-downloader/issues)
