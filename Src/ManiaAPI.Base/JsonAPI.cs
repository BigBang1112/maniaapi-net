using ManiaAPI.Base.Converters;
using ManiaAPI.Base.Exceptions;
using ManiaAPI.Base.Extensions;
using System.Net.Http.Json;
using System.Text.Json;

namespace ManiaAPI.Base;

public abstract class JsonAPI : IDisposable
{
    public HttpClient Client { get; }

    internal static JsonSerializerOptions JsonSerializerOptions { get; } = new(JsonSerializerDefaults.Web);
    
    internal JsonSerializerOptions JsonSerializerOptionsInObject { get; }

    protected bool AutomaticallyAuthorize { get; }

    static JsonAPI()
    {
        JsonSerializerOptions.Converters.Add(new TimeInt32Converter());
    }

    protected JsonAPI(string baseUrl, bool automaticallyAuthorize)
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };

        AutomaticallyAuthorize = automaticallyAuthorize;

        JsonSerializerOptionsInObject = new(JsonSerializerOptions);
    }

    /// <summary>
    /// Does a general API call that expects a JSON result. If access token was requested but is expired, new one is requested.
    /// </summary>
    /// <typeparam name="T">Type of the response object.</typeparam>
    /// <param name="endpoint">Endpoint to call.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response object asynchronously.</returns>
    /// <exception cref="ApiRequestException">API responded with an error message.</exception>
    protected virtual async Task<T> GetApiAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        using var response = await Client.GetAsync(endpoint, cancellationToken);

        await response.EnsureSuccessStatusCodeAsync(cancellationToken);

#if DEBUG
        if (typeof(T) == typeof(string))
        {
            return (T)(object)await response.Content.ReadAsStringAsync(cancellationToken);
        }
#endif

        return await response.Content.ReadFromJsonAsync<T>(JsonSerializerOptions, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
