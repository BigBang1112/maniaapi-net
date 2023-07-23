﻿using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record Campaign(int Id,
                              Guid SeasonUid,
                              string Name,
                              string Color,
                              int UseCase,
                              int ClubId,
                              Guid LeaderboardGroupUid,
                              [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset PublicationTimestamp,
                              [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset StartTimestamp,
                              [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset EndTimestamp,
                              [property: JsonConverter(typeof(NullableDateTimeOffsetUnixConverter))] DateTimeOffset? RankingSentTimestamp,
                              [property: JsonConverter(typeof(NullableIntConverter))] int? Year,
                              [property: JsonConverter(typeof(NullableIntConverter))] int? Week,
                              [property: JsonConverter(typeof(NullableIntConverter))] int? Day,
                              [property: JsonConverter(typeof(NullableIntConverter))] int? MonthYear,
                              [property: JsonConverter(typeof(NullableIntConverter))] int? Month,
                              [property: JsonConverter(typeof(NullableIntConverter))] int? MonthDay,
                              bool Published,
                              CampaignMap[] Playlist,
                              Season[] LatestSeasons,
                              Category[] Categories,
                              Media Media,
                              [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset EditionTimestamp);