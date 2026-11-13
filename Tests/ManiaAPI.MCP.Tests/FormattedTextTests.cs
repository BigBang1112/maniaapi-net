using System.Text.Json.Nodes;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Engine;

namespace ManiaAPI.MCP.Tests;

public class FormattedTextTests
{
    [Test]
    [Arguments("$h[bigbang1112]$i$n$o$bbbBIGBANG1112", "BIGBANG1112")]
    [Arguments("$i$555B$fffig$555B$fffang$5551$fff112", "BigBang1112")]
    [Arguments("$h[bigbang1112]$fff$o$n$t$iBigBang1112$h  $z$40F", "BigBang1112  ")]
    public async Task MasterServerNicknamesGainPlainSiblings(string nickname, string expected)
    {
        // Shape of masterserver.*.player_infos.
        var raw = new JsonObject
        {
            ["result"] = new JsonObject
            {
                ["login"] = "bigbang1112",
                ["nickname"] = nickname,
                ["zone"] = "World|Czech republic|Jihoceský kraj"
            }
        };

        FormattedText.AddPlain(raw);

        await Assert.That(raw["result"]!["nickname"]!.GetValue<string>()).IsEqualTo(nickname);
        await Assert.That(raw["result"]!["nicknamePlain"]!.GetValue<string>()).IsEqualTo(expected);
        await Assert.That(raw["result"]!.AsObject().ContainsKey("loginPlain")).IsFalse();
    }

    [Test]
    public async Task OnlyFormattedNameFieldsGainPlainSiblings()
    {
        // Shape of masterserver.tmuf.ladder_player_rankings, plus fields that must stay untouched.
        var raw = new JsonObject
        {
            ["players"] = new JsonArray(
                new JsonObject { ["rank"] = 1, ["nickname"] = "$f00Red", ["leagueLogoUrl"] = "http://x/$f00.png" },
                new JsonObject { ["rank"] = 2, ["nickname"] = "Plain" }),
            ["description"] = "$o not a name field"
        };

        FormattedText.AddPlain(raw);

        var players = raw["players"]!.AsArray();

        await Assert.That(players[0]!["nicknamePlain"]!.GetValue<string>()).IsEqualTo("Red");
        await Assert.That(players[0]!.AsObject().ContainsKey("leagueLogoUrlPlain")).IsFalse();
        await Assert.That(players[1]!.AsObject().ContainsKey("nicknamePlain")).IsFalse();
        await Assert.That(raw.ContainsKey("descriptionPlain")).IsFalse();
    }

    [Test]
    public async Task SelectingAFormattedFieldKeepsItsPlainSibling()
    {
        var raw = new JsonObject
        {
            ["result"] = new JsonObject { ["login"] = "bigbang1112", ["nickname"] = "$o$f00Big", ["zone"] = "World" }
        };

        FormattedText.AddPlain(raw);

        var result = ResultShaper.Shape(raw, new ResultShape(["result.nickname"], null, null), new ShapingOptions());
        var data = result["data"]!["result"]!.AsObject();

        await Assert.That(data["nickname"]!.GetValue<string>()).IsEqualTo("$o$f00Big");
        await Assert.That(data["nicknamePlain"]!.GetValue<string>()).IsEqualTo("Big");
        await Assert.That(data.ContainsKey("login")).IsFalse();
    }
}
