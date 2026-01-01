using KestrelsDev.FileApi.Services.ConfigurationService;
using KestrelsDev.FileApi.Services.ChecksumService;
using KestrelsDev.FileApi.Services.FileStorageService;
using KestrelsDev.FileApi.Middleware;
using KestrelsDev.FileApi.Services.ChecksumBackgroundService;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

builder.Services.AddSingleton<IChecksumService, ChecksumService>();

builder.Services.AddHostedService<ChecksumBackgroundService>();

builder.Services.AddScoped<IFileStorageService, FileStorageService>();

WebApplication app = builder.Build();

app.UseHttpsRedirection();

app.UseWhen(context => context.Request.Path.StartsWithSegments("/upload"), appBuilder =>
{
    appBuilder.UseMiddleware<AuthenticationMiddleware>();
});

app.MapControllers();

app.Run();
