namespace ManiaAPI.MCP.Catalog;

/// <summary>
/// Hand-written discovery metadata that generation can't infer: search synonyms, related operations,
/// usage guidance, and descriptions of common parameters. Account requirements live in <c>Overrides.accounts</c>.
/// TMNF operations share the TMUF entries, since both use the same master server interface.
/// </summary>
public static class OperationOverrides
{
    /// <summary>Search synonyms, added to every operation whose ID contains the key.</summary>
    private static readonly (string Contains, string Tags)[] TagRules =
    [
        ("track_of_the_day", "TOTD track of the day daily map"),
        ("cup_of_the_day", "COTD cup of the day daily cup competition"),
        ("cup_of_the_week", "COTW cup of the week weekly cup"),
        // PB/WR mean map records, not competition or challenge rankings.
        ("top_leaderboard", "WR world record top best times"),
        ("surrounding_records", "PB personal best nearby times"),
        ("leaderboard_position", "PB personal best rank position time"),
        ("map_record", "PB WR personal best world record times"),
        ("account_records", "PB personal best times"),
        ("player_records", "PB personal best times"),
        ("leaderboard", "ranking top"),
        ("leader_board", "WR world record ranking top"),
        ("medal", "author gold silver bronze medal times"),
        ("seasonal_campaign", "official campaign season quarterly"),
        ("weekly_short", "weekly shorts"),
        ("weekly_grand", "weekly grands"),
        ("matchmaking", "ranked MM rank division"),
        ("super_royal", "royal"),
        ("trophy", "trophies trophy points"),
        ("zone", "region country continent location flag"),
        ("display_names", "player name nickname username lookup"),
        ("account_ids", "player name nickname username resolve lookup"),
        ("favorite", "favourite liked saved"),
        ("skin", "car skin livery"),
        ("map_info", "map details author medal times thumbnail download"),
        ("ladder", "ladder points LP ranking"),
        ("scores", "rankings leaderboard scores"),
        ("player_infos", "player profile nickname"),
        ("check_login", "validate login exists"),
        ("title", "title pack"),
        ("search_maps", "find community maps tracks"),
        ("search_tracks", "find community tracks maps"),
        ("replay", "replays ghost run"),
        ("gbx", "map file download"),
        ("thumbnail", "image preview picture"),
        ("competition", "cup tournament event"),
        ("club", "club members"),
    ];

    private static readonly Dictionary<string, string[]> RelatedOperations = new(StringComparer.Ordinal)
    {
        ["nadeo.live.top_leaderboard"] = ["nadeo.live.surrounding_records", "nadeo.live.leaderboard_position_by_time", "nadeo.live.club_top_leaderboard", "nadeo.live.map_info", "trackmania.api.display_names"],
        ["nadeo.live.surrounding_records"] = ["nadeo.live.top_leaderboard", "nadeo.live.leaderboard_position_by_time", "trackmania.api.display_names"],
        ["nadeo.live.leaderboard_position_by_time"] = ["nadeo.live.leaderboard_positions_by_time", "nadeo.live.top_leaderboard", "nadeo.live.surrounding_records"],
        ["nadeo.live.club_top_leaderboard"] = ["nadeo.live.top_leaderboard", "nadeo.live.club", "trackmania.api.display_names"],
        ["nadeo.live.map_info"] = ["nadeo.live.map_infos", "nadeo.core.map_info", "nadeo.live.top_leaderboard", "mx.search_maps"],
        ["nadeo.core.map_info"] = ["nadeo.core.map_infos", "nadeo.live.map_info", "nadeo.core.map_records"],
        ["nadeo.core.map_records"] = ["nadeo.core.map_record_by_id", "nadeo.core.account_records", "nadeo.live.top_leaderboard"],
        ["nadeo.live.track_of_the_days"] = ["nadeo.live.track_of_the_day_info", "nadeo.meet.current_cup_of_the_day", "nadeo.meet.cups_of_the_day", "nadeo.live.map_infos"],
        ["nadeo.live.track_of_the_day_info"] = ["nadeo.live.track_of_the_days", "nadeo.live.map_info", "nadeo.live.top_leaderboard"],
        ["nadeo.meet.current_cup_of_the_day"] = ["nadeo.meet.cups_of_the_day", "nadeo.meet.competition_rounds", "nadeo.meet.competition_leaderboard", "nadeo.live.track_of_the_days"],
        ["nadeo.meet.cups_of_the_day"] = ["nadeo.meet.current_cup_of_the_day", "nadeo.meet.competition_leaderboard", "nadeo.meet.competition_rounds"],
        ["nadeo.live.seasonal_campaigns"] = ["nadeo.live.campaign_leaderboard", "nadeo.live.map_infos", "nadeo.live.top_leaderboard", "nadeo.live.weekly_short_campaigns"],
        ["nadeo.live.weekly_short_campaigns"] = ["nadeo.live.weekly_grand_campaigns", "nadeo.live.seasonal_campaigns", "nadeo.live.map_infos"],
        ["nadeo.live.weekly_grand_campaigns"] = ["nadeo.live.weekly_short_campaigns", "nadeo.live.seasonal_campaigns", "nadeo.live.map_infos"],
        ["nadeo.live.club"] = ["nadeo.live.club_members", "nadeo.live.club_activities", "nadeo.live.club_campaigns_by_club", "nadeo.live.clubs"],
        ["nadeo.live.clubs"] = ["nadeo.live.club", "nadeo.live.club_members"],
        ["nadeo.core.player_zone"] = ["nadeo.core.player_zones", "nadeo.core.zones", "trackmania.api.account_ids"],
        ["nadeo.core.player_zones"] = ["nadeo.core.player_zone", "nadeo.core.zones"],
        ["nadeo.core.zones"] = ["nadeo.core.player_zone", "nadeo.live.top_leaderboard"],
        ["nadeo.core.player_trophy_summary"] = ["nadeo.core.player_trophy_history", "nadeo.live.player_trophy_rankings"],
        ["nadeo.meet.matchmaking_summary"] = ["nadeo.meet.matchmaking_rankings", "nadeo.meet.matchmaking_player_ranks", "nadeo.meet.matchmaking_divisions"],
        ["nadeo.meet.matchmaking_player_ranks"] = ["nadeo.meet.matchmaking_rankings", "nadeo.meet.matchmaking_divisions", "trackmania.api.account_ids"],
        ["trackmania.api.account_ids"] = ["trackmania.api.display_names", "nadeo.core.player_zone"],
        ["trackmania.api.display_names"] = ["trackmania.api.account_ids"],
        ["mx.search_maps"] = ["mx.map_gbx_header", "mx.map_gbx_url", "mx.search_replays", "mx.search_users"],
        ["mx.search_mappacks"] = ["mx.search_maps", "mx.mappack_thumbnail_url"],
        ["tmx.search_tracks"] = ["tmx.track_gbx_header", "tmx.track_gbx_url", "tmx.replays", "tmx.search_users"],
        ["tmx.search_trackpacks"] = ["tmx.search_tracks", "tmx.search_users"],
        ["masterserver.tmuf.player_infos"] = ["masterserver.tmuf.check_login", "masterserver.tmuf.player_achievements", "masterserver.tmuf.ladder_player_rankings"],
        ["masterserver.tmuf.ladder_player_rankings"] = ["masterserver.tmuf.ladder_league_rankings", "masterserver.tmuf.leagues", "masterserver.tmuf.player_infos"],
        ["masterserver.mp4.player_infos"] = ["masterserver.mp4.check_login", "masterserver.mp4.campaign_leader_board"],
        ["masterserver.mp4.campaign_leader_board"] = ["masterserver.mp4.map_leader_board", "masterserver.mp4.campaign_leader_board_summaries", "maniaplanet.web.title_by_uid"],
        ["maniaplanet.web.title_by_uid"] = ["maniaplanet.web.title_scripts", "maniaplanet.ingame.title_by_uid", "maniaplanet.web.search_titles"],
    };

    private static readonly Dictionary<string, string> Guidance = new(StringComparer.Ordinal)
    {
        ["nadeo.live.top_leaderboard"] = "Use for the best times on a map, worldwide or in one zone. Use surrounding_records for records near a given time and leaderboard_position_by_time for the rank a time would reach. Results contain account IDs; resolve names with trackmania.api.display_names.",
        ["nadeo.live.surrounding_records"] = "Use to see the records just above and below a given score on a map.",
        ["nadeo.live.leaderboard_position_by_time"] = "Use to find which rank a given time would reach on a map.",
        ["nadeo.live.track_of_the_days"] = "Use for the Track of the Day (TOTD) maps by month; length and offset page through months.",
        ["nadeo.meet.current_cup_of_the_day"] = "Use for the current Cup of the Day (COTD); the result is empty when there is none.",
        ["nadeo.live.seasonal_campaigns"] = "Use for the official seasonal campaigns and their map UIDs.",
        ["trackmania.api.account_ids"] = "Use to turn display names into account IDs, which NadeoAPI operations take. At most 50 names per request.",
        ["trackmania.api.display_names"] = "Use to turn account IDs from leaderboards or clubs into display names. At most 50 IDs per request.",
        ["nadeo.core.player_zone"] = "Use for a player's zone (country or region) by account ID.",
        ["mx.search_maps"] = "Use to find community maps on Mania Exchange by name, author, or IDs. Then use mx.map_gbx_header for map details or mx.map_gbx_url for the file.",
        ["tmx.search_tracks"] = "Use to find community tracks on TrackMania Exchange by name, author, or IDs. Then use tmx.track_gbx_header for track details or tmx.track_gbx_url for the file.",
        ["masterserver.tmuf.player_infos"] = "Use with the player's login, not their display nickname.",
        ["masterserver.mp4.player_infos"] = "Use with the player's login, not their display nickname.",
    };

    private static readonly Dictionary<string, string> Parameters = new(StringComparer.Ordinal)
    {
        ["mapUid"] = "Map UID, the map's string identifier (not the mapId GUID).",
        ["mapUids"] = "Map UIDs, the maps' string identifiers (not mapId GUIDs).",
        ["mapId"] = "Map ID (GUID), not the map UID.",
        ["mapIds"] = "Map IDs (GUIDs), not map UIDs.",
        ["accountId"] = "Account ID (GUID). Resolve a display name with trackmania.api.account_ids.",
        ["accountIds"] = "Account IDs (GUIDs). Resolve display names with trackmania.api.account_ids.",
        ["groupUid"] = "Leaderboard group UID: \"Personal_Best\" for the map's all-time leaderboard, or a campaign's season UID.",
        ["zoneId"] = "Zone ID (GUID) from nadeo.core.zones.",
        ["clubId"] = "Club ID.",
        ["length"] = "Number of items to return.",
        ["offset"] = "Number of items to skip, for paging.",
        ["login"] = "Player login (the account name, not the display nickname).",
        ["logins"] = "Player logins (account names, not display nicknames).",
        ["titleId"] = "ManiaPlanet title ID, for example TMStadium@nadeo.",
        ["displayNames"] = "Display names to resolve; at most 50 per request.",
    };

    public static string Tags(string id)
    {
        return string.Join(" ", TagRules.Where(rule => id.Contains(rule.Contains, StringComparison.Ordinal)).Select(rule => rule.Tags));
    }

    public static string[] Related(string id)
    {
        if (RelatedOperations.TryGetValue(id, out var related))
        {
            return related;
        }

        return RelatedOperations.TryGetValue(Shared(id), out related)
            ? related.Select(x => x.Replace("masterserver.tmuf.", "masterserver.tmnf.", StringComparison.Ordinal)).ToArray()
            : [];
    }

    public static string? WhenToUse(string id)
    {
        if (Guidance.TryGetValue(id, out var guidance) || Guidance.TryGetValue(Shared(id), out guidance))
        {
            return guidance;
        }

        // Exchange *_response operations return the file itself; the matching _gbx operation summarizes it.
        if (!id.EndsWith("_response", StringComparison.Ordinal))
        {
            return null;
        }

        var summary = id[..^"_response".Length];

        return summary.EndsWith("_gbx", StringComparison.Ordinal)
            ? $"Downloads the file as a maniaapi:// resource. Use {summary} for a summary of its header without the file."
            : "Downloads the file as a maniaapi:// resource.";
    }

    public static string? ParameterDescription(string name)
    {
        return Parameters.GetValueOrDefault(name);
    }

    private static string Shared(string id)
    {
        return id.StartsWith("masterserver.tmnf.", StringComparison.Ordinal)
            ? "masterserver.tmuf." + id["masterserver.tmnf.".Length..]
            : id;
    }
}
