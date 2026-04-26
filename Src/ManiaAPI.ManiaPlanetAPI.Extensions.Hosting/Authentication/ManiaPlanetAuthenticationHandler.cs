using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Authentication;

internal class ManiaPlanetAuthenticationHandler(
    IOptionsMonitor<ManiaPlanetAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
    ) : OAuthHandler<ManiaPlanetAuthenticationOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity,
                                                                          AuthenticationProperties properties,
                                                                          OAuthTokenResponse tokens)
    {
        using var payload = JsonDocument.Parse(await RequestAsync(Options.UserInformationEndpoint, tokens));

        var principal = new ClaimsPrincipal(identity);
        var context = new OAuthCreatingTicketContext(
            principal,
            properties,
            Context,
            Scheme,
            Options,
            Backchannel,
            tokens,
            payload.RootElement);

        context.RunClaimActions();

        if (Options.Scope.Contains("email"))
        {
            var email = await RequestAsync(ManiaPlanetAuthenticationDefaults.EmailEndpoint, tokens);

            if (!string.IsNullOrEmpty(email))
            {
                identity.AddClaim(new(ClaimTypes.Email, email, ClaimValueTypes.String, Options.ClaimsIssuer ?? Scheme.Name));
            }
        }

        await Events.CreatingTicket(context);

        return await base.CreateTicketAsync(identity, properties, tokens);
    }

    private async Task<string> RequestAsync(string requestUri, OAuthTokenResponse tokens)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue(tokens.TokenType ?? "Bearer", tokens.AccessToken);

        using var response = await Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, Context.RequestAborted);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(Context.RequestAborted);
    }
}
