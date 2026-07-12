using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct UserItemFields
{
    public bool UserId { get; init; }
    public bool Name { get; init; }
    public bool RegisteredAt { get; init; }
    public bool CustomTitle { get; init; }
    public bool UserComments { get; init; }
    public bool Tracks { get; init; }
    public bool TrackPacks { get; init; }
    public bool ForumPosts { get; init; }
    public bool ForumThreads { get; init; }
    public bool VideosCreated { get; init; }
    public bool VideosPosted { get; init; }
    public bool TrackCommentsReceived { get; init; }
    public bool TrackCommentsGiven { get; init; }
    public bool TrackAwardsReceived { get; init; }
    public bool TrackAwardsGiven { get; init; }
    public bool IsSupporter { get; init; }
    public bool IsModerator { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly UserItemFields All = new()
    {
        UserId = true,
        Name = true,
        RegisteredAt = true,
        CustomTitle = true,
        UserComments = true,
        Tracks = true,
        TrackPacks = true,
        ForumPosts = true,
        ForumThreads = true,
        VideosCreated = true,
        VideosPosted = true,
        TrackCommentsReceived = true,
        TrackCommentsGiven = true,
        TrackAwardsReceived = true,
        TrackAwardsGiven = true,
        IsSupporter = true,
        IsModerator = true,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (UserId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItem.UserId));
            first = false;
        }

        if (Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItem.Name));
            first = false;
        }

        if (RegisteredAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItem.RegisteredAt));
            first = false;
        }

        if (CustomTitle)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItem.CustomTitle));
            first = false;
        }

        if (UserComments)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItem.UserComments));
            first = false;
        }

        if (Tracks)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItem.Tracks));
            first = false;
        }

        if (TrackPacks)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItem.TrackPacks));
            first = false;
        }

        if (ForumPosts)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItem.ForumPosts));
            first = false;
        }

        if (ForumThreads)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItem.ForumThreads));
            first = false;
        }

        if (VideosCreated)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItem.VideosCreated));
            first = false;
        }

        if (VideosPosted)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItem.VideosPosted));
            first = false;
        }

        if (TrackCommentsReceived)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItem.TrackCommentsReceived));
            first = false;
        }

        if (TrackCommentsGiven)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItem.TrackCommentsGiven));
            first = false;
        }

        if (TrackAwardsReceived)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItem.TrackAwardsReceived));
            first = false;
        }

        if (TrackAwardsGiven)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItem.TrackAwardsGiven));
            first = false;
        }

        if (IsSupporter)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItem.IsSupporter));
            first = false;
        }

        if (IsModerator)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.UserItem.IsModerator));
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

public partial record UserItem
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}
