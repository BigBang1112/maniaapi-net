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
    public async Task CharacterLimitCutsLongTextAndNotesIt()
    {
        var raw = new JsonObject { ["id"] = 7, ["value"] = new string('x', 2000) };
        var result = ResultShaper.Shape(raw, new ResultShape(MaxChars: 256), new ShapingOptions());
        var value = result["data"]!["value"]!.GetValue<string>();

        await Assert.That(result.ToJsonString().Length).IsLessThanOrEqualTo(256);
        await Assert.That(result["data"]!["id"]!.GetValue<int>()).IsEqualTo(7);
        await Assert.That(value).StartsWith("xxxx");
        await Assert.That(value).EndsWith("…");
        await Assert.That(Notes(result).Single()).StartsWith($"$.value: text cut from 2000 to {value.Length - 1} characters");
    }

    [Test]
    public async Task CharacterLimitCutsTextAtTheRoot()
    {
        var result = ResultShaper.Shape(JsonValue.Create(new string('z', 5000)), new ResultShape(MaxChars: 256), new ShapingOptions());

        await Assert.That(result.ToJsonString().Length).IsLessThanOrEqualTo(256);
        await Assert.That(result["data"]!.GetValue<string>()).StartsWith("zzzz");
        await Assert.That(Notes(result).Single()).StartsWith("$: text cut from 5000");
    }

    [Test]
    public async Task CharacterLimitKeepsLeadingItemsAndCountsEveryDroppedItem()
    {
        var raw = new JsonObject
        {
            ["records"] = new JsonArray(Enumerable.Range(0, 60).Select(i => (JsonNode?)new JsonObject
            {
                ["position"] = i,
                ["accountId"] = $"00000000-0000-0000-0000-{i:000000000000}"
            }).ToArray())
        };

        var result = ResultShaper.Shape(raw, new ResultShape(MaxChars: 2000), new ShapingOptions());
        var records = result["data"]!["records"]!.AsArray();
        var kept = records.Count;

        await Assert.That(result.ToJsonString().Length).IsLessThanOrEqualTo(2000);
        await Assert.That(kept).IsGreaterThan(1).And.IsLessThan(50);
        await Assert.That(records.Select(x => x!["position"]!.GetValue<int>())).IsEquivalentTo(Enumerable.Range(0, kept));
        await Assert.That(Notes(result).Single())
            .IsEqualTo($"$.records: {60 - kept} more items ({50 - kept} cut to fit maxChars); use the operation's pagination parameters, fewer fields, or a smaller page to continue.");
    }

    [Test]
    public async Task NotesInsideCutItemsAreDropped()
    {
        var raw = new JsonArray(Enumerable.Range(0, 3).Select(_ => (JsonNode?)new JsonObject
        {
            ["inner"] = new JsonArray(Enumerable.Range(0, 60).Select(i => (JsonNode?)JsonValue.Create(i)).ToArray())
        }).ToArray());

        var result = ResultShaper.Shape(raw, new ResultShape(MaxChars: 400), new ShapingOptions());
        var notes = Notes(result);

        await Assert.That(result.ToJsonString().Length).IsLessThanOrEqualTo(400);
        await Assert.That(result["data"]!.AsArray().Count).IsEqualTo(1);
        await Assert.That(notes.Any(x => x.StartsWith("$[0].inner:", StringComparison.Ordinal))).IsTrue();
        await Assert.That(notes.Any(x => x.StartsWith("$[1]", StringComparison.Ordinal) || x.StartsWith("$[2]", StringComparison.Ordinal))).IsFalse();
    }

    [Test]
    public async Task ResultWithNothingLargeToCutIsDroppedWithANote()
    {
        var raw = new JsonObject(Enumerable.Range(0, 200).Select(i => new KeyValuePair<string, JsonNode?>("field" + i, i)));
        var result = ResultShaper.Shape(raw, new ResultShape(MaxChars: 256), new ShapingOptions());

        await Assert.That(result.ToJsonString().Length).IsLessThanOrEqualTo(256);
        await Assert.That(result["data"]).IsNull();
        await Assert.That(Notes(result).Single()).Contains("could not be cut down");
    }

    [Test]
    public async Task ExistingNotesAreKept()
    {
        var result = ResultShaper.Shape(new JsonObject { ["zone"] = 1 }, null, new ShapingOptions(), ["trophies unavailable: denied"]);

        await Assert.That(Notes(result)).IsEquivalentTo(["trophies unavailable: denied"]);
    }

    private static string[] Notes(JsonObject result)
    {
        return result["truncation"]!.AsArray().Select(x => x!.GetValue<string>()).ToArray();
    }

    [Test]
    public async Task ServerCharacterLimitCannotBeBypassed()
    {
        await Assert.That(() => ResultShaper.Shape(null,
            new ResultShape(MaxChars: 1000001), new ShapingOptions())).Throws<ArgumentException>();
    }
}
