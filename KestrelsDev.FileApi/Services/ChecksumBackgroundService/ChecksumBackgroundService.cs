using KestrelsDev.FileApi.Models;
using KestrelsDev.FileApi.Services.ChecksumService;
using KestrelsDev.FileApi.Services.ConfigurationService;

namespace KestrelsDev.FileApi.Services.ChecksumBackgroundService;

public class ChecksumBackgroundService(
    IConfigurationService configService,
    IChecksumService checksumService,
    ILogger<ChecksumBackgroundService> logger) : BackgroundService, IChecksumBackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Checksum Background Service starting...");

        string? uploadPath = configService.UploadPath;
        if (string.IsNullOrEmpty(uploadPath) || !Directory.Exists(uploadPath))
        {
            logger.LogWarning("Upload path is invalid or does not exist. Skipping initial cache population.");
            return;
        }

        DirectoryInfo dirInfo = new(uploadPath);
        FileInfo[] files = dirInfo.GetFiles();

        logger.LogInformation("Found {Count} files to process for checksum cache.", files.Length);

        foreach (FileInfo file in files)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                string checksum = await checksumService.CalculateChecksumFromFileAsync(file.FullName);

                FileInfoDto fileInfo = new FileInfoDto(
                    file.Name,
                    Math.Round(file.Length / (1024.0 * 1024.0), 2),
                    file.Length,
                    checksum,
                    file.CreationTime
                );

                checksumService.AddOrUpdateFile(fileInfo);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing file {FileName} during startup.", file.Name);
            }
        }

        logger.LogInformation("Checksum Background Service completed initial cache population.");
    }
}