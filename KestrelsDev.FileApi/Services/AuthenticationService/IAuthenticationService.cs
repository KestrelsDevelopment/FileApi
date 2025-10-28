namespace KestrelsDev.FileApi.Services.AuthenticationService;

public interface IAuthenticationService
{
    bool ValidateApiKey(string? providedKey);
}
