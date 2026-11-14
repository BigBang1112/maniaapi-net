using ManiaAPI.MasterServer.MP4;
using ManiaAPI.MCP.Catalog;
using TmEssentials;

namespace ManiaAPI.MCP.Tests;

public class TimeSerializationTests
{
    [Test]
    public async Task MasterServerTimesAreIntegerMilliseconds()
    {
        var item = new LeaderboardItem<TimeInt32>(32, "sevaban", "$F0FKII", TimeInt32.FromMilliseconds(113558), "", "");

        var json = CatalogMetadata.Result(item)!;

        await Assert.That(json["score"]!.GetValue<int>()).IsEqualTo(113558);
    }

    [Test]
    public async Task TimeSchemaIsInteger()
    {
        var schema = CatalogMetadata.Schema<LeaderboardItem<TimeInt32>>();
        var score = schema["properties"]!["score"]!;

        await Assert.That(score["type"]!.GetValue<string>()).IsEqualTo("integer");
        await Assert.That(score["description"]!.GetValue<string>()).Contains("milliseconds");
    }
}
