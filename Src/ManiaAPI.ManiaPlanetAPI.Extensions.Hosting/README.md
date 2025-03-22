# ManiaAPI.ManiaPlanetAPI.Extensions.Hosting

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.ManiaPlanetAPI.Extensions.Hosting?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.ManiaPlanetAPI.Extensions.Hosting/)

Provides ManiaPlanet OAuth2 authorization for ASP.NET Core applications.

## Setup

```cs
using ManiaAPI.ManiaPlanetAPI.Extensions.Hosting;

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddManiaPlanet(options =>
    {
        options.ClientId = config["OAuth2:ManiaPlanet:Id"];
        options.ClientSecret = config["OAuth2:ManiaPlanet:Secret"];

        Array.ForEach(new[] { "basic", "dedicated", "titles" }, options.Scope.Add);
    });
```