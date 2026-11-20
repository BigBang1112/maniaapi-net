namespace ManiaAPI.Testing;

internal static class UserAgentAssertions
{
    public const string Value = "MyApp/1.0 (contact: app@example.com)";

    public static HttpClient CreateClient()
    {
        var client = FakeHttpMessageHandler.Json("{}").CreateClient("https://example.test/");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("ExistingApp/2.0");
        return client;
    }

    public static async Task VerifyAsync(HttpClient client, bool replacesExisting = false)
    {
        var header = client.DefaultRequestHeaders.UserAgent.ToString();
        await Assert.That(header).StartsWith(Value + " ");
        await Assert.That(header).Contains("ManiaAPI.NET/");
        await Assert.That(client.DefaultRequestHeaders.UserAgent.Count(x => x.Product?.Name == "MyApp")).IsEqualTo(1);
        await Assert.That(client.DefaultRequestHeaders.UserAgent.Count(x => x.Product?.Name == "ManiaAPI.NET")).IsEqualTo(1);
        if (replacesExisting)
        {
            await Assert.That(header).DoesNotContain("ExistingApp/2.0");
        }
    }
}
