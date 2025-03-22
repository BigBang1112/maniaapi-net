using System.Text.Json.Serialization;

namespace ManiaAPI.ManiaPlanetAPI;

public sealed record Title(string Uid,
                           int Cost,
                           string Name,
                           string Description,
                           string Punchline,
                           [property: JsonPropertyName("card_url")] string CardUrl,
                           [property: JsonPropertyName("download_url")] string DownloadUrl,
                           [property: JsonPropertyName("background_url")] string BackgroundUrl,
                           [property: JsonPropertyName("logo_url")] string LogoUrl,
                           [property: JsonPropertyName("title_maker_uid")] string TitleMakerUid,
                           bool Public);