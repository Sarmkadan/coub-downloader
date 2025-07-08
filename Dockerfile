# =============================================================================
# Dockerfile for Coub Downloader
# Author: Vladyslav Zaiets | https://sarmkadan.com
# Multi-stage build optimized for production
# =============================================================================

# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS builder

WORKDIR /src

# Copy project file and restore dependencies
COPY ["CoubDownloader.csproj", "."]
RUN dotnet restore "CoubDownloader.csproj"

# Copy source code
COPY . .

# Build and publish
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine

# Install FFmpeg and other dependencies
RUN apk add --no-cache \
    ffmpeg \
    curl \
    ca-certificates \
    && rm -rf /var/cache/apk/*

# Create app user
RUN addgroup -g 1000 app && adduser -D -u 1000 -G app app

WORKDIR /app

# Copy published application from builder
COPY --from=builder /app/publish .

# Change ownership
RUN chown -R app:app /app

# Create download directory
RUN mkdir -p /downloads && chown -R app:app /downloads

# Switch to app user
USER app

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Default command
ENTRYPOINT ["dotnet", "CoubDownloader.dll"]
CMD ["--help"]

# Labels
LABEL maintainer="Vladyslav Zaiets <vladyslav.zaiets@amdaris.com>"
LABEL description="Download and convert Coub videos to MP4/Shorts format"
LABEL version="1.0.0"
LABEL org.opencontainers.image.source="https://github.com/vladyslav-zaiets/coub-downloader"
