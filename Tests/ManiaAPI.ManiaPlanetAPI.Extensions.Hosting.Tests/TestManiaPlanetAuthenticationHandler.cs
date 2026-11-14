using System.Security.Claims;
using System.Text.Encodings.Web;
using ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Tests;

/// <summary>
/// Exposes <see cref="ManiaPlanetAuthenticationHandler"/>'s ticket creation, the step after the code is exchanged for tokens.
/// </summary>
internal sealed class TestManiaPlanetAuthenticationHandler(IOptionsMonitor<ManiaPlanetAuthenticationOptions> options)
    : ManiaPlanetAuthenticationHandler(options, NullLoggerFactory.Instance, UrlEncoder.Default)
{
    public static async Task<TestManiaPlanetAuthenticationHandler> CreateAsync(ManiaPlanetAuthenticationOptions options)
    {
        var handler = new TestManiaPlanetAuthenticationHandler(new StaticOptionsMonitor<ManiaPlanetAuthenticationOptions>(options));
        var scheme = new AuthenticationScheme(ManiaPlanetAuthenticationDefaults.AuthenticationScheme, null, typeof(TestManiaPlanetAuthenticationHandler));
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
