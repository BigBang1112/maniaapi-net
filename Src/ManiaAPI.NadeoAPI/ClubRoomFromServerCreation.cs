namespace ManiaAPI.NadeoAPI;

/// <summary>
/// Creates a club room that points to an existing dedicated server.
/// </summary>
public sealed record ClubRoomFromServerCreation
{
    public required string Name { get; init; }

    /// <summary>
    /// Login of the dedicated server.
    /// </summary>
    public required string Login { get; init; }

    public int? FolderId { get; init; }
}
