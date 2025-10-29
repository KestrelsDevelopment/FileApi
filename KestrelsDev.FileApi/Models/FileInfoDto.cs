namespace KestrelsDev.FileApi.Models;

public record FileInfoDto(
    string FileName,
    double SizeMB,
    DateTime CreatedAt
);