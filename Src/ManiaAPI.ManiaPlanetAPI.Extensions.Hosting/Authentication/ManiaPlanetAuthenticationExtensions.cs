using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Authentication;

public static class ManiaPlanetAuthenticationExtensions
{
    public static AuthenticationBuilder AddManiaPlanet(this AuthenticationBuilder builder)
    {
        return builder.AddManiaPlanet(ManiaPlanetAuthenticationDefaults.AuthenticationScheme, options => { });
    }

    public static AuthenticationBuilder AddManiaPlanet(this AuthenticationBuilder builder, Action<ManiaPlanetAuthenticationOptions> configuration)
    {
        return builder.AddManiaPlanet(ManiaPlanetAuthenticationDefaults.AuthenticationScheme, configuration);
    }

    public static AuthenticationBuilder AddManiaPlanet(this AuthenticationBuilder builder, string scheme, Action<ManiaPlanetAuthenticationOptions> configuration)
    {
        return builder.AddManiaPlanet(scheme, ManiaPlanetAuthenticationDefaults.DisplayName, configuration);
    }

    public static AuthenticationBuilder AddManiaPlanet(this AuthenticationBuilder builder, string scheme, string displayName, Action<ManiaPlanetAuthenticationOptions> configuration)
    {
        return builder.AddOAuth<ManiaPlanetAuthenticationOptions, ManiaPlanetAuthenticationHandler>(scheme, displayName, configuration);
    }
}
