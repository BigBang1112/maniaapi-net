using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct ReplayItemMXFields
{
    public bool ReplayId { get; init; }
    public global::ManiaAPI.TMX.UserFields User { get; init; }
    public bool Map { get; init; }
    public bool ReplayTime { get; init; }
    public bool ReplayPoints { get; init; }
    public bool Respawns { get; init; }
    public bool ReplayAt { get; init; }
    public bool TrackAt { get; init; }
    public bool Position { get; init; }
    public bool Score { get; init; }
    public bool Season { get; init; }
    public bool HasFile { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly ReplayItemMXFields All = new()
    {
        ReplayId = true,
        User = global::ManiaAPI.TMX.UserFields.All,
        Map = true,
        ReplayTime = true,
        ReplayPoints = true,
        Respawns = true,
        ReplayAt = true,
        TrackAt = true,
        Position = true,
        Score = true,
        Season = true,
        HasFile = true,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (ReplayId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItemMX.ReplayId));
            first = false;
        }

        if (User.UserId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItemMX.User));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.UserId));
            first = false;
        }

        if (User.Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItemMX.User));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.Name));
            first = false;
        }

        if (Map)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItemMX.Map));
            first = false;
        }

        if (ReplayTime)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItemMX.ReplayTime));
            first = false;
        }

        if (ReplayPoints)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItemMX.ReplayPoints));
            first = false;
        }

        if (Respawns)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItemMX.Respawns));
            first = false;
        }

        if (ReplayAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItemMX.ReplayAt));
            first = false;
        }

        if (TrackAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItemMX.TrackAt));
            first = false;
        }

        if (Position)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItemMX.Position));
            first = false;
        }

        if (Score)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItemMX.Score));
            first = false;
        }

        if (Season)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItemMX.Season));
            first = false;
        }

        if (HasFile)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.ReplayItemMX.HasFile));
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

public partial record ReplayItemMX
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}
