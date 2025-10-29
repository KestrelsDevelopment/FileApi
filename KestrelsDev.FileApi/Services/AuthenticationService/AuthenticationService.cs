using KestrelsDev.FileApi.Services.ConfigurationService;

namespace KestrelsDev.FileApi.Services.AuthenticationService;

public class AuthenticationService(
    IConfigurationService configService,
    ILogger<AuthenticationService> logger) : IAuthenticationService
{
    public bool ValidateApiKey(string? providedKey)
    {
        string expectedPsk = configService.UploadPsk;
        bool isValid = expectedPsk == providedKey;
        
        if (isValid)
        {
            logger.LogDebug("API key validation successful");
        }
        else
        {
            logger.LogWarning("API key validation failed");
        }
        
        return isValid;
    }
}
