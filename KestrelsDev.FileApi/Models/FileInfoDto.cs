namespace KestrelsDev.FileApi.Models;

public record FileInfoDto(
    string FileName,
    double SizeB,
    string Checksum,
    DateTime CreatedAt
)
{
    public double SizeMb { get; } = Math.Round(SizeB / (1024.0 * 1024.0), 2);
}