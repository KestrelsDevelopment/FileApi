using KestrelsDev.FileApi.Services.ConfigurationService;
using Microsoft.Extensions.Primitives;

namespace KestrelsDev.FileApi.Middleware;

public class AuthenticationMiddleware(
    RequestDelegate next,
    IConfigurationService configService,
    ILogger<AuthenticationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out StringValues authorizationHeader))
        {
            logger.LogWarning("Authentication failed: Missing Authorization header");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Missing Authorization header");
            return;
        }

        string headerValue = authorizationHeader.ToString();
        if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Authentication failed: Invalid authentication scheme");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid authentication scheme");
            return;
        }

        string extractedApiKey = headerValue["Bearer ".Length..].Trim();

        if (configService.UploadPsk != extractedApiKey)
        {
            logger.LogWarning("Authentication failed: Invalid API key");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid API key");
            return;
        }

        logger.LogDebug("Authentication successful");
        await next(context);
    }
}