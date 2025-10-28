namespace KestrelsDev.FileApi.Services.ChecksumService;

public interface IChecksumService
{
    Task<string> CalculateChecksumAsync(IFormFile file);
    Task<string> CalculateChecksumFromFileAsync(string filePath);
    bool ChecksumsMatch(string checksum1, string checksum2);
}
