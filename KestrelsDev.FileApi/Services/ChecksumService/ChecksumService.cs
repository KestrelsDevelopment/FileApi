namespace KestrelsDev.FileApi.Services.ChecksumService;

using System.Collections.Concurrent;
using System.Security.Cryptography;
using Models;

public class ChecksumService(ILogger<ChecksumService> logger) : IChecksumService
{
    private readonly ConcurrentDictionary<string, FileInfoDto> _fileCache = new();

    public async Task<string> CalculateChecksumAsync(IFormFile file)
    {
        logger.LogInformation("Calculating checksum for file: {FileName}, Size: {FileSize} bytes", 
            file.FileName, file.Length);
        
        try
        {
            using SHA256 sha256 = SHA256.Create();
            await using Stream stream = file.OpenReadStream();
            byte[] hash = await Task.Run(() => sha256.ComputeHash(stream));
            string checksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            
            return checksum;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating checksum for file: {FileName}", file.FileName);
            return string.Empty;
        }
    }
    
    public async Task<string> CalculateChecksumFromFileAsync(string filePath)
    {
        try
        {
            using SHA256 sha256 = SHA256.Create();
            await using FileStream stream = File.OpenRead(filePath);
            byte[] hash = await sha256.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating checksum for file: {FilePath}", filePath);
            return string.Empty;
        }
    }
    
    public void AddOrUpdateFile(FileInfoDto fileInfo)
    {
        _fileCache.AddOrUpdate(fileInfo.FileName, fileInfo, (_, _) => fileInfo);
        logger.LogDebug("Cache updated for file: {FileName}", fileInfo.FileName);
    }

    public void RemoveFile(string fileName)
    {
        if (_fileCache.TryRemove(fileName, out _))
        {
            logger.LogInformation("File removed from cache: {FileName}", fileName);
        }
    }

    public IEnumerable<FileInfoDto> GetCachedFiles()
    {
        // Return files sorted by creation date (newest first)
        return _fileCache.Values.OrderByDescending(f => f.CreatedAt);
    }
    
    public bool ChecksumsMatch(string checksum1, string checksum2)
    {
        if (string.IsNullOrEmpty(checksum1) || string.IsNullOrEmpty(checksum2)) return false;
        
        bool match = checksum1.Equals(checksum2, StringComparison.OrdinalIgnoreCase);
        
        if (!match)
        {
            logger.LogWarning("Checksum mismatch detected.");
        }
        return match;
    }
}