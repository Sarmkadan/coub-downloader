# Development Guide

This guide covers local development setup, debugging, and testing for Coub Downloader.

## Quick Setup

```bash
# Clone repository
git clone https://github.com/vladyslav-zaiets/coub-downloader.git
cd coub-downloader

# Install dependencies
dotnet restore

# Build and run
dotnet build
dotnet run
```

## Development Environment

### Recommended Tools

- **IDE**: Visual Studio 2024, VS Code, or JetBrains Rider
- **Git Client**: GitHub Desktop, TortoiseGit, or CLI
- **.NET SDK**: 10.0 or later
- **FFmpeg**: 6.0 or later for video testing

### VS Code Setup

Install extensions:
- C# (powered by OmniSharp)
- C# Dev Kit
- .NET Runtime Installer
- REST Client (for API testing)

### Visual Studio 2024

- Workload: ".NET desktop development"
- Optional: "Cloud development with Azure"

### JetBrains Rider

- Built-in .NET support
- Excellent debugging and profiling
- Native Docker support

## Project Structure

```
coub-downloader/
├── Application/              # Business logic
│   └── Services/            # Service implementations
├── Domain/                  # Core models & rules
│   ├── Constants/
│   ├── Enums/
│   ├── Models/
│   └── Exceptions/
├── Infrastructure/          # External integration
│   ├── Integration/         # API clients, FFmpeg
│   ├── Repositories/        # Data access
│   ├── Caching/
│   ├── Events/
│   └── Utilities/
├── Presentation/            # CLI layer
│   └── CLI/
├── examples/                # Example programs
├── docs/                   # Documentation
└── Tests/                  # Unit tests
```

## Build & Test

### Using Makefile (Linux/macOS)

```bash
# Build
make build

# Test
make test

# Format code
make format

# Publish
make publish
```

### Using Build Scripts

```bash
# Linux/macOS
./build.sh all

# Windows
./build.cmd all
```

### Using dotnet CLI

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build -c Release

# Run tests
dotnet test

# Run specific test
dotnet test --filter "TestClass"

# Code analysis
dotnet analyzers

# Format code
dotnet format

# Publish
dotnet publish -c Release -o bin/Release/net10.0/publish
```

## Debugging

### Visual Studio / Rider

1. Set breakpoint (click in gutter)
2. Press F5 to start debugging
3. Use Debug menu for controls

### VS Code

Create `.vscode/launch.json`:

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/bin/Debug/net10.0/CoubDownloader.dll",
            "args": ["download", "--url", "https://coub.com/view/2a3b4c5d"],
            "cwd": "${workspaceFolder}",
            "stopAtEntry": false,
            "console": "internalConsole"
        }
    ]
}
```

### Command Line Debugging

```bash
# Enable verbose logging
dotnet run --loglevel debug

# Run with dotnet debugger
dotnet --version
dotnet publish
dotnet ./<path-to-dll> [args]
```

## Testing

### Test Structure

```csharp
[Fact]
public async Task DownloadAsync_WithValidUrl_ReturnsSuccessfulResult()
{
    // Arrange - Setup test conditions
    var mockService = new Mock<ICoubDownloadService>();
    
    // Act - Execute the test
    var result = await mockService.Object.DownloadAsync("https://coub.com/view/123");
    
    // Assert - Verify results
    Assert.NotNull(result);
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

# Run specific test class
dotnet test --filter "CoubDownloadServiceTests"

# Run with verbose output
dotnet test -v normal

# Run async tests with timeout
dotnet test --test-adapter-path:. --logger:"console;verbosity=detailed"
```

### Test Types

1. **Unit Tests**: Single component in isolation
2. **Integration Tests**: Multiple components together
3. **System Tests**: Full application workflow

## Performance Profiling

### Using .NET Diagnostics

```bash
# Capture trace
dotnet trace collect -p <process-id>

# Analyze with Speedscope
dotnet trace convert nettrace-file-name.nettrace --format Speedscope
```

### Using Visual Studio Profiler

1. Debug > Start Diagnostic Tools
2. Select profiler (CPU, Memory, etc.)
3. Run application
4. Stop and analyze results

### Memory Profiling

```bash
# Take heap snapshot
dotnet-dump collect -p <process-id> -o core_<timestamp>.dump

# Analyze dump
dotnet-dump analyze core_<timestamp>.dump
```

## Logging

### Configure Logging

Set in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Warning"
    }
  }
}
```

Or via environment variable:

```bash
DOTNET_LogLevel__Default=Debug
```

### Log Levels

- **Trace**: Very detailed diagnostic info
- **Debug**: Debugging-level info
- **Information**: General informational messages
- **Warning**: Warning messages
- **Error**: Error messages
- **Critical**: Critical errors

## Code Style & Quality

### EditorConfig

Automatically enforced via `.editorconfig`. Integrates with IDE.

### Code Formatting

```bash
# Auto-format all files
dotnet format

# Format specific project
dotnet format --project CoubDownloader.csproj

# Check without fixing
dotnet format --verify-no-changes
```

### Code Analysis

```bash
# Run Roslyn analyzers
dotnet build

# Treat warnings as errors
dotnet build -p:TreatWarningsAsErrors=true
```

## Documentation

### XML Comments

```csharp
/// <summary>
/// Describes the method purpose in one sentence
/// </summary>
/// <param name="url">Parameter description</param>
/// <returns>Return value description</returns>
/// <exception cref="CoubDownloaderException">When download fails</exception>
/// <remarks>Additional details if needed</remarks>
public async Task<DownloadResult> DownloadAsync(string url)
{
}
```

### Generate Documentation

```bash
# Build documentation
dotnet build /p:GenerateDocumentationFile=true
```

## Git Workflow

### Create Feature Branch

```bash
git checkout -b feature/my-feature
```

### Commit Changes

```bash
git add .
git commit -m "feat: Add new feature

Detailed explanation of changes.

Closes #123"
```

### Push & Create PR

```bash
git push origin feature/my-feature
# Then create PR on GitHub
```

### Update from Main

```bash
git fetch upstream
git rebase upstream/main
git push -f origin feature/my-feature
```

## Docker Development

### Build Image Locally

```bash
docker build -t coub-downloader:dev .
```

### Run Container

```bash
docker run -it \
  -v /path/to/downloads:/downloads \
  -e COUB_LOG_LEVEL=Debug \
  coub-downloader:dev \
  download --url https://coub.com/view/123
```

### Docker Compose Development

```bash
# Start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

## Troubleshooting

### Build Issues

```bash
# Clean build
dotnet clean
dotnet restore
dotnet build

# Rebuild solution
dotnet build --no-incremental
```

### Test Issues

```bash
# Run specific test with detailed output
dotnet test --filter "TestName" -v d

# Increase timeout
dotnet test --configuration Debug
```

### Runtime Issues

```bash
# Enable detailed logging
export DOTNET_DebugType=embedded
export DOTNET_DebugWriteToStdErr=1

# Run with console logger
dotnet run --loglevel debug
```

## Performance Tips

1. **Use Release Build**: `dotnet build -c Release`
2. **Enable Tiered JIT**: `DOTNET_TieredCompilation=1`
3. **Optimize Throughput**: `DOTNET_TieredCompilationQuickJit=1`
4. **Monitor Memory**: Use dotnet-counters
5. **Profile Regularly**: Use built-in profilers

## Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [C# Language Reference](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [xUnit.net Testing Framework](https://xunit.net/)
- [Moq Mocking Library](https://github.com/moq/moq4)
- [FFmpeg Documentation](https://ffmpeg.org/documentation.html)

## Getting Help

- Check [GitHub Issues](https://github.com/vladyslav-zaiets/coub-downloader/issues)
- Review [Contributing Guidelines](./CONTRIBUTING.md)
- Read [Architecture Guide](./docs/architecture.md)
- Email: vladyslav.zaiets@amdaris.com

## Additional Scripts

### Update Dependencies

```bash
dotnet outdated
dotnet package search <package-name>
dotnet add package <package-name>@<version>
```

### Clean Local Cache

```bash
dotnet nuget locals all --clear
```

Happy developing! 🚀
