# Contributing

Contributions of all kinds are welcome — bug fixes, new features, documentation improvements, and more.

## Requirements

- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- [FFmpeg](https://ffmpeg.org/download.html) (required at runtime, not for build/test)
- Git

## Building locally

```bash
# Clone the repository
git clone https://github.com/sarmkadan/coub-downloader.git
cd coub-downloader

# Restore dependencies
dotnet restore

# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build --configuration Release
```

## Running tests

```bash
# Run all tests
dotnet test

# Run with detailed output and generate a TRX report
dotnet test --verbosity normal --logger "trx" --results-directory TestResults

# Run a specific test project
dotnet test tests/coub-downloader.Tests/coub-downloader.Tests.csproj
```

## Workflow

1. **Fork** the repository and create a branch from `main`.
2. **Name your branch** descriptively: `feature/your-feature` or `fix/issue-description`.
3. **Make your changes** and keep each commit focused on one logical change.
4. **Run tests** and ensure they all pass before opening a PR.
5. **Open a Pull Request** against the `main` branch with a clear description of what changed and why.

## Pull request guidelines

- Keep PRs small and focused — one feature or fix per PR.
- Link related GitHub issues in the PR description using `Closes #<issue>`.
- Ensure `dotnet build` and `dotnet test` both pass locally.
- Do not lower test coverage — add tests for new behaviour.
- Update documentation (README, docs/) if your change affects public behaviour or configuration.

## Code style

The project uses `.editorconfig` for consistent formatting. Before submitting, verify formatting:

```bash
dotnet format --verify-no-changes
```

Additional conventions:

- Follow the existing coding patterns in each layer (Domain, Application, Infrastructure, Presentation).
- Write XML documentation comments (`///`) for all public types and members.
- Use `_camelCase` for private fields and `PascalCase` for everything else.
- Prefer dependency injection over static access.
- **Keep ALL existing author headers** — do not remove them from files.

## Reporting issues

Use [GitHub Issues](https://github.com/sarmkadan/coub-downloader/issues) to report bugs or request features. For bugs, include:

- Steps to reproduce
- Expected vs actual behaviour
- .NET SDK version (`dotnet --version`)
- Operating system and FFmpeg version

## License

By contributing you agree that your contributions will be licensed under the [MIT License](LICENSE).
