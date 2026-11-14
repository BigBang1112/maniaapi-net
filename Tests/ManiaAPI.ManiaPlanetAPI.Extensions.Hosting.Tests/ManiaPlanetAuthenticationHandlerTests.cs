using System.Security.Claims;
using System.Text.Json;
using ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Tests;

public class ManiaPlanetAuthenticationHandlerTests
{
    private const string AccessToken = "access-token";
    private const string Login = "bigbang1112";
    private const string Nickname = "BigBang1112";
    private const string Zone = "World|Europe|Czech Republic";
    private const string Email = "player@example.com";
    private const string UserJson = $$"""{"login":"{{Login}}","nickname":"{{Nickname}}","path":"{{Zone}}"}""";

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
            if (context.User.TryGetProperty("login", out _))
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

        await Assert.That(ticket.Principal.FindFirstValue(ClaimTypes.NameIdentifier)).IsEqualTo(Login);
        await Assert.That(ticket.Principal.FindFirstValue(ClaimTypes.Name)).IsEqualTo(Nickname);
        await Assert.That(ticket.Principal.FindFirstValue(ManiaPlanetAuthenticationConstants.Claims.Zone)).IsEqualTo(Zone);
    }

    [Test]
    public async Task CreateTicketAsync_WithEmailScope_AddsEmailClaim()
    {
        var handler = await CreateHandlerAsync(scopes: ["basic", "email"]);
        using var tokens = Tokens();

        var ticket = await handler.CreateTicketFromTokensAsync(tokens);

        await Assert.That(ticket.Principal.FindFirstValue(ClaimTypes.Email)).IsEqualTo(Email);
    }

    [Test]
    public async Task CreateTicketAsync_WithoutEmailScope_RequestsOnlyUserInformation()
    {
        var backchannel = Backchannel();
        var handler = await CreateHandlerAsync(backchannel: backchannel);
        using var tokens = Tokens();

        await handler.CreateTicketFromTokensAsync(tokens);

        await Assert.That(backchannel.Request.Url).IsEqualTo(ManiaPlanetAuthenticationDefaults.UserInformationEndpoint);
    }

    [Test]
    public async Task CreateTicketAsync_SendsAccessTokenToUserEndpoint()
    {
        var backchannel = Backchannel();
        var handler = await CreateHandlerAsync(backchannel: backchannel);
        using var tokens = Tokens();

        await handler.CreateTicketFromTokensAsync(tokens);

        await Assert.That(backchannel.Request.Authorization?.Scheme).IsEqualTo("Bearer");
        await Assert.That(backchannel.Request.Authorization?.Parameter).IsEqualTo(AccessToken);
    }

    private static async Task<TestManiaPlanetAuthenticationHandler> CreateHandlerAsync(
        Func<OAuthCreatingTicketContext, Task>? onCreatingTicket = null,
        FakeHttpMessageHandler? backchannel = null,
        string[]? scopes = null)
    {
        var options = new ManiaPlanetAuthenticationOptions
        {
            Backchannel = (backchannel ?? Backchannel()).CreateClient()
        };

        foreach (var scope in scopes ?? [])
        {
            options.Scope.Add(scope);
        }

        if (onCreatingTicket is not null)
        {
            options.Events.OnCreatingTicket = onCreatingTicket;
        }

        return await TestManiaPlanetAuthenticationHandler.CreateAsync(options);
    }

    /// <summary>
    /// Answers the user information endpoint with <see cref="UserJson"/> and the email endpoint with <see cref="Email"/>.
    /// </summary>
    private static FakeHttpMessageHandler Backchannel()
    {
        return new FakeHttpMessageHandler(request => request.RequestUri!.ToString() == ManiaPlanetAuthenticationDefaults.EmailEndpoint
            ? FakeResponse.Text(Email)
            : FakeResponse.Json(UserJson));
    }

    private static OAuthTokenResponse Tokens()
    {
        return OAuthTokenResponse.Success(JsonDocument.Parse($$"""{"access_token":"{{AccessToken}}","token_type":"Bearer"}"""));
    }
}
