namespace KestrelsDev.FileApi.Services.FileStorageService;

public interface IFileStorageService
{
    Task<(bool Success, string? ErrorMessage)> SaveFileAsync(IFormFile file, string fileName);
    bool FileExists(string fileName);
    FileInfo? GetFile(string? fileName = null);
    IEnumerable<FileInfo> GetAllFiles();
    Task CleanupOldFilesAsync();
}
