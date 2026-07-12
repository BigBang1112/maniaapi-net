using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct TagFields
{
    public bool TagId { get; init; }
    public bool Name { get; init; }
    public bool Color { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly TagFields All = new()
    {
        TagId = true,
        Name = true,
        Color = true,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (TagId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.Tag.TagId));
            first = false;
        }

        if (Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.Tag.Name));
            first = false;
        }

        if (Color)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.Tag.Color));
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

public partial record Tag
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}
