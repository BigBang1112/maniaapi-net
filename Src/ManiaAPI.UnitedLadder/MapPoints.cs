using System.Text.Json.Serialization;

namespace ManiaAPI.UnitedLadder;

public sealed record MapPoints(string Uid, string Name,
                               [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] double Points,
                               string Envi);
