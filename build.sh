#!/bin/bash

# =============================================================================
# Build script for Coub Downloader
# Author: Vladyslav Zaiets | https://sarmkadan.com
# Supports: Linux, macOS
# =============================================================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
PROJECT="CoubDownloader.csproj"
CONFIGURATION="Release"
OUTPUT_DIR="bin/${CONFIGURATION}/net10.0/publish"
VERSION="1.2.0"

# Functions
log_info() {
    echo -e "${GREEN}ℹ${NC} $1"
}

log_success() {
    echo -e "${GREEN}✓${NC} $1"
}

log_error() {
    echo -e "${RED}✗${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

# Check prerequisites
check_prerequisites() {
    log_info "Checking prerequisites..."

    if ! command -v dotnet &> /dev/null; then
        log_error ".NET SDK not found"
        echo "  Install from: https://dotnet.microsoft.com/download"
        exit 1
    fi

    local dotnet_version=$(dotnet --version)
    log_success ".NET SDK version: $dotnet_version"

    if ! command -v ffmpeg &> /dev/null; then
        log_warning "FFmpeg not found (optional for building)"
    else
        log_success "FFmpeg found"
    fi
}

# Clean build artifacts
clean() {
    log_info "Cleaning build artifacts..."
    dotnet clean "$PROJECT" --configuration "$CONFIGURATION" 2>/dev/null || true
    rm -rf bin obj publish
    log_success "Clean completed"
}

# Restore packages
restore() {
    log_info "Restoring NuGet packages..."
    dotnet restore "$PROJECT"
    log_success "Restore completed"
}

# Build project
build() {
    log_info "Building project (${CONFIGURATION})..."
    dotnet build "$PROJECT" \
        --configuration "$CONFIGURATION" \
        --no-restore \
        --verbosity normal
    log_success "Build completed"
}

# Run tests
run_tests() {
    log_info "Running tests..."
    dotnet test \
        --configuration "$CONFIGURATION" \
        --no-build \
        --verbosity normal
    log_success "Tests completed"
}

# Publish application
publish() {
    log_info "Publishing for distribution..."
    dotnet publish "$PROJECT" \
        --configuration "$CONFIGURATION" \
        --output "$OUTPUT_DIR" \
        --self-contained \
        -p:PublishTrimmed=true \
        -p:PublishSingleFile=true
    log_success "Publish completed"
    echo "  Output: $(pwd)/$OUTPUT_DIR"
}

# Install globally
install() {
    log_info "Installing Coub Downloader..."

    if [ ! -d "$OUTPUT_DIR" ]; then
        log_error "Publish directory not found. Run build and publish first."
        exit 1
    fi

    local install_dir="/usr/local/bin"

    if [ ! -w "$install_dir" ]; then
        log_warning "Cannot write to $install_dir without sudo"
        install_dir="$HOME/.local/bin"
        mkdir -p "$install_dir"
    fi

    cp "$OUTPUT_DIR/CoubDownloader" "$install_dir/coub-downloader"
    chmod +x "$install_dir/coub-downloader"

    log_success "Installation completed"
    echo "  Location: $install_dir/coub-downloader"

    # Add to PATH if necessary
    if [[ ":$PATH:" != *":$install_dir:"* ]]; then
        log_warning "Add to PATH: export PATH=\"$install_dir:\$PATH\""
    fi

    echo "  Verify: coub-downloader --version"
}

# Format code
format() {
    log_info "Formatting code..."
    dotnet format "$PROJECT"
    log_success "Formatting completed"
}

# Analyze code
analyze() {
    log_info "Analyzing code..."
    dotnet build "$PROJECT" \
        --configuration "$CONFIGURATION" \
        --no-restore \
        --no-incremental \
        -p:TreatWarningsAsErrors=true
    log_success "Analysis completed"
}

# Build Docker image
docker_build() {
    log_info "Building Docker image..."
    docker build -t coub-downloader:latest \
        -t coub-downloader:v"$VERSION" \
        --build-arg VERSION="$VERSION" .
    log_success "Docker image built"
}

# Show help
show_help() {
    cat << EOF
Coub Downloader - Build Script
Author: Vladyslav Zaiets

Usage: $(basename "$0") [COMMAND]

Commands:
  check       Check prerequisites
  clean       Clean build artifacts
  restore     Restore NuGet packages
  build       Build project
  test        Run tests
  publish     Publish for distribution
  install     Install globally
  format      Format code
  analyze     Analyze code
  docker      Build Docker image
  all         Run all steps (clean, restore, build, test, publish)
  help        Show this help message

Examples:
  $(basename "$0") build
  $(basename "$0") all
  $(basename "$0") install

EOF
}

# Main
main() {
    case "${1:-help}" in
        check)
            check_prerequisites
            ;;
        clean)
            clean
            ;;
        restore)
            restore
            ;;
        build)
            check_prerequisites
            restore
            build
            ;;
        test)
            build
            run_tests
            ;;
        publish)
            run_tests
            publish
            ;;
        install)
            publish
            install
            ;;
        format)
            format
            ;;
        analyze)
            analyze
            ;;
        docker)
            docker_build
            ;;
        all)
            check_prerequisites
            clean
            restore
            build
            run_tests
            publish
            ;;
        help)
            show_help
            ;;
        *)
            log_error "Unknown command: $1"
            echo ""
            show_help
            exit 1
            ;;
    esac
}

# Run main
main "$@"
