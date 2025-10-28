namespace KestrelsDev.FileApi.Services.ConfigurationService;

public class ConfigurationService(ILogger<ConfigurationService> logger) : IConfigurationService
{
    public string? UploadPath { get; } = ReadUploadPath(logger);

    public string? UploadPsk { get; } = ReadUploadPsk(logger);

    public int MaxFiles { get; } = ReadMaxFiles(logger);

    private static string? ReadUploadPath(ILogger<ConfigurationService> logger)
    {
        string? uploadPath = Environment.GetEnvironmentVariable("API_UPLOAD_PATH");
        if (string.IsNullOrWhiteSpace(uploadPath))
        {
            logger.LogWarning("API_UPLOAD_PATH environment variable is not set");
        }
        else
        {
            logger.LogInformation("Upload path configured: {UploadPath}", uploadPath);
        }
        return uploadPath;
    }
    
    private static string? ReadUploadPsk(ILogger<ConfigurationService> logger)
    {
        string? uploadPsk = Environment.GetEnvironmentVariable("API_UPLOAD_PSK");
        if (string.IsNullOrWhiteSpace(uploadPsk))
        {
            logger.LogWarning("API_UPLOAD_PSK environment variable is not set - authentication will fail");
        }
        else
        {
            logger.LogInformation("API_UPLOAD_PSK configured");
        }
        return uploadPsk;
    }
    
    private static int ReadMaxFiles(ILogger<ConfigurationService> logger)
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
