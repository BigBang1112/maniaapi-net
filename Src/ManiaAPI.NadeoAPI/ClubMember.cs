using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubMember(Guid AccountId,
                                int ClubId,
                                string Role,
                                [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset CreationTimestamp,
                                bool Vip,
                                bool Moderator,
                                bool HasFeatured,
                                bool Pin,
                                bool UseTag);