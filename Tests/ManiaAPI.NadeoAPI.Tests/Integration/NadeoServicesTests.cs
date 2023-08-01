﻿using Microsoft.Extensions.Configuration;
using System;
using Xunit;

namespace ManiaAPI.NadeoAPI.Tests.Integration;

public class NadeoServicesTests
{
    [Fact]
    public async void RequestManagement()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<AutoGeneratedProgram>()
            .Build();

        var login = configuration.GetValue<string>("DedicatedServer:Login") ?? throw new Exception("DedicatedServer:Login user secret is required");
        var password = configuration.GetValue<string>("DedicatedServer:Password") ?? throw new Exception("DedicatedServer:Password user secret is required");

        var nadeoServices = new NadeoServices();
        
        var accountList = new Guid[]
        {
            Guid.Parse("6a43df20-cd1a-4b3b-87b9-a6835a9b416d"),
            Guid.Parse("faedcf21-d61a-4305-9ffe-680b2ee5d65e")
        };

        var mapIds = new Guid[]
        {
            Guid.Parse("df781bdb-6f9a-4efc-855b-dbef446e1b8a"),
            Guid.Parse("db589f15-ef00-4de4-b3ee-9383f1a1eecf")
        };

        // Act
        await nadeoServices.AuthorizeAsync(login, password, AuthorizationMethod.DedicatedServer);

        // Assert
        //var stuff = await nadeoServices.GetPlayerClubTagsAsync(accountList);
    }
}
