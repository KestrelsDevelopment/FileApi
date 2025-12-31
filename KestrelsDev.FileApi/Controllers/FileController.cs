
using KestrelsDev.FileApi.Models;
using KestrelsDev.FileApi.Services.ConfigurationService;
using KestrelsDev.FileApi.Services.FileStorageService;
using KestrelsDev.FileApi.Services.ChecksumService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace KestrelsDev.FileApi.Controllers;

[ApiController]
[Route("")]
public class FileController(
    ILogger<FileController> logger,
    IConfigurationService configService,
    IChecksumService checksumService,
    IFileStorageService fileStorageService) : ControllerBase
{
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    [HttpPost("upload")]
    [RequestSizeLimit(10737418240)] // 10GB
    [RequestFormLimits(MultipartBodyLengthLimit = 10737418240)] // 10GB
    public async Task<ActionResult> Upload(
        [FromForm] IFormFile file,
        [FromHeader] string? checksum,
        [FromHeader] string authorization)
    {
        // Validate configuration
        if (configService.UploadPath is null)
            return StatusCode(503, "Server not configured properly");
        
        string fileName = Path.GetFileName(file.FileName);
        
        // Validate checksum if provided
        if (checksum is not null)
        {
            string fileCheckSum = await checksumService.CalculateChecksumAsync(file);
            if (!checksumService.ChecksumsMatch(fileCheckSum, checksum))
                return BadRequest("Checksums do not match, file corrupted in transit");
            
            // Check if file already exists with same checksum
            if (fileStorageService.FileExists(fileName))
            {
                try
                {
                    string filePath = Path.Combine(configService.UploadPath, fileName);
                    string existingFileChecksum = await checksumService.CalculateChecksumFromFileAsync(filePath);
                    if (checksumService.ChecksumsMatch(existingFileChecksum, checksum))
                        return Ok("File already exists, no action taken");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error checking existing file checksum");
                }
            }
        }
        
        // Save the file
        (bool success, string? errorMessage) = await fileStorageService.SaveFileAsync(file, fileName);
        if (!success)
            return StatusCode(507, errorMessage ?? "File upload failed");
        
        // Cleanup old files
        _ = Task.Run(async () =>
        {
            try
            {
                await fileStorageService.CleanupOldFilesAsync();
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to cleanup old files");
            }
        });

        return StatusCode(201, new { latestPath= "/download", exactPath= "/download?fileName=" + fileName});
    }
    
    [HttpGet("download")]
    public ActionResult Download([FromQuery] string? fileName)
    {
        if (configService.UploadPath is null)
            return StatusCode(503, "Server not configured properly");
        
        if (!Directory.Exists(configService.UploadPath))
            return StatusCode(503, "Upload directory does not exist");
        
        FileInfo? fileToDownload = fileStorageService.GetFile(fileName);
        
        if (fileToDownload is null)
        {
            return string.IsNullOrWhiteSpace(fileName)
                ? NotFound("No files available for download")
                : NotFound($"File '{fileName}' not found");
        }
        
        try
        {
            Stream fileStream = new FileStream(
                fileToDownload.FullName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
            
            // Detect MIME type from file extension
            if (!ContentTypeProvider.TryGetContentType(fileToDownload.Name, out string? contentType))
            {
                contentType = "application/octet-stream";
            }
            
            return File(fileStream, contentType, fileToDownload.Name, enableRangeProcessing: true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error reading file {FileName}", fileToDownload.FullName);
            return StatusCode(500, "Error reading file");
        }
    }
    
    [HttpGet("list")]
    public ActionResult<IEnumerable<FileInfoDto>> List()
    {
        if (configService.UploadPath is null)
            return StatusCode(503, "Server not configured properly");
        
        if (!Directory.Exists(configService.UploadPath))
            return StatusCode(503, "Upload directory does not exist");
        
        try
        {
            IEnumerable<FileInfoDto> fileList = fileStorageService.GetAllFiles()
                .Select(f => new FileInfoDto(
                    f.Name,
                    Math.Round(f.Length / (1024.0 * 1024.0), 2),
                    f.Length,
                    checksumService.CalculateChecksumFromFileAsync(f.FullName).Result,
                    f.CreationTime
                ));
            
            return Ok(fileList);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error listing files");
            return StatusCode(500, "Error listing files");
        }
    }
    
    [HttpGet("health")]
    public ActionResult HealthCheck() => Ok();
}