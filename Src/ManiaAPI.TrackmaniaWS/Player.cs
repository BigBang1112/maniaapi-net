using ManiaAPI.TrackmaniaWS.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaWS;

public sealed record Player([property: JsonConverter(typeof(StringInt32Converter))] int Id,
                            string Login,
                            string Nickname,
                            [property: JsonConverter(typeof(StringBooleanConverter))] bool United,
                            string Path,
                            [property: JsonConverter(typeof(StringInt32Converter))] int IdZone);
