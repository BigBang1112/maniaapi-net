using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record Meta(string? Vanity,
                          string? Comment,
                          bool Nadeo,
                          string? Twitch,
                          [property: JsonPropertyName("youtube")] string? YouTube,
                          string? Mastodon,
                          string? Twitter);