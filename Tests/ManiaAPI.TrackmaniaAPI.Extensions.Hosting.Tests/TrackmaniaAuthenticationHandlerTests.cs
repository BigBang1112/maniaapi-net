using System.Security.Claims;
using System.Text.Json;
using ManiaAPI.TrackmaniaAPI.Extensions.Hosting.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace ManiaAPI.TrackmaniaAPI.Extensions.Hosting.Tests;

public class TrackmaniaAuthenticationHandlerTests
{
    private const string AccessToken = "access-token";
    private const string AccountId = "6a43df20-cd1a-4b3b-87b9-a6835a9b416d";
    private const string DisplayName = "BigBang1112";
    private const string UserJson = $$"""{"accountId":"{{AccountId}}","displayName":"{{DisplayName}}"}""";

    [Test]
    public async Task CreateTicketAsync_RaisesCreatingTicketOnce()
    {
        var calls = 0;
        var handler = await CreateHandlerAsync(context =>
        {
            calls++;
            return Task.CompletedTask;
        });
        using var tokens = Tokens();

        await handler.CreateTicketFromTokensAsync(tokens);

        await Assert.That(calls).IsEqualTo(1);
    }

    [Test]
    public async Task CreateTicketAsync_ReturnsPrincipalSetFromUserPayload()
    {
        var replacement = new ClaimsPrincipal(new ClaimsIdentity([new Claim("custom", "value")], "custom"));
        var handler = await CreateHandlerAsync(context =>
        {
            // Like an app that builds its own principal from the user information
            if (context.User.TryGetProperty("displayName", out _))
            {
                context.Principal = replacement;
            }

            return Task.CompletedTask;
        });
        using var tokens = Tokens();

        var ticket = await handler.CreateTicketFromTokensAsync(tokens);

        await Assert.That(ticket.Principal).IsSameReferenceAs(replacement);
    }

    [Test]
    public async Task CreateTicketAsync_MapsUserClaims()
    {
        var handler = await CreateHandlerAsync();
        using var tokens = Tokens();

        var ticket = await handler.CreateTicketFromTokensAsync(tokens);

        await Assert.That(ticket.Principal.FindFirstValue(ClaimTypes.NameIdentifier)).IsEqualTo(AccountId);
        await Assert.That(ticket.Principal.FindFirstValue(ClaimTypes.Name)).IsEqualTo(DisplayName);
    }

    [Test]
    public async Task CreateTicketAsync_SendsAccessTokenToUserEndpoint()
    {
        var backchannel = FakeHttpMessageHandler.Json(UserJson);
        var handler = await CreateHandlerAsync(backchannel: backchannel);
        using var tokens = Tokens();

        await handler.CreateTicketFromTokensAsync(tokens);

        await Assert.That(backchannel.Request.Url).IsEqualTo(TrackmaniaAuthenticationDefaults.UserInformationEndpoint);
        await Assert.That(backchannel.Request.Authorization?.Scheme).IsEqualTo("Bearer");
        await Assert.That(backchannel.Request.Authorization?.Parameter).IsEqualTo(AccessToken);
    }

    private static async Task<TestTrackmaniaAuthenticationHandler> CreateHandlerAsync(
        Func<OAuthCreatingTicketContext, Task>? onCreatingTicket = null,
        FakeHttpMessageHandler? backchannel = null)
    {
        var options = new TrackmaniaAuthenticationOptions
        {
            Backchannel = (backchannel ?? FakeHttpMessageHandler.Json(UserJson)).CreateClient()
        };

        if (onCreatingTicket is not null)
        {
            options.Events.OnCreatingTicket = onCreatingTicket;
        }

        return await TestTrackmaniaAuthenticationHandler.CreateAsync(options);
    }

    private static OAuthTokenResponse Tokens()
    {
        return OAuthTokenResponse.Success(JsonDocument.Parse($$"""{"access_token":"{{AccessToken}}","token_type":"Bearer"}"""));
    }
}
