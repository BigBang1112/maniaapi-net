using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct VideoItemFields
{
    public bool VideoId { get; init; }
    public global::ManiaAPI.TMX.UserFields Poster { get; init; }
    public global::ManiaAPI.TMX.UserFields Creator { get; init; }
    public bool Title { get; init; }
    public bool Author { get; init; }
    public bool Link { get; init; }
    public bool LinkId { get; init; }
    public bool Thumbnail { get; init; }
    public bool PostedAt { get; init; }
    public bool PublishedAt { get; init; }
    public bool Length { get; init; }
    public bool MapCount { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly VideoItemFields All = new()
    {
        VideoId = true,
        Poster = global::ManiaAPI.TMX.UserFields.All,
        Creator = global::ManiaAPI.TMX.UserFields.All,
        Title = true,
        Author = true,
        Link = true,
        LinkId = true,
        Thumbnail = true,
        PostedAt = true,
        PublishedAt = true,
        Length = true,
        MapCount = true,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (VideoId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.VideoItem.VideoId));
            first = false;
        }

        if (Poster.UserId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.VideoItem.Poster));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.UserId));
            first = false;
        }

        if (Poster.Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.VideoItem.Poster));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.Name));
            first = false;
        }

        if (Creator.UserId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.VideoItem.Creator));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.UserId));
            first = false;
        }

        if (Creator.Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.VideoItem.Creator));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.Name));
            first = false;
        }

        if (Title)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.VideoItem.Title));
            first = false;
        }

        if (Author)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.VideoItem.Author));
            first = false;
        }

        if (Link)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.VideoItem.Link));
            first = false;
        }

        if (LinkId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.VideoItem.LinkId));
            first = false;
        }

        if (Thumbnail)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.VideoItem.Thumbnail));
            first = false;
        }

        if (PostedAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.VideoItem.PostedAt));
            first = false;
        }

        if (PublishedAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.VideoItem.PublishedAt));
            first = false;
        }

        if (Length)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.VideoItem.Length));
            first = false;
        }

        if (MapCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.VideoItem.MapCount));
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

public partial record VideoItem
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}
