using System.Collections.Immutable;
using System.Net.Http.Json;

namespace ManiaAPI.NadeoAPI;

public interface INadeoMeetServices : INadeoAPI
{
    Task<CupOfTheDay?> GetCurrentCupOfTheDayAsync(CancellationToken cancellationToken = default);
    Task<CupOfTheDayCollection> GetCupsOfTheDayAsync(CupOfTheDayType type, int length = 10, int offset = 0, CancellationToken cancellationToken = default);

    Task<ImmutableList<Competition>> GetCompetitionsAsync(int length = 10, int offset = 0, CancellationToken cancellationToken = default);
    /// <summary>
    /// </summary>
    /// <param name="competitionId">Either the numerical <see cref="Competition.Id"/> or the <see cref="Competition.LiveId"/>.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Competition> GetCompetitionAsync(string competitionId, CancellationToken cancellationToken = default);
    Task<ImmutableList<CompetitionLeaderboardEntry>> GetCompetitionLeaderboardAsync(string competitionId, int length = 10, int offset = 0, CancellationToken cancellationToken = default);
    Task<ImmutableList<CompetitionParticipant>> GetCompetitionParticipantsAsync(string competitionId, int length = 10, int offset = 0, CancellationToken cancellationToken = default);
    Task<ImmutableList<CompetitionRound>> GetCompetitionRoundsAsync(string competitionId, CancellationToken cancellationToken = default);
    Task<ImmutableList<CompetitionTeam>> GetCompetitionTeamsAsync(string competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    /// <param name="length"></param>
    /// <param name="offset"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ClubCompetitionListItemCollection> GetMyClubCompetitionsAsync(int length = 10, int offset = 0, CancellationToken cancellationToken = default);
    Task<ClubCompetitionDetails> GetClubCompetitionAsync(int clubCompetitionId, CancellationToken cancellationToken = default);

    Task<CompetitionRoundMatchCollection> GetCompetitionRoundMatchesAsync(string roundId, int length = 10, int offset = 0, CancellationToken cancellationToken = default);
    /// <summary>
    /// </summary>
    /// <param name="compMatchId">A competition match ID, as retrieved from <see cref="GetCompetitionRoundMatchesAsync"/>. Not the same as a regular match ID.</param>
    /// <param name="length"></param>
    /// <param name="offset"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CompetitionMatchResults> GetCompetitionMatchResultsAsync(string compMatchId, int length = 10, int offset = 0, CancellationToken cancellationToken = default);
    Task<ImmutableList<CompetitionMatchResults>> GetCompetitionPlayerMatchesAsync(string competitionId, Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Match information is only stored on Nadeo's servers for about a month.
    /// </summary>
    /// <param name="matchId">Either the numerical match ID or its live ID.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MatchInfo> GetMatchAsync(string matchId, CancellationToken cancellationToken = default);
    Task<ImmutableList<MatchParticipant>> GetMatchParticipantsAsync(string matchId, CancellationToken cancellationToken = default);
    Task<ImmutableList<MatchTeamResult>> GetMatchTeamsAsync(string matchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Challenges are separate leaderboard structures that can be part of a competition, for example in the form of a qualifying session.
    /// </summary>
    /// <param name="length"></param>
    /// <param name="offset"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ImmutableList<Challenge>> GetChallengesAsync(int length = 10, int offset = 0, CancellationToken cancellationToken = default);
    Task<Challenge> GetChallengeAsync(string challengeId, CancellationToken cancellationToken = default);
    Task<ChallengeLeaderboard> GetChallengeLeaderboardAsync(string challengeId, int length = 10, int offset = 0, CancellationToken cancellationToken = default);
    Task<ImmutableList<ChallengeMapRecord>> GetChallengeMapRecordsAsync(string challengeId, string mapUid, int length = 10, int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// This request requires authentication through service account. Be careful with this request - if you send heartbeats but don't join and complete a match once it becomes available, the account may be penalized. To stop queueing, use <see cref="CancelMatchmakingAsync"/>.
    /// </summary>
    /// <param name="matchmakingType">See the glossary for a list of available matchmaking types and their IDs.</param>
    /// <param name="code"></param>
    /// <param name="playWith">The account IDs of the players to queue with. Leave empty to queue solo.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MatchmakingHeartbeatStatus> SendMatchmakingHeartbeatAsync(int matchmakingType, string code, IEnumerable<Guid> playWith, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    /// <param name="matchmakingType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CancelMatchmakingAsync(int matchmakingType, CancellationToken cancellationToken = default);
    Task<MatchmakingRankingCollection> GetMatchmakingRankingsAsync(int matchmakingType, int length = 10, int offset = 0, CancellationToken cancellationToken = default);
    Task<MatchmakingRankingCollection> GetMatchmakingPlayerRanksAsync(int matchmakingType, IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<MatchmakingProgressionCollection> GetMatchmakingPlayerProgressionsAsync(int matchmakingType, IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<MatchmakingDivisionCollection> GetMatchmakingDivisionsAsync(int matchmakingType, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    /// <param name="matchmakingType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MatchmakingPlayerStatus> GetMatchmakingPlayerStatusAsync(int matchmakingType, CancellationToken cancellationToken = default);
    Task<MatchmakingSummary> GetMatchmakingSummaryAsync(CancellationToken cancellationToken = default);

    Task<SuperRoyalStatus> GetSuperRoyalStatusAsync(CancellationToken cancellationToken = default);
    Task<SuperRoyalStatistics> GetSuperRoyalStatisticsAsync(CancellationToken cancellationToken = default);
}

public class NadeoMeetServices : NadeoAPI, INadeoMeetServices
{
    public override string Audience => nameof(NadeoLiveServices);
    public override string BaseAddress => "https://meet.trackmania.nadeo.club/api";

    public NadeoMeetServices(HttpClient client, NadeoAPIHandler? handler = null, bool automaticallyAuthorize = true)
        : base(client, handler ?? new NadeoAPIHandler(), automaticallyAuthorize)
    {
    }

    public NadeoMeetServices(bool automaticallyAuthorize = true) : this(new HttpClient(), new NadeoAPIHandler(), automaticallyAuthorize)
    {
    }

    public virtual async Task<CupOfTheDay?> GetCurrentCupOfTheDayAsync(CancellationToken cancellationToken = default)
    {
        return await GetNullableJsonAsync("cup-of-the-day/current", NadeoAPIJsonContext.Default.CupOfTheDay, cancellationToken);
    }

    public virtual async Task<CupOfTheDayCollection> GetCupsOfTheDayAsync(CupOfTheDayType type, int length = 10, int offset = 0, CancellationToken cancellationToken = default)
    {
        var typeStr = type switch
        {
            CupOfTheDayType.COTD => "cotd",
            CupOfTheDayType.COTW => "cotw",
            CupOfTheDayType.GrandRace => "grand_race",
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Not expected cup of the day type value: {type}")
        };

        return await GetJsonAsync($"cups-of-the-day?type={typeStr}&length={length}&offset={offset}", NadeoAPIJsonContext.Default.CupOfTheDayCollection, cancellationToken);
    }

    public virtual async Task<ImmutableList<Competition>> GetCompetitionsAsync(int length = 10, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"competitions?length={length}&offset={offset}", NadeoAPIJsonContext.Default.ImmutableListCompetition, cancellationToken);
    }

    public virtual async Task<Competition> GetCompetitionAsync(string competitionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(competitionId);

        return await GetJsonAsync($"competitions/{competitionId}", NadeoAPIJsonContext.Default.Competition, cancellationToken);
    }

    public virtual async Task<ImmutableList<CompetitionLeaderboardEntry>> GetCompetitionLeaderboardAsync(string competitionId, int length = 10, int offset = 0, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(competitionId);

        return await GetJsonAsync($"competitions/{competitionId}/leaderboard?length={length}&offset={offset}",
            NadeoAPIJsonContext.Default.ImmutableListCompetitionLeaderboardEntry, cancellationToken);
    }

    public virtual async Task<ImmutableList<CompetitionParticipant>> GetCompetitionParticipantsAsync(string competitionId, int length = 10, int offset = 0, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(competitionId);

        return await GetJsonAsync($"competitions/{competitionId}/participants?length={length}&offset={offset}",
            NadeoAPIJsonContext.Default.ImmutableListCompetitionParticipant, cancellationToken);
    }

    public virtual async Task<ImmutableList<CompetitionRound>> GetCompetitionRoundsAsync(string competitionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(competitionId);

        return await GetJsonAsync($"competitions/{competitionId}/rounds", NadeoAPIJsonContext.Default.ImmutableListCompetitionRound, cancellationToken);
    }

    public virtual async Task<ImmutableList<CompetitionTeam>> GetCompetitionTeamsAsync(string competitionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(competitionId);

        return await GetJsonAsync($"competitions/{competitionId}/mode-teams", NadeoAPIJsonContext.Default.ImmutableListCompetitionTeam, cancellationToken);
    }

    public virtual async Task<ClubCompetitionListItemCollection> GetMyClubCompetitionsAsync(int length = 10, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"me/club-competitions?length={length}&offset={offset}",
            NadeoAPIJsonContext.Default.ClubCompetitionListItemCollection, cancellationToken);
    }

    public virtual async Task<ClubCompetitionDetails> GetClubCompetitionAsync(int clubCompetitionId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"club-competitions/{clubCompetitionId}", NadeoAPIJsonContext.Default.ClubCompetitionDetails, cancellationToken);
    }

    public virtual async Task<CompetitionRoundMatchCollection> GetCompetitionRoundMatchesAsync(string roundId, int length = 10, int offset = 0, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(roundId);

        return await GetJsonAsync($"rounds/{roundId}/matches?length={length}&offset={offset}",
            NadeoAPIJsonContext.Default.CompetitionRoundMatchCollection, cancellationToken);
    }

    public virtual async Task<CompetitionMatchResults> GetCompetitionMatchResultsAsync(string compMatchId, int length = 10, int offset = 0, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(compMatchId);

        return await GetJsonAsync($"matches/{compMatchId}/results?length={length}&offset={offset}",
            NadeoAPIJsonContext.Default.CompetitionMatchResults, cancellationToken);
    }

    public virtual async Task<ImmutableList<CompetitionMatchResults>> GetCompetitionPlayerMatchesAsync(string competitionId, Guid accountId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(competitionId);

        return await GetJsonAsync($"competitions/{competitionId}/participants/{accountId}/matches",
            NadeoAPIJsonContext.Default.ImmutableListCompetitionMatchResults, cancellationToken);
    }

    public virtual async Task<MatchInfo> GetMatchAsync(string matchId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(matchId);

        return await GetJsonAsync($"matches/{matchId}", NadeoAPIJsonContext.Default.MatchInfo, cancellationToken);
    }

    public virtual async Task<ImmutableList<MatchParticipant>> GetMatchParticipantsAsync(string matchId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(matchId);

        return await GetJsonAsync($"matches/{matchId}/participants", NadeoAPIJsonContext.Default.ImmutableListMatchParticipant, cancellationToken);
    }

    public virtual async Task<ImmutableList<MatchTeamResult>> GetMatchTeamsAsync(string matchId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(matchId);

        return await GetJsonAsync($"matches/{matchId}/teams", NadeoAPIJsonContext.Default.ImmutableListMatchTeamResult, cancellationToken);
    }

    public virtual async Task<ImmutableList<Challenge>> GetChallengesAsync(int length = 10, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"challenges?length={length}&offset={offset}", NadeoAPIJsonContext.Default.ImmutableListChallenge, cancellationToken);
    }

    public virtual async Task<Challenge> GetChallengeAsync(string challengeId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(challengeId);

        return await GetJsonAsync($"challenges/{challengeId}", NadeoAPIJsonContext.Default.Challenge, cancellationToken);
    }

    public virtual async Task<ChallengeLeaderboard> GetChallengeLeaderboardAsync(string challengeId, int length = 10, int offset = 0, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(challengeId);

        return await GetJsonAsync($"challenges/{challengeId}/leaderboard?length={length}&offset={offset}",
            NadeoAPIJsonContext.Default.ChallengeLeaderboard, cancellationToken);
    }

    public virtual async Task<ImmutableList<ChallengeMapRecord>> GetChallengeMapRecordsAsync(string challengeId, string mapUid, int length = 10, int offset = 0, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(challengeId);
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        return await GetJsonAsync($"challenges/{challengeId}/records/maps/{mapUid}?length={length}&offset={offset}",
            NadeoAPIJsonContext.Default.ImmutableListChallengeMapRecord, cancellationToken);
    }

    public virtual async Task<MatchmakingHeartbeatStatus> SendMatchmakingHeartbeatAsync(int matchmakingType, string code, IEnumerable<Guid> playWith, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(new MatchmakingHeartbeatRequest(code, playWith.ToImmutableList()), NadeoAPIJsonContext.Default.MatchmakingHeartbeatRequest);
        return await PostJsonAsync($"matchmaking/{matchmakingType}/heartbeat", jsonContent, NadeoAPIJsonContext.Default.MatchmakingHeartbeatStatus, cancellationToken);
    }

    public virtual async Task CancelMatchmakingAsync(int matchmakingType, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, $"matchmaking/{matchmakingType}/cancel", cancellationToken: cancellationToken);
    }

    public virtual async Task<MatchmakingRankingCollection> GetMatchmakingRankingsAsync(int matchmakingType, int length = 10, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"matchmaking/{matchmakingType}/leaderboard?length={length}&offset={offset}",
            NadeoAPIJsonContext.Default.MatchmakingRankingCollection, cancellationToken);
    }

    public virtual async Task<MatchmakingRankingCollection> GetMatchmakingPlayerRanksAsync(int matchmakingType, IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        var query = string.Join('&', accountIds.Select(id => $"players[]={id}"));
        return await GetJsonAsync($"matchmaking/{matchmakingType}/leaderboard/players?{query}",
            NadeoAPIJsonContext.Default.MatchmakingRankingCollection, cancellationToken);
    }

    public virtual async Task<MatchmakingProgressionCollection> GetMatchmakingPlayerProgressionsAsync(int matchmakingType, IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        var query = string.Join('&', accountIds.Select(id => $"players[]={id}"));
        return await GetJsonAsync($"matchmaking/{matchmakingType}/progression/players?{query}",
            NadeoAPIJsonContext.Default.MatchmakingProgressionCollection, cancellationToken);
    }

    public virtual async Task<MatchmakingDivisionCollection> GetMatchmakingDivisionsAsync(int matchmakingType, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"matchmaking/{matchmakingType}/division/display-rules", NadeoAPIJsonContext.Default.MatchmakingDivisionCollection, cancellationToken);
    }

    public virtual async Task<MatchmakingPlayerStatus> GetMatchmakingPlayerStatusAsync(int matchmakingType, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"matchmaking/{matchmakingType}/player-status", NadeoAPIJsonContext.Default.MatchmakingPlayerStatus, cancellationToken);
    }

    public virtual async Task<MatchmakingSummary> GetMatchmakingSummaryAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("official/summary", NadeoAPIJsonContext.Default.MatchmakingSummary, cancellationToken);
    }

    public virtual async Task<SuperRoyalStatus> GetSuperRoyalStatusAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("me/super-royal/current", NadeoAPIJsonContext.Default.SuperRoyalStatus, cancellationToken);
    }

    public virtual async Task<SuperRoyalStatistics> GetSuperRoyalStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("me/super-royal/stats", NadeoAPIJsonContext.Default.SuperRoyalStatistics, cancellationToken);
    }
}
