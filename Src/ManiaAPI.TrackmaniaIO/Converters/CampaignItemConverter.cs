using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO.Converters;

public class CampaignItemConverter : JsonConverter<CampaignItem>
{
    public override CampaignItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(CampaignItem));

        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(ref reader);

        if (dict is null)
        {
            return null;
        }

        var id = 0;
        var clubId = 0;
        var name = "";
        var timestamp = default(DateTimeOffset);
        var mapcount = 0;

        foreach (var (propName, element) in dict)
        {
            switch (propName)
            {
                case "id": id = element.GetInt32(); break;
                case "clubid": clubId = element.GetInt32(); break;
                case "name": name = element.GetString(); break;
                case "mapcount": mapcount = element.GetInt32(); break;
                case "timestamp":
                    timestamp = DateTimeOffset.FromUnixTimeSeconds(element.GetInt64());
                    break;
            }
        }

        return clubId == 0
            ? new OfficialCampaignItem(id, name, timestamp, mapcount)
            : new CustomCampaignItem(id, clubId, name, timestamp, mapcount);
    }

    public override void Write(Utf8JsonWriter writer, CampaignItem? value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }
}
