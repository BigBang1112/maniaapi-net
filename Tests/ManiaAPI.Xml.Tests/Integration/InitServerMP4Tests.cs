using ManiaAPI.Xml.MP3;
using ManiaAPI.Xml.MP4;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ManiaAPI.Xml.Tests.Integration;

public class InitServerMP4Tests
{
    [Fact]
    public async Task TestAsync()
    {
        var server = new InitServerMP4();

        var response = await server.TestAsync();

        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetWaitingParamsAsync_ReturnsMasterServers()
    {
        var server = new InitServerMP4();

        var waitingParams = await server.GetWaitingParamsAsync();

        Assert.NotEmpty(waitingParams.MasterServers);
    }

    [Fact]
    public async Task GetAccountFromSteamUserAsync_ReturnsLogin()
    {
        var server = new InitServerMP4();

        var login = await server.GetAccountFromSteamUserAsync(76561198060959523);

        Assert.NotNull(login);
    }

    [Fact]
    public async Task GetAccountFromUplayUserAsync_ReturnsLogin()
    {
        var server = new InitServerMP4();

        var login = await server.GetAccountFromUplayUserAsync(Guid.Parse("0bca2165-693d-41de-b723-632966e8de74"));

        Assert.NotNull(login);
    }

    [Fact]
    public async Task GetWebIdentityFromManiaplanetLoginAsync_ReturnsWebIdentities()
    {
        var server = new InitServerMP4();

        var webIdentities = await server.GetWebIdentityFromManiaplanetLoginAsync("futurecat", "riolu", "eddyey");

        Assert.NotEmpty(webIdentities);
    }

    [Fact]
    public async Task GetManiaplanetLoginFromWebIdentityAsync_ReturnsLogins()
    {
        var server = new InitServerMP4();

        var login = await server.GetManiaplanetLoginFromWebIdentityAsync("uplay", Guid.Parse("e8c8b8eb-24d1-49ac-a368-036c6feac5c4"));

        Assert.NotNull(login);
    }
}
