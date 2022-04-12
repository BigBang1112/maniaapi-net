using ManiaAPI.Base;
using ManiaAPI.Base.Exceptions;
using System.Net.Http.Json;
using System.Text.Json;

namespace ManiaAPI.Base.Extensions;

public static class HttpResponseMessageExtensions
{
    /// <exception cref="ApiRequestException"></exception>
    public static async ValueTask<HttpResponseMessage> EnsureSuccessStatusCodeAsync(this HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        var message = default(string);

        if (response.Content.Headers.ContentLength > 0)
        {
            try
            {
                var dictionaryResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(JsonAPI.JsonSerializerOptions, cancellationToken);

                if (dictionaryResponse is not null && dictionaryResponse.TryGetValue("message", out object? messageObject))
                {
                    message = messageObject.ToString();
                }
            }
            catch
            {
                // All non-json responses
            }
        }

        throw new ApiRequestException(message ?? response.ReasonPhrase, response.StatusCode);
    }
}
