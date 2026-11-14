using System.Security.Claims;
using System.Text.Encodings.Web;
using ManiaAPI.TrackmaniaAPI.Extensions.Hosting.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ManiaAPI.TrackmaniaAPI.Extensions.Hosting.Tests;

/// <summary>
/// Exposes <see cref="TrackmaniaAuthenticationHandler"/>'s ticket creation, the step after the code is exchanged for tokens.
/// </summary>
internal sealed class TestTrackmaniaAuthenticationHandler(IOptionsMonitor<TrackmaniaAuthenticationOptions> options)
    : TrackmaniaAuthenticationHandler(options, NullLoggerFactory.Instance, UrlEncoder.Default)
{
    public static async Task<TestTrackmaniaAuthenticationHandler> CreateAsync(TrackmaniaAuthenticationOptions options)
    {
        var handler = new TestTrackmaniaAuthenticationHandler(new StaticOptionsMonitor<TrackmaniaAuthenticationOptions>(options));
        var scheme = new AuthenticationScheme(TrackmaniaAuthenticationDefaults.AuthenticationScheme, null, typeof(TestTrackmaniaAuthenticationHandler));
        await handler.InitializeAsync(scheme, new DefaultHttpContext());
        return handler;
    }

    public Task<AuthenticationTicket> CreateTicketFromTokensAsync(OAuthTokenResponse tokens)
    {
        return CreateTicketAsync(new ClaimsIdentity(Options.ClaimsIssuer), new AuthenticationProperties(), tokens);
    }
}

internal sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
{
    public T CurrentValue => value;
    public T Get(string? name) => value;
    public IDisposable? OnChange(Action<T, string?> listener) => null;
}
