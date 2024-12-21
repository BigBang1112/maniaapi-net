namespace ManiaAPI.NadeoAPI.Extensions.Gbx;

internal sealed record MapInfoSubmit(int AuthorScore,
                                     int GoldScore,
                                     int SilverScore,
                                     int BronzeScore,
                                     Guid Author,
                                     string CollectionName,
                                     string MapStyle,
                                     string MapType,
                                     string MapUid,
                                     string Name,
                                     bool IsPlayable);
