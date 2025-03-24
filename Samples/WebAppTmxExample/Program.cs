using ManiaAPI.TMX;
using System.Collections.Immutable;
using System.Net;
using WebAppTmxExample.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//
// Setup all TMX sites
foreach (TmxSite site in Enum.GetValues<TmxSite>())
{
    builder.Services.AddHttpClient($"{nameof(TMX)}_{site}");

    builder.Services.AddKeyedScoped(site, (provider, key) => new TMX(
        provider.GetRequiredService<IHttpClientFactory>().CreateClient($"{nameof(TMX)}_{key}"), site));
    builder.Services.AddScoped(provider => provider.GetRequiredKeyedService<TMX>(site));
}

builder.Services.AddScoped(provider => Enum.GetValues<TmxSite>()
    .ToImmutableDictionary(site => site, site => provider.GetRequiredKeyedService<TMX>(site)));
//
//

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
