using ManiaAPI.NadeoAPI.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubBucket(string Type,
                                ImmutableList<ClubBucketItem> BucketItemList,
                                int BucketItemCount,
                                int PopularityLevel,
                                int PopularityValue,
                                [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset CreationTimestamp,
                                Guid CreatorAccountId,
                                Guid LatestEditorAccountId,
                                int Id,
                                int ClubId,
                                string ClubName,
                                string Name,
                                string MediaUrl,
                                string MediaUrlPngLarge,
                                string MediaUrlPngMedium,
                                string MediaUrlPngSmall,
                                string MediaUrlDds,
                                string MediaTheme);