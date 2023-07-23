using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record Media([property: JsonPropertyName("buttonbackground")] string ButtonBackground,
                           [property: JsonPropertyName("buttonforeground")] string ButtonForeground,
                           string Decal,
                           [property: JsonPropertyName("popupbackground")] string PopUpBackground,
                           [property: JsonPropertyName("popup")] string PopUp,
                           [property: JsonPropertyName("livebuttonbackground")] string LiveButtonBackground,
                           [property: JsonPropertyName("livebuttonforeground")] string LiveButtonForeground);
