using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct UserItemMXFields
{
    public bool Name { get; init; }
    public bool UserId { get; init; }
    public bool RegisteredAt { get; init; }
    public bool CustomTitle { get; init; }
    public bool IsSupporter { get; init; }
    public bool IsModerator { get; init; }
    public bool Bio { get; init; }
    public bool ModBio { get; init; }
    public bool MapCount { get; init; }
    public bool MappackCount { get; init; }
    public bool ThreadCount { get; init; }
    public bool PostCount { get; init; }
    public bool AwardsReceivedCount { get; init; }
    public bool AwardsGivenCount { get; init; }
    public bool CommentsReceivedCount { get; init; }
    public bool CommentsGivenCount { get; init; }
    public bool VideosCreatedCount { get; init; }
    public bool VideosPostedCount { get; init; }
    public bool FavoritesReceivedCount { get; init; }
    public bool ReplayCount { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly UserItemMXFields All = new()
    {
        Name = true,
        UserId = true,
        RegisteredAt = true,
        CustomTitle = true,
        IsSupporter = true,
        IsModerator = true,
        Bio = true,
        ModBio = true,
        MapCount = true,
        MappackCount = true,
        ThreadCount = true,
        PostCount = true,
        AwardsReceivedCount = true,
        AwardsGivenCount = true,
        CommentsReceivedCount = true,
        CommentsGivenCount = true,
        VideosCreatedCount = true,
        VideosPostedCount = true,
        FavoritesReceivedCount = true,
        ReplayCount = true,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.Name));
            first = false;
        }

        if (UserId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.UserId));
            first = false;
        }

        if (RegisteredAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.RegisteredAt));
            first = false;
        }

        if (CustomTitle)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.CustomTitle));
            first = false;
        }

        if (IsSupporter)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.IsSupporter));
            first = false;
        }

        if (IsModerator)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.IsModerator));
            first = false;
        }

        if (Bio)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.Bio));
            first = false;
        }

        if (ModBio)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.ModBio));
            first = false;
        }

        if (MapCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.MapCount));
            first = false;
        }

        if (MappackCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.MappackCount));
            first = false;
        }

        if (ThreadCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.ThreadCount));
            first = false;
        }

        if (PostCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.PostCount));
            first = false;
        }

        if (AwardsReceivedCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.AwardsReceivedCount));
            first = false;
        }

        if (AwardsGivenCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.AwardsGivenCount));
            first = false;
        }

        if (CommentsReceivedCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.CommentsReceivedCount));
            first = false;
        }

        if (CommentsGivenCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.CommentsGivenCount));
            first = false;
        }

        if (VideosCreatedCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.VideosCreatedCount));
            first = false;
        }

        if (VideosPostedCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.VideosPostedCount));
            first = false;
        }

        if (FavoritesReceivedCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.FavoritesReceivedCount));
            first = false;
        }

        if (ReplayCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItemMX.ReplayCount));
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

public partial record UserItemMX
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}
