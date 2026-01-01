namespace KestrelsDev.FileApi.Models;

public record FileInfoDto(
    string FileName,
    double SizeMb,
    double SizeB,
    string Checksum,
    DateTime CreatedAt
);