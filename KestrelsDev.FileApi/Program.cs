WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<KestrelsDev.FileApi.Services.ConfigurationService.IConfigurationService, 
    KestrelsDev.FileApi.Services.ConfigurationService.ConfigurationService>();

builder.Services.AddScoped<KestrelsDev.FileApi.Services.AuthenticationService.IAuthenticationService, 
    KestrelsDev.FileApi.Services.AuthenticationService.AuthenticationService>();

builder.Services.AddScoped<KestrelsDev.FileApi.Services.ChecksumService.IChecksumService, 
    KestrelsDev.FileApi.Services.ChecksumService.ChecksumService>();

builder.Services.AddScoped<KestrelsDev.FileApi.Services.FileStorageService.IFileStorageService, 
    KestrelsDev.FileApi.Services.FileStorageService.FileStorageService>();

WebApplication app = builder.Build();

app.UseHttpsRedirection();


app.MapControllers();

app.Run();
