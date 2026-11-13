namespace ManiaAPI.ManiaPlanetAPI;

/// <summary>
/// Totals of 1 to 5 star ratings ("notes") on channel maps.
/// </summary>
/// <param name="Maps">Number of maps with ratings.</param>
/// <param name="Players">Number of players who rated.</param>
/// <param name="Notes1Star">Number of 1 star ratings.</param>
/// <param name="Notes2Star">Number of 2 star ratings.</param>
/// <param name="Notes3Star">Number of 3 star ratings.</param>
/// <param name="Notes4Star">Number of 4 star ratings.</param>
/// <param name="Notes5Star">Number of 5 star ratings.</param>
/// <param name="Notes">Number of ratings.</param>
/// <param name="UniqueNotes">Number of ratings, counting each player once per map.</param>
public sealed record ChannelStarsFeedbackStatus(
    int Maps,
    int Players,
    int Notes1Star,
    int Notes2Star,
    int Notes3Star,
    int Notes4Star,
    int Notes5Star,
    int Notes,
    int UniqueNotes);
