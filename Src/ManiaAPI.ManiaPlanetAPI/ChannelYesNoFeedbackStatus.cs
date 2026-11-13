namespace ManiaAPI.ManiaPlanetAPI;

/// <summary>
/// Totals of yes/no votes on channel maps.
/// </summary>
/// <param name="Maps">Number of maps with votes.</param>
/// <param name="VotesYes">Number of yes votes.</param>
/// <param name="VotesNo">Number of no votes.</param>
/// <param name="Votes">Number of votes.</param>
public sealed record ChannelYesNoFeedbackStatus(int Maps, int VotesYes, int VotesNo, int Votes);
