using WebAppXmlExample;
using WebAppXmlExample.Components;
using Microsoft.Extensions.Caching.Hybrid;
using ManiaAPI.Xml.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register the services
builder.Services.AddMasterServerTMT();
builder.Services.AddMasterServerMP4();

builder.Services.AddHostedService<StartupHostedService>();

builder.Services.AddHybridCache(options =>
{
    options.MaximumPayloadBytes = 1024 * 1024 * 16;
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(1),
        LocalCacheExpiration = TimeSpan.FromMinutes(1)
    };
});

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
