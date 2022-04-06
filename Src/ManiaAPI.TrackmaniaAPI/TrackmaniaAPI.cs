namespace ManiaAPI.TrackmaniaAPI;

public class TrackmaniaAPI
{
    public HttpClient Client { get; }

    public TrackmaniaAPI()
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri("https://api.trackmania.com/")
        };

        Client.DefaultRequestHeaders.Add("User-Agent", "ManiaAPI.NET (TrackmaniaAPI) by BigBang1112");
    }

    /// <summary>
    /// Authorizes with the official API using OAuth2 client credentials.
    /// </summary>
    /// <param name="clientId">Client ID.</param>
    /// <param name="clientSecret">Client secret.</param>
    public async Task AuthorizeAsync(string clientId, string clientSecret)
    {
        var values = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "scope", "" }
        };

        using var response = await Client.PostAsync("api/access_token", new FormUrlEncodedContent(values));
    
        var str = await response.Content.ReadAsStringAsync();
    }
}
