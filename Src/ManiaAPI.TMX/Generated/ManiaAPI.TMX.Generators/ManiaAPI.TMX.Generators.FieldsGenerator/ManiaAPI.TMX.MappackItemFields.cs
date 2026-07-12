using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct MappackItemFields
{
    public bool MappackId { get; init; }
    public bool Name { get; init; }
    public bool Description { get; init; }
    public bool Type { get; init; }
    public bool IsRequest { get; init; }
    public bool SubmitLimit { get; init; }
    public bool MappackValue { get; init; }
    public bool MapCount { get; init; }
    public bool IsPublic { get; init; }
    public bool MaplistReleased { get; init; }
    public bool VideoUrl { get; init; }
    public global::ManiaAPI.TMX.UserFields Owner { get; init; }
    public bool Tags { get; init; }
    public bool Managers { get; init; }
    public bool CreatedAt { get; init; }
    public bool UpdatedAt { get; init; }
    public bool RequestEndAt { get; init; }
    public bool MaplistAt { get; init; }
    public bool ActivityAt { get; init; }
    public bool LBEndAt { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly MappackItemFields All = new()
    {
        MappackId = true,
        Name = true,
        Description = true,
        Type = true,
        IsRequest = true,
        SubmitLimit = true,
        MappackValue = true,
        MapCount = true,
        IsPublic = true,
        MaplistReleased = true,
        VideoUrl = true,
        Owner = global::ManiaAPI.TMX.UserFields.All,
        Tags = true,
        Managers = true,
        CreatedAt = true,
        UpdatedAt = true,
        RequestEndAt = true,
        MaplistAt = true,
        ActivityAt = true,
        LBEndAt = true,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (MappackId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.MappackId));
            first = false;
        }

        if (Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.Name));
            first = false;
        }

        if (Description)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.Description));
            first = false;
        }

        if (Type)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.Type));
            first = false;
        }

        if (IsRequest)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.IsRequest));
            first = false;
        }

        if (SubmitLimit)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.SubmitLimit));
            first = false;
        }

        if (MappackValue)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.MappackValue));
            first = false;
        }

        if (MapCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.MapCount));
            first = false;
        }

        if (IsPublic)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.IsPublic));
            first = false;
        }

        if (MaplistReleased)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.MaplistReleased));
            first = false;
        }

        if (VideoUrl)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.VideoUrl));
            first = false;
        }

        if (Owner.UserId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.Owner));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.UserId));
            first = false;
        }

        if (Owner.Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.Owner));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.Name));
            first = false;
        }

        if (Tags)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.Tags));
            first = false;
        }

        if (Managers)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.Managers));
            first = false;
        }

        if (CreatedAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.CreatedAt));
            first = false;
        }

        if (UpdatedAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.UpdatedAt));
            first = false;
        }

        if (RequestEndAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.RequestEndAt));
            first = false;
        }

        if (MaplistAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.MaplistAt));
            first = false;
        }

        if (ActivityAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.ActivityAt));
            first = false;
        }

        if (LBEndAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MappackItem.LBEndAt));
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

public partial record MappackItem
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}
