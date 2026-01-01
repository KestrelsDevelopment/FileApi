using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using KestrelsDev.FileApi.Services.ConfigurationService;
using KestrelsDev.FileApi.Services.ChecksumService;
using KestrelsDev.FileApi.Services.FileStorageService;
using KestrelsDev.FileApi.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

builder.Services.AddScoped<IChecksumService, ChecksumService>();

builder.Services.AddScoped<IFileStorageService, FileStorageService>();

WebApplication app = builder.Build();

app.UseHttpsRedirection();

app.UseWhen(context => context.Request.Path.StartsWithSegments("/upload"), appBuilder =>
{
    appBuilder.UseMiddleware<AuthenticationMiddleware>();
});

app.MapControllers();

app.Run();
