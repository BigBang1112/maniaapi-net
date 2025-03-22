using ManiaAPI.NadeoAPI;
using ManiaAPI.NadeoAPI.Extensions.Gbx;
using Spectre.Console;

using var ns = new NadeoServices();

var login = AnsiConsole.Ask<string>("Enter Ubisoft Connect [green]login[/]:");

var password = AnsiConsole.Prompt(
    new TextPrompt<string>("Enter Ubisoft Connect [green]password[/]:")
        .PromptStyle("red")
        .Secret());

await ns.AuthorizeAsync(login, password, AuthorizationMethod.UbisoftAccount);

AnsiConsole.MarkupLine("[green]Authorization successful[/]");

var mapInfo = await ns.UpdateMapAsync(Guid.Parse(args[0]), args[1]);

AnsiConsole.WriteLine(mapInfo.ToString());