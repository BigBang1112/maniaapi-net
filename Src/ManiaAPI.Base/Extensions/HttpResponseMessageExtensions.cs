using ManiaAPI.Base;
using ManiaAPI.Base.Exceptions;
using System.Net.Http.Json;

namespace ManiaAPI.Base.Extensions;

public static class HttpResponseMessageExtensions
{
    public static async ValueTask<HttpResponseMessage> EnsureSuccessStatusCodeAsync<TErrorReponse>(this HttpResponseMessage response, CancellationToken cancellationToken = default)
        where TErrorReponse : ErrorResponse
    {
        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        var errorResponse = default(TErrorReponse);

        if (response.Content.Headers.ContentLength > 0)
        {
            errorResponse = await response.Content.ReadFromJsonAsync<TErrorReponse>(JsonAPI.JsonSerializerOptions, cancellationToken);
        }

        throw new ApiRequestException(response.ReasonPhrase, response.StatusCode, errorResponse);
    }

    public static async ValueTask<HttpResponseMessage> EnsureSuccessStatusCodeAsync(this HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        return await response.EnsureSuccessStatusCodeAsync<ErrorResponse>(cancellationToken);
    }
}
