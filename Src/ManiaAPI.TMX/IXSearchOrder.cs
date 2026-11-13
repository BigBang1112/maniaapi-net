namespace ManiaAPI.TMX;

/// <summary>
/// Order of Item Exchange item and set searches. The default is <see cref="UploadedNewest"/>.
/// </summary>
public enum IXSearchOrder
{
    NameAscending = 0,
    UploaderAscending = 1,
    UploadedNewest = 2,
    UploadedOldest = 3,
    UpdatedNewest = 4,
    UpdatedOldest = 5,
    LikesDescending = 6,
    LikesAscending = 7,
    CommentsDescending = 8,
    CommentsAscending = 9,
    DownloadsDescending = 10,
    DownloadsAscending = 11,
    ScoreDescending = 12,
    ScoreAscending = 13,
    FileSizeDescending = 18,
    FileSizeAscending = 19
}
