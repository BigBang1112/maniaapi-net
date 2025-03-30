# ManiaAPI.TrackmaniaAPI.Extensions.Hosting

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.TrackmaniaAPI.Extensions.Hosting?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.TrackmaniaAPI.Extensions.Hosting/)

Provides Trackmania OAuth2 authorization for ASP.NET Core applications and an efficient way to inject `TrackmaniaAPI` into your application.

## Setup `TrackmaniaAPI` injection

`TrackmaniaAPI` will be available as a transient, with a singleton handling of credentials. This will make sure the `HttpClient` beneath is handled properly.

```cs
using ManiaAPI.TrackmaniaAPI.Extensions.Hosting;

builder.Services.AddTrackmaniaAPI();
```

## Setup OAuth2

For the list of scopes, see [the API docs](https://api.trackmania.com/doc). Generate your credentials [here](https://api.trackmania.com/manager). **The redirect URL is the `/signin-trackmania` relative to the web root**, for example: `https://localhost:7864/signin-trackmania`.

```cs
using ManiaAPI.TrackmaniaAPI.Extensions.Hosting;
using ManiaAPI.TrackmaniaAPI.Extensions.Hosting.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie()
    .AddTrackmania(options =>
    {
        options.ClientId = builder.Configuration["OAuth2:Trackmania:Id"]!;
        options.ClientSecret = builder.Configuration["OAuth2:Trackmania:Secret"]!;
        
        options.Scope.Add("clubs");
    });

var app = builder.Build();

app.MapGet("/login", () =>
{
    return TypedResults.Challenge(new() { RedirectUri = "/" }, [TrackmaniaAuthenticationDefaults.AuthenticationScheme]);
});

app.Run();
```

You can inject `TrackmaniaAPI` if you create a special HTTP client handler to provide the token from `HttpContext.GetTokenAsync("access_token")` and use that to get more information from the authorized user. Don't forget to set `SaveTokens = true` in options.
