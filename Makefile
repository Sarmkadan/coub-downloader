# =============================================================================
# Makefile for Coub Downloader
# Author: Vladyslav Zaiets | https://sarmkadan.com
# =============================================================================

.PHONY: help build test clean run docker lint format restore publish install uninstall

# Variables
DOTNET := dotnet
PROJECT := CoubDownloader.csproj
CONFIGURATION := Release
OUTPUT_DIR := bin/$(CONFIGURATION)/net10.0/publish
DOCKER_IMAGE := vladyslav-zaiets/coub-downloader
DOCKER_TAG := latest
VERSION := 1.2.0

# Determine OS
UNAME_S := $(shell uname -s)
ifeq ($(UNAME_S),Linux)
    OS := linux
endif
ifeq ($(UNAME_S),Darwin)
    OS := macos
endif
ifeq ($(OS_TYPE),Windows_NT)
    OS := windows
endif

# Default target
help:
	@echo "Coub Downloader - Build Targets"
	@echo "================================"
	@echo ""
	@echo "Available targets:"
	@echo "  make build          - Build the project"
	@echo "  make test           - Run tests"
	@echo "  make clean          - Clean build artifacts"
	@echo "  make run            - Run the application"
	@echo "  make docker-build   - Build Docker image"
	@echo "  make docker-run     - Run in Docker"
	@echo "  make docker-push    - Push Docker image to registry"
	@echo "  make lint           - Run code analysis"
	@echo "  make format         - Format code"
	@echo "  make restore        - Restore NuGet packages"
	@echo "  make publish        - Publish for distribution"
	@echo "  make install        - Install globally (Linux/macOS)"
	@echo "  make uninstall      - Uninstall from system (Linux/macOS)"
	@echo "  make docs           - Generate documentation"
	@echo "  make help           - Show this help message"
	@echo ""

# Restore NuGet packages
restore:
	@echo "Restoring NuGet packages..."
	$(DOTNET) restore $(PROJECT)
	@echo "✓ Restore completed"

# Build the project
build: restore
	@echo "Building $(PROJECT) ($(CONFIGURATION))..."
	$(DOTNET) build $(PROJECT) -c $(CONFIGURATION) --no-restore
	@echo "✓ Build completed"

# Run tests
test: build
	@echo "Running tests..."
	$(DOTNET) test --configuration $(CONFIGURATION) --no-build --verbosity normal
	@echo "✓ Tests completed"

# Run the application
run: build
	@echo "Running application..."
	$(DOTNET) run --project $(PROJECT) --no-build

# Clean build artifacts
clean:
	@echo "Cleaning build artifacts..."
	$(DOTNET) clean $(PROJECT)
	rm -rf bin obj publish
	@echo "✓ Clean completed"

# Code analysis
lint:
	@echo "Running code analysis..."
	$(DOTNET) build $(PROJECT) -c $(CONFIGURATION) --no-restore
	@echo "✓ Linting completed"

# Format code
format:
	@echo "Formatting code..."
	$(DOTNET) format $(PROJECT)
	@echo "✓ Formatting completed"

# Publish for distribution
publish: test
	@echo "Publishing for $(OS) x64..."
	$(DOTNET) publish $(PROJECT) \
		-c $(CONFIGURATION) \
		-o $(OUTPUT_DIR) \
		--self-contained \
		-p:PublishTrimmed=true \
		-p:PublishSingleFile=true
	@echo "✓ Publish completed to $(OUTPUT_DIR)"

# Docker build
docker-build:
	@echo "Building Docker image: $(DOCKER_IMAGE):$(DOCKER_TAG)"
	docker build -t $(DOCKER_IMAGE):$(DOCKER_TAG) \
		-t $(DOCKER_IMAGE):latest \
		-t $(DOCKER_IMAGE):v$(VERSION) .
	@echo "✓ Docker image built"

# Docker run
docker-run:
	@echo "Running Docker container..."
	docker run -it \
		-v $(PWD)/downloads:/downloads \
		$(DOCKER_IMAGE):$(DOCKER_TAG)

# Docker push
docker-push: docker-build
	@echo "Pushing Docker image to registry..."
	docker push $(DOCKER_IMAGE):$(DOCKER_TAG)
	docker push $(DOCKER_IMAGE):latest
	docker push $(DOCKER_IMAGE):v$(VERSION)
	@echo "✓ Docker image pushed"

# Install globally (Linux/macOS)
install: publish
	@echo "Installing Coub Downloader..."
ifdef INSTALL_PREFIX
	INSTALL_DIR=$(INSTALL_PREFIX)/bin
else
	INSTALL_DIR=/usr/local/bin
endif
	cp $(OUTPUT_DIR)/CoubDownloader $(INSTALL_DIR)/coub-downloader
	chmod +x $(INSTALL_DIR)/coub-downloader
	@echo "✓ Installation completed"
	@echo "  Location: $(INSTALL_DIR)/coub-downloader"
	@echo "  Run: coub-downloader --version"

# Uninstall from system (Linux/macOS)
uninstall:
	@echo "Uninstalling Coub Downloader..."
	rm -f /usr/local/bin/coub-downloader
	@echo "✓ Uninstall completed"

# Generate documentation
docs:
	@echo "Generating documentation..."
	@echo "Documentation files:"
	@echo "  - README.md"
	@echo "  - docs/getting-started.md"
	@echo "  - docs/architecture.md"
	@echo "  - docs/api-reference.md"
	@echo "  - docs/deployment.md"
	@echo "  - docs/faq.md"
	@echo "✓ Documentation generated"

# Docker Compose
compose-up:
	@echo "Starting Docker Compose..."
	docker-compose up -d
	@echo "✓ Docker Compose started"

compose-down:
	@echo "Stopping Docker Compose..."
	docker-compose down
	@echo "✓ Docker Compose stopped"

# Check dependencies
check-deps:
	@echo "Checking dependencies..."
	@echo -n "  .NET SDK: "
	@$(DOTNET) --version
	@echo -n "  FFmpeg: "
	@ffmpeg -version 2>/dev/null | head -1 || echo "NOT INSTALLED"
	@echo "✓ Dependency check completed"

# Development setup
dev-setup: restore
	@echo "Setting up development environment..."
	$(DOTNET) tool install -g dotnet-format
	@echo "✓ Development setup completed"

# Run CI pipeline locally
ci: clean restore build lint test
	@echo "✓ CI pipeline completed"

# Full build and test
all: ci publish docker-build
	@echo "✓ Full build pipeline completed"

.PHONY: build test clean run docker lint format restore publish install uninstall docs ci
