using System.Text.Json.Nodes;
using ManiaAPI.MCP.Configuration;

namespace ManiaAPI.MCP.Engine;

public static class ResultShaper
{
    private const int MaxNotes = 100;
    private const int MinStringLength = 32;
    private const int MaxReductions = 256;

    // Room for the note a new cut adds, so most results fit after a single reduction.
    private const int NoteAllowance = 160;

    /// <summary>
    /// Projects, limits and size-caps a result into <c>{ data, truncation }</c>. The data is always cut
    /// structurally (trailing array items, then the tail of long strings), so the text and structured forms
    /// stay valid, identical JSON, and every cut is described in <c>truncation</c>.
    /// </summary>
    /// <param name="notes">Notes already produced for this data, such as those of combined featured-tool calls.</param>
    public static JsonObject Shape(JsonNode? value, ResultShape? shape, ShapingOptions options, IEnumerable<string>? notes = null)
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

        var existing = notes?.ToArray() ?? [];
        var cuts = new List<Cut>();
        Limit(data, "$", maxItems, cuts);

        var envelope = new JsonObject { ["data"] = data, ["truncation"] = Notes(existing, cuts) };
        var size = envelope.ToJsonString().Length;

        for (var reductions = 0; size > maxChars; reductions++)
        {
            if (reductions == MaxReductions || !Reduce(envelope["data"], size - maxChars, cuts))
            {
                // Nothing large is left to cut, such as an object with very many small fields.
                return new JsonObject
                {
                    ["data"] = null,
                    ["truncation"] = new JsonArray("Result exceeded maxChars and could not be cut down; request fewer fields, a smaller page, or a larger maxChars.")
                };
            }

            envelope["truncation"] = Notes(existing, cuts);
            size = envelope.ToJsonString().Length;
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

                // Selecting a formatted name also keeps its deformatted sibling.
                if (obj.TryGetPropertyValue(name + FormattedText.PlainSuffix, out var plain) && !result.ContainsKey(name + FormattedText.PlainSuffix))
                {
                    result[name + FormattedText.PlainSuffix] = plain?.DeepClone();
                }
            }
        }

        return result;
    }

    private static void Limit(JsonNode? node, string path, int maxItems, List<Cut> cuts)
    {
        if (node is JsonArray array)
        {
            var removed = array.Count - maxItems;

            while (array.Count > maxItems)
            {
                array.RemoveAt(array.Count - 1);
            }

            if (removed > 0)
            {
                Find(cuts, path).LimitedItems += removed;
            }

            for (var i = 0; i < array.Count; i++)
            {
                Limit(array[i], $"{path}[{i}]", maxItems, cuts);
            }
        }
        else if (node is JsonObject obj)
        {
            foreach (var (key, child) in obj)
            {
                Limit(child, path + "." + key, maxItems, cuts);
            }
        }
    }

    /// <summary>
    /// Cuts down the largest array or string, aiming to free <paramref name="excess"/> characters. Arrays keep
    /// their leading items, so the result stays a prefix the caller can continue with pagination.
    /// </summary>
    private static bool Reduce(JsonNode? data, int excess, List<Cut> cuts)
    {
        (JsonNode Node, string Path, int Size)? largest = null;
        FindLargest(data, "$", ref largest);

        if (largest is not { } found)
        {
            return false;
        }

        var (node, path, _) = found;
        var target = excess + NoteAllowance;

        if (node is JsonArray array)
        {
            var removed = 0;

            for (var freed = 0; array.Count > 1 && freed < target; removed++)
            {
                var last = array.Count - 1;
                freed += (array[last]?.ToJsonString().Length ?? 4) + 1;
                array.RemoveAt(last);
                Forget(cuts, $"{path}[{last}]");
            }

            Find(cuts, path).CutItems += removed;

            return true;
        }

        var text = node.GetValue<string>();
        var keep = Math.Min(text.Length - 16, Math.Max(MinStringLength, text.Length - target));

        if (char.IsHighSurrogate(text[keep - 1]))
        {
            keep--;
        }

        node.ReplaceWith(text[..keep] + "…");

        var cut = Find(cuts, path);
        cut.OriginalLength ??= text.Length;
        cut.KeptLength = keep;

        return true;
    }

    private static void FindLargest(JsonNode? node, string path, ref (JsonNode Node, string Path, int Size)? largest)
    {
        switch (node)
        {
            case JsonArray array:
                if (array.Count > 1)
                {
                    Consider(array, path, array.ToJsonString().Length, ref largest);
                }

                for (var i = 0; i < array.Count; i++)
                {
                    FindLargest(array[i], $"{path}[{i}]", ref largest);
                }

                break;

            case JsonObject obj:
                foreach (var (key, child) in obj)
                {
                    FindLargest(child, path + "." + key, ref largest);
                }

                break;

            case JsonValue value when value.TryGetValue<string>(out var text) && text.Length > MinStringLength * 2:
                Consider(value, path, text.Length, ref largest);
                break;
        }

        static void Consider(JsonNode node, string path, int size, ref (JsonNode Node, string Path, int Size)? largest)
        {
            if (largest is null || size > largest.Value.Size)
            {
                largest = (node, path, size);
            }
        }
    }

    private static Cut Find(List<Cut> cuts, string path)
    {
        var cut = cuts.Find(x => x.Path == path);

        if (cut is null)
        {
            cut = new Cut(path);
            cuts.Add(cut);
        }

        return cut;
    }

    /// <summary>Removes notes about an array item that was itself cut, and everything inside it.</summary>
    private static void Forget(List<Cut> cuts, string path)
    {
        cuts.RemoveAll(x => x.Path == path ||
            x.Path.StartsWith(path + ".", StringComparison.Ordinal) ||
            x.Path.StartsWith(path + "[", StringComparison.Ordinal));
    }

    private static JsonArray Notes(string[] existing, List<Cut> cuts)
    {
        var notes = existing.Concat(cuts.Select(cut => cut.ToString())).ToArray();
        var result = new JsonArray(notes.Take(MaxNotes).Select(note => (JsonNode?)JsonValue.Create(note)).ToArray());

        if (notes.Length > MaxNotes)
        {
            result.Add($"{notes.Length - MaxNotes} more parts of the result were cut.");
        }

        return result;
    }

    private sealed class Cut(string path)
    {
        public string Path { get; } = path;
        public int LimitedItems { get; set; }
        public int CutItems { get; set; }
        public int? OriginalLength { get; set; }
        public int KeptLength { get; set; }

        public override string ToString()
        {
            if (OriginalLength is { } original)
            {
                return $"{Path}: text cut from {original} to {KeptLength} characters to fit maxChars.";
            }

            return CutItems == 0
                ? $"{Path}: {LimitedItems} more items; use the operation's pagination parameters to continue."
                : $"{Path}: {LimitedItems + CutItems} more items ({CutItems} cut to fit maxChars); use the operation's pagination parameters, fewer fields, or a smaller page to continue.";
        }
    }
}
