namespace ManiaAPI.MCP.Catalog;

public sealed record GameContext(string Toolset, string? Family, string[] TmxSites, string[] MxSites);
