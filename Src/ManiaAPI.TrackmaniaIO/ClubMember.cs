using ManiaAPI.TrackmaniaIO.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record ClubMember(Player Player,
                                [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset JoinTime,
                                string Role,
                                [property: JsonPropertyName("vip")] bool VIP);