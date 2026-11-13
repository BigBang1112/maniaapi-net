using System.Collections.Immutable;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ManiaAPI.TMX;

/// <summary>
/// Client of Item Exchange. Its API has no v2 yet, so this wraps the v1 methods the MX API documentation lists as active.
/// </summary>
public class IX : IIX
{
    public const string BaseAddress = "https://item.exchange/";

    /// <summary>
    /// Smallest page size of searches.
    /// </summary>
    public const int MinLimit = 10;

    /// <summary>
    /// Largest page size of searches.
    /// </summary>
    public const int MaxLimit = 200;

    public HttpClient Client { get; }

    public IX(HttpClient client)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));

        var headers = Client.DefaultRequestHeaders;

        const string product = "ManiaAPI.NET";
        const string version = PackageInfo.Version;

        var libraryExists = headers.UserAgent.Any(h => h.Product?.Name == product && h.Product?.Version == version);

        if (!libraryExists)
        {
            headers.UserAgent.Add(new ProductInfoHeaderValue(product, version));
            headers.UserAgent.Add(new ProductInfoHeaderValue("(TMX; Discord=bigbang1112)"));
        }

        Client.BaseAddress = new Uri(BaseAddress);
    }

    public IX() : this(new HttpClient()) { }

    public readonly record struct SearchItemsParameters
    {
        /// <summary>
        /// Page size from 10 to 200 (default: 40).
        /// </summary>
        public int? Limit { get; init; }

        /// <summary>
        /// Page of the results, starting at 1.
        /// </summary>
        public int? Page { get; init; }

        public IXSearchOrder? PrimaryOrder { get; init; }
        public IXSearchOrder? SecondaryOrder { get; init; }

        /// <summary>
        /// Special search mode. User modes need <see cref="UserId"/>.
        /// </summary>
        public IXSearchMode? Mode { get; init; }

        /// <summary>
        /// Partial or full names of items.
        /// </summary>
        public string[]? ItemName { get; init; }

        /// <summary>
        /// Partial or full file names of items.
        /// </summary>
        public string[]? FileName { get; init; }

        /// <summary>
        /// MX user ID of the uploader.
        /// </summary>
        public long? UserId { get; init; }

        /// <summary>
        /// Partial or full MX usernames of the uploader.
        /// </summary>
        public string[]? Uploader { get; init; }

        /// <summary>
        /// Partial or full ingame logins of the item's creator.
        /// </summary>
        public string[]? AuthorLogin { get; init; }

        /// <summary>
        /// Partial MX usernames or ingame logins of the item's creator.
        /// </summary>
        public string[]? AnyAuthor { get; init; }

        public IXItemType? ItemType { get; init; }
        public IXEnvironment[]? Collections { get; init; }

        /// <summary>
        /// ID of the set the items are in.
        /// </summary>
        public long? SetId { get; init; }

        /// <summary>
        /// Minimum length of the uploader's description.
        /// </summary>
        public int? CommentsMinLength { get; init; }

        /// <summary>
        /// Only items with a custom thumbnail.
        /// </summary>
        public bool? CustomThumbnail { get; init; }

        /// <summary>
        /// For blocks, names of the blocks the custom blocks were made from.
        /// </summary>
        public string[]? OriginalBlock { get; init; }

        /// <summary>
        /// For blocks, the map editor navigation of the original block, like <c>1-1-1</c> or <c>3-4-2-1</c>.
        /// </summary>
        public string? Nav { get; init; }

        /// <summary>
        /// Only items that aren't in a set.
        /// </summary>
        public bool? NotInSet { get; init; }

        public IXGame? Game { get; init; }

        /// <summary>
        /// Tag IDs of which items must have one, or all with <see cref="AllTags"/>.
        /// </summary>
        public int[]? Tags { get; init; }

        /// <summary>
        /// Tag IDs items must not have.
        /// </summary>
        public int[]? ExcludedTags { get; init; }

        /// <summary>
        /// Items must have all <see cref="Tags"/> instead of one of them.
        /// </summary>
        public bool? AllTags { get; init; }

        /// <summary>
        /// Only unreleased items instead of released ones.
        /// </summary>
        public bool? Unreleased { get; init; }
    }

    public readonly record struct SearchItemSetsParameters
    {
        /// <summary>
        /// Page size from 10 to 200 (default: 40).
        /// </summary>
        public int? Limit { get; init; }

        /// <summary>
        /// Page of the results, starting at 1.
        /// </summary>
        public int? Page { get; init; }

        public IXSearchOrder? PrimaryOrder { get; init; }
        public IXSearchOrder? SecondaryOrder { get; init; }

        /// <summary>
        /// Special search mode. User modes need <see cref="UserId"/>.
        /// </summary>
        public IXSearchMode? Mode { get; init; }

        /// <summary>
        /// Partial or full names of sets.
        /// </summary>
        public string[]? SetName { get; init; }

        /// <summary>
        /// Partial or full file names of sets.
        /// </summary>
        public string[]? FileName { get; init; }

        /// <summary>
        /// MX user ID of the uploader.
        /// </summary>
        public long? UserId { get; init; }

        /// <summary>
        /// Partial or full MX usernames of the uploader.
        /// </summary>
        public string[]? Author { get; init; }

        public IXEnvironment[]? Collections { get; init; }

        /// <summary>
        /// Minimum length of the uploader's description.
        /// </summary>
        public int? CommentsMinLength { get; init; }

        public IXGame? Game { get; init; }

        /// <summary>
        /// Tag IDs of which sets must have one.
        /// </summary>
        public int[]? Tags { get; init; }

        /// <summary>
        /// Only unreleased sets instead of released ones.
        /// </summary>
        public bool? Unreleased { get; init; }
    }

    public virtual async Task<ImmutableList<IXItem>> GetItemsAsync(IEnumerable<long> itemIds, CancellationToken cancellationToken = default)
    {
        var ids = JoinIds(itemIds);

        if (ids.Length == 0)
        {
            return [];
        }

        return await GetJsonAsync($"api/item/get_item_info/multi/{ids}", TMXJsonContext.Default.ImmutableListIXItem, cancellationToken) ?? [];
    }

    public virtual async Task<ImmutableList<IXItemSet>> GetItemSetsAsync(IEnumerable<long> setIds, CancellationToken cancellationToken = default)
    {
        var ids = JoinIds(setIds);

        if (ids.Length == 0)
        {
            return [];
        }

        return await GetJsonAsync($"api/set/get_set_info/multi/{ids}", TMXJsonContext.Default.ImmutableListIXItemSet, cancellationToken) ?? [];
    }

    public virtual async Task<IXBasket?> GetBasketAsync(long userId, CancellationToken cancellationToken = default)
    {
        using var response = await Client.GetAsync($"api/user/get_basket/{userId}", cancellationToken);

        response.EnsureSuccessStatusCode();

        // An unknown user is redirected to the HTML home page
        if (response.Content.Headers.ContentType?.MediaType != "application/json")
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync(TMXJsonContext.Default.IXBasket, cancellationToken);
    }

    public virtual async Task<IXSearchResult<IXItem>> SearchItemsAsync(SearchItemsParameters parameters, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync(BuildItemSearch(parameters, random: false), TMXJsonContext.Default.IXSearchResultIXItem, cancellationToken)
            ?? throw new JsonException("The response body is null.");
    }

    public virtual async Task<IXItem?> GetRandomItemAsync(SearchItemsParameters parameters, CancellationToken cancellationToken = default)
    {
        var result = await GetJsonAsync(BuildItemSearch(parameters, random: true), TMXJsonContext.Default.IXSearchResultIXItem, cancellationToken);
        return result?.Results.FirstOrDefault();
    }

    public virtual async Task<IXSearchResult<IXItemSet>> SearchItemSetsAsync(SearchItemSetsParameters parameters, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync(BuildItemSetSearch(parameters, random: false), TMXJsonContext.Default.IXSearchResultIXItemSet, cancellationToken)
            ?? throw new JsonException("The response body is null.");
    }

    public virtual async Task<IXItemSet?> GetRandomItemSetAsync(SearchItemSetsParameters parameters, CancellationToken cancellationToken = default)
    {
        var result = await GetJsonAsync(BuildItemSetSearch(parameters, random: true), TMXJsonContext.Default.IXSearchResultIXItemSet, cancellationToken);
        return result?.Results.FirstOrDefault();
    }

    public string GetItemDownloadUrl(long itemId) => $"{Client.BaseAddress}item/download/{itemId}";
    public string GetItemDownloadUrl(IXItem item) => GetItemDownloadUrl(item.Id);

    public virtual async Task<HttpResponseMessage> GetItemDownloadResponseAsync(long itemId, CancellationToken cancellationToken = default)
    {
        return await Client.GetAsync(GetItemDownloadUrl(itemId), cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> GetItemDownloadResponseAsync(IXItem item, CancellationToken cancellationToken = default)
    {
        return await GetItemDownloadResponseAsync(item.Id, cancellationToken);
    }

    public string GetItemSetDownloadUrl(long setId) => $"{Client.BaseAddress}set/download/{setId}";
    public string GetItemSetDownloadUrl(IXItemSet set) => GetItemSetDownloadUrl(set.Id);

    public virtual async Task<HttpResponseMessage> GetItemSetDownloadResponseAsync(long setId, CancellationToken cancellationToken = default)
    {
        return await Client.GetAsync(GetItemSetDownloadUrl(setId), cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> GetItemSetDownloadResponseAsync(IXItemSet set, CancellationToken cancellationToken = default)
    {
        return await GetItemSetDownloadResponseAsync(set.Id, cancellationToken);
    }

    public string GetBasketDownloadUrl(long userId, bool ignoreDirectories = false) => ignoreDirectories
        ? $"{Client.BaseAddress}basket/download/{userId}?IgnDir=true"
        : $"{Client.BaseAddress}basket/download/{userId}";

    public virtual async Task<HttpResponseMessage> GetBasketDownloadResponseAsync(long userId, bool ignoreDirectories = false, CancellationToken cancellationToken = default)
    {
        return await Client.GetAsync(GetBasketDownloadUrl(userId, ignoreDirectories), cancellationToken);
    }

    public string GetItemIconUrl(long itemId) => $"{Client.BaseAddress}item/icon/{itemId}";
    public string GetItemIconUrl(IXItem item) => GetItemIconUrl(item.Id);

    public virtual async Task<HttpResponseMessage> GetItemIconResponseAsync(long itemId, CancellationToken cancellationToken = default)
    {
        return await Client.GetAsync(GetItemIconUrl(itemId), cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> GetItemIconResponseAsync(IXItem item, CancellationToken cancellationToken = default)
    {
        return await GetItemIconResponseAsync(item.Id, cancellationToken);
    }

    public string GetItemThumbnailUrl(long itemId) => $"{Client.BaseAddress}item/thumbnail/{itemId}";
    public string GetItemThumbnailUrl(IXItem item) => GetItemThumbnailUrl(item.Id);

    public virtual async Task<HttpResponseMessage> GetItemThumbnailResponseAsync(long itemId, CancellationToken cancellationToken = default)
    {
        return await Client.GetAsync(GetItemThumbnailUrl(itemId), cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> GetItemThumbnailResponseAsync(IXItem item, CancellationToken cancellationToken = default)
    {
        return await GetItemThumbnailResponseAsync(item.Id, cancellationToken);
    }

    public string GetItemSetImageUrl(long setId, int position = 1)
    {
        // Without a position, or outside of 1 to 5, the API fails with 500
        ArgumentOutOfRangeException.ThrowIfLessThan(position, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(position, 5);

        return $"{Client.BaseAddress}set/image/{setId}/{position}";
    }

    public string GetItemSetImageUrl(IXItemSet set, int position = 1) => GetItemSetImageUrl(set.Id, position);

    public virtual async Task<HttpResponseMessage> GetItemSetImageResponseAsync(long setId, int position = 1, CancellationToken cancellationToken = default)
    {
        return await Client.GetAsync(GetItemSetImageUrl(setId, position), cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> GetItemSetImageResponseAsync(IXItemSet set, int position = 1, CancellationToken cancellationToken = default)
    {
        return await GetItemSetImageResponseAsync(set.Id, position, cancellationToken);
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<T?> GetJsonAsync<T>(string url, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken)
    {
        using var response = await Client.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync(typeInfo, cancellationToken);
    }

    private static string BuildItemSearch(SearchItemsParameters parameters, bool random)
    {
        var query = new SearchQuery("itemsearch/search", parameters.Limit, parameters.Page, random);
        query.Add("priord", parameters.PrimaryOrder);
        query.Add("secord", parameters.SecondaryOrder);
        query.Add("mode", parameters.Mode);
        query.Add("itemname", parameters.ItemName);
        query.Add("filename", parameters.FileName);
        query.Add("userid", parameters.UserId);
        query.Add("uploader", parameters.Uploader);
        query.Add("authorlogin", parameters.AuthorLogin);
        query.Add("anyauthor", parameters.AnyAuthor);
        query.Add("itype", parameters.ItemType);
        query.Add("collections", parameters.Collections);
        query.Add("setid", parameters.SetId);
        query.Add("commentsminlength", parameters.CommentsMinLength);
        query.Add("customthumb", parameters.CustomThumbnail);
        query.Add("originalblock", parameters.OriginalBlock);
        query.Add("nav", parameters.Nav);
        query.Add("setinclusion", parameters.NotInSet);
        query.Add("game", parameters.Game);
        query.Add("tags", parameters.Tags);
        query.Add("etags", parameters.ExcludedTags);
        query.Add("tagsinc", parameters.AllTags);
        query.Add("unreleased", parameters.Unreleased);
        return query.ToString();
    }

    private static string BuildItemSetSearch(SearchItemSetsParameters parameters, bool random)
    {
        var query = new SearchQuery("setsearch/search", parameters.Limit, parameters.Page, random);
        query.Add("priord", parameters.PrimaryOrder);
        query.Add("secord", parameters.SecondaryOrder);
        query.Add("mode", parameters.Mode);
        query.Add("setname", parameters.SetName);
        query.Add("filename", parameters.FileName);
        query.Add("userid", parameters.UserId);
        query.Add("author", parameters.Author);
        query.Add("collections", parameters.Collections);
        query.Add("commentsminlength", parameters.CommentsMinLength);
        query.Add("game", parameters.Game);
        query.Add("tags", parameters.Tags);
        query.Add("unreleased", parameters.Unreleased);
        return query.ToString();
    }

    private static string JoinIds(IEnumerable<long> ids)
    {
        ArgumentNullException.ThrowIfNull(ids);
        return string.Join(',', ids.Distinct().Select(id => id.ToString(CultureInfo.InvariantCulture)));
    }

    /// <summary>
    /// Query of the v1 search pages, which answer JSON with <c>api=on</c>. Multiple values are comma-separated.
    /// </summary>
    private sealed class SearchQuery
    {
        private readonly StringBuilder sb;

        public SearchQuery(string path, int? limit, int? page, bool random)
        {
            sb = new StringBuilder(path).Append("?api=on&format=json");

            if (limit is { } value)
            {
                // Outside of this range, the API answers an HTML error page
                ArgumentOutOfRangeException.ThrowIfLessThan(value, MinLimit, "Limit");
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxLimit, "Limit");
            }

            Add("limit", limit);
            Add("page", page);

            if (random)
            {
                Add("random", 1L);
            }
        }

        public void Add(string name, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                sb.Append('&').Append(name).Append('=').Append(Uri.EscapeDataString(value));
            }
        }

        public void Add(string name, long? value) => Add(name, value?.ToString(CultureInfo.InvariantCulture));
        public void Add(string name, bool? value) => Add(name, value is null ? null : value.Value ? "1" : "0");
        public void Add<TEnum>(string name, TEnum? value) where TEnum : struct, Enum => Add(name, value is null ? null : Convert.ToInt64(value.Value, CultureInfo.InvariantCulture));
        public void Add(string name, string[]? values) => Add(name, values is null ? null : string.Join(',', values));
        public void Add(string name, int[]? values) => Add(name, values is null ? null : string.Join(',', values.Select(v => v.ToString(CultureInfo.InvariantCulture))));
        public void Add<TEnum>(string name, TEnum[]? values) where TEnum : struct, Enum => Add(name, values is null ? null : string.Join(',', values.Select(v => Convert.ToInt64(v, CultureInfo.InvariantCulture))));

        public override string ToString() => sb.ToString();
    }
}
