using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct MapFeatureFields
{
    public bool Comment { get; init; }
    public bool Pinned { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly MapFeatureFields All = new()
    {
        Comment = true,
        Pinned = true,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (Comment)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapFeature.Comment));
            first = false;
        }

        if (Pinned)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapFeature.Pinned));
            first = false;
        }

        if (AdditionalFields is not null)
        {
            foreach (var additionalField in AdditionalFields)
            {
                if (!first) sb.Append("%2C");
                sb.Append(additionalField);
                first = false;
            }
        }

        return !first;
    }
}

public partial record struct MapFeature
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}
