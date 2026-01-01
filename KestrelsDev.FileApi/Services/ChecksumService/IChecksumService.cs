using KestrelsDev.FileApi.Models;

namespace KestrelsDev.FileApi.Services.ChecksumService;

/// <summary>
/// Provides checksum calculation, validation, and cache management services.
/// </summary>
public interface IChecksumService
{
    Task<string> CalculateChecksumAsync(IFormFile file);
    
    Task<string> CalculateChecksumFromFileAsync(string filePath);
    
    /// <summary>
    /// Adds or updates a file in the cache.
    /// </summary>
    void AddOrUpdateFile(FileInfoDto fileInfo);
    
    /// <summary>
    /// Removes a file from the cache by its name.
    /// </summary>
    void RemoveFile(string fileName);
    
    /// <summary>
    /// Retrieves all files currently in the cache.
    /// </summary>
    IEnumerable<FileInfoDto> GetCachedFiles();
    
    bool ChecksumsMatch(string checksum1, string checksum2);
}