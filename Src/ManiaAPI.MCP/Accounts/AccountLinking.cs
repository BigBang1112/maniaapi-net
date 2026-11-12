using System.Text.Json.Nodes;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.ManiaPlanetAPI.Extensions.Hosting;
using ManiaAPI.TrackmaniaAPI.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;

namespace ManiaAPI.MCP.Accounts;

public static class AccountLinking
{
    public static void Add(IServiceCollection services, McpOptions options)
    {
        services.AddSingleton<LinkedAccountStore>();
        services.AddSingleton<LinkedAccountTokens>();

        if (!options.AccountLinking.Enabled)
        {
            return;
        }

        var authentication = services.AddAuthentication("LinkCookie").AddCookie("LinkCookie");

        if (options.TrackmaniaAPI.Configured)
        {
            authentication.AddTrackmania("LinkTrackmania", settings =>
                Configure(settings, options.TrackmaniaAPI, options.AccountLinking.TrackmaniaScopes, "trackmania"));
        }

        if (options.ManiaPlanet.Configured)
        {
            authentication.AddManiaPlanet("LinkManiaplanet", settings =>
                Configure(settings, options.ManiaPlanet, options.AccountLinking.ManiaPlanetScopes, "maniaplanet"));
        }
    }

    private static void Configure(
        Microsoft.AspNetCore.Authentication.OAuth.OAuthOptions settings,
        Configuration.OAuthOptions credentials,
        string[] scopes,
        string provider)
    {
        settings.ClientId = credentials.ClientId!;
        settings.ClientSecret = credentials.ClientSecret!;
        settings.UsePkce = true;
        settings.SaveTokens = false;
        settings.Scope.Clear();

        foreach (var scope in scopes)
        {
            settings.Scope.Add(scope);
        }

        settings.Events.OnCreatingTicket = async context =>
        {
            if (!context.Properties.Items.TryGetValue("ManiaApiCaller", out var caller) || string.IsNullOrEmpty(caller) ||
                string.IsNullOrEmpty(context.AccessToken))
            {
                throw new InvalidOperationException("The account-link request is invalid.");
            }

            var granted = context.TokenResponse.Response?.RootElement.TryGetProperty("scope", out var scopeValue) == true
                ? scopeValue.GetString()!.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                : scopes;

            var tokens = new LinkedTokens(context.AccessToken, context.RefreshToken,
                TimeProvider.System.GetUtcNow().Add(context.ExpiresIn ?? TimeSpan.FromHours(1)), granted);

            var store = context.HttpContext.RequestServices.GetRequiredService<LinkedAccountStore>();
            await using var lease = await store.LockAsync(caller, provider, context.HttpContext.RequestAborted);
            await store.WriteAsync(caller, provider, tokens, context.HttpContext.RequestAborted);
        };

        settings.Events.OnTicketReceived = async context =>
        {
            context.HandleResponse();
            context.Response.ContentType = "text/plain";
            context.Response.Headers.CacheControl = "no-store";
            await context.Response.WriteAsync("Player account linked. Return to your MCP client.", context.HttpContext.RequestAborted);
        };

        settings.Events.OnRemoteFailure = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Account linking failed or was declined. Start a new link request.", context.HttpContext.RequestAborted);
        };
    }

    public static void Map(WebApplication app, McpOptions options)
    {
        if (!options.AccountLinking.Enabled)
        {
            return;
        }

        var origin = new Uri(options.AccountLinking.PublicOrigin!);
        var protector = app.Services.GetRequiredService<IDataProtectionProvider>()
            .CreateProtector("ManiaAPI.MCP.AccountLinkTicket.v1").ToTimeLimitedDataProtector();

        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/account-links") ||
                context.Request.Path == "/signin-trackmania" || context.Request.Path == "/signin-maniaplanet")
            {
                context.Request.Scheme = origin.Scheme;
                context.Request.Host = new HostString(origin.Authority);
                context.Response.Headers.CacheControl = "no-store";
            }

            await next(context);
        });

        app.UseAuthentication();

        app.MapPost("/account-links/{provider}", (HttpContext context, string provider) =>
        {
            ValidateProvider(provider, options);
            var caller = (string)context.Items["ManiaApiIdentity"]!;
            var ticket = protector.Protect(caller + "|" + provider, TimeSpan.FromMinutes(5));
            var url = options.AccountLinking.PublicOrigin!.TrimEnd('/') + "/account-links/" + provider +
                "/start?ticket=" + Uri.EscapeDataString(ticket);

            return Results.Text(new JsonObject { ["url"] = url }.ToJsonString(), "application/json");
        });

        app.MapGet("/account-links/{provider}/start", async (HttpContext context, string provider, string ticket) =>
        {
            ValidateProvider(provider, options);
            string[] parts;

            try
            {
                parts = protector.Unprotect(ticket).Split('|');
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            if (parts.Length != 2 || parts[1] != provider)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            await context.ChallengeAsync(provider == "trackmania" ? "LinkTrackmania" : "LinkManiaplanet",
                new AuthenticationProperties
                {
                    Items = { ["ManiaApiCaller"] = parts[0] },
                    RedirectUri = "/account-links/complete"
                });
        });

        app.MapGet("/account-links/status", async (HttpContext context, LinkedAccountStore store) =>
        {
            var caller = (string)context.Items["ManiaApiIdentity"]!;
            var status = new JsonObject();

            foreach (var provider in new[] { "trackmania", "maniaplanet" })
            {
                status[provider] = await store.ReadAsync(caller, provider, context.RequestAborted) is not null;
            }

            return Results.Text(status.ToJsonString(), "application/json");
        });

        app.MapDelete("/account-links/{provider}", async (HttpContext context, string provider, LinkedAccountStore store) =>
        {
            ValidateProvider(provider, options);
            var caller = (string)context.Items["ManiaApiIdentity"]!;
            await using var lease = await store.LockAsync(caller, provider, context.RequestAborted);
            store.Delete(caller, provider);

            return Results.NoContent();
        });
    }

    private static void ValidateProvider(string provider, McpOptions options)
    {
        if (provider is not ("trackmania" or "maniaplanet") ||
            !(provider == "trackmania" ? options.TrackmaniaAPI.Configured : options.ManiaPlanet.Configured))
        {
            throw new BadHttpRequestException("This OAuth provider is not configured.");
        }
    }
}
