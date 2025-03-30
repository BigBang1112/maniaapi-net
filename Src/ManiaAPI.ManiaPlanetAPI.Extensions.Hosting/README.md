# ManiaAPI.ManiaPlanetAPI.Extensions.Hosting

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.ManiaPlanetAPI.Extensions.Hosting?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.ManiaPlanetAPI.Extensions.Hosting/)

Provides ManiaPlanet OAuth2 authorization for ASP.NET Core applications and an efficient way to inject `ManiaPlanetAPI` into your application.

## Setup `ManiaPlanetAPI` injection

`ManiaPlanetAPI` will be available as a transient, with a singleton handling of credentials. This will make sure the `HttpClient` beneath is handled properly.

```cs
using ManiaAPI.ManiaPlanetAPI.Extensions.Hosting;

builder.Services.AddManiaPlanetAPI();
```

## Setup OAuth2

For the list of scopes, see [here at the bottom](https://doc.maniaplanet.com/web-services/oauth2). Generate your credentials [here](https://maniaplanet.com/web-services-manager). **The redirect URL is the `/signin-maniaplanet` relative to the web root**, for example: `https://localhost:7864/signin-maniaplanet`.

```cs
using ManiaAPI.ManiaPlanetAPI.Extensions.Hosting;
using ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie()
    .AddManiaPlanet(options =>
    {
        options.ClientId = builder.Configuration["OAuth2:ManiaPlanet:Id"]!;
        options.ClientSecret = builder.Configuration["OAuth2:ManiaPlanet:Secret"]!;

        Array.ForEach(["basic", "dedicated", "titles"], options.Scope.Add);
    });

var app = builder.Build();

app.MapGet("/login", () =>
{
    return TypedResults.Challenge(new() { RedirectUri = "/" }, [ManiaPlanetAuthenticationDefaults.AuthenticationScheme]);
});

app.Run();
```

You can inject `ManiaPlanetAPI` if you create a special HTTP client handler to provide the token from `HttpContext.GetTokenAsync("access_token")` and use that to get more information from the authorized user. Don't forget to set `SaveTokens = true` in options.