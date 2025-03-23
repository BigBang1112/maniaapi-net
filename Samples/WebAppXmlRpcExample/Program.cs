using System.Net;
using WebAppXmlRpcExample.Components;
using ManiaAPI.XmlRpc.TMT;
using WebAppXmlRpcExample;
using System.Collections.Immutable;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register the services

//
// Setup all TMT services
foreach (Platform platform in Enum.GetValues<Platform>())
{
    builder.Services.AddHttpClient($"{nameof(InitServerTMT)}_{platform}", client => client.BaseAddress = new Uri(InitServerTMT.GetDefaultAddress(platform)));
    builder.Services.AddHttpClient($"{nameof(MasterServerTMT)}_{platform}")
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip
        });

    builder.Services.AddKeyedScoped(platform, (provider, key) => new InitServerTMT(
        provider.GetRequiredService<IHttpClientFactory>().CreateClient($"{nameof(InitServerTMT)}_{key}")));

    builder.Services.AddKeyedSingleton(platform, (provider, key) => new MasterServerTMT(
        provider.GetRequiredService<IHttpClientFactory>().CreateClient($"{nameof(MasterServerTMT)}_{key}")));
    builder.Services.AddSingleton(provider => provider.GetRequiredKeyedService<MasterServerTMT>(platform));
}

builder.Services.AddSingleton(provider => Enum.GetValues<Platform>()
    .ToImmutableDictionary(platform => platform, platform => provider.GetRequiredKeyedService<MasterServerTMT>(platform)));
//
//

builder.Services.AddHostedService<StartupHostedService>();

builder.Services.AddHybridCache();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
