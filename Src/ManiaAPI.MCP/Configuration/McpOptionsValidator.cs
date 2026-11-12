using Microsoft.Extensions.Options;

namespace ManiaAPI.MCP.Configuration;

public sealed class McpOptionsValidator : IValidateOptions<McpOptions>
{
    public ValidateOptionsResult Validate(string? name, McpOptions options)
    {
        List<string> errors = [];

        if (options.Toolsets.Any(x => !McpOptions.SupportedToolsets.Contains(x, StringComparer.Ordinal)))
        {
            errors.Add("Toolsets must use the supported game or exchange toolset names.");
        }

        if (options.MasterServer.Games.Any(x => x is not ("TMUF" or "TMNF" or "MP3" or "MP4" or "TMT")))
        {
            errors.Add("MasterServer:Games must contain TMUF, TMNF, MP3, MP4, or TMT.");
        }

        if (options.Featured.Keys.Any(x => !McpOptions.SupportedToolsets.Contains(x, StringComparer.Ordinal)))
        {
            errors.Add("Featured must use supported toolset names.");
        }

        if (options.AccountLinking.Enabled &&
            (string.IsNullOrWhiteSpace(options.AccountLinking.StorePath) ||
             string.IsNullOrWhiteSpace(options.AccountLinking.DataProtectionKeyPath) ||
             !Uri.TryCreate(options.AccountLinking.PublicOrigin, UriKind.Absolute, out var origin) ||
             origin.AbsolutePath != "/" || !string.IsNullOrEmpty(origin.Query) || !string.IsNullOrEmpty(origin.Fragment) ||
             !string.IsNullOrEmpty(origin.UserInfo) ||
             (origin.Scheme != "https" && !(origin.Scheme == "http" && origin.IsLoopback))))
        {
            errors.Add("AccountLinking requires StorePath, DataProtectionKeyPath, and an HTTPS PublicOrigin (HTTP is permitted on loopback).");
        }

        if (options.Exchange.Sites.Any(x => !Enum.GetNames<TMX.TmxSite>().Contains(x)) ||
            options.Exchange.MxSites.Any(x => !Enum.GetNames<TMX.MxSite>().Contains(x)))
        {
            errors.Add("Exchange sites must use the existing TmxSite and MxSite enum names.");
        }

        if (options.Shaping.MaxItems is < 1 or > 10000 || options.Shaping.MaxChars < 256 ||
            options.Shaping.MaxCharsLimit < options.Shaping.MaxChars || options.Shaping.MaxCharsLimit > 1000000 ||
            options.Shaping.MaxResourceBytes is < 1 or > 104857600)
        {
            errors.Add("Shaping limits must be positive: MaxItems <= 10000, 256 <= MaxChars <= MaxCharsLimit <= 1000000.");
        }

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
