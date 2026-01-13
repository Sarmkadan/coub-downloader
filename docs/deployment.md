# Deployment Guide

## Docker Deployment

### Building Docker Image

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0
RUN apt-get update && apt-get install -y ffmpeg && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CoubDownloader.dll"]
```

### Running Container

```bash
docker run -it \
  -v /path/to/downloads:/downloads \
  -e COUB_OUTPUT_PATH=/downloads \
  coub-downloader:latest \
  download --url https://coub.com/view/2a3b4c5d --output /downloads/video.mp4
```

### Docker Compose

```yaml
version: '3.8'

services:
  coub-downloader:
    build: .
    container_name: coub-downloader
    volumes:
      - ./downloads:/downloads
      - ./config:/app/config
    environment:
      COUB_OUTPUT_PATH: /downloads
      COUB_LOG_LEVEL: Information
    command: download --url https://coub.com/view/2a3b4c5d --output /downloads/video.mp4
```

## Linux Deployment

### Debian/Ubuntu

```bash
# Install dependencies
sudo apt-get update
sudo apt-get install -y dotnet-runtime-10.0 ffmpeg

# Download and install
wget https://github.com/vladyslav-zaiets/coub-downloader/releases/download/v1.0.0/coub-downloader-linux-x64.tar.gz
tar xzf coub-downloader-linux-x64.tar.gz
sudo mv coub-downloader /usr/local/bin/

# Verify
coub-downloader --version
```

### Systemd Service

Create `/etc/systemd/system/coub-downloader.service`:

```ini
[Unit]
Description=Coub Downloader Service
After=network.target

[Service]
Type=simple
User=coub
WorkingDirectory=/home/coub/coub-downloader
ExecStart=/usr/local/bin/coub-downloader batch --config config.json
Restart=on-failure
RestartSec=10

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl daemon-reload
sudo systemctl enable coub-downloader
sudo systemctl start coub-downloader
```

## Windows Deployment

### Standalone Installation

1. Download from [Releases](https://github.com/vladyslav-zaiets/coub-downloader/releases)
2. Extract to `C:\Program Files\CoubDownloader\`
3. Add to PATH: `C:\Program Files\CoubDownloader`
4. Verify: `coub-downloader --version`

### Windows Service

Use NSSM (Non-Sucking Service Manager):

```bash
# Install NSSM
choco install nssm

# Create service
nssm install CoubDownloader "C:\Program Files\CoubDownloader\coub-downloader.exe"
nssm set CoubDownloader AppDirectory "C:\Program Files\CoubDownloader"

# Start service
nssm start CoubDownloader
```

## Cloud Deployment

### AWS Deployment

1. **Create EC2 Instance:**
   ```bash
   # Launch t3.medium instance with Ubuntu 22.04
   # Allocate 50GB EBS volume
   ```

2. **Install Dependencies:**
   ```bash
   sudo apt-get update
   sudo apt-get install -y dotnet-runtime-10.0 ffmpeg
   ```

3. **Deploy Application:**
   ```bash
   wget https://github.com/vladyslav-zaiets/coub-downloader/releases/download/v1.0.0/coub-downloader-linux-x64.tar.gz
   tar xzf coub-downloader-linux-x64.tar.gz
   sudo mv coub-downloader /usr/local/bin/
   ```

4. **Setup S3 Output:**
   ```bash
   # Install AWS CLI
   pip install awscli
   aws configure
   ```

### Azure Deployment

1. **Create Container Instance:**
   ```bash
   az container create \
     --resource-group myResourceGroup \
     --name coub-downloader \
     --image vladyslav-zaiets/coub-downloader:latest \
     --cpu 2 \
     --memory 2
   ```

2. **Configure Volume:**
   ```bash
   az container create \
     --resource-group myResourceGroup \
     --name coub-downloader \
     --image vladyslav-zaiets/coub-downloader:latest \
     --azure-file-volume-account-name mystorageaccount \
     --azure-file-volume-account-key storagekey \
     --azure-file-volume-share-name myshare \
     --azure-file-volume-mount-path /downloads
   ```

### Google Cloud Deployment

1. **Create Compute Instance:**
   ```bash
   gcloud compute instances create coub-downloader \
     --image-family=ubuntu-2204-lts \
     --image-project=ubuntu-os-cloud \
     --machine-type=e2-medium
   ```

2. **Deploy Application:**
   ```bash
   gcloud compute scp coub-downloader-linux-x64.tar.gz coub-downloader:~
   gcloud compute ssh coub-downloader --command="tar xzf coub-downloader-linux-x64.tar.gz"
   ```

## Performance Optimization

### CPU Optimization

```bash
# Enable multi-threaded FFmpeg
export FFMPEG_THREAD_QUEUE_SIZE=512
export FFMPEG_THREAD_COUNT=8

# Run with specific CPU cores
taskset -c 0-3 coub-downloader download --url <url> --output output.mp4
```

### Memory Optimization

```bash
# Limit memory usage
ulimit -v 2097152  # 2GB

# Run with .NET memory limits
DOTNET_GCHeapCount=2 DOTNET_GCHeapAffinitizeMask=3 coub-downloader
```

### Storage Optimization

```bash
# Use fast SSD for temporary files
export TEMP=/ssd/temp
export TMP=/ssd/tmp

# Enable disk caching
coub-downloader --config appsettings.json
```

## Monitoring and Logging

### Application Logging

```bash
# Enable verbose logging
coub-downloader --log-level debug download --url <url>

# Log to file
coub-downloader --log-file app.log download --url <url>
```

### System Monitoring

```bash
# Monitor CPU and Memory
watch -n 1 'ps aux | grep coub-downloader'

# Monitor Disk I/O
iostat -d 1

# Monitor Network
iftop -n
```

### Health Checks

```bash
# Check application status
curl http://localhost:5000/health

# Get metrics
curl http://localhost:5000/metrics
```

## Backup and Recovery

### Backup Configuration

```bash
# Backup configuration files
tar czf coub-config-backup.tar.gz /etc/coub-downloader/

# Upload to storage
gsutil cp coub-config-backup.tar.gz gs://my-bucket/
```

### Disaster Recovery

```bash
# Restore from backup
tar xzf coub-config-backup.tar.gz -C /

# Verify restored files
ls -la /etc/coub-downloader/
```

## Security Considerations

### API Key Management

```bash
# Store API keys in environment variables
export COUB_API_KEY="your-api-key"
export COUB_API_SECRET="your-api-secret"

# Or use configuration files with restricted permissions
chmod 600 /etc/coub-downloader/secrets.json
```

### Network Security

```bash
# Run behind firewall
sudo ufw allow 5000/tcp

# Use HTTPS
coub-downloader --use-https --cert /path/to/cert.pem
```

### File Permissions

```bash
# Restrict download directory permissions
chmod 750 /downloads
chown coub:coub /downloads

# Restrict application permissions
sudo chown coub:coub /usr/local/bin/coub-downloader
chmod 755 /usr/local/bin/coub-downloader
```
