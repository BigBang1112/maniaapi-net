
using ManiaAPI.ManiaPlanetAPI;
using ManiaAPI.TrackmaniaAPI;

namespace WebAppAuthorizationExample;

public sealed class AuthorizeHostedService : IHostedService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IConfiguration config;

    public AuthorizeHostedService(IServiceProvider serviceProvider, IConfiguration config)
    {
        this.serviceProvider = serviceProvider;
        this.config = config;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var trackmaniaAPI = scope.ServiceProvider.GetRequiredService<TrackmaniaAPI>();
        await trackmaniaAPI.AuthorizeAsync(config["OAuth2:Trackmania:Id"]!, config["OAuth2:Trackmania:Secret"]!, cancellationToken);

        var maniaPlanetAPI = scope.ServiceProvider.GetRequiredService<ManiaPlanetAPI>();
        await maniaPlanetAPI.AuthorizeAsync(config["OAuth2:ManiaPlanet:Id"]!, config["OAuth2:ManiaPlanet:Secret"]!, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
