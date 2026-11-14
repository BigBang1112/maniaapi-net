using Microsoft.Extensions.Configuration;

namespace ManiaAPI.MCP.Tests;

/// <summary>
/// Configuration for a test server, with the required <c>ManiaAPI:UserAgent</c> set unless the test sets it.
/// </summary>
internal static class TestConfiguration
{
    public const string UserAgent = "ManiaAPI.MCP.Tests/1.0";

    public static IConfiguration Create(IDictionary<string, string?>? settings = null)
    {
        var values = new Dictionary<string, string?> { ["ManiaAPI:UserAgent"] = UserAgent };

        foreach (var (key, value) in settings ?? new Dictionary<string, string?>())
        {
            values[key] = value;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
