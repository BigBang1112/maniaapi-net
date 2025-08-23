# ManiaAPI.UnitedLadder

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.UnitedLadder?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.UnitedLadder/)

Wraps https://ul.unitedascenders.xyz/ (new UnitedLadder).

## Features

- Get player info (registration ID from login for example)

More will be added in the future.

## Setup

```cs
using ManiaAPI.UnitedLadder;

var ul = new UnitedLadder();
```

or with DI, using an injected `HttpClient`:

```cs
using ManiaAPI.UnitedLadder;

builder.Services.AddHttpClient<UnitedLadder>();
```