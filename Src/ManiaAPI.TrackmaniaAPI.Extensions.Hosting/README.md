# ManiaAPI.TrackmaniaAPI.Extensions.Hosting

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.TrackmaniaAPI.Extensions.Hosting?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.TrackmaniaAPI.Extensions.Hosting/)

Provides Trackmania OAuth2 authorization for ASP.NET Core applications.

## Setup

```cs
using ManiaAPI.TrackmaniaAPI.Extensions.Hosting;

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddTrackmania(options =>
    {
        options.ClientId = config["OAuth2:Trackmania:Id"];
        options.ClientSecret = config["OAuth2:Trackmania:Secret"];

        Array.ForEach(new[] { "basic", "dedicated", "titles" }, options.Scope.Add);
    });
```