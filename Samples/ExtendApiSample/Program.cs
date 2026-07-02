using ManiaAPI.NadeoAPI;
using Spectre.Console;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

using var ens = new ExtendedNadeoServices();

var login = AnsiConsole.Ask<string>("Enter service account [green]login[/]:");

var password = AnsiConsole.Prompt(
    new TextPrompt<string>("Enter service account [green]password[/]:")
        .PromptStyle("red")
        .Secret());

await ens.AuthorizeAsync(login, password);

var submittedMaps = await ens.GetSubmittedMapsAsync();

foreach (var map in submittedMaps)
{
    AnsiConsole.WriteLine(map.Name);
}

internal class ExtendedNadeoServices : NadeoServices
{
    public async Task<ImmutableList<MapInfo>> GetSubmittedMapsAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("maps/by-submitter", MyJsonContext.Default.ImmutableListMapInfo, cancellationToken);
    }
}

[JsonSerializable(typeof(ImmutableList<MapInfo>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class MyJsonContext : JsonSerializerContext;