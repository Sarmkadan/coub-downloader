# Contributing

Thanks for helping improve CoubDownloader. This guide covers how to build, test and submit changes.

## Prerequisites

- **.NET SDK 10.0** — `global.json` pins `10.0.100` with `rollForward: latestMinor`, so any installed 10.0.x SDK is picked up. The project targets `net10.0`.
- **Git**
- **FFmpeg** — only needed at runtime for conversion and editing features. Not required to build or run the tests.

Check your SDK with `dotnet --version`.

## Building

The repository root contains both `coub-downloader.sln` and `CoubDownloader.csproj`, so a bare `dotnet build` fails with `MSB1011`. Name the solution explicitly:

```bash
dotnet restore coub-downloader.sln
dotnet build coub-downloader.sln --configuration Release
```

`make build` runs the same steps against the main project. `make ci` runs clean, restore, build, lint and test in sequence.

## Running tests

Tests live in `tests/coub-downloader.Tests/` and use xUnit, FluentAssertions and Moq.

```bash
dotnet test coub-downloader.sln --configuration Release
```

To run a subset, add a filter:

```bash
dotnet test coub-downloader.sln --filter "FullyQualifiedName~CoubApiClientTests"
```

Benchmarks in `benchmarks/` use BenchmarkDotNet and are not part of the test run.

## Code style

- Formatting rules are in `.editorconfig`: 4-space indentation, LF line endings, final newline, no trailing whitespace, braces preferred on all control blocks (reported as a warning), and Allman-style opening braces.
- `Nullable` is enabled for all projects through `Directory.Build.props`. Write nullable-correct code; do not suppress warnings with `!` unless you have checked the value.
- Warnings do **not** fail the build (`TreatWarningsAsErrors` is `false`). Still, do not add new warnings in code you touch.
- Naming: `_camelCase` for private fields, `I` prefix for interfaces (one per service, in the same folder as its implementation).
- Respect the layering: Presentation → Application → Domain. Infrastructure implements Application/Domain interfaces. Domain must not reference other layers.
- Use Serilog for logging and `System.Text.Json` for JSON.
- Run `make format` (or `dotnet format`) before committing if you want to fix formatting automatically. CI does not enforce it.

## Branching and pull request workflow

1. Branch from an up-to-date `main`.
2. Keep each PR small and focused on one change. Split unrelated work into separate PRs.
3. Add or update tests for behaviour you change.
4. Make sure `dotnet build` and `dotnet test` pass locally before pushing.
5. Open the PR against `main`. CI (`.github/workflows/ci.yml` and `build.yml`) builds and tests on pull requests, and it must pass before merge.
6. In the PR description, explain what changed and why, and list how you tested it. Link related issues with `Closes #<number>`.

Commit messages follow Conventional Commits, as in the existing history (`feat:`, `fix:`, `fix(tests):`, `docs:`, `chore:`).

## Reporting issues

Report bugs and feature requests on [GitHub Issues](https://github.com/sarmkadan/coub-downloader/issues). For bugs, include:

- Steps to reproduce
- Expected and actual behaviour
- Output of `dotnet --version`
- Operating system and FFmpeg version, if the bug involves conversion

For security vulnerabilities, follow [SECURITY.md](SECURITY.md) instead of opening a public issue.
