﻿using Microsoft.Extensions.Configuration;

namespace ManiaAPI.ManiaPlanetAPI.Tests.Integration;

public class ManiaPlanetAPITests
{
    [Fact]
    public async Task AuthorizeAsync()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<AutoGeneratedProgram>()
            .Build();

        var clientId = configuration.GetValue<string>("ClientId") ?? throw new Exception("ClientId is required");
        var clientSecret = configuration.GetValue<string>("ClientSecret") ?? throw new Exception("ClientSecret is required");

        var api = new ManiaPlanetAPI();
        await api.AuthorizeAsync(clientId, clientSecret);
    }
}
