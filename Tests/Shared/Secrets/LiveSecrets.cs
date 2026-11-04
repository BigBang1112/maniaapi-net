using Microsoft.Extensions.Configuration;

namespace ManiaAPI.Testing;

/// <summary>
/// Reads credentials for live tests from an environment variable (CI), or from the project's user secrets (local development).
/// A test that asks for a missing credential is skipped instead of failing.
/// </summary>
internal static class LiveSecrets
{
    private static readonly IConfiguration userSecrets = new ConfigurationBuilder()
        .AddUserSecrets(typeof(LiveSecrets).Assembly, optional: true)
        .Build();

    /// <param name="userSecretKey">Key in the project's user secrets, like <c>ClientId</c>.</param>
    /// <param name="environmentVariable">Environment variable that takes priority, like <c>TRACKMANIA_API_CLIENTID</c>.</param>
    public static string Require(string userSecretKey, string environmentVariable)
    {
        var value = Environment.GetEnvironmentVariable(environmentVariable);

        if (string.IsNullOrWhiteSpace(value))
        {
            value = userSecrets[userSecretKey];
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            Skip.Test($"Credential missing. Set the {environmentVariable} environment variable or the '{userSecretKey}' user secret.");
        }

        return value!;
    }
}
