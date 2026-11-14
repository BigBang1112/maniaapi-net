using Microsoft.Extensions.Configuration;

namespace ManiaAPI.Testing;

/// <summary>
/// Settings for live tests, each under one key like <c>NadeoAPI:ServiceAccount:Login</c>.
/// A key is read from the user secrets shared by every test project, and an environment variable overrides it:
/// <c>MANIAAPI_</c>, then the key with <c>:</c> replaced by <c>__</c>, like <c>MANIAAPI_NADEOAPI__SERVICEACCOUNT__LOGIN</c>.
/// </summary>
internal static class LiveSettings
{
    public const string EnvironmentVariablePrefix = "MANIAAPI_";
    public const string EnabledKey = "LiveTests:Enabled";

    private static readonly IConfiguration configuration = new ConfigurationBuilder()
        .AddUserSecrets(typeof(LiveSettings).Assembly, optional: true)
        .AddEnvironmentVariables(EnvironmentVariablePrefix)
        .Build();

    /// <summary>
    /// Whether live tests run, from <c>LiveTests:Enabled</c> set to <c>true</c> or <c>1</c>.
    /// </summary>
    public static bool Enabled => configuration[EnabledKey] is { } value
        && (value == "1" || (bool.TryParse(value, out var enabled) && enabled));

    public static class NadeoAPI
    {
        /// <summary>
        /// A dedicated server account. It can read only its own records, see <c>NadeoServices</c>.
        /// </summary>
        public static class DedicatedServer
        {
            public static string Login => Require("NadeoAPI:DedicatedServer:Login");
            public static string Password => Require("NadeoAPI:DedicatedServer:Password");
        }

        /// <summary>
        /// A service account, from https://www.trackmania.com/player/service-account.
        /// </summary>
        public static class ServiceAccount
        {
            public static string Login => Require("NadeoAPI:ServiceAccount:Login");
            public static string Password => Require("NadeoAPI:ServiceAccount:Password");
        }
    }

    public static class Trackmania
    {
        public static string ClientId => Require("Trackmania:ClientId");
        public static string ClientSecret => Require("Trackmania:ClientSecret");
    }

    public static class ManiaPlanet
    {
        public static string ClientId => Require("ManiaPlanet:ClientId");
        public static string ClientSecret => Require("ManiaPlanet:ClientSecret");
    }

    /// <summary>
    /// An API user of the old TMF web services, like <c>tmf_yourapp</c>.
    /// </summary>
    public static class TrackmaniaWS
    {
        public static string Username => Require("TrackmaniaWS:Username");
        public static string Password => Require("TrackmaniaWS:Password");
    }

    /// <summary>
    /// The environment variable that sets <paramref name="key"/>, like <c>MANIAAPI_NADEOAPI__SERVICEACCOUNT__LOGIN</c> for <c>NadeoAPI:ServiceAccount:Login</c>.
    /// </summary>
    public static string ToEnvironmentVariable(string key)
    {
        return EnvironmentVariablePrefix + key.Replace(":", "__").ToUpperInvariant();
    }

    /// <summary>
    /// Returns the setting, or skips the current test when it isn't set.
    /// </summary>
    private static string Require(string key)
    {
        var value = configuration[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            Skip.Test($"'{key}' is not set. Set it as a user secret, or as the {ToEnvironmentVariable(key)} environment variable.");
        }

        return value!;
    }
}
