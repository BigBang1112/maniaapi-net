using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using SimplestGitSourceGenerator;

namespace ManiaAPI.MCP.Hosting;

/// <summary>
/// What the server announces about itself on initialization: its identity and the instructions clients give the model.
/// </summary>
public static class ServerMetadata
{
    public const string Instructions =
        "ManiaAPI.MCP gives access to the web services of Trackmania (2020), TrackMania United/Nations Forever, " +
        "ManiaPlanet (Trackmania 2 and ShootMania), Trackmania Turbo, and Mania Exchange.\n" +
        "- Find an operation with maniaapi_search, read its arguments with maniaapi_describe, then run it with maniaapi_invoke. " +
        "Other listed tools run common operations directly.\n" +
        "- Pass the game to maniaapi_search when the user names one. TM2 and Trackmania 2 are ManiaPlanet, " +
        "TMUF and TMNF are TrackMania Forever, and SM is ShootMania.\n" +
        "- UnitedLadder (united.ladder.*) covers only TrackMania Nations Forever (TMNF) and TrackMania United Forever (TMUF).\n" +
        "- Use shape in maniaapi_invoke to select fields and limit large results.\n" +
        "- Writes are refused unless the server operator allows them, and then need confirmation.\n" +
        "- Credentials stay on the server. Don't ask the user for passwords or API keys.";

    public static void Apply(McpServerOptions options)
    {
        options.ServerInfo = new Implementation
        {
            Name = "ManiaAPI.MCP",
            Title = "ManiaAPI.NET",
            Description = "Stateless MCP gateway for ManiaAPI.NET.",
            Version = $"{PackageInfo.Version}+{SimplestGit.CommitHash}",
            WebsiteUrl = "https://github.com/BigBang1112/maniaapi-net",
            Icons =
            [
                new Icon
                {
                    Source = "https://raw.githubusercontent.com/BigBang1112/maniaapi-net/main/logo_icon_outline.png",
                    MimeType = "image/png",
                    Sizes = ["128x128"]
                }
            ]
        };
        options.ServerInstructions = Instructions;
    }
}
