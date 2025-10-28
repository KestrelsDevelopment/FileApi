namespace KestrelsDev.FileApi.Services.ChecksumService;

using System.Security.Cryptography;

public class ChecksumService(ILogger<ChecksumService> logger) : IChecksumService
{
    public async Task<string> CalculateChecksumAsync(IFormFile file)
    {
        logger.LogInformation("Calculating checksum for file: {FileName}, Size: {FileSize} bytes", 
            file.FileName, file.Length);
        
        using SHA256 sha256 = SHA256.Create();
        await using Stream stream = file.OpenReadStream();
        byte[] hash = await Task.Run(() => sha256.ComputeHash(stream));
        string checksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        
        logger.LogInformation("Checksum calculated successfully for file: {FileName}, Checksum: {Checksum}", 
            file.FileName, checksum);
        
        return checksum;
    }
    
    public async Task<string> CalculateChecksumFromFileAsync(string filePath)
    {
        logger.LogInformation("Calculating checksum from file path: {FilePath}", filePath);
        
        try
        {
            using SHA256 sha256 = SHA256.Create();
            await using FileStream stream = File.OpenRead(filePath);
            byte[] hash = await sha256.ComputeHashAsync(stream);
            string checksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            
            logger.LogInformation("Checksum calculated successfully for file: {FilePath}, Checksum: {Checksum}", 
                filePath, checksum);
            
            return checksum;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating checksum for file: {FilePath}", filePath);
            throw;
        }
    }
    
    public bool ChecksumsMatch(string checksum1, string checksum2)
    {
        bool match = checksum1.Equals(checksum2, StringComparison.OrdinalIgnoreCase);
        
        logger.LogDebug("Comparing checksums - Match: {Match}, Checksum1: {Checksum1}, Checksum2: {Checksum2}", 
            match, checksum1, checksum2);
        
        return match;
    }
}