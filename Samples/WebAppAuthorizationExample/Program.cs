using ManiaAPI.ManiaPlanetAPI;
using ManiaAPI.ManiaPlanetAPI.Extensions.Hosting;
using ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Authentication;
using ManiaAPI.TrackmaniaAPI.Extensions.Hosting;
using ManiaAPI.TrackmaniaAPI.Extensions.Hosting.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebAppAuthorizationExample;
using WebAppAuthorizationExample.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie()
    .AddManiaPlanet(options =>
    {
        options.ClientId = builder.Configuration["OAuth2:ManiaPlanet:Id"]!;
        options.ClientSecret = builder.Configuration["OAuth2:ManiaPlanet:Secret"]!;

        Array.ForEach(["basic", "dedicated", "titles"], options.Scope.Add);

        options.SaveTokens = true;
    })
    .AddTrackmania(options =>
    {
        options.ClientId = builder.Configuration["OAuth2:Trackmania:Id"]!;
        options.ClientSecret = builder.Configuration["OAuth2:Trackmania:Secret"]!;

        options.SaveTokens = true;
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<ManiaPlanetAPI>()
    .AddHttpMessageHandler<UserDelegatingHandler>();
builder.Services.AddTransient<UserDelegatingHandler>();

// Add services to the container.
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

app.MapGet("/login/maniaplanet", () =>
{
    return TypedResults.Challenge(new() { RedirectUri = "/" }, [ManiaPlanetAuthenticationDefaults.AuthenticationScheme]);
});

app.MapGet("/login/trackmania", () =>
{
    return TypedResults.Challenge(new() { RedirectUri = "/" }, [TrackmaniaAuthenticationDefaults.AuthenticationScheme]);
});

app.MapGet("/logout", () =>
{
    return TypedResults.SignOut(new() { RedirectUri = "/" });
}).RequireAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
