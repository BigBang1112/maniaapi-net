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
