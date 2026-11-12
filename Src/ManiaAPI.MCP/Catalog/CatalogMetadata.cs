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
