using ManiaAPI.TrackmaniaWS.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaWS;

/// <summary>
/// Short manialink code.
/// </summary>
/// <param name="Code">Short code.</param>
/// <param name="Url">URL the code points to.</param>
/// <param name="Login">Login of the owner.</param>
/// <param name="CoppersCost">Coppers needed to visit it.</param>
public sealed record Manialink(string Code,
                               string Url,
                               string Login,
                               [property: JsonConverter(typeof(StringInt32Converter))] int CoppersCost);
