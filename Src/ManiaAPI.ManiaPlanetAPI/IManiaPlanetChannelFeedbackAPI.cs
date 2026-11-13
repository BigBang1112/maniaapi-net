namespace ManiaAPI.ManiaPlanetAPI;

/// <summary>
/// The ManiaPlanet Channel Feedback API (feedback.prod.live.maniaplanet.com), which holds the votes and ratings players give on channel maps.
/// Only its public route is wrapped; the vote and rating routes need the game servers' credentials.
/// </summary>
public interface IManiaPlanetChannelFeedbackAPI
{
    HttpClient Client { get; }

    /// <summary>
    /// Gets the totals of yes/no votes and 1 to 5 star ratings across all channel maps.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The feedback totals.</returns>
    Task<ChannelFeedbackStatus> GetStatusAsync(CancellationToken cancellationToken = default);
}
