using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using KestrelsDev.FileApi.Services.ConfigurationService;
using KestrelsDev.FileApi.Services.AuthenticationService;
using KestrelsDev.FileApi.Services.ChecksumService;
using KestrelsDev.FileApi.Services.FileStorageService;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

builder.Services.AddScoped<IChecksumService, ChecksumService>();

builder.Services.AddScoped<IFileStorageService, FileStorageService>();

WebApplication app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
