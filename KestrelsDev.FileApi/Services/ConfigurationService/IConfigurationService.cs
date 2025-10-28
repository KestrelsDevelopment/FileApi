namespace KestrelsDev.FileApi.Services.ConfigurationService;

public interface IConfigurationService
{
    string? UploadPath { get; }
    string? UploadPsk { get; }
    int MaxFiles { get; }
}