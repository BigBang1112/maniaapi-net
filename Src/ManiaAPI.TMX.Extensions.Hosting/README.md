# ManiaAPI.TMX.Extensions.Hosting

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.TMX.Extensions.Hosting?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.TMX.Extensions.Hosting/)

Provides an efficient way to inject all TMX services into your application.

## Setup

```cs
using ManiaAPI.TMX.Extensions.Hosting;

builder.Services.AddTMX();
```

Features this setup brings:

- You can inject `ImmutableDictionary<TmxSite, TMX>` to get all TMX sites as individual instances
- If you don't need specific site context, you can inject `IEnumerable<TMX>` to get all TMX sites
- Specific `TMX` can be injected using `[FromKeyedServices(TmxSite.TMNF)]`

> [!WARNING]
> If you just inject `TMX` alone, it will give the last-registered one (in this case, Original). If you need a specific site, use `[FromKeyedServices(...)]`.

## Resilience

HTTP requests can transiently fail, so it's a good idea to add some retry logic.

`TmxOptions.ConfigureHttpClient` gives you access to each site's `IHttpClientBuilder`, so you can add [`Microsoft.Extensions.Http.Resilience`](https://www.nuget.org/packages/Microsoft.Extensions.Http.Resilience) to automatically retry requests, apply timeouts, and use circuit breakers:

```cs
using ManiaAPI.TMX.Extensions.Hosting;

builder.Services.AddTMX(options =>
{
    options.ConfigureHttpClient = http => http.AddStandardResilienceHandler();
});
```