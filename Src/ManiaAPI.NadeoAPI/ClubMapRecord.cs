using System.Text.Json.Serialization;
using TmEssentials;
using TmEssentials.Converters;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubMapRecord(string GroupUid,
                                    string MapUid,
                                    [property: JsonConverter(typeof(JsonTimeInt32Converter))] TimeInt32 Score,
                                    int ClubId,
                                    int Position);
