using ManiaAPI.NadeoAPI.Exceptions;
using System.Net.Http.Json;

namespace ManiaAPI.NadeoAPI.Extensions;

public static class HttpResponseMessageExtensions
{
    public static async ValueTask<HttpResponseMessage> EnsureSuccessStatusCodeAsync(this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = default(ErrorResponse);

            if (response.Content.Headers.ContentLength > 0)
            {
                errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            }

            throw new NadeoApiRequestException(response.ReasonPhrase, response.StatusCode, errorResponse);
        }

        return response;
    }
}
