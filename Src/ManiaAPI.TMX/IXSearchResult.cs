using System.Collections.Immutable;

namespace ManiaAPI.TMX;

/// <summary>
/// A page of Item Exchange search results.
/// </summary>
/// <typeparam name="T"><see cref="IXItem"/> or <see cref="IXItemSet"/>.</typeparam>
public sealed record IXSearchResult<T>
{
    public ImmutableList<T> Results { get; init; } = [];

    /// <summary>
    /// Number of results of the whole search, across all pages.
    /// </summary>
    public int TotalItemCount { get; init; }
}
