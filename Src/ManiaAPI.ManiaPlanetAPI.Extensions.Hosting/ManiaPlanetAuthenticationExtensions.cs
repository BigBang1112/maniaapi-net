using ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.ManiaPlanetAPI.Extensions.Hosting;

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
        builder.AddOAuth<ManiaPlanetAuthenticationOptions, ManiaPlanetAuthenticationHandler>(scheme, displayName, configuration);
        builder.Services.PostConfigure<ManiaPlanetAuthenticationOptions>(scheme, options =>
        {
            if (!string.IsNullOrWhiteSpace(options.UserAgent) && options.Backchannel is { } backchannel)
            {
                backchannel.DefaultRequestHeaders.UserAgent.Clear();
                backchannel.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
            }
        });
        return builder;
    }
}
