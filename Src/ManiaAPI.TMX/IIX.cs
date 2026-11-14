using System.Collections.Immutable;

namespace ManiaAPI.TMX;

/// <summary>
/// Item Exchange (item.exchange), which hosts custom items, blocks and item sets for ManiaPlanet and Trackmania.
/// </summary>
public interface IIX : IClient
{
    /// <summary>
    /// Gets items by their IDs. Unknown IDs are left out.
    /// </summary>
    /// <param name="itemIds">Item IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ImmutableList<IXItem>> GetItemsAsync(IEnumerable<long> itemIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets item sets with their items by their IDs. Unknown IDs are left out.
    /// </summary>
    /// <param name="setIds">Item set IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ImmutableList<IXItemSet>> GetItemSetsAsync(IEnumerable<long> setIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user's basket of items.
    /// </summary>
    /// <param name="userId">MX user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The basket, or null when the user doesn't exist.</returns>
    Task<IXBasket?> GetBasketAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches items. Results are paged with <see cref="IX.SearchItemsParameters.Page"/>.
    /// </summary>
    Task<IXSearchResult<IXItem>> SearchItemsAsync(IX.SearchItemsParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a random item matching the search parameters, or null when nothing matches.
    /// </summary>
    Task<IXItem?> GetRandomItemAsync(IX.SearchItemsParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches item sets. Results are paged with <see cref="IX.SearchItemSetsParameters.Page"/>; sets in results have no items.
    /// </summary>
    Task<IXSearchResult<IXItemSet>> SearchItemSetsAsync(IX.SearchItemSetsParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a random item set matching the search parameters, or null when nothing matches.
    /// </summary>
    Task<IXItemSet?> GetRandomItemSetAsync(IX.SearchItemSetsParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// URL to download an item: a .Gbx file, or a .zip when the item needs several files.
    /// </summary>
    string GetItemDownloadUrl(long itemId);
    Task<HttpResponseMessage> GetItemDownloadResponseAsync(long itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// URL to download an item set as a .zip.
    /// </summary>
    string GetItemSetDownloadUrl(long setId);
    Task<HttpResponseMessage> GetItemSetDownloadResponseAsync(long setId, CancellationToken cancellationToken = default);

    /// <summary>
    /// URL to download a user's basket of items as a .zip.
    /// </summary>
    /// <param name="userId">MX user ID.</param>
    /// <param name="ignoreDirectories">Put all items at the root of the .zip instead of in their directories.</param>
    string GetBasketDownloadUrl(long userId, bool ignoreDirectories = false);
    Task<HttpResponseMessage> GetBasketDownloadResponseAsync(long userId, bool ignoreDirectories = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// URL of an item's icon as shown ingame. Items without an icon return a placeholder image.
    /// </summary>
    string GetItemIconUrl(long itemId);
    Task<HttpResponseMessage> GetItemIconResponseAsync(long itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// URL of an item's custom thumbnail, see <see cref="IXItem.HasThumbnail"/>.
    /// </summary>
    string GetItemThumbnailUrl(long itemId);
    Task<HttpResponseMessage> GetItemThumbnailResponseAsync(long itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// URL of an item set's image.
    /// </summary>
    /// <param name="setId">Item set ID.</param>
    /// <param name="position">Image position from 1 to 5, at most <see cref="IXItemSet.ImageCount"/>.</param>
    string GetItemSetImageUrl(long setId, int position = 1);
    Task<HttpResponseMessage> GetItemSetImageResponseAsync(long setId, int position = 1, CancellationToken cancellationToken = default);
}
