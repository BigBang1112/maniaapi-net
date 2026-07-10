# ManiaAPI.NadeoAPI.Extensions.Hosting

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.NadeoAPI.Extensions.Hosting?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.NadeoAPI.Extensions.Hosting/)

Provides an efficient way to inject all Nadeo services into your application.

## Setup

Providing `options.Credentials` is optional, but setting it will automatically authorize on the first request and maintain that connection, so you don't have to call `AuthorizeAsync`.

```cs
using ManiaAPI.TrackmaniaAPI.Extensions.Hosting;

builder.Services.AddNadeoAPI(options =>
{
    options.Credentials = new NadeoAPICredentials(
        builder.Configuration["NadeoAPI:Login"]!,
        builder.Configuration["NadeoAPI:Password"]!);
});
```

Features this setup brings:
- `NadeoServices`, `NadeoLiveServices`, and `NadeoMeetServices` will be available as transients
- HTTP client will be handled properly
- Credentials will be handled as a singleton

## Resilience

Since the setup exposes each service's `IHttpClientBuilder` through `configureNadeoServices`, `configureNadeoLiveServices`, and `configureNadeoMeetServices`, you can add [`Microsoft.Extensions.Http.Resilience`](https://www.nuget.org/packages/Microsoft.Extensions.Http.Resilience) to automatically retry requests, apply timeouts, and use circuit breakers:

```cs
using ManiaAPI.NadeoAPI.Extensions.Hosting;

builder.Services.AddNadeoAPI(options =>
{
    options.Credentials = new NadeoAPICredentials(
        builder.Configuration["NadeoAPI:Login"]!,
        builder.Configuration["NadeoAPI:Password"]!);
},
configureNadeoServices: http => http.AddStandardResilienceHandler(),
configureNadeoLiveServices: http => http.AddStandardResilienceHandler(),
configureNadeoMeetServices: http => http.AddStandardResilienceHandler());
```