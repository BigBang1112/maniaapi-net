namespace ManiaAPI.ManiaPlanetAPI.Tests;

public class DisposeTests
{
    [Test]
    public async Task ManiaPlanetIngameAPI_Dispose_DisposesClient()
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        var api = new ManiaPlanetIngameAPI(fake.CreateClient(ManiaPlanetIngameAPI.BaseAddress), "MyApp/1.0");

        api.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => api.Client.GetAsync("test"));
    }

    [Test]
    public async Task ManiaPlanetChannelRecordAPI_Dispose_DisposesClient()
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        var api = new ManiaPlanetChannelRecordAPI(fake.CreateClient(ManiaPlanetChannelRecordAPI.BaseAddress), "MyApp/1.0");

        api.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => api.Client.GetAsync("test"));
    }

    [Test]
    public async Task ManiaPlanetChannelFeedbackAPI_Dispose_DisposesClient()
    {
        var fake = FakeHttpMessageHandler.Json("{}");
        var api = new ManiaPlanetChannelFeedbackAPI(fake.CreateClient(ManiaPlanetChannelFeedbackAPI.BaseAddress), "MyApp/1.0");

        api.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => api.Client.GetAsync("test"));
    }
}
