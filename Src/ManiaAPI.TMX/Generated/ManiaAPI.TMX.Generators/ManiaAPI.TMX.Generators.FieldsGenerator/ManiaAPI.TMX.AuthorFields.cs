using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct AuthorFields
{
    public global::ManiaAPI.TMX.UserFields User { get; init; }
    public bool Role { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly AuthorFields All = new()
    {
        User = global::ManiaAPI.TMX.UserFields.All,
        Role = true,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (User.UserId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.Author.User));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.UserId));
            first = false;
        }

        if (User.Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.Author.User));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.Name));
            first = false;
        }

        if (Role)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.Author.Role));
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

public partial record Author
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}
