using System.Text.Json.Nodes;

namespace ManiaAPI.MCP.Catalog;

public static class BinaryResult
{
    public static JsonObject Metadata(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return new JsonObject
        {
            ["status"] = (int)response.StatusCode,
            ["contentType"] = response.Content.Headers.ContentType?.ToString(),
            ["length"] = response.Content.Headers.ContentLength,
            ["url"] = response.RequestMessage?.RequestUri?.ToString()
        };
    }
}
