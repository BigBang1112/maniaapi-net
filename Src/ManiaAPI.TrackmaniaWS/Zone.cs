using ManiaAPI.TrackmaniaWS.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaWS;

/// <summary>
/// Zone.
/// </summary>
/// <param name="Id">Zone ID.</param>
/// <param name="Name">Zone name.</param>
/// <param name="Path">Full path, like <c>World|France</c>.</param>
/// <param name="PathIds">IDs of the parent zones, from the world down. Only set when zones are listed.</param>
public sealed record Zone([property: JsonConverter(typeof(StringInt32Converter))] int Id,
                          string Name,
                          string Path,
                          [property: JsonConverter(typeof(StringInt32ListConverter))] ImmutableList<int>? PathIds = null);
