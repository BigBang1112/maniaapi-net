# Tests

The tests use [TUnit](https://tunit.dev) on Microsoft Testing Platform, which [`global.json`](../global.json) selects for `dotnet test`. Every test project targets .NET 11, 10, 9, and 8.

```sh
dotnet test
```

## Unit and live tests

**Unit tests** run by default. They replace the network with a fake `HttpMessageHandler` (or, for GBXRemote, a loopback TCP server), so they are fast and don't depend on any service being online.

**Live tests** send real requests. They are marked with `[LiveTest]`, which puts them in the `Live` category and retries them twice (after 2 and then 4 seconds), because the services fail now and then on their own. They are skipped unless the `LiveTests:Enabled` setting is `true`.

```sh
# Once, for every test project
dotnet user-secrets -p Tests/ManiaAPI.NadeoAPI.Tests set "LiveTests:Enabled" true

dotnet test --treenode-filter "/*/*/*/*[Category=Live]" --ignore-exit-code 8
```

Some test projects have no live tests, so the filter matches nothing in them. Microsoft Testing Platform reports a run with zero tests as a failure with exit code 8. `--ignore-exit-code 8` stops those projects from failing the run.

## Settings

Live test settings are read by [`LiveSettings`](Shared/LiveSettings.cs). Each setting has one key, which is read from:

1. **User secrets**, for local development. Every test project shares the same store (`ManiaAPI.NET.Tests`), so a setting is set once, through any test project.
2. **Environment variables**, for CI. They override user secrets. The name is `MANIAAPI_` followed by the key, with `:` replaced by `__`.

A live test whose credentials are missing is skipped.

| Key | Environment variable | Used by |
|---|---|---|
| `LiveTests:Enabled` | `MANIAAPI_LIVETESTS__ENABLED` | every live test |
| `NadeoAPI:DedicatedServer:Login` | `MANIAAPI_NADEOAPI__DEDICATEDSERVER__LOGIN` | `ManiaAPI.NadeoAPI.Tests` |
| `NadeoAPI:DedicatedServer:Password` | `MANIAAPI_NADEOAPI__DEDICATEDSERVER__PASSWORD` | |
| `NadeoAPI:ServiceAccount:Login` | `MANIAAPI_NADEOAPI__SERVICEACCOUNT__LOGIN` | `ManiaAPI.NadeoAPI.Tests`, `ManiaAPI.NadeoAPI.Extensions.Hosting.Tests` |
| `NadeoAPI:ServiceAccount:Password` | `MANIAAPI_NADEOAPI__SERVICEACCOUNT__PASSWORD` | |
| `Trackmania:ClientId` | `MANIAAPI_TRACKMANIA__CLIENTID` | `ManiaAPI.TrackmaniaAPI.Tests`, `ManiaAPI.TrackmaniaAPI.Extensions.Hosting.Tests` |
| `Trackmania:ClientSecret` | `MANIAAPI_TRACKMANIA__CLIENTSECRET` | |
| `ManiaPlanet:ClientId` | `MANIAAPI_MANIAPLANET__CLIENTID` | `ManiaAPI.ManiaPlanetAPI.Tests`, `ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Tests` |
| `ManiaPlanet:ClientSecret` | `MANIAAPI_MANIAPLANET__CLIENTSECRET` | |
| `TrackmaniaWS:Username` | `MANIAAPI_TRACKMANIAWS__USERNAME` | `ManiaAPI.TrackmaniaWS.Tests` |
| `TrackmaniaWS:Password` | `MANIAAPI_TRACKMANIAWS__PASSWORD` | |

```sh
dotnet user-secrets -p Tests/ManiaAPI.NadeoAPI.Tests set "NadeoAPI:ServiceAccount:Login" "<login>"
```

The Nadeo API has two kinds of accounts, and some endpoints work with only one of them, so they have separate settings. Every test in `NadeoLiveTests` runs once with each, so the tests show which endpoints are for a service account only: with a dedicated server account, those tests check that the request is denied (`401`).

Either kind can be left out. Its tests are then skipped.

A service account acts as the player account that created it, so the tests don't expect it to have no maps, clubs or records.

The TrackmaniaWS API has no setting: its live test only checks that wrong credentials are rejected.

## Reports

In CI, every project writes a TRX and an HTML report into `TestResults`, which the workflow uploads as the `live-test-results` artifact. Locally, add `--report-trx` to get the same.

## Layout

- [`Shared`](Shared) is compiled into every test project: the fake HTTP handlers, a JWT builder for authorization tests, `LiveSettings`, and `LiveTestAttribute`.
- Live tests are in the `Live` folder of each project.
