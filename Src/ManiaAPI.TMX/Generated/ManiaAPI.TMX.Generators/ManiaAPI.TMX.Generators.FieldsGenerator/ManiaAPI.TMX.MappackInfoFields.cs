using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct MappackInfoFields
{
    public bool MappackId { get; init; }
    public bool MapStatus { get; init; }
    public bool MapPosition { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly MappackInfoFields All = new()
    {
        MappackId = true,
        MapStatus = true,
        MapPosition = true,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (MappackId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackInfo.MappackId));
            first = false;
        }

        if (MapStatus)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackInfo.MapStatus));
            first = false;
        }

        if (MapPosition)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackInfo.MapPosition));
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

public partial record struct MappackInfo
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}
