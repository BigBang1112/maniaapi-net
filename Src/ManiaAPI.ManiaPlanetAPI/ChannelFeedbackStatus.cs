using System.Text.Json.Serialization;

namespace ManiaAPI.ManiaPlanetAPI;

/// <summary>
/// Totals of the feedback players gave on channel maps.
/// </summary>
/// <param name="YesNo">Yes/no votes.</param>
/// <param name="Stars">1 to 5 star ratings.</param>
public sealed record ChannelFeedbackStatus(
    [property: JsonPropertyName("feedbackYesNo")] ChannelYesNoFeedbackStatus YesNo,
    [property: JsonPropertyName("feedbackStars")] ChannelStarsFeedbackStatus Stars);
