using System.Text.Json.Serialization;

namespace ManiaAPI.ManiaPlanetAPI;

public sealed record Map(string Uid,
                         string Name,
                         [property: JsonPropertyName("author_login")] string AuthorLogin,
                         [property: JsonPropertyName("download_url")] string DownloadUrl,
                         bool Private,
                         [property: JsonPropertyName("thumbnail_url")] string ThumbnailUrl);