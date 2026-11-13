using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;

namespace ManiaAPI.MCP.Catalog;

public static class CatalogMetadata
{
    public static JsonObject Input(params (string Name, JsonNode Schema, bool Required)[] parameters)
    {
        var properties = new JsonObject();
        var required = new JsonArray();

        foreach (var (name, schema, isRequired) in parameters)
        {
            PrefixReferences(schema, "#/properties/" + name);
            properties[name] = schema;

            if (isRequired)
            {
                required.Add(name);
            }
        }

        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = required,
            ["additionalProperties"] = false
        };
    }

    /// <summary>
    /// Merges object schemas into one flat object schema: the union of their properties, with only the
    /// properties every schema requires kept as required. Clients such as the Anthropic API reject
    /// <c>anyOf</c>/<c>oneOf</c>/<c>allOf</c> at the top level of a tool's input schema, so overloads and
    /// featured-tool modes are exposed this way; the engine still checks each combination against the
    /// original schemas before calling upstream.
    /// </summary>
    public static JsonObject Merge(IEnumerable<JsonObject> schemas)
    {
        var properties = new List<(string Name, List<JsonNode> Variants)>();
        HashSet<string>? required = null;

        foreach (var schema in schemas)
        {
            if (schema["properties"] is JsonObject source)
            {
                foreach (var (name, property) in source)
                {
                    var index = properties.FindIndex(x => x.Name == name);

                    if (index < 0)
                    {
                        properties.Add((name, []));
                        index = properties.Count - 1;
                    }

                    var variants = properties[index].Variants;
                    var shape = Shape(property);
                    var same = variants.FirstOrDefault(variant => Shape(variant) == shape);

                    if (same is null)
                    {
                        variants.Add(property?.DeepClone() ?? new JsonObject());
                    }
                    else if (same is JsonObject kept && property is JsonObject other)
                    {
                        // Overloads can document the same parameter differently: keep the fuller description,
                        // and no default unless every overload agrees on it.
                        if ((other["description"]?.GetValue<string>().Length ?? 0) > (kept["description"]?.GetValue<string>().Length ?? 0))
                        {
                            kept["description"] = other["description"]!.DeepClone();
                        }

                        if (kept["default"]?.ToJsonString() != other["default"]?.ToJsonString())
                        {
                            kept.Remove("default");
                        }
                    }
                }
            }

            var names = (schema["required"] as JsonArray ?? []).Select(x => x!.GetValue<string>());

            if (required is null)
            {
                required = names.ToHashSet(StringComparer.Ordinal);
            }
            else
            {
                required.IntersectWith(names);
            }
        }

        var merged = new JsonObject();

        foreach (var (name, variants) in properties)
        {
            if (variants.Count == 1)
            {
                merged[name] = variants[0];
                continue;
            }

            // Property schemas keep references relative to "#/properties/<name>"; nest them one level deeper.
            for (var i = 0; i < variants.Count; i++)
            {
                RebaseReferences(variants[i], "#/properties/" + name, "#/properties/" + name + "/anyOf/" + i);
            }

            merged[name] = new JsonObject { ["anyOf"] = new JsonArray(variants.ToArray()) };
        }

        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = merged,
            ["required"] = new JsonArray(properties
                .Where(x => required?.Contains(x.Name) == true)
                .Select(x => (JsonNode?)JsonValue.Create(x.Name)).ToArray()),
            ["additionalProperties"] = false
        };
    }

    /// <summary>A property schema's JSON without its annotations, to compare what values it accepts.</summary>
    private static string Shape(JsonNode? property)
    {
        if (property is not JsonObject obj)
        {
            return property?.ToJsonString() ?? "{}";
        }

        var copy = (JsonObject)obj.DeepClone();
        copy.Remove("description");
        copy.Remove("default");

        return copy.ToJsonString();
    }

    /// <summary>
    /// A parameter's JSON Schema with its description (from the XML doc's <c>&lt;param&gt;</c>, or the
    /// override for common parameter names) and its default value.
    /// </summary>
    public static JsonNode Parameter<T>(string name, string? description, bool hasDefault = false, T defaultValue = default!)
    {
        var schema = Schema<T>();

        if (schema is JsonObject obj)
        {
            if ((description ?? OperationOverrides.ParameterDescription(name)) is { } text)
            {
                obj["description"] = text;
            }

            if (hasDefault)
            {
                obj["default"] = JsonSerializer.SerializeToNode(defaultValue,
                    Serialization.CatalogJsonContext.Default.Options.GetTypeInfo<T>());
            }
        }

        return schema;
    }

    private static void RebaseReferences(JsonNode? node, string from, string to)
    {
        if (node is JsonObject obj)
        {
            if (obj["$ref"] is JsonValue reference && reference.TryGetValue<string>(out var value) &&
                (value == from || value.StartsWith(from + "/", StringComparison.Ordinal)))
            {
                obj["$ref"] = to + value[from.Length..];
            }

            foreach (var child in obj.Select(x => x.Value))
            {
                RebaseReferences(child, from, to);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var child in array)
            {
                RebaseReferences(child, from, to);
            }
        }
    }

    public static JsonNode Schema<T>()
    {
        var metadata = Serialization.CatalogJsonContext.Default.Options.GetTypeInfo<T>();

        return JsonSchemaExporter.GetJsonSchemaAsNode(metadata,
            new JsonSchemaExporterOptions { TreatNullObliviousAsNonNullable = true });
    }

    internal static void PrefixReferences(JsonNode? node, string prefix)
    {
        if (node is JsonObject obj)
        {
            if (obj["$ref"] is JsonValue reference && reference.TryGetValue<string>(out var value) && value.StartsWith('#'))
            {
                obj["$ref"] = prefix + value[1..];
            }

            foreach (var child in obj.Select(x => x.Value))
            {
                PrefixReferences(child, prefix);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var child in array)
            {
                PrefixReferences(child, prefix);
            }
        }
    }

    public static T Argument<T>(JsonElement arguments, string name, T fallback = default!)
    {
        if (!arguments.TryGetProperty(name, out var value))
        {
            return fallback;
        }

        var metadata = Serialization.CatalogJsonContext.Default.Options.GetTypeInfo<T>();

        return JsonSerializer.Deserialize(value, metadata)!;
    }

    public static JsonNode? Result<T>(T value)
    {
        var metadata = Serialization.CatalogJsonContext.Default.Options.GetTypeInfo<T>();

        return JsonSerializer.SerializeToNode(value, metadata);
    }
}
