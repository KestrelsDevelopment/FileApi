namespace KestrelsDev.FileApi.Models;

public record FileInfoDto(
    string FileName,
    double SizeMB,
    double sizeB,
    DateTime CreatedAt
);