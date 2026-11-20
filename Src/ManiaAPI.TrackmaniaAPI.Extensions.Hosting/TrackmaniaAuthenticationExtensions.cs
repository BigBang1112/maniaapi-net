using ManiaAPI.TrackmaniaAPI.Extensions.Hosting.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.TrackmaniaAPI.Extensions.Hosting;

public static class TrackmaniaAuthenticationExtensions
{
    public static AuthenticationBuilder AddTrackmania(this AuthenticationBuilder builder)
    {
        return builder.AddTrackmania(TrackmaniaAuthenticationDefaults.AuthenticationScheme, options => { });
    }

    public static AuthenticationBuilder AddTrackmania(this AuthenticationBuilder builder, Action<TrackmaniaAuthenticationOptions> configuration)
    {
        return builder.AddTrackmania(TrackmaniaAuthenticationDefaults.AuthenticationScheme, configuration);
    }

    public static AuthenticationBuilder AddTrackmania(this AuthenticationBuilder builder, string scheme, Action<TrackmaniaAuthenticationOptions> configuration)
    {
        return builder.AddTrackmania(scheme, TrackmaniaAuthenticationDefaults.DisplayName, configuration);
    }

    public static AuthenticationBuilder AddTrackmania(this AuthenticationBuilder builder, string scheme, string displayName, Action<TrackmaniaAuthenticationOptions> configuration)
    {
        builder.AddOAuth<TrackmaniaAuthenticationOptions, TrackmaniaAuthenticationHandler>(scheme, displayName, configuration);
        builder.Services.PostConfigure<TrackmaniaAuthenticationOptions>(scheme, options =>
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
