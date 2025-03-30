# ManiaAPI.NadeoAPI.Extensions.Hosting

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.NadeoAPI.Extensions.Hosting?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.NadeoAPI.Extensions.Hosting/)

Provides an efficient way to inject all Nadeo services into your application.

## Setup

```cs
using ManiaAPI.TrackmaniaAPI.Extensions.Hosting;

builder.Services.AddNadeoAPI(options =>
{
    options.Credentials = new NadeoAPICredentials(
        builder.Configuration["NadeoAPI:Login"]!,
        builder.Configuration["NadeoAPI:Password"]!,
        AuthorizationMethod.DedicatedServer);
});
```

Features this setup brings:
- `NadeoServices`, `NadeoLiveServices`, and `NadeoMeetServices` will be available as transients
- HTTP client will be handled properly
- Credentials will be handled as a singleton