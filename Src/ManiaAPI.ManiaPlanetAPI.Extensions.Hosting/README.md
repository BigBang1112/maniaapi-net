# ManiaAPI.ManiaPlanetAPI.Extensions.Hosting

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.ManiaPlanetAPI.Extensions.Hosting?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.ManiaPlanetAPI.Extensions.Hosting/)

Provides ManiaPlanet OAuth2 authorization for ASP.NET Core applications.

## Setup

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