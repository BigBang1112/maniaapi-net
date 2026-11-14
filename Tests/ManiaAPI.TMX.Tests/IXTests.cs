namespace ManiaAPI.TMX.Tests;

public class IXTests
{
    // Shape as returned by the live API
    private const string ItemJson = """
        {"ID":40503,"Name":"GroundGreenLightFabric","UserID":19774,"Username":"Banano_TV","Description":"","AuthorLogin":"b3JcpeKNQgWNefq9Y95SwQ",
         "OriginalBlock":"TrackWallSlopeUBottomInGround","Type":11,"TypeName":"Block","Collection":5,"CollectionName":"Stadium2020","Game":2,
         "GameName":"Trackmania","Downloads":2916,"Score":85,"FileName":"GroundGreenLightFabric.Block.Gbx","Uploaded":"2021-04-07T11:41:13.65",
         "Updated":"2021-04-07T11:41:13.65","SetID":10730,"SetName":"Ground-Concrete-Colorized","Directory":"Ground-Concrete-Colorized",
         "ZipIndex":"8","Visible":true,"Unreleased":false,"LikeCount":0,"CommentCount":0,"FileSize":18,"Tags":"45,30","HasThumbnail":false}
        """;

    [Test]
    public async Task GetItemsAsync_RequestsCommaSeparatedIds()
    {
        var fake = FakeHttpMessageHandler.Json($"[{ItemJson}]");
        using var ix = CreateIx(fake);

        var items = await ix.GetItemsAsync([256, 40503, 256]);

        await Assert.That(fake.Request.Url).IsEqualTo("https://item.exchange/api/item/get_item_info/multi/256,40503");
        await Assert.That(items[0].Id).IsEqualTo(40503);
        await Assert.That(items[0].SetId).IsEqualTo(10730);
        await Assert.That(items[0].Type).IsEqualTo(IXItemType.Block);
        await Assert.That(items[0].Collection).IsEqualTo(IXEnvironment.Stadium2020);
        await Assert.That(items[0].Game).IsEqualTo(IXGame.Trackmania);
        await Assert.That(items[0].Unlisted).IsNull();
    }

    [Test]
    public async Task GetItemsAsync_NoIds_ReturnsEmptyWithoutRequest()
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        using var ix = CreateIx(fake);

        await Assert.That(await ix.GetItemsAsync([])).IsEmpty();
        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task GetItemSetsAsync_ReturnsSetsWithItems()
    {
        var fake = FakeHttpMessageHandler.Json($$$"""[{"ID":41,"Name":"Valley Rocks","UserID":256,"Collection":3,"Game":1,"ImageCount":2,"Items":[{{{ItemJson}}}]}]""");
        using var ix = CreateIx(fake);

        var sets = await ix.GetItemSetsAsync([41]);

        await Assert.That(fake.Request.Url).IsEqualTo("https://item.exchange/api/set/get_set_info/multi/41");
        await Assert.That(sets[0].Collection).IsEqualTo(IXEnvironment.Valley);
        await Assert.That(sets[0].ImageCount).IsEqualTo(2);
        await Assert.That(sets[0].Items!.Single().Name).IsEqualTo("GroundGreenLightFabric");
    }

    [Test]
    public async Task GetBasketAsync_ReturnsBasket()
    {
        var fake = FakeHttpMessageHandler.Json($$$"""{"BasketUserID":1814,"BasketUsername":"Ozon","BasketFileSize":228,"Items":[{{{ItemJson}}}],"LastChanged":"2021-11-23T22:10:40.477"}""");
        using var ix = CreateIx(fake);

        var basket = await ix.GetBasketAsync(1814);

        await Assert.That(fake.Request.Url).IsEqualTo("https://item.exchange/api/user/get_basket/1814");
        await Assert.That(basket!.BasketUsername).IsEqualTo("Ozon");
        await Assert.That(basket.Items).Count().IsEqualTo(1);
    }

    [Test]
    public async Task GetBasketAsync_UnknownUserRedirectedToHomePage_ReturnsNull()
    {
        var fake = FakeHttpMessageHandler.Text("<html></html>", "text/html");
        using var ix = CreateIx(fake);

        await Assert.That(await ix.GetBasketAsync(999999999)).IsNull();
    }

    [Test]
    public async Task SearchItemsAsync_BuildsQuery()
    {
        var fake = FakeHttpMessageHandler.Json("""{"results":[],"totalItemCount":0}""");
        using var ix = CreateIx(fake);

        await ix.SearchItemsAsync(new IX.SearchItemsParameters
        {
            Limit = 50,
            Page = 2,
            PrimaryOrder = IXSearchOrder.DownloadsDescending,
            Mode = IXSearchMode.User,
            UserId = 19774,
            ItemName = ["rock", "tree & bush"],
            ItemType = IXItemType.Block,
            Collections = [IXEnvironment.Valley, IXEnvironment.Stadium2020],
            Tags = [45, 30],
            AllTags = true,
            CustomThumbnail = true,
            NotInSet = true,
            Game = IXGame.Trackmania
        });

        // AbsoluteUri keeps the escaping as sent; Url (Uri.ToString) would unescape the spaces
        await Assert.That(fake.Request.Uri.AbsoluteUri).IsEqualTo(
            "https://item.exchange/itemsearch/search?api=on&format=json&limit=50&page=2&priord=10&mode=1&itemname=rock%2Ctree%20%26%20bush" +
            "&userid=19774&itype=11&collections=3%2C5&customthumb=1&setinclusion=1&game=2&tags=45%2C30&tagsinc=1");
    }

    [Test]
    public async Task SearchItemsAsync_ReturnsResultsAndTotal()
    {
        var fake = FakeHttpMessageHandler.Json($$$"""{"results":[{{{ItemJson}}}],"totalItemCount":26910}""");
        using var ix = CreateIx(fake);

        var result = await ix.SearchItemsAsync(new IX.SearchItemsParameters());

        await Assert.That(fake.Request.Url).IsEqualTo("https://item.exchange/itemsearch/search?api=on&format=json");
        await Assert.That(result.TotalItemCount).IsEqualTo(26910);
        await Assert.That(result.Results.Single().Id).IsEqualTo(40503);
    }

    [Test]
    [Arguments(9)]
    [Arguments(201)]
    public async Task SearchItemsAsync_LimitOutsideRange_ThrowsWithoutRequest(int limit)
    {
        var fake = FakeHttpMessageHandler.Json("{}");
        using var ix = CreateIx(fake);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ix.SearchItemsAsync(new IX.SearchItemsParameters { Limit = limit }));
        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task GetRandomItemAsync_RequestsRandomResult()
    {
        var fake = FakeHttpMessageHandler.Json($$$"""{"results":[{{{ItemJson}}}],"totalItemCount":183712}""");
        using var ix = CreateIx(fake);

        var item = await ix.GetRandomItemAsync(new IX.SearchItemsParameters { Game = IXGame.Trackmania });

        await Assert.That(fake.Request.Url).IsEqualTo("https://item.exchange/itemsearch/search?api=on&format=json&random=1&game=2");
        await Assert.That(item!.Id).IsEqualTo(40503);
    }

    [Test]
    public async Task SearchItemSetsAsync_BuildsQuery()
    {
        var fake = FakeHttpMessageHandler.Json("""{"results":[{"ID":11159,"Name":"StadiumCircuitSlopeToBase","Items":null}],"totalItemCount":857}""");
        using var ix = CreateIx(fake);

        var result = await ix.SearchItemSetsAsync(new IX.SearchItemSetsParameters { SetName = ["Rocks"], Author = ["xrayjay"], Unreleased = false });

        await Assert.That(fake.Request.Url).IsEqualTo("https://item.exchange/setsearch/search?api=on&format=json&setname=Rocks&author=xrayjay&unreleased=0");
        await Assert.That(result.Results.Single().Items).IsNull();
    }

    [Test]
    public async Task Urls_PointToDownloadsAndImages()
    {
        using var ix = new IX();

        await Assert.That(ix.GetItemDownloadUrl(256)).IsEqualTo("https://item.exchange/item/download/256");
        await Assert.That(ix.GetItemSetDownloadUrl(41)).IsEqualTo("https://item.exchange/set/download/41");
        await Assert.That(ix.GetBasketDownloadUrl(1814)).IsEqualTo("https://item.exchange/basket/download/1814");
        await Assert.That(ix.GetBasketDownloadUrl(1814, ignoreDirectories: true)).IsEqualTo("https://item.exchange/basket/download/1814?IgnDir=true");
        await Assert.That(ix.GetItemIconUrl(256)).IsEqualTo("https://item.exchange/item/icon/256");
        await Assert.That(ix.GetItemThumbnailUrl(256)).IsEqualTo("https://item.exchange/item/thumbnail/256");
        await Assert.That(ix.GetItemSetImageUrl(41, 2)).IsEqualTo("https://item.exchange/set/image/41/2");
    }

    [Test]
    [Arguments(0)]
    [Arguments(6)]
    public async Task GetItemSetImageUrl_PositionOutsideRange_Throws(int position)
    {
        using var ix = new IX();

        await Assert.That(() => ix.GetItemSetImageUrl(41, position)).Throws<ArgumentOutOfRangeException>();
    }

    private static IX CreateIx(FakeHttpMessageHandler fake) => new(fake.CreateClient());
}
