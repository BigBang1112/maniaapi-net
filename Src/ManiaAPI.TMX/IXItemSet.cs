using System.Collections.Immutable;

namespace ManiaAPI.TMX;

/// <summary>
/// A set of items on Item Exchange, uploaded together as a .zip.
/// </summary>
public sealed record IXItemSet
{
    public long Id { get; init; }
    public string Name { get; init; } = "";

    /// <summary>
    /// MX user ID of the uploader.
    /// </summary>
    public long UserId { get; init; }

    /// <summary>
    /// MX username of the uploader.
    /// </summary>
    public string Username { get; init; } = "";

    /// <summary>
    /// Description by the uploader, in Markdown.
    /// </summary>
    public string? Description { get; init; }

    public int Downloads { get; init; }
    public IXEnvironment Collection { get; init; }
    public string CollectionName { get; init; } = "";
    public IXGame Game { get; init; }
    public string GameName { get; init; } = "";

    /// <summary>
    /// Total score of the set's items: maps using them plus awards on those maps.
    /// </summary>
    public int Score { get; init; }

    /// <summary>
    /// File name without the .zip extension.
    /// </summary>
    public string FileName { get; init; } = "";

    public DateTimeOffset Uploaded { get; init; }
    public DateTimeOffset Updated { get; init; }

    /// <summary>
    /// Items in the set. Only returned by <see cref="IIX.GetItemSetsAsync"/>; null in search results.
    /// </summary>
    public ImmutableList<IXItem>? Items { get; init; }

    public bool Visible { get; init; }
    public bool Unreleased { get; init; }

    /// <summary>
    /// Total file size of the set's items in KB.
    /// </summary>
    public int FileSize { get; init; }

    public int LikeCount { get; init; }
    public int CommentCount { get; init; }

    /// <summary>
    /// Comma-separated tag IDs.
    /// </summary>
    public string? Tags { get; init; }

    /// <summary>
    /// Number of images, see <see cref="IIX.GetItemSetImageUrl(long, int)"/>.
    /// </summary>
    public int ImageCount { get; init; }
}
