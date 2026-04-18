using KestrelsDev.FileApi.Services.ConfigurationService;
using KestrelsDev.FileApi.Services.ChecksumService;
using KestrelsDev.FileApi.Services.FileStorageService;
using KestrelsDev.FileApi.Middleware;
using KestrelsDev.FileApi.Services.ChecksumCacheInitializationService;
using KestrelsDev.KestrelsCore.Web;

WebApplicationBuilder builder = KestrelsCoreApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

builder.Services.AddSingleton<IChecksumService, ChecksumService>();

builder.Services.AddHostedService<ChecksumCacheInitializationService>();

builder.Services.AddScoped<IFileStorageService, FileStorageService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

WebApplication app = builder.Build();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCorsPolicy");
}
else
{
    app.UseCors();
}

app.UseWhen(context => context.Request.Path.StartsWithSegments("/upload"), appBuilder =>
{
    appBuilder.UseMiddleware<AuthenticationMiddleware>();
});

app.MapControllers();

app.Run();
