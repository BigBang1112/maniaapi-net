using System.Collections.Immutable;

namespace ManiaAPI.TMX;

/// <summary>
/// A user's basket of items on Item Exchange, grouped by set and then ordered by when the items were added.
/// </summary>
public sealed record IXBasket
{
    /// <summary>
    /// MX user ID of the basket owner.
    /// </summary>
    public long BasketUserId { get; init; }

    /// <summary>
    /// MX username of the basket owner.
    /// </summary>
    public string BasketUsername { get; init; } = "";

    /// <summary>
    /// Total file size of the basket's items in KB.
    /// </summary>
    public int BasketFileSize { get; init; }

    public ImmutableList<IXItem> Items { get; init; } = [];
    public DateTimeOffset LastChanged { get; init; }
}
