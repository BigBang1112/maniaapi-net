using System.Text.Json.Nodes;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Engine;

namespace ManiaAPI.MCP.Tests;

public class ShapingTests
{
    [Test]
    public async Task ProjectsNestedArrayFieldsAndReportsTruncation()
    {
        var raw = JsonNode.Parse("""
            {"top":{"top":[{"accountId":"one","score":1,"secret":"x"},{"accountId":"two","score":2}]},"other":true}
            """);

        var result = ResultShaper.Shape(raw,
            new ResultShape(["top.top[].accountId", "top.top[].score"], MaxItems: 1), new ShapingOptions());

        await Assert.That(result["data"]!["top"]!["top"]!.AsArray().Count).IsEqualTo(1);
        await Assert.That(result["data"]!["top"]!["top"]![0]!["accountId"]!.GetValue<string>()).IsEqualTo("one");
        await Assert.That(result.ToJsonString()).DoesNotContain("secret");
        await Assert.That(result["truncation"]!.AsArray().Count).IsEqualTo(1);
        await Assert.That(raw!["top"]!["top"]!.AsArray().Count).IsEqualTo(2);
    }

    [Test]
    public async Task CharacterLimitKeepsValidJson()
    {
        var raw = new JsonObject { ["value"] = new string('x', 2000) };
        var result = ResultShaper.Shape(raw, new ResultShape(MaxChars: 256), new ShapingOptions());

        await Assert.That(result.ToJsonString().Length).IsLessThanOrEqualTo(256);
        await Assert.That(result["data"]).IsNull();
        await Assert.That(result["truncation"]!.AsArray().Count).IsEqualTo(1);
    }

    [Test]
    public async Task ServerCharacterLimitCannotBeBypassed()
    {
        await Assert.That(() => ResultShaper.Shape(null,
            new ResultShape(MaxChars: 1000001), new ShapingOptions())).Throws<ArgumentException>();
    }
}
