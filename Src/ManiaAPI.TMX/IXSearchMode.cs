namespace ManiaAPI.TMX;

/// <summary>
/// Special mode of Item Exchange item and set searches.
/// </summary>
public enum IXSearchMode
{
    Normal = 0,

    /// <summary>
    /// Items or sets uploaded by the user given as the user ID.
    /// </summary>
    User = 1,

    Latest = 2,
    RecentlyLiked = 3,
    BestOfTheWeek = 4,
    BestOfTheMonth = 5,
    AllTimeFavorites = 6,

    /// <summary>
    /// Items or sets downloaded by the user given as the user ID.
    /// </summary>
    UserDownloads = 7,

    /// <summary>
    /// Items or sets liked by the user given as the user ID.
    /// </summary>
    UserLikes = 8,

    SupporterUploads = 10,
    BestScoreOfTheWeek = 12,
    BestScoreOfTheMonth = 13,
    BestScoreOfAllTime = 14
}
