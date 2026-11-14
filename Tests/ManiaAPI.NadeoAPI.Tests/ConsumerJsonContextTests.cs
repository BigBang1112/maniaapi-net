using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI.Tests;

/// <summary>
/// An app that serializes Nadeo API models with its own source-generated context, which is how AOT apps return them from their own endpoints.
/// The generated code can only use the models' converters when they are public.
/// </summary>
public class ConsumerJsonContextTests
{
    private const string CampaignCollectionJson = """{"campaignList":[],"nextRequestTimestamp":1700000000,"relativeNextRequest":0,"itemCount":0}""";

    [Test]
    public async Task Deserialize_UnixTimestamp_UsesNadeoConverter()
    {
        var collection = JsonSerializer.Deserialize(CampaignCollectionJson, ConsumerJsonContext.Default.CampaignCollection);

        await Assert.That(collection!.NextRequestTimestamp).IsEqualTo(DateTimeOffset.FromUnixTimeSeconds(1_700_000_000));
    }

    [Test]
    public async Task Serialize_UnixTimestamp_UsesNadeoConverter()
    {
        var collection = new CampaignCollection([], DateTimeOffset.FromUnixTimeSeconds(1_700_000_000), 0, 0);

        var json = JsonSerializer.Serialize(collection, ConsumerJsonContext.Default.CampaignCollection);

        await Assert.That(json).IsEqualTo(CampaignCollectionJson);
    }

    [Test]
    public async Task Converters_AreAllPublic()
    {
        var nonPublic = typeof(NadeoServices).Assembly.GetTypes()
            .Where(type => typeof(JsonConverter).IsAssignableFrom(type) && !type.IsPublic)
            .Select(type => type.FullName);

        await Assert.That(nonPublic).IsEmpty();
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(CampaignCollection))]
internal sealed partial class ConsumerJsonContext : JsonSerializerContext;
