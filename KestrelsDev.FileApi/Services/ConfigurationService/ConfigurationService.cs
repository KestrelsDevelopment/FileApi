namespace KestrelsDev.FileApi.Services.ConfigurationService;

public class ConfigurationService(ILogger<ConfigurationService> logger) : IConfigurationService
{
    private string? _uploadPath;
    public string? UploadPath => _uploadPath ??= ReadUploadPath();

    private string? _uploadPsk;
    public string? UploadPsk => _uploadPsk ??= ReadUploadPsk();

    private int? _maxFiles;
    public int MaxFiles => _maxFiles ??= ReadMaxFiles();

    private string? ReadUploadPath()
    {
        string? uploadPath = Environment.GetEnvironmentVariable("API_UPLOAD_PATH");
        if (string.IsNullOrWhiteSpace(uploadPath))
        {
            logger.LogCritical("API_UPLOAD_PATH environment variable is not set");
            Environment.Exit(1);
        }
        else
        {
            logger.LogInformation("Upload path configured: {UploadPath}", uploadPath);
        }
        return uploadPath;
    }
    
    private string? ReadUploadPsk()
    {
        string? uploadPsk = Environment.GetEnvironmentVariable("API_UPLOAD_PSK");
        if (string.IsNullOrWhiteSpace(uploadPsk))
        {
            logger.LogCritical("API_UPLOAD_PSK environment variable is not set - authentication will fail");
            Environment.Exit(1);
        }
        else
        {
            logger.LogInformation("API_UPLOAD_PSK configured");
        }
        return uploadPsk;
    }
    
    private int ReadMaxFiles()
    {
        string? maxFilesEnv = Environment.GetEnvironmentVariable("API_UPLOAD_MAX_FILES");
        if (maxFilesEnv is not null && int.TryParse(maxFilesEnv, out int parsedMaxFiles))
        {
            logger.LogInformation("Using configured max files value: {MaxFiles}", parsedMaxFiles);
            return parsedMaxFiles;
        }
        
        logger.LogInformation("API_UPLOAD_MAX_FILES not set or invalid, using default value: 5");
        return 5;
    }
}
