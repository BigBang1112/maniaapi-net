﻿using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI.JsonContexts;

[JsonSerializable(typeof(ImmutableArray<Account>))]
[JsonSerializable(typeof(AuthorizationBody))]
[JsonSerializable(typeof(AuthorizationResponse))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(JwtPayloadNadeoAPI))]
[JsonSerializable(typeof(MapInfoLiveCollection))]
[JsonSerializable(typeof(ImmutableArray<MapRecord>))]
[JsonSerializable(typeof(MedalRecordCollection))]
[JsonSerializable(typeof(ImmutableArray<ManiapubCollection>))]
[JsonSerializable(typeof(TopLeaderboardCollection))]
[JsonSerializable(typeof(UbisoftAuthenticationTicket))]
[JsonSerializable(typeof(TrackOfTheDayCollection))]
[JsonSerializable(typeof(TrackOfTheDayInfo))]
[JsonSerializable(typeof(CampaignCollection))]
[JsonSerializable(typeof(SeasonPlayerRankingCollection))]
[JsonSerializable(typeof(ImmutableArray<PlayerZone>))]
[JsonSerializable(typeof(Dictionary<string, ApiRoute>))]
[JsonSerializable(typeof(ImmutableArray<Zone>))]
[JsonSerializable(typeof(ImmutableArray<PlayerClubTag>))]
[JsonSerializable(typeof(ImmutableArray<MapInfo>))]
[JsonSerializable(typeof(CupOfTheDay))]
[JsonSerializable(typeof(Competition))]
[JsonSerializable(typeof(Challenge))]
[JsonSerializable(typeof(DailyChannelJoin))]
[JsonSerializable(typeof(ClubCampaignCollection))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class NadeoAPIJsonContext : JsonSerializerContext { }
