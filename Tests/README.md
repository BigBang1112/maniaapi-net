# Tests

The tests use [TUnit](https://tunit.dev) on Microsoft Testing Platform, which [`global.json`](../global.json) selects for `dotnet test`. Every test project targets .NET 10, 9, and 8.

```sh
dotnet test
```

## Unit and live tests

**Unit tests** run by default. They replace the network with a fake `HttpMessageHandler` (or, for GBXRemote, a loopback TCP server), so they are fast and don't depend on any service being online.

**Live tests** send real requests. They are marked with `[Category("Live"), LiveTest]` and are skipped unless `MANIAAPI_LIVE_TESTS` is `true`:

```sh
# PowerShell: $env:MANIAAPI_LIVE_TESTS = "true"
MANIAAPI_LIVE_TESTS=true dotnet test --treenode-filter "/*/*/*/*[Category=Live]"
```

Live tests retry twice, because the services fail now and then on their own.

## Credentials

Some live tests need credentials. Each one is read from an environment variable first, then from the test project's user secrets. A live test whose credentials are missing is skipped.

| Project | User secret | Environment variable |
|---|---|---|
| `ManiaAPI.NadeoAPI.Tests` | `DedicatedServer:Login` | `NADEO_API_LOGIN` |
| | `DedicatedServer:Password` | `NADEO_API_PASSWORD` |
| `ManiaAPI.TrackmaniaAPI.Tests` | `ClientId` | `TRACKMANIA_API_CLIENTID` |
| | `ClientSecret` | `TRACKMANIA_API_CLIENTSECRET` |
| `ManiaAPI.ManiaPlanetAPI.Tests` | `ClientId` | `MANIAPLANET_API_CLIENTID` |
| | `ClientSecret` | `MANIAPLANET_API_CLIENTSECRET` |

```sh
dotnet user-secrets -p Tests/ManiaAPI.NadeoAPI.Tests set "DedicatedServer:Login" "<login>"
```

## Layout

- [`Shared`](Shared) is compiled into every test project: the fake HTTP handler, a JWT builder for authorization tests, and `LiveTestAttribute`.
- [`Shared/Secrets`](Shared/Secrets) is compiled only into projects with a `UserSecretsId`.
- Live tests are in the `Live` folder of each project.
