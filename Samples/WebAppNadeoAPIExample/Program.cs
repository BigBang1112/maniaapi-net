using ManiaAPI.NadeoAPI;
using ManiaAPI.NadeoAPI.Extensions.Hosting;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddNadeoAPI(options =>
{
    options.Credentials = new NadeoAPICredentials(
        builder.Configuration["NadeoAPI:Login"]!,
        builder.Configuration["NadeoAPI:Password"]!,
        AuthorizationMethod.DedicatedServer);
});

builder.Services.AddMemoryCache();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

app.MapGet("/zones", async (NadeoServices nadeoServices, IMemoryCache cache, CancellationToken cancellationToken) =>
{
    return TypedResults.Ok(await cache.GetOrCreateAsync("zones", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
        return await nadeoServices.GetZonesAsync(cancellationToken);
    }));
});

app.Run();

[JsonSerializable(typeof(ImmutableArray<Zone>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
