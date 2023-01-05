# Contributing to Coub Downloader

Thank you for your interest in contributing! This document provides guidelines and instructions for contributing.

## Code of Conduct

Be respectful and professional in all interactions. We're building a friendly community.

## Getting Started

### Prerequisites

- .NET SDK 10.0 or later
- Git
- FFmpeg 6.0+
- An IDE (Visual Studio, VS Code, JetBrains Rider)

### Setup Development Environment

```bash
# Clone repository
git clone https://github.com/vladyslav-zaiets/coub-downloader.git
cd coub-downloader

# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run tests
dotnet test
```

## Development Workflow

### 1. Fork and Clone

```bash
# Fork on GitHub, then:
git clone https://github.com/YOUR-USERNAME/coub-downloader.git
cd coub-downloader
git remote add upstream https://github.com/vladyslav-zaiets/coub-downloader.git
```

### 2. Create Feature Branch

```bash
git checkout -b feature/my-feature
# or
git checkout -b fix/my-bug
```

Branch naming:
- `feature/description` for new features
- `fix/description` for bug fixes
- `docs/description` for documentation
- `test/description` for tests

### 3. Make Changes

Follow our code style and conventions (see below).

### 4. Test Locally

```bash
# Run all checks
make ci

# Or individual steps:
dotnet restore
dotnet build -c Release
dotnet test
dotnet format
```

### 5. Commit Changes

```bash
git add .
git commit -m "Type: Brief description

Longer explanation if needed.

Closes #123"
```

Commit message format:
- **feat**: New feature
- **fix**: Bug fix
- **docs**: Documentation changes
- **test**: Test additions/changes
- **perf**: Performance improvements
- **refactor**: Code refactoring
- **chore**: Build, dependencies, etc.

### 6. Push and Create Pull Request

```bash
git push origin feature/my-feature
```

Then open a PR on GitHub.

## Code Style

### C# Conventions

```csharp
// Namespaces match folder structure
namespace CoubDownloader.Application.Services;

/// <summary>XML documentation comments on public members</summary>
public class MyClass
{
    // PascalCase for public members
    public string PublicProperty { get; set; }
    
    // _camelCase for private fields
    private string _privateField;
    
    // Brief comment only if WHY is non-obvious
    public async Task ProcessAsync()
    {
        // Implementation
    }
}
```

### Rules

- **Naming**: PascalCase for classes/methods, camelCase for variables
- **Async**: Use `async`/`await` for I/O operations
- **SOLID**: Follow SOLID principles
- **DRY**: Don't repeat yourself
- **Comments**: Only when WHY is non-obvious
- **Line Length**: Keep under 100 characters
- **Indentation**: 4 spaces

### Use EditorConfig

The `.editorconfig` file maintains consistent style automatically.

```bash
# Format code
dotnet format
```

## Testing

### Write Tests

- Unit tests in `Tests/` directory
- One test file per class
- Clear test names describing what is tested

```csharp
[Fact]
public async Task DownloadAsync_WithValidUrl_ReturnsResult()
{
    // Arrange
    var service = new CoubDownloadService(mockRepository);
    var url = "https://coub.com/view/2a3b4c5d";
    
    // Act
    var result = await service.DownloadAsync(url);
    
    // Assert
    Assert.NotNull(result);
}
```

### Run Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "ClassName"

# With coverage
dotnet test /p:CollectCoverage=true
```

## Documentation

### Update README.md

If your change affects usage:
- Update the appropriate section
- Add examples if needed
- Update table of contents if adding major sections

### API Documentation

- Add XML comments to public members
- Document parameters, returns, exceptions
- Include usage examples

```csharp
/// <summary>Downloads a Coub video</summary>
/// <param name="url">Coub video URL</param>
/// <returns>Download result with file path</returns>
/// <exception cref="CoubDownloaderException">On download failure</exception>
public async Task<DownloadResult> DownloadAsync(string url)
```

## Commit Message Examples

```
feat: Add hardware GPU acceleration support

- Support NVIDIA NVENC, AMD VCE, Intel Quick Sync
- Add GPU codec selection in settings
- Update documentation with GPU requirements

Closes #42
```

```
fix: Resolve audio sync issue with variable frame rates

The audio duration calculation didn't account for variable frame rates
in the video, causing sync issues. Fixed by using FFmpeg's duration
detection instead.

Closes #35
```

```
docs: Add deployment guide for AWS and Azure

Added comprehensive deployment guide covering:
- EC2 instance setup
- Azure container instances
- Google Cloud setup

Related to #28
```

## Pull Request Checklist

- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] Code formatted (`dotnet format`)
- [ ] No breaking changes (or clearly documented)
- [ ] Commit messages follow conventions
- [ ] Branch is up to date with main

## Review Process

1. Automated checks must pass (CI/CD)
2. Code review by maintainers
3. Address review feedback
4. Approval and merge

### What Reviewers Look For

- Code quality and style
- Test coverage
- Documentation
- Breaking changes
- Security implications
- Performance impact

## Reporting Issues

### Bug Report

```markdown
## Description
Brief description of the bug

## Steps to Reproduce
1. Step one
2. Step two
3. Step three

## Expected Behavior
What should happen

## Actual Behavior
What actually happens

## Environment
- OS: Ubuntu 22.04
- .NET Version: 10.0.1
- App Version: 1.2.0
```

### Feature Request

```markdown
## Description
Brief description of the feature

## Motivation
Why is this needed?

## Examples
How would it be used?

## Additional Context
Any other context
```

## Performance Considerations

- Profile before optimizing
- Document performance implications
- Consider memory usage
- Test with realistic data sizes
- Avoid premature optimization

## Security

- Never commit secrets (API keys, passwords)
- Validate all user input
- Use HTTPS for API calls
- Check dependencies for vulnerabilities
- Report security issues privately

## Getting Help

- Check existing issues and discussions
- Review documentation
- Ask in GitHub Discussions
- Email: vladyslav.zaiets@amdaris.com

## Recognition

- Contributors listed in README
- Added to CHANGELOG
- Credit in commit message

Thank you for contributing! 🚀
