using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace KestrelsDev.FileApi.Controllers;

[ApiController]
[Route("")]
public class FileController(ILogger<FileController> logger) : ControllerBase
{
    [HttpPost("upload")]
    [RequestSizeLimit(10737418240)] // 10GB
    [RequestFormLimits(MultipartBodyLengthLimit = 10737418240)] // 10GB
    public async Task<ActionResult> Upload([FromForm]IFormFile file, [FromHeader]string? checksum, [FromHeader]string authorization)
    {
        string? psk = Environment.GetEnvironmentVariable("API_UPLOAD_PSK");
        string? path = Environment.GetEnvironmentVariable("API_UPLOAD_PATH");
        
        if (psk is null || path is null) 
            return StatusCode(503, "ENV not set");
        
        if (psk != authorization) 
            return Unauthorized("Invalid PSK");

        string fileName = Path.GetFileName(file.FileName);
        string filePath = Path.Combine(path, fileName);
        
        if (checksum is not null)
        {
            string fileCheckSum = await CalculateChecksum(file);
            if (!fileCheckSum.Equals(checksum, StringComparison.OrdinalIgnoreCase)) 
                return BadRequest("Checksums do not match, File got Mangled in Transit");
            
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    string existingFileChecksum = await CalculateChecksumFromFile(filePath);
                    if (existingFileChecksum.Equals(checksum, StringComparison.OrdinalIgnoreCase))
                        return Ok("File Already Exists no Action Taken");
                }
            }
            catch (Exception e)
            {
                logger.LogError("Error Checking for Existing File: {Error}", e);
            }
        }
        
        try
        {
            await using FileStream fs = new(filePath, FileMode.Create);
            await file.CopyToAsync(fs);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(507, "File Upload Failed");
        }
        
        try
        {
            CleanupOldFiles(path);
        }
        catch (Exception e)
        {
            logger.LogWarning("Failed to cleanup old files: {Error}", e);
        }

        return StatusCode(201, "Upload Successful");
    }
    
    [HttpGet("download")]
    public ActionResult Download([FromQuery] string? fileName)
    {
        string? path = Environment.GetEnvironmentVariable("API_UPLOAD_PATH");
        
        if (path is null)
            return StatusCode(503, "ENV not set");
        
        if (!Directory.Exists(path))
            return StatusCode(503, "Upload directory does not exist");
        
        DirectoryInfo dirInfo = new(path);
        FileInfo[] files = dirInfo.GetFiles();
        
        if (files.Length == 0)
            return NotFound("No files available for download");
        
        FileInfo? fileToDownload;
        
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileToDownload = files.OrderByDescending(f => f.CreationTime).First();
        }
        else
        {
            fileToDownload = files.FirstOrDefault(f => f.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            
            if (fileToDownload is null)
                return NotFound($"File '{fileName}' not found");
        }
        
        try
        {
            Stream fileStream = new FileStream(fileToDownload.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(fileStream, "application/octet-stream", fileToDownload.Name, enableRangeProcessing: true);
        }
        catch (Exception e)
        {
            logger.LogError("Error reading file {File}: {Error}", fileToDownload.FullName, e);
            return StatusCode(500, "Error reading file");
        }
    }
    
    [HttpGet("list")]
    public ActionResult List()
    {
        string? path = Environment.GetEnvironmentVariable("API_UPLOAD_PATH");
        
        if (path is null)
            return StatusCode(503, "ENV not set");
        
        if (!Directory.Exists(path))
            return StatusCode(503, "Upload directory does not exist");
        
        try
        {
            DirectoryInfo dirInfo = new(path);
            FileInfo[] files = dirInfo.GetFiles()
                .OrderByDescending(f => f.CreationTime)
                .ToArray();
            
            var fileList = files.Select(f => new
            {
                fileName = f.Name,
                sizeMB = Math.Round(f.Length / (1024.0 * 1024.0), 2),
                createdAt = f.CreationTime,
            });
            
            return Ok(fileList);
        }
        catch (Exception e)
        {
            logger.LogError("Error listing files: {Error}", e);
            return StatusCode(500, "Error listing files");
        }
    }
    
    private async Task<string> CalculateChecksum(IFormFile file)
    {
        using SHA256 sha256 = SHA256.Create();
        await using Stream stream = file.OpenReadStream();
        byte[] hash = await Task.Run(() => sha256.ComputeHash(stream));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
    
    private async Task<string> CalculateChecksumFromFile(string filePath)
    {
        using SHA256 sha256 = SHA256.Create();
        await using FileStream stream = System.IO.File.OpenRead(filePath);
        byte[] hash = await sha256.ComputeHashAsync(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
    
    private void CleanupOldFiles(string directoryPath)
    {
        string? maxFilesEnv = Environment.GetEnvironmentVariable("API_UPLOAD_MAX_FILES");
        int maxFiles = 5;
        
        if (maxFilesEnv is not null && int.TryParse(maxFilesEnv, out int parsedMaxFiles))
        {
            maxFiles = parsedMaxFiles;
        }
        
        if (maxFiles <= 0)
            return;
        

        DirectoryInfo dirInfo = new(directoryPath);
        IEnumerable<FileInfo> files = dirInfo.GetFiles()
            .OrderBy(f => f.CreationTime)
            .ToList();
        
        if (files.Count() > maxFiles)
        {
            foreach (FileInfo file in files.Take(files.Count() - maxFiles))
            {
                try
                {
                    file.Delete();
                    logger.LogInformation("Deleted File: {File}", file.FullName);
                }
                catch (Exception e)
                {
                    logger.LogError("Error Deleting File {File}: {Error}", file.FullName, e);
                }
            }
        }
    }
}