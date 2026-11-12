using System.Text.Json.Nodes;
using ManiaAPI.MCP.Configuration;

namespace ManiaAPI.MCP.Engine;

public static class ResultShaper
{
    public static JsonObject Shape(JsonNode? value, ResultShape? shape, ShapingOptions options)
    {
        var maxItems = shape?.MaxItems ?? options.MaxItems;
        var maxChars = shape?.MaxChars ?? options.MaxChars;

        if (maxItems < 1 || maxItems > 10000 || maxChars < 256 || maxChars > options.MaxCharsLimit)
        {
            throw new ArgumentException("Shape limits are outside the server's configured bounds.");
        }

        var fields = shape?.Fields;

        if (fields is { Length: > 100 } || fields?.Any(x => string.IsNullOrWhiteSpace(x) || x.Length > 256) == true)
        {
            throw new ArgumentException("fields accepts up to 100 non-empty paths of at most 256 characters.");
        }

        var data = fields is { Length: > 0 }
            ? Project(value, fields.Select(x => x.Split('.')).ToArray())
            : value?.DeepClone();

        var notes = new JsonArray();
        Limit(data, "$", maxItems, notes);

        var envelope = new JsonObject { ["data"] = data, ["truncation"] = notes };

        if (envelope.ToJsonString().Length > maxChars)
        {
            // Never slice JSON text: keep structured and text responses valid and identical.
            envelope["data"] = null;
            envelope["truncation"] = new JsonArray("Result exceeded maxChars; request fewer fields, a smaller page, or a larger maxChars.");
        }

        return envelope;
    }

    private static JsonNode? Project(JsonNode? node, string[][] paths)
    {
        if (paths.Any(p => p.Length == 0))
        {
            return node?.DeepClone();
        }

        if (node is JsonArray array)
        {
            return new JsonArray(array.Select(x => Project(x, paths)).ToArray());
        }

        if (node is not JsonObject obj)
        {
            return null;
        }

        var result = new JsonObject();

        foreach (var group in paths.GroupBy(x => x[0]))
        {
            var name = group.Key.EndsWith("[]", StringComparison.Ordinal) ? group.Key[..^2] : group.Key;

            if (obj.TryGetPropertyValue(name, out var child))
            {
                result[name] = Project(child, group.Select(p => p.Skip(1).ToArray()).ToArray());
            }
        }

        return result;
    }

    private static void Limit(JsonNode? node, string path, int maxItems, JsonArray notes)
    {
        if (node is JsonArray array)
        {
            var removed = array.Count - maxItems;

            while (array.Count > maxItems)
            {
                array.RemoveAt(array.Count - 1);
            }

            if (removed > 0 && notes.Count < 100)
            {
                notes.Add($"{path}: {removed} more items; use the operation's pagination parameters to continue.");
            }

            for (var i = 0; i < array.Count; i++)
            {
                Limit(array[i], $"{path}[{i}]", maxItems, notes);
            }
        }
        else if (node is JsonObject obj)
        {
            foreach (var (key, child) in obj)
            {
                Limit(child, path + "." + key, maxItems, notes);
            }
        }
    }
}
