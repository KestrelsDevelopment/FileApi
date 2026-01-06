namespace KestrelsDev.FileApi.Services.FileStorageService;

/// <summary>
/// Provides file storage operations including saving, retrieving, and managing uploaded files.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves an uploaded file to the configured upload directory.
    /// </summary>
    /// <param name="file">The file to save.</param>
    /// <param name="fileName">The name to use when saving the file.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a tuple indicating success status and an optional error message.</returns>
    Task<(bool Success, string? ErrorMessage)> SaveFileAsync(IFormFile file, string fileName);
    
    /// <summary>
    /// Checks if a file with the specified name exists in the upload directory.
    /// </summary>
    /// <param name="fileName">The name of the file to check.</param>
    /// <returns><c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
    bool FileExists(string fileName);
    
    /// <summary>
    /// Retrieves a file from the upload directory. If no filename is specified, it returns the most recently created file.
    /// </summary>
    /// <param name="fileName">The optional name of the file to retrieve. If <c>null</c> or empty, returns the newest file.</param>
    /// <returns>A <see cref="FileInfo"/> object representing the file, or <c>null</c> if not found or the directory doesn't exist.</returns>
    FileInfo? GetFile(string? fileName = null);
    
    /// <summary>
    /// Retrieves all files from the upload directory, ordered by creation time (newest first).
    /// </summary>
    /// <returns>An enumerable collection of <see cref="FileInfo"/> objects representing all files in the upload directory.</returns>
    IEnumerable<FileInfo> GetAllFiles();
    
    /// <summary>
    /// Removes old files from the upload directory to maintain the maximum file count limit.
    /// </summary>
    /// <returns>A task that represents the asynchronous cleanup operation.</returns>
    Task CleanupOldFilesAsync();
}
