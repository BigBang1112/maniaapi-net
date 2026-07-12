using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct UserFields
{
    public bool UserId { get; init; }
    public bool Name { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly UserFields All = new()
    {
        UserId = true,
        Name = true,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (UserId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.User.UserId));
            first = false;
        }

        if (Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.User.Name));
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

public partial record User
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}
