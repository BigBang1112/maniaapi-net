using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct TrackpackItemFields
{
    public bool PackId { get; init; }
    public bool PackName { get; init; }
    public bool Tracks { get; init; }
    public bool PackValue { get; init; }
    public bool AllowsTrackSubmissions { get; init; }
    public bool IsLegacy { get; init; }
    public bool Downloads { get; init; }
    public bool CreatedAt { get; init; }
    public bool UpdatedAt { get; init; }
    public global::ManiaAPI.TMX.UserFields Creator { get; init; }
    public bool Managers { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly TrackpackItemFields All = new()
    {
        PackId = true,
        PackName = true,
        Tracks = true,
        PackValue = true,
        AllowsTrackSubmissions = true,
        IsLegacy = true,
        Downloads = true,
        CreatedAt = true,
        UpdatedAt = true,
        Creator = global::ManiaAPI.TMX.UserFields.All,
        Managers = true,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (PackId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackpackItem.PackId));
            first = false;
        }

        if (PackName)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackpackItem.PackName));
            first = false;
        }

        if (Tracks)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackpackItem.Tracks));
            first = false;
        }

        if (PackValue)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackpackItem.PackValue));
            first = false;
        }

        if (AllowsTrackSubmissions)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackpackItem.AllowsTrackSubmissions));
            first = false;
        }

        if (IsLegacy)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackpackItem.IsLegacy));
            first = false;
        }

        if (Downloads)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackpackItem.Downloads));
            first = false;
        }

        if (CreatedAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackpackItem.CreatedAt));
            first = false;
        }

        if (UpdatedAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackpackItem.UpdatedAt));
            first = false;
        }

        if (Creator.UserId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackpackItem.Creator));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.UserId));
            first = false;
        }

        if (Creator.Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackpackItem.Creator));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.Name));
            first = false;
        }

        if (Managers)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackpackItem.Managers));
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

public partial record TrackpackItem
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}
