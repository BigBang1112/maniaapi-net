using System.Collections.Immutable;

namespace ManiaAPI.UnitedLadder;

public sealed record Player(int Id,
                            string Login,
                            string Nickname,
                            bool United,
                            string Path,
                            int IdZone,
                            bool Legacy,
                            ImmutableList<Tag>? Tags)
{
    /// <summary>Zone identifier, also available through the original <see cref="IdZone"/> property.</summary>
    // A nullable setter prevents older serializers from overwriting IdZone with zero when zoneId is absent.
    public int? ZoneId { get => IdZone; init => IdZone = value ?? IdZone; }

    public SoloData? Solo { get; init; }
    public ImmutableDictionary<string, MultiplayerEnvironmentData>? Multiplayer { get; init; }
    public PlayerMeta? Meta { get; init; }
}
