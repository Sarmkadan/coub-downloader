# Architecture Guide

## Project Structure

```
coub-downloader/
├── Application/              # Business logic layer
│   └── Services/            # Service implementations
├── Domain/                  # Core domain models
│   ├── Constants/
│   ├── Enums/
│   ├── Exceptions/
│   ├── Extensions/
│   └── Models/
├── Infrastructure/          # External integrations & utilities
│   ├── BackgroundJobs/
│   ├── Caching/
│   ├── Configuration/
│   ├── Diagnostics/
│   ├── Events/
│   ├── Integration/         # API clients, FFmpeg wrapper
│   ├── Middleware/
│   ├── Pipeline/
│   ├── Reporting/
│   ├── Repositories/        # Data access
│   ├── Security/
│   ├── Statistics/
│   └── Utilities/
├── Presentation/            # CLI layer
│   ├── CLI/
│   └── Formatters/
├── examples/                # Example programs
└── docs/                   # Documentation
```

## Layered Architecture

### Presentation Layer
- **CLI Interface**: Command-line argument parsing and user interaction
- **Formatters**: Output formatting (JSON, CSV, Table)
- **ResponsibleFor**: User input handling, result presentation

### Application Layer
- **Services**: Core business logic
  - `CoubDownloadService`: Video download orchestration
  - `VideoConversionService`: FFmpeg-based encoding
  - `AudioProcessingService`: Audio looping & synchronization
  - `BatchProcessingService`: Multi-video processing
- **ResponsibleFor**: Workflow orchestration, business rules

### Domain Layer
- **Models**: `CoubVideo`, `DownloadTask`, `BatchJob`, `AudioTrack`, etc.
- **Enums**: `VideoFormat`, `VideoQuality`, `ProcessingState`, etc.
- **Exceptions**: Custom exception hierarchy
- **Extensions**: Domain object extensions
- **ResponsibleFor**: Core domain concepts, validation rules

### Infrastructure Layer
- **API Clients**: `CoubApiClient` for Coub API
- **FFmpeg Integration**: `FFmpegWrapper` for video encoding
- **Data Access**: In-memory repositories (pluggable)
- **Caching**: Response and metadata caching
- **Event Bus**: Application event system
- **Middleware**: Error handling, logging, rate limiting
- **Utilities**: Helper classes and extensions

## Component Interaction Flow

```
User Input
    ↓
[CLI Parser] → Parse arguments
    ↓
[Command Handler] → Route to appropriate service
    ↓
[Service Layer] → Orchestrate business logic
    ↓
[Domain Models] → Validate and transform data
    ↓
[Infrastructure] → Execute external operations
    ├─ CoubApiClient → Fetch video metadata
    ├─ FFmpegWrapper → Encode video
    └─ Repository → Persist state
    ↓
[Event Bus] → Publish completion events
    ↓
[Formatter] → Format output
    ↓
User Output
```

## Dependency Injection

All services are registered using Microsoft.Extensions.DependencyInjection:

```csharp
services.AddCoubDownloaderServices();
```

This registers:
- All service interfaces and implementations
- Repositories
- HTTP clients
- Configuration providers
- Event bus

## Design Patterns Used

### 1. Service Layer Pattern
Encapsulates business logic and orchestrates operations across domain models and repositories.

### 2. Repository Pattern
Abstracts data access, allowing for flexible storage implementations (in-memory, SQL, NoSQL).

### 3. Dependency Injection
Loose coupling through constructor injection using Microsoft.Extensions.DependencyInjection.

### 4. Event-Driven Architecture
Application events (`DownloadStarted`, `ConversionCompleted`, etc.) allow subscribers to react to key operations.

### 5. Pipeline Pattern
ConversionPipeline chains operations: download → validate → convert → sync-audio → save.

### 6. Strategy Pattern
Audio looping uses strategies (Repeat, Fade, Silent) selected at runtime.

### 7. Object Pool Pattern
Reduces GC pressure for frequently allocated objects.

### 8. Retry Pattern
Exponential backoff retry logic for transient failures.

## Key Abstractions

### ICoubDownloadService
```csharp
Task<DownloadResult> DownloadAsync(string url, ConversionSettings? settings);
Task<CoubVideo> GetVideoMetadataAsync(string url);
```

### IVideoConversionService
```csharp
Task<string> ConvertVideoAsync(string inputPath, string outputPath, ConversionSettings settings);
Task<bool> IsFfmpegAvailableAsync();
```

### IAudioProcessingService
```csharp
Task<AudioTrack> ProcessAudioAsync(AudioTrack track, string videoPath, double targetDuration);
double CalculateLoopedDuration(AudioTrack track);
```

### IBatchProcessingService
```csharp
Task<BatchJob> CreateBatchJobAsync(string name, string outputDirectory, ConversionSettings settings);
Task AddTasksAsync(string batchId, IEnumerable<DownloadTask> tasks);
Task<BatchJob> GetBatchStatusAsync(string batchId);
```

## Configuration Flow

```
appsettings.json
    ↓
Environment Variables (override)
    ↓
ConfigurationManager
    ↓
Services
```

## Error Handling

Three-tier error handling:

1. **Domain Exceptions**: `CoubDownloaderException` base class
2. **Service-Level Recovery**: Automatic retry with backoff
3. **Global Middleware**: `ErrorHandlingMiddleware` catches unhandled exceptions

## Testing Strategy

- **Unit Tests**: Test services in isolation with mocked dependencies
- **Integration Tests**: Test with actual FFmpeg and Coub API
- **Example Programs**: Demonstrate real-world usage

## Performance Considerations

### Caching
- API responses cached to reduce network calls
- Metadata caching reduces repeated API calls

### Parallelization
- Batch processing supports configurable concurrency
- Multiple downloads can happen in parallel

### Memory Management
- Object pooling for frequently allocated types
- Streaming for large file operations
- Proper disposal of resources

### Hardware Acceleration
- Optional GPU encoding (NVIDIA NVENC, AMD VCE, Intel Quick Sync)
- Reduces CPU load for video encoding

## Extensibility Points

### Custom Services
Implement service interfaces and register in DI:
```csharp
services.AddScoped<ICoubDownloadService, MyCustomDownloadService>();
```

### Custom Repositories
Implement `IRepository<T>` for different storage backends:
```csharp
services.AddScoped<ICoubVideoRepository, SqlServerCoubVideoRepository>();
```

### Event Subscriptions
Subscribe to application events:
```csharp
eventBus.Subscribe<DownloadCompletedEvent>(async @event => { /* ... */ });
```

### Custom Formatters
Implement `IOutputFormatter` for different output formats:
```csharp
public class XmlFormatter : IOutputFormatter { /* ... */ }
```
