namespace ManiaAPI.TMX;

/// <summary>
/// An item on Item Exchange.
/// </summary>
public sealed record IXItem
{
    public long Id { get; init; }

    /// <summary>
    /// Name of the item, usually the file name without extensions.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// MX user ID of the uploader.
    /// </summary>
    public long UserId { get; init; }

    /// <summary>
    /// MX username of the uploader.
    /// </summary>
    public string Username { get; init; } = "";

    public string? Description { get; init; }

    /// <summary>
    /// Ingame login of the item's creator. It may be missing or wrong for items made with NadeoImporter.
    /// </summary>
    public string? AuthorLogin { get; init; }

    /// <summary>
    /// For blocks, the block the custom block was made from.
    /// </summary>
    public string? OriginalBlock { get; init; }

    public IXItemType Type { get; init; }
    public string TypeName { get; init; } = "";
    public IXEnvironment Collection { get; init; }
    public string CollectionName { get; init; } = "";
    public IXGame Game { get; init; }
    public string GameName { get; init; } = "";
    public int Downloads { get; init; }

    /// <summary>
    /// Number of maps using the item plus awards on those maps.
    /// </summary>
    public int Score { get; init; }

    public string FileName { get; init; } = "";
    public DateTimeOffset Uploaded { get; init; }
    public DateTimeOffset Updated { get; init; }

    /// <summary>
    /// ID of the set the item was uploaded in, or 0 when it isn't in a set.
    /// </summary>
    public long SetId { get; init; }

    public string? SetName { get; init; }

    /// <summary>
    /// Directory of the item inside its set, without leading and trailing slashes.
    /// </summary>
    public string? Directory { get; init; }

    /// <summary>
    /// Comma-separated indices of the item's files inside its set's .zip. More than one means the item needs several files.
    /// </summary>
    public string? ZipIndex { get; init; }

    public bool Visible { get; init; }

    /// <summary>
    /// Whether the item is hidden from search. The API doesn't always return it.
    /// </summary>
    public bool? Unlisted { get; init; }

    public bool Unreleased { get; init; }
    public int LikeCount { get; init; }
    public int CommentCount { get; init; }

    /// <summary>
    /// File size in KB.
    /// </summary>
    public int FileSize { get; init; }

    /// <summary>
    /// Comma-separated tag IDs.
    /// </summary>
    public string? Tags { get; init; }

    /// <summary>
    /// Whether the item has a custom thumbnail, see <see cref="IIX.GetItemThumbnailUrl(long)"/>.
    /// </summary>
    public bool HasThumbnail { get; init; }
}
