using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace KestrelsDev.FileApi.Controllers;

[ApiController]
[Route("")]
public class FileController : ControllerBase
{
    [HttpPost("upload")]
    public async Task<ActionResult> Upload([FromBody]IFormFile file, [FromHeader]string? checksum, [FromHeader]string authorization)
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
            if (fileCheckSum != checksum) 
                return BadRequest("Checksums do not match, File got Mangled in Transit");
            
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    string existingFileChecksum = await CalculateChecksumFromFile(filePath);
                    if (existingFileChecksum == checksum)
                        return Ok("File Already Exists no Action Taken");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); // TODO TO CHANGE
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
            Console.WriteLine($"Warning: Failed to cleanup old files: {e.Message}"); // todo Change To Proper Logger
        }

        return StatusCode(201, "Upload Successful");
    }
    
    [HttpGet("")]
    public ActionResult Download()
    {
        return Ok();
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
        // Get max files from environment variable, default to 5
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
            .OrderByDescending(f => f.CreationTime)
            .ToList();
        
        if (files.Count() > maxFiles)
        {
           
        }
    }
}