﻿using System;
using Xunit;

namespace ManiaAPI.TrackmaniaIO.Integration;

public class TrackmaniaIOTests
{
    [Fact]
    public async void RequestManagement()
    {
        // Arrange
        var startingRateLimitRemaining = TrackmaniaIO.RateLimitRemaining;

        // Act
        var campaigns = await TrackmaniaIO.GetSeasonalCampaignsAsync();

        var rateLimitRemainingAfterFirstRequest = TrackmaniaIO.RateLimitRemaining;

        var customCampaign = await TrackmaniaIO.GetCustomCampaignAsync(clubId: 21571, campaignId: 5886);
       
        var rateLimitPerRequest = rateLimitRemainingAfterFirstRequest - TrackmaniaIO.RateLimitRemaining;
        
        var officialCampaign = await TrackmaniaIO.GetSeasonalCampaignAsync(campaignId: 130);
        var leaderboard = await TrackmaniaIO.GetLeaderboardAsync("3987d489-03ae-4645-9903-8f7679c3a418", "XJ_JEjWGoAexDWe8qfaOjEcq5l8");
        var recentWorldRecords = await TrackmaniaIO.GetRecentWorldRecordsAsync("3987d489-03ae-4645-9903-8f7679c3a418");

        // Test a case if rate limit is reached, mocked
        TrackmaniaIO.RateLimitRemaining = 0;
        TrackmaniaIO.RateLimitReset = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(3);

        var campaignsWhenRateLimited = await TrackmaniaIO.GetSeasonalCampaignsAsync();

        // Case of the rate limit being refreshed is still not being tested, due to the usage of the X-Ratelimit-Remaining value given by the actual request

        // Assert
        Assert.Null(startingRateLimitRemaining);
        Assert.Equal(expected: 1, actual: rateLimitPerRequest);
        
        Assert.IsType<CampaignCollection>(campaigns);
        Assert.IsType<Campaign>(customCampaign);
        Assert.IsType<Campaign>(officialCampaign);
        Assert.IsType<Leaderboard>(leaderboard);
        Assert.IsType<WorldRecord[]>(recentWorldRecords);

        Assert.IsType<CampaignCollection>(campaignsWhenRateLimited);
    }
}
