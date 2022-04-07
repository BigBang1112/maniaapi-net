using ManiaAPI.TrackmaniaAPI.Exceptions;
using System.Net.Http.Json;

namespace ManiaAPI.TrackmaniaAPI.Extensions;

public static class HttpResponseMessageExtensions
{
    /// <summary>
    /// Ensures that the status code is a successful one, otherwise throws <see cref="TrackmaniaApiRequestException"/>.
    /// </summary>
    /// <param name="response">Response message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The same response message.</returns>
    /// <exception cref="TrackmaniaApiRequestException">API responded with an error message.</exception>
    public static async ValueTask<HttpResponseMessage> EnsureSuccessStatusCodeAsync(this HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        var errorResponse = default(ErrorResponse);

        if (response.Content.Headers.ContentLength > 0)
        {
            errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>(options: null, cancellationToken);
        }

        throw new TrackmaniaApiRequestException(response.ReasonPhrase, response.StatusCode, errorResponse);
    }
}
