using KestrelsDev.FileApi.Services.ConfigurationService;


namespace KestrelsDev.FileApi.Services.FileStorageService;

public class FileStorageService(
    IConfigurationService configService,
    ILogger<FileStorageService> logger) : IFileStorageService
{
    public async Task<(bool Success, string? ErrorMessage)> SaveFileAsync(IFormFile file, string fileName)
    {
        string? uploadPath = configService.UploadPath;
        if (uploadPath is null)
            return (false, "Upload path not configured");
        
        string filePath = Path.Combine(uploadPath, fileName);
        
        try
        {
            await using FileStream fs = new(filePath, FileMode.Create);
            await file.CopyToAsync(fs);
            return (true, null);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to save file {FileName}", fileName);
            return (false, "File upload failed");
        }
    }
    
    public bool FileExists(string fileName)
    {
        string? uploadPath = configService.UploadPath;
        if (uploadPath is null)
            return false;
        
        string filePath = Path.Combine(uploadPath, fileName);
        return File.Exists(filePath);
    }
    
    public FileInfo? GetFile(string? fileName = null)
    {
        string? uploadPath = configService.UploadPath;
        if (uploadPath is null || !Directory.Exists(uploadPath))
            return null;
        
        DirectoryInfo dirInfo = new(uploadPath);
        FileInfo[] files = dirInfo.GetFiles();
        
        if (files.Length == 0)
            return null;
        
        return string.IsNullOrWhiteSpace(fileName) 
            ? files.OrderByDescending(f => f.CreationTime).FirstOrDefault() 
            : files.FirstOrDefault(f => f.Name.Equals(fileName,
                StringComparison.OrdinalIgnoreCase));
    }
    
    public IEnumerable<FileInfo> GetAllFiles()
    {
        string? uploadPath = configService.UploadPath;
        if (uploadPath is null || !Directory.Exists(uploadPath))
            return [];
        
        DirectoryInfo dirInfo = new(uploadPath);
        return dirInfo.GetFiles().OrderByDescending(f => f.CreationTime);
    }
    
    public Task CleanupOldFilesAsync()
    {
        return Task.Run(() =>
        {
            string? uploadPath = configService.UploadPath;
            if (uploadPath is null || !Directory.Exists(uploadPath))
                return;
            
            int maxFiles = configService.MaxFiles;
            if (maxFiles <= 0)
                return;
            
            DirectoryInfo dirInfo = new(uploadPath);
            List<FileInfo> files = dirInfo.GetFiles()
                .OrderBy(f => f.CreationTime)
                .ToList();

            if (files.Count <= maxFiles) return;
            foreach (FileInfo file in files.Take(files.Count - maxFiles))
            {
                try
                {
                    file.Delete();
                    logger.LogInformation("Deleted old file: {FileName}", file.FullName);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error deleting file {FileName}", file.FullName);
                }
            }
        });
    }
}
