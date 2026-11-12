using System.Collections.Immutable;
using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Engine;
using ManiaAPI.MCP.Tools;
using ManiaAPI.MasterServer.Extensions.Hosting;
using ManiaAPI.MasterServer.TMUF;
using ManiaAPI.MasterServer.MP4;
using ManiaAPI.ManiaPlanetAPI;
using ManiaAPI.ManiaPlanetAPI.Extensions.Hosting;
using ManiaAPI.TrackmaniaWS;
using ManiaAPI.TrackmaniaWS.Extensions.Hosting;
using ManiaAPI.TMX.Extensions.Hosting;
using ManiaAPI.TrackmaniaAPI;
using ManiaAPI.TrackmaniaAPI.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection;
using ManiaAPI.MCP.Resources;
using ManiaAPI.MCP.Accounts;

namespace ManiaAPI.MCP.Hosting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddManiaAPI(
        this IServiceCollection services,
        IConfiguration configuration,
        string[]? toolsets = null)
    {
        services.AddSingleton<IValidateOptions<McpOptions>, McpOptionsValidator>();

        services.AddOptions<McpOptions>()
            .Bind(configuration.GetSection(McpOptions.Section))
            .PostConfigure(options =>
            {
                // The binder appends configured array items to non-empty defaults, so a configured array
                // would only ever add to the defaults. Replace them instead.
                var section = configuration.GetSection(McpOptions.Section);
                ReplaceArray(section, "Toolsets", values => options.Toolsets = values);
                ReplaceArray(section, "MasterServer:Games", values => options.MasterServer.Games = values);
                ReplaceArray(section, "Exchange:Sites", values => options.Exchange.Sites = values);
                ReplaceArray(section, "Exchange:MxSites", values => options.Exchange.MxSites = values);
                ReplaceArray(section, "AccountLinking:TrackmaniaScopes", values => options.AccountLinking.TrackmaniaScopes = values);
                ReplaceArray(section, "AccountLinking:ManiaPlanetScopes", values => options.AccountLinking.ManiaPlanetScopes = values);

                if (toolsets is not null)
                {
                    options.Toolsets = toolsets;
                }
            })
            .ValidateOnStart();

        services.AddSingleton(provider => provider.GetRequiredService<IOptions<McpOptions>>().Value);
        services.AddSingleton<NadeoAccount>();
        services.AddSingleton<INadeoAccount>(provider => provider.GetRequiredService<NadeoAccount>());
        services.AddHostedService(provider => provider.GetRequiredService<NadeoAccount>());
        services.AddSingleton<UpstreamHealth>();
        services.AddSingleton<WriteHostingGuard>();
        services.AddHostedService(provider => provider.GetRequiredService<WriteHostingGuard>());
        var protection = services.AddDataProtection().SetApplicationName("ManiaAPI.MCP");
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<WriteConfirmation>();
        services.AddScoped<LegacyClients>();
        services.AddScoped<LinkedClients>();

        var options = configuration.GetSection(McpOptions.Section).Get<McpOptions>() ?? new McpOptions();

        if (options.AccountLinking.Enabled && !string.IsNullOrWhiteSpace(options.AccountLinking.DataProtectionKeyPath))
        {
            protection.PersistKeysToFileSystem(new DirectoryInfo(options.AccountLinking.DataProtectionKeyPath));
        }

        AccountLinking.Add(services, options);

        services.AddTMX(exchange => exchange.ConfigureHttpClient = builder => builder.ConfigureHttpClient(client =>
            client.MaxResponseContentBufferSize = options.Shaping.MaxResourceBytes));
        services.AddMasterServerTMUF();
        services.AddHttpClient("MasterServerTMNF", client => client.BaseAddress = new Uri(MasterServerTMUF.DefaultNationsUrl));
        services.AddKeyedScoped("tmnf", (provider, _) => new MasterServerTMUF(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient("MasterServerTMNF")));
        services.AddMasterServerMP3();
        services.AddMasterServerMP4();
        services.AddKeyedScoped("player", (provider, _) =>
        {
            var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(MasterServerMP4));
            client.BaseAddress = new Uri(MasterServerMP4.DefaultUrlRelay02);

            return new MasterServerMP4(client);
        });
        services.AddMasterServerTMT();
        services.AddManiaPlanetIngameAPI();
        services.AddManiaPlanetAPI(o =>
        {
            if (options.ManiaPlanet.Configured)
            {
                o.Credentials = new ManiaPlanetAPICredentials(
                    options.ManiaPlanet.ClientId!, options.ManiaPlanet.ClientSecret!, options.ManiaPlanet.Scopes.ToImmutableArray());
            }
        });

        if (options.TrackmaniaWS.Configured)
        {
            services.AddTrackmaniaWS(new TrackmaniaWSOptions
            {
                Credentials = new TrackmaniaWSCredentials(options.TrackmaniaWS.Username!, options.TrackmaniaWS.Password!)
            });
        }

        services.AddHttpClient<UnitedLadder.UnitedLadder>();
        services.AddTrackmaniaAPI(o =>
        {
            if (options.TrackmaniaAPI.Configured)
            {
                o.Credentials = new TrackmaniaAPICredentials(
                    options.TrackmaniaAPI.ClientId!,
                    options.TrackmaniaAPI.ClientSecret!,
                    options.TrackmaniaAPI.Scopes.ToImmutableArray());
            }
        });

        services.AddSingleton(_ => GeneratedCatalog.Create()
            .Concat(GbxOperations.Create())
            .Concat(ScoreUrlOperations.Create())
            .ToArray());
        services.AddSingleton(provider => new OperationEngine(
            provider.GetRequiredService<McpOptions>(),
            provider.GetRequiredService<Operation[]>(),
            provider.GetRequiredService<UpstreamHealth>()));

        services.AddSingleton(provider => new OperationSearch(provider.GetRequiredService<Operation[]>()));
        services.AddHttpContextAccessor();
        services.AddSingleton<FeaturedTools>();
        services.AddSingleton<GatewayTools>();
        services.AddSingleton<ResourceService>();

        return services;
    }

    private static void ReplaceArray(IConfigurationSection section, string key, Action<string[]> set)
    {
        if (section.GetSection(key).Get<string[]>() is { } values)
        {
            set(values);
        }
    }
}
