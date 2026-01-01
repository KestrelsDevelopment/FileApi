namespace KestrelsDev.FileApi.Services.ChecksumService;

/// <summary>
/// Provides checksum calculation and validation services using SHA256 hashing.
/// </summary>
public interface IChecksumService
{
    /// <summary>
    /// Calculates the SHA256 checksum for an uploaded file.
    /// </summary>
    /// <param name="file">The file to calculate the checksum for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the hexadecimal checksum string in lowercase, or an empty string if an error occurs.</returns>
    Task<string> CalculateChecksumAsync(IFormFile file);
    
    /// <summary>
    /// Calculates the SHA256 checksum for a file at the specified path.
    /// </summary>
    /// <param name="filePath">The path to the file to calculate the checksum for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the hexadecimal checksum string in lowercase, or an empty string if an error occurs.</returns>
    Task<string> CalculateChecksumFromFileAsync(string filePath);

    /// <summary>
    /// Manually sets the checksum for a specific file path in the cache.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="checksum">The checksum to store.</param>
    void SetChecksum(string filePath, string checksum);
    
    /// <summary>
    /// Compares two checksums to determine if they match (case-insensitive comparison).
    /// </summary>
    /// <param name="checksum1">The first checksum to compare.</param>
    /// <param name="checksum2">The second checksum to compare.</param>
    /// <returns><c>true</c> if the checksums match; otherwise, <c>false</c>.</returns>
    bool ChecksumsMatch(string checksum1, string checksum2);
}
