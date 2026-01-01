namespace KestrelsDev.FileApi.Services.ChecksumService;

using System.Collections.Concurrent;
using System.Security.Cryptography;
using KestrelsDev.FileApi.Services.ConfigurationService;

public class ChecksumService : IChecksumService
{
    private readonly ILogger<ChecksumService> _logger;
    private readonly ConcurrentDictionary<string, string> _checksumCache = new();

    public ChecksumService(ILogger<ChecksumService> logger, IConfigurationService configService)
    {
        _logger = logger;
        InitializeCache(configService.UploadPath);
    }

    private void InitializeCache(string? uploadPath)
    {
        if (string.IsNullOrEmpty(uploadPath) || !Directory.Exists(uploadPath))
        {
            _logger.LogWarning("Upload path is invalid or does not exist. Skipping checksum cache initialization.");
            return;
        }

        Task.Run(async () =>
        {
            _logger.LogInformation("Starting background checksum calculation for files in {UploadPath}", uploadPath);
            try
            {
                string[] files = Directory.GetFiles(uploadPath);
                foreach (string file in files)
                {
                    // CalculateChecksumFromFileAsync handles caching internally
                    await CalculateChecksumFromFileAsync(file);
                }
                _logger.LogInformation("Finished background checksum calculation. Cached {Count} files.", _checksumCache.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during background checksum calculation");
            }
        });
    }

    public async Task<string> CalculateChecksumAsync(IFormFile file)
    {
        _logger.LogInformation("Calculating checksum for file: {FileName}, Size: {FileSize} bytes", 
            file.FileName, file.Length);
        
        try
        {
            using SHA256 sha256 = SHA256.Create();
            await using Stream stream = file.OpenReadStream();
            byte[] hash = await Task.Run(() => sha256.ComputeHash(stream));
            string checksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        
            _logger.LogInformation("Checksum calculated successfully for file: {FileName}, Checksum: {Checksum}", 
                file.FileName, checksum);
        
            return checksum;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating checksum for file: {FileName}", file.FileName);
            return string.Empty;
        }
    }
    
    public async Task<string> CalculateChecksumFromFileAsync(string filePath)
    {
        string fullPath = Path.GetFullPath(filePath);

        if (_checksumCache.TryGetValue(fullPath, out string? cachedChecksum))
        {
            _logger.LogDebug("Cache hit for file: {FilePath}", fullPath);
            return cachedChecksum;
        }

        _logger.LogInformation("Calculating checksum from file path: {FilePath}", fullPath);
        
        try
        {
            using SHA256 sha256 = SHA256.Create();
            await using FileStream stream = File.OpenRead(fullPath);
            byte[] hash = await sha256.ComputeHashAsync(stream);
            string checksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            
            _checksumCache.AddOrUpdate(fullPath, checksum, (_, _) => checksum);

            _logger.LogInformation("Checksum calculated successfully for file: {FilePath}, Checksum: {Checksum}", 
                fullPath, checksum);
            
            return checksum;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating checksum for file: {FilePath}", fullPath);
            return string.Empty;
        }
    }

    public void SetChecksum(string filePath, string checksum)
    {
        string fullPath = Path.GetFullPath(filePath);
        _checksumCache.AddOrUpdate(fullPath, checksum, (_, _) => checksum);
        _logger.LogInformation("Checksum manually set for file: {FilePath}", fullPath);
    }
    
    public bool ChecksumsMatch(string checksum1, string checksum2)
    {
        bool match = checksum1.Equals(checksum2, StringComparison.OrdinalIgnoreCase);
        
        if (!match)
        {
            _logger.LogWarning("Checksum mismatch detected - Checksum1: {Checksum1}, Checksum2: {Checksum2}", 
                checksum1, checksum2);
            
        }
        else
        {
            _logger.LogDebug("Comparing checksums - Match: {Match}, Checksum1: {Checksum1}, Checksum2: {Checksum2}", 
                match, checksum1, checksum2);
        }
        return match;
    }
}