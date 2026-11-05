namespace ManiaAPI.NadeoAPI.Tests;

public class NadeoAPIErrorTests
{
    private static readonly Guid SkinId = Guid.Parse("8d1f5c2a-3b4e-4f60-9a7b-1c2d3e4f5a6b");

    [Test]
    public async Task GetZonesAsync_ErrorResponse_ThrowsWithErrorDetails()
    {
        var fake = FakeHttpMessageHandler.Json("""
            {
              "code": "C-AA-00-01",
              "correlation_id": "abc",
              "message": "There was a validation error.",
              "info": { "field": "mapIdList" }
            }
            """, HttpStatusCode.BadRequest);
        using var ns = new NadeoServices(fake.CreateClient());

        var ex = await Assert.ThrowsAsync<NadeoAPIResponseException>(() => ns.GetZonesAsync());

        await Assert.That(ex!.Message).IsEqualTo("There was a validation error.");
        await Assert.That(ex.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(ex.Response?.Code).IsEqualTo("C-AA-00-01");
        await Assert.That(ex.Response?.CorrelationId).IsEqualTo("abc");
        await Assert.That(ex.Response?.Info["field"]).IsEqualTo("mapIdList");
    }

    [Test]
    public async Task GetZonesAsync_ErrorWithoutJson_ThrowsWithReasonPhrase()
    {
        var fake = new FakeHttpMessageHandler(_ => BadGatewayHtml());
        using var ns = new NadeoServices(fake.CreateClient());

        var ex = await Assert.ThrowsAsync<NadeoAPIResponseException>(() => ns.GetZonesAsync());

        await Assert.That(ex!.Message).IsEqualTo("Bad Gateway");
        await Assert.That(ex.StatusCode).IsEqualTo(HttpStatusCode.BadGateway);
        await Assert.That(ex.Response).IsNull();
    }

    [Test]
    public async Task GetZonesAsync_NetworkFailure_ThrowsResponseException()
    {
        var fake = new FakeHttpMessageHandler(_ => throw new HttpRequestException("Connection refused"));
        using var ns = new NadeoServices(fake.CreateClient());

        var ex = await Assert.ThrowsAsync<NadeoAPIResponseException>(() => ns.GetZonesAsync());

        await Assert.That(ex!.Message).IsEqualTo("Connection refused");
    }

    [Test]
    public async Task GetSkinInfoAsync_NotFound_ReturnsNull()
    {
        var fake = FakeHttpMessageHandler.Json("""{"code":"","correlation_id":"","message":"Not found","info":{}}""", HttpStatusCode.NotFound);
        using var ns = new NadeoServices(fake.CreateClient());

        var skin = await ns.GetSkinInfoAsync(SkinId);

        await Assert.That(skin).IsNull();
    }

    [Test]
    public async Task GetSkinInfoAsync_OtherError_Throws()
    {
        var fake = FakeHttpMessageHandler.Json("""{"code":"","correlation_id":"","message":"Forbidden","info":{}}""", HttpStatusCode.Forbidden);
        using var ns = new NadeoServices(fake.CreateClient());

        await Assert.ThrowsAsync<NadeoAPIResponseException>(() => ns.GetSkinInfoAsync(SkinId));
    }

    private static HttpResponseMessage BadGatewayHtml()
    {
        var response = FakeResponse.Text("<html>Bad gateway</html>", "text/html", HttpStatusCode.BadGateway);
        response.ReasonPhrase = "Bad Gateway";
        return response;
    }
}
