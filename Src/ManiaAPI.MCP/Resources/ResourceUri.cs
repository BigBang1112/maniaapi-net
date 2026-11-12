using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace ManiaAPI.MCP.Resources;

public static class ResourceUri
{
    public static string Create(string operation, JsonElement arguments)
    {
        var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(arguments.GetRawText()));

        return $"maniaapi://operation/{operation}?arguments={encoded}";
    }

    public static (string Operation, JsonElement Arguments) Parse(string value)
    {
        if (value.Length > 32768 || !Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            uri.Scheme != "maniaapi" || uri.Host != "operation")
        {
            throw new ArgumentException("Expected a maniaapi resource URI returned by this server.");
        }

        var query = QueryHelpers.ParseQuery(uri.Query);

        if (!query.TryGetValue("arguments", out var arguments) || arguments.Count != 1)
        {
            throw new ArgumentException("The resource URI must contain its original arguments.");
        }

        try
        {
            var bytes = WebEncoders.Base64UrlDecode(arguments[0]!);
            using var document = JsonDocument.Parse(bytes);

            return (Uri.UnescapeDataString(uri.AbsolutePath.Trim('/')), document.RootElement.Clone());
        }
        catch (Exception exception) when (exception is FormatException or JsonException)
        {
            throw new ArgumentException("Invalid resource arguments.");
        }
    }
}
