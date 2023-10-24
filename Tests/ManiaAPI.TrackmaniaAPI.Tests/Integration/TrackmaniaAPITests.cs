﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ManiaAPI.TrackmaniaAPI.Tests.Integration;

public class TrackmaniaAPITests
{
    // test authorize
    [Fact]
    public async Task RequestManagement()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<AutoGeneratedProgram>()
            .Build();

        var clientId = configuration.GetValue<string>("ClientId") ?? throw new Exception("ClientId is required");
        var clientSecret = configuration.GetValue<string>("ClientSecret") ?? throw new Exception("ClientSecret is required");

        var client = new TrackmaniaAPI();

        await client.AuthorizeAsync(clientId, clientSecret);

        var displayNames = await client.GetDisplayNamesAsync(
        [
            Guid.Parse("6a43df20-cd1a-4b3b-87b9-a6835a9b416d"),
            Guid.Parse("faedcf21-d61a-4305-9ffe-680b2ee5d65e")
        ]);

        Assert.IsType<Dictionary<Guid, string>>(displayNames);

    }
}
