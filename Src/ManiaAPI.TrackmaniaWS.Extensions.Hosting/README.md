# ManiaAPI.TrackmaniaWS.Extensions.Hosting

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.TrackmaniaWS.Extensions.Hosting?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.TrackmaniaWS.Extensions.Hosting/)

Provides an efficient way to inject `TrackmaniaWS` into your application.

## Setup

```cs
using ManiaAPI.TrackmaniaWS.Extensions.Hosting;

builder.Services.AddTrackmaniaWS(new TrackmaniaWSOptions
{
    Credentials = new("tmf_yourapp", "password")
});
```

## Resilience

`AddTrackmaniaWS` returns an `IHttpClientBuilder`, so you can chain [`Microsoft.Extensions.Http.Resilience`](https://www.nuget.org/packages/Microsoft.Extensions.Http.Resilience) directly onto it to add retries, timeouts, and circuit breakers:

```cs
using ManiaAPI.TrackmaniaWS.Extensions.Hosting;

builder.Services.AddTrackmaniaWS(new TrackmaniaWSOptions
{
    Credentials = new("tmf_yourapp", "password")
})
.AddStandardResilienceHandler();
```
