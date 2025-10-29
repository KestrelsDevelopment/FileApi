namespace KestrelsDev.FileApi.Services.AuthenticationService;

/// <summary>
/// Provides authentication services for API key validation.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Validates the provided API key against the configured pre-shared key.
    /// </summary>
    /// <param name="providedKey">The API key to validate.</param>
    /// <returns><c>true</c> if the provided key matches the configured key; otherwise, <c>false</c>.</returns>
    bool ValidateApiKey(string? providedKey);
}
