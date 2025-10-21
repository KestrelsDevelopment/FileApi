# FileApi

A lightweight ASP.NET Core Web API for file upload, download, and management operations with built-in checksum validation and automatic file cleanup.

## Features

- **File Upload** - Upload files up to 10GB with SHA256 checksum verification
- **File Download** - Download files by name or retrieve the most recent file
- **File Listing** - List all available files with metadata (size, creation date, sha256)
- **Automatic Cleanup** - Configurable automatic deletion of old files
- **PSK Authentication** - Pre-shared key authentication for uploads
- **Resume Support** - Range request support for interrupted downloads
- **Docker Support** - Ready-to-deploy Docker configuration

## Prerequisites

- .NET 9.0 SDK
- Docker (optional, for containerized deployment)

## Configuration

The API uses environment variables for configuration:

| Variable | Required | Description | Default |
|----------|----------|-------------|---------|
| `API_UPLOAD_PSK` | Yes | Pre-shared key for upload authentication | - |
| `API_UPLOAD_PATH` | Yes | Directory path for file storage | - |
| `API_UPLOAD_MAX_FILES` | No | Maximum number of files to retain (0 = unlimited) | 5 |

### Example Configuration

TODO

## API Endpoints

### POST /upload
Upload a file with optional checksum verification.

**Headers:**
- `Authorization`: Pre-shared key (required)
- `Checksum`: SHA256 hash of the file (optional)

**Form Data:**
- `file`: File to upload (max 10GB)

**Responses:**
- `201` - Upload successful
- `200` - File already exists with matching checksum
- `400` - Checksum mismatch
- `401` - Invalid PSK
- `503` - Server misconfigured
- `507` - Insufficient storage

### GET /download
Download a file.

**Query Parameters:**
- `fileName`: Specific file name (optional, defaults to most recent file)

**Responses:**
- `200` - File content with range support
- `503` - File or directory not found
- `500` - Error reading file
- `503` - Server misconfigured

### GET /list
List all available files.

**Responses:**
- `200` - JSON array of files with metadata:
  ```json
  [
  {
  "fileName": "example.txt",
  "sizeMB": 1.25,
  "createdAt": "2025-10-21T10:30:00"
  }
  ]
  ```
- `503` - Directory not found
- `500` - Error listing files
- `503` - Server misconfigured

## Security Considerations

- Store `API_UPLOAD_PSK` securely (use Docker secrets, environment variables, or key vaults)
- Consider implementing rate limiting for production use
- The API accepts uploads up to 10GB - adjust `RequestSizeLimit` as needed
- Download endpoint is public - consider adding authentication if needed
- Ensure proper file system permissions for `API_UPLOAD_PATH`
