using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct MapMedalsFields
{
    public bool Author { get; init; }
    public bool Gold { get; init; }
    public bool Silver { get; init; }
    public bool Bronze { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly MapMedalsFields All = new()
    {
        Author = true,
        Gold = true,
        Silver = true,
        Bronze = true,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (Author)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapMedals.Author));
            first = false;
        }

        if (Gold)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapMedals.Gold));
            first = false;
        }

        if (Silver)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapMedals.Silver));
            first = false;
        }

        if (Bronze)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapMedals.Bronze));
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

public partial record struct MapMedals
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}
