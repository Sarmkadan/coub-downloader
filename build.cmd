@REM =============================================================================
@REM Build script for Coub Downloader (Windows)
@REM Author: Vladyslav Zaiets | https://sarmkadan.com
@REM =============================================================================

@echo off
setlocal enabledelayedexpansion

REM Configuration
set PROJECT=CoubDownloader.csproj
set CONFIGURATION=Release
set OUTPUT_DIR=bin\%CONFIGURATION%\net10.0\publish
set VERSION=1.2.0

REM Check if command is provided
if "%1"=="" (
    call :show_help
    exit /b 0
)

REM Process command
if "%1"=="check" (
    call :check_prerequisites
) else if "%1"=="clean" (
    call :clean
) else if "%1"=="restore" (
    call :restore
) else if "%1"=="build" (
    call :check_prerequisites
    call :restore
    call :build
) else if "%1"=="test" (
    call :build
    call :run_tests
) else if "%1"=="publish" (
    call :run_tests
    call :publish
) else if "%1"=="install" (
    call :publish
    call :install
) else if "%1"=="format" (
    call :format
) else if "%1"=="analyze" (
    call :analyze
) else if "%1"=="docker" (
    call :docker_build
) else if "%1"=="all" (
    call :check_prerequisites
    call :clean
    call :restore
    call :build
    call :run_tests
    call :publish
) else if "%1"=="help" (
    call :show_help
) else (
    echo Unknown command: %1
    echo.
    call :show_help
    exit /b 1
)

exit /b 0

:check_prerequisites
    echo [*] Checking prerequisites...

    dotnet --version >nul 2>&1
    if errorlevel 1 (
        echo [X] .NET SDK not found
        echo     Install from: https://dotnet.microsoft.com/download
        exit /b 1
    )

    for /f "tokens=*" %%i in ('dotnet --version') do (
        echo [OK] .NET SDK version: %%i
    )

    ffmpeg -version >nul 2>&1
    if errorlevel 1 (
        echo [!] FFmpeg not found (optional for building^)
    ) else (
        echo [OK] FFmpeg found
    )

    echo.
    exit /b 0

:clean
    echo [*] Cleaning build artifacts...
    dotnet clean %PROJECT% --configuration %CONFIGURATION% 2>nul
    if exist bin rmdir /s /q bin
    if exist obj rmdir /s /q obj
    if exist publish rmdir /s /q publish
    echo [OK] Clean completed
    echo.
    exit /b 0

:restore
    echo [*] Restoring NuGet packages...
    dotnet restore %PROJECT%
    echo [OK] Restore completed
    echo.
    exit /b 0

:build
    echo [*] Building project ^(%CONFIGURATION%^)...
    dotnet build %PROJECT% --configuration %CONFIGURATION% --no-restore --verbosity normal
    echo [OK] Build completed
    echo.
    exit /b 0

:run_tests
    echo [*] Running tests...
    dotnet test --configuration %CONFIGURATION% --no-build --verbosity normal
    echo [OK] Tests completed
    echo.
    exit /b 0

:publish
    echo [*] Publishing for distribution...
    dotnet publish %PROJECT% ^
        --configuration %CONFIGURATION% ^
        --output %OUTPUT_DIR% ^
        --self-contained ^
        -p:PublishTrimmed=true ^
        -p:PublishSingleFile=true
    echo [OK] Publish completed
    echo     Output: %CD%\%OUTPUT_DIR%
    echo.
    exit /b 0

:install
    echo [*] Installing Coub Downloader...

    if not exist "%OUTPUT_DIR%" (
        echo [X] Publish directory not found. Run build and publish first.
        exit /b 1
    )

    set INSTALL_DIR=%ProgramFiles%\CoubDownloader
    mkdir "%INSTALL_DIR%" 2>nul

    copy "%OUTPUT_DIR%\CoubDownloader.exe" "%INSTALL_DIR%\coub-downloader.exe"

    echo [OK] Installation completed
    echo     Location: %INSTALL_DIR%\coub-downloader.exe
    echo.
    exit /b 0

:format
    echo [*] Formatting code...
    dotnet format %PROJECT%
    echo [OK] Formatting completed
    echo.
    exit /b 0

:analyze
    echo [*] Analyzing code...
    dotnet build %PROJECT% ^
        --configuration %CONFIGURATION% ^
        --no-restore ^
        --no-incremental ^
        -p:TreatWarningsAsErrors=true
    echo [OK] Analysis completed
    echo.
    exit /b 0

:docker_build
    echo [*] Building Docker image...
    docker build -t coub-downloader:latest -t coub-downloader:v%VERSION% --build-arg VERSION=%VERSION% .
    echo [OK] Docker image built
    echo.
    exit /b 0

:show_help
    echo Coub Downloader - Build Script (Windows^)
    echo Author: Vladyslav Zaiets
    echo.
    echo Usage: %0 [COMMAND]
    echo.
    echo Commands:
    echo   check       Check prerequisites
    echo   clean       Clean build artifacts
    echo   restore     Restore NuGet packages
    echo   build       Build project
    echo   test        Run tests
    echo   publish     Publish for distribution
    echo   install     Install globally
    echo   format      Format code
    echo   analyze     Analyze code
    echo   docker      Build Docker image
    echo   all         Run all steps
    echo   help        Show this help message
    echo.
    echo Examples:
    echo   %0 build
    echo   %0 all
    echo   %0 install
    echo.
    exit /b 0
