namespace ManiaAPI.NadeoAPI;

/// <summary>
/// Edits a club room that points to a dedicated server.
/// </summary>
public sealed record ClubRoomFromServerEdition
{
    public string? Name { get; init; }

    /// <summary>
    /// Login of the dedicated server.
    /// </summary>
    public string? Login { get; init; }

    public int? FolderId { get; init; }
}
