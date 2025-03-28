﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ManiaAPI.TrackmaniaAPI.Extensions.Hosting.Authentication;

internal class TrackmaniaAuthenticationHandler(
    IOptionsMonitor<TrackmaniaAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
    ) : OAuthHandler<TrackmaniaAuthenticationOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity,
                                                                          AuthenticationProperties properties,
                                                                          OAuthTokenResponse tokens)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, Options.UserInformationEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue(tokens.TokenType ?? "Bearer", tokens.AccessToken);

        using var response = await Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, Context.RequestAborted);

        response.EnsureSuccessStatusCode();

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Context.RequestAborted));

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

        await Events.CreatingTicket(context);

        return await base.CreateTicketAsync(identity, properties, tokens);
    }
}
