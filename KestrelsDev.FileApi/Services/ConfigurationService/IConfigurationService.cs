namespace KestrelsDev.FileApi.Services.ConfigurationService;

/// <summary>
/// Provides access to application configuration settings from environment variables.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Gets the upload directory path from the API_UPLOAD_PATH environment variable.
    /// </summary>
    /// <value>The configured upload path, or <c>null</c> if not set.</value>
    string? UploadPath { get; }
    
    /// <summary>
    /// Gets the pre-shared key for API authentication from the API_UPLOAD_PSK environment variable.
    /// </summary>
    /// <value>The configured pre-shared key.</value>
    string UploadPsk { get; }
    
    /// <summary>
    /// Gets the maximum number of files to retain from the API_UPLOAD_MAX_FILES environment variable.
    /// </summary>
    /// <value>The maximum number of files, or 5 if not configured.</value>
    int MaxFiles { get; }
}