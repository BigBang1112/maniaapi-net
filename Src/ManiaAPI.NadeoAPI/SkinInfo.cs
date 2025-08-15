using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record SkinInfo(Guid CreatorId,
                              string Checksum,
                              [property: JsonPropertyName("filename")] string FileName,
                              string FileOptimizedUrl,
                              string FileUrl,
                              string SkinDisplayName,
                              Guid SkinId,
                              string SkinName,
                              string SkinType,
                              string ThumbnailUrl,
                              DateTimeOffset Timestamp);