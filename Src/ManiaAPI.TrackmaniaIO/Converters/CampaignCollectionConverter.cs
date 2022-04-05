using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO.Converters;

public class CampaignCollectionConverter : JsonConverter<CampaignCollection>
{
    public override CampaignCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(CampaignCollection));

        var tempCampaignCollection = JsonSerializer.Deserialize<Temporary.CampaignCollection>(ref reader, TrackmaniaIO.JsonSerializerOptions);

        if (tempCampaignCollection is null)
        {
            return null;
        }

        return new CampaignCollection(tempCampaignCollection.Campaigns, tempCampaignCollection.Page);
    }

    public override void Write(Utf8JsonWriter writer, CampaignCollection? value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
