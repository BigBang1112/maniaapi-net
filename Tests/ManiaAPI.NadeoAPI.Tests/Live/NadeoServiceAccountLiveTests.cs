namespace ManiaAPI.NadeoAPI.Tests.Live;

/// <summary>
/// Requests that are documented to require a service account, mostly because they read other players' data.
/// </summary>
[LiveTest]
[NotInParallel("Nadeo")]
public class NadeoServiceAccountLiveTests
{
    private const NadeoAccount Account = NadeoAccount.ServiceAccount;

    private static readonly Guid[] AccountIds =
    [
        Guid.Parse("4a983dc3-a938-4564-8a1c-8be20779f069"),
        Guid.Parse("5dda004f-add9-43ad-b9cd-7a90101aa8f2")
    ];

    private static readonly Guid[] MapIds =
    [
        Guid.Parse("ed53486f-a80d-419b-b073-54bb8944d502"),
        Guid.Parse("e8a02ba9-f418-463d-8a83-ac5fc465ca8a")
    ];

    [Test]
    public async Task NadeoServices_OtherAccountRecords()
    {
        var ns = await LiveNadeo.GetServicesAsync(Account);
        var nls = await LiveNadeo.GetLiveServicesAsync(Account);
        var accountId = AccountIds[0];
        var seasonId = Guid.Parse((await nls.GetSeasonalCampaignsAsync(1)).CampaignList[0].SeasonUid);

        var recordsByMapIds = await ns.GetAccountRecordsByMapIdsAsync(accountId, MapIds);
        var recordsBySeasonIds = await ns.GetAccountRecordsBySeasonIdsAsync(accountId, [seasonId]);

        await Assert.That(recordsByMapIds).IsNotEmpty();
        await Assert.That(recordsByMapIds.Select(x => x.AccountId)).DoesNotContain(x => x != accountId);
        await Assert.That(recordsBySeasonIds.Select(x => x.AccountId)).DoesNotContain(x => x != accountId);
    }

    [Test]
    public async Task NadeoServices_PlayerClubTags()
    {
        var ns = await LiveNadeo.GetServicesAsync(Account);

        var clubTags = await ns.GetPlayerClubTagsAsync(AccountIds);

        await Assert.That(clubTags.Select(x => x.AccountId)).IsEquivalentTo(AccountIds);
    }

    [Test]
    public async Task NadeoServices_PlayerTrophies()
    {
        var ns = await LiveNadeo.GetServicesAsync(Account);

        var summary = await ns.GetPlayerTrophySummaryAsync(AccountIds[0]);
        var history = await ns.GetPlayerTrophyHistoryAsync(AccountIds[0], count: 5);

        await Assert.That(summary.AccountId).IsEqualTo(AccountIds[0]);
        await Assert.That(history.Data).IsNotEmpty();
    }
}
