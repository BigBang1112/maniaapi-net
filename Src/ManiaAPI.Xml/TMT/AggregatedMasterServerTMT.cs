using System.Collections.Immutable;
using TmEssentials;

namespace ManiaAPI.Xml.TMT;

/// <summary>
/// Interface for the <see cref="AggregatedMasterServerTMT"/> class.
/// </summary>
public interface IAggregatedMasterServerTMT
{
    Task<AggregatedSummaryZoneResponse<int>> GetCampaignLeaderBoardSummariesResponseAsync(IEnumerable<string> zones, IProgress<AggregatedSummaryZoneProgress<int>>? progress = null, CancellationToken cancellationToken = default);
    Task<AggregatedSummaryZoneResponse<int>> GetCampaignLeaderBoardSummariesResponseAsync(string zone = "World", IProgress<AggregatedSummaryZoneProgress<int>>? progress = null, CancellationToken cancellationToken = default);
    Task<AggregatedSummaryZoneResponse<int>> GetCampaignLeaderBoardSummariesResponseAsync(params IEnumerable<string> zones);
    Task<ImmutableArray<AggregatedSummaryZone<int>>> GetCampaignLeaderBoardSummariesAsync(IEnumerable<string> zones, IProgress<AggregatedSummaryZoneProgress<int>>? progress = null, CancellationToken cancellationToken = default);
    Task<ImmutableArray<AggregatedSummaryZone<int>>> GetCampaignLeaderBoardSummariesAsync(string zone = "World", IProgress<AggregatedSummaryZoneProgress<int>>? progress = null, CancellationToken cancellationToken = default);
    Task<ImmutableArray<AggregatedSummaryZone<int>>> GetCampaignLeaderBoardSummariesAsync(params IEnumerable<string> zones);
    Task<AggregatedSummaryZoneResponse<TimeInt32>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, IEnumerable<string> zones, IProgress<AggregatedSummaryZoneProgress<TimeInt32>>? progress = null, CancellationToken cancellationToken = default);
    Task<AggregatedSummaryZoneResponse<TimeInt32>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, string zone = "World", IProgress<AggregatedSummaryZoneProgress<TimeInt32>>? progress = null, CancellationToken cancellationToken = default);
    Task<AggregatedSummaryZoneResponse<TimeInt32>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, params IEnumerable<string> zones);
    Task<ImmutableArray<AggregatedSummaryZone<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, IEnumerable<string> zones, IProgress<AggregatedSummaryZoneProgress<TimeInt32>>? progress = null, CancellationToken cancellationToken = default);
    Task<ImmutableArray<AggregatedSummaryZone<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, string zone = "World", IProgress<AggregatedSummaryZoneProgress<TimeInt32>>? progress = null, CancellationToken cancellationToken = default);
    Task<ImmutableArray<AggregatedSummaryZone<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, params IEnumerable<string> zones);

    Task<AggregatedSummaryResponse<int>> GetCampaignLeaderBoardSummaryResponseAsync(string zone = "World", IProgress<AggregatedSummaryProgress<int>>? progress = null, CancellationToken cancellationToken = default);
    Task<AggregatedSummary<int>> GetCampaignLeaderBoardSummaryAsync(string zone = "World", IProgress<AggregatedSummaryProgress<int>>? progress = null, CancellationToken cancellationToken = default);
    Task<AggregatedSummaryResponse<TimeInt32>> GetMapLeaderBoardSummaryResponseAsync(string mapUid, string zone = "World", IProgress<AggregatedSummaryProgress<TimeInt32>>? progress = null, CancellationToken cancellationToken = default);
    Task<AggregatedSummary<TimeInt32>> GetMapLeaderBoardSummaryAsync(string mapUid, string zone = "World", IProgress<AggregatedSummaryProgress<TimeInt32>>? progress = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Provides access to Trackmania Turbo master servers of all platforms. Requests are done in parallel, and the results are deeply combined where possible, usually via <see cref="AggregatedRecordUnit{T}"/> and differentiated by platform.
/// </summary>
public class AggregatedMasterServerTMT : IAggregatedMasterServerTMT
{
    private readonly IDictionary<Platform, MasterServerTMT> masterServers;

    public AggregatedMasterServerTMT(IDictionary<Platform, MasterServerTMT> masterServers)
    {
        this.masterServers = masterServers;
    }

    public virtual async Task<AggregatedSummaryZoneResponse<int>> GetCampaignLeaderBoardSummariesResponseAsync(IEnumerable<string> zones, IProgress<AggregatedSummaryZoneProgress<int>>? progress = null, CancellationToken cancellationToken = default)
    {
        return await AggregateSummariesAsync(async x => await x.GetCampaignLeaderBoardSummariesResponseAsync(zones, cancellationToken), progress);
    }

    public async Task<AggregatedSummaryZoneResponse<int>> GetCampaignLeaderBoardSummariesResponseAsync(string zone = "World", IProgress<AggregatedSummaryZoneProgress<int>>? progress = null, CancellationToken cancellationToken = default)
    {
        return await GetCampaignLeaderBoardSummariesResponseAsync([zone], progress, cancellationToken);
    }

    public async Task<AggregatedSummaryZoneResponse<int>> GetCampaignLeaderBoardSummariesResponseAsync(params IEnumerable<string> zones)
    {
        return await GetCampaignLeaderBoardSummariesResponseAsync(zones, default);
    }

    public async Task<ImmutableArray<AggregatedSummaryZone<int>>> GetCampaignLeaderBoardSummariesAsync(IEnumerable<string> zones, IProgress<AggregatedSummaryZoneProgress<int>>? progress = null, CancellationToken cancellationToken = default)
    {
        return (await GetCampaignLeaderBoardSummariesResponseAsync(zones, progress, cancellationToken)).Summaries;
    }

    public async Task<ImmutableArray<AggregatedSummaryZone<int>>> GetCampaignLeaderBoardSummariesAsync(string zone = "World", IProgress<AggregatedSummaryZoneProgress<int>>? progress = null, CancellationToken cancellationToken = default)
    {
        return await GetCampaignLeaderBoardSummariesAsync([zone], progress, cancellationToken);
    }

    public async Task<ImmutableArray<AggregatedSummaryZone<int>>> GetCampaignLeaderBoardSummariesAsync(params IEnumerable<string> zones)
    {
        return await GetCampaignLeaderBoardSummariesAsync(zones, default);
    }

    public virtual async Task<AggregatedSummaryZoneResponse<TimeInt32>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, IEnumerable<string> zones, IProgress<AggregatedSummaryZoneProgress<TimeInt32>>? progress = null, CancellationToken cancellationToken = default)
    {
        return await AggregateSummariesAsync(async x => await x.GetMapLeaderBoardSummariesResponseAsync(mapUid, zones, cancellationToken), progress);
    }

    public async Task<AggregatedSummaryZoneResponse<TimeInt32>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, string zone = "World", IProgress<AggregatedSummaryZoneProgress<TimeInt32>>? progress = null, CancellationToken cancellationToken = default)
    {
        return await GetMapLeaderBoardSummariesResponseAsync(mapUid, [zone], progress, cancellationToken);
    }

    public async Task<AggregatedSummaryZoneResponse<TimeInt32>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, params IEnumerable<string> zones)
    {
        return await GetMapLeaderBoardSummariesResponseAsync(mapUid, zones, default);
    }

    public async Task<ImmutableArray<AggregatedSummaryZone<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, IEnumerable<string> zones, IProgress<AggregatedSummaryZoneProgress<TimeInt32>>? progress = null, CancellationToken cancellationToken = default)
    {
        return (await GetMapLeaderBoardSummariesResponseAsync(mapUid, zones, progress, cancellationToken)).Summaries;
    }

    public async Task<ImmutableArray<AggregatedSummaryZone<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, string zone = "World", IProgress<AggregatedSummaryZoneProgress<TimeInt32>>? progress = null, CancellationToken cancellationToken = default)
    {
        return await GetMapLeaderBoardSummariesAsync(mapUid, [zone], progress, cancellationToken);
    }

    public async Task<ImmutableArray<AggregatedSummaryZone<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, params IEnumerable<string> zones)
    {
        return await GetMapLeaderBoardSummariesAsync(mapUid, zones, default);
    }

    public async Task<AggregatedSummaryResponse<int>> GetCampaignLeaderBoardSummaryResponseAsync(string zone = "World", IProgress<AggregatedSummaryProgress<int>>? progress = null, CancellationToken cancellationToken = default)
    {
        return await AggregateSummariesAsync(async x => await x.GetCampaignLeaderBoardSummaryResponseAsync(zone, cancellationToken), progress);
    }

    public async Task<AggregatedSummary<int>> GetCampaignLeaderBoardSummaryAsync(string zone = "World", IProgress<AggregatedSummaryProgress<int>>? progress = null, CancellationToken cancellationToken = default)
    {
        return (await GetCampaignLeaderBoardSummaryResponseAsync(zone, progress, cancellationToken)).Summary;
    }

    public async Task<AggregatedSummaryResponse<TimeInt32>> GetMapLeaderBoardSummaryResponseAsync(string mapUid, string zone = "World", IProgress<AggregatedSummaryProgress<TimeInt32>>? progress = null, CancellationToken cancellationToken = default)
    {
        return await AggregateSummariesAsync(async x => await x.GetMapLeaderBoardSummaryResponseAsync(mapUid, zone, cancellationToken), progress);
    }

    public async Task<AggregatedSummary<TimeInt32>> GetMapLeaderBoardSummaryAsync(string mapUid, string zone = "World", IProgress<AggregatedSummaryProgress<TimeInt32>>? progress = null, CancellationToken cancellationToken = default)
    {
        return (await GetMapLeaderBoardSummaryResponseAsync(mapUid, zone, progress, cancellationToken)).Summary;
    }

    private async Task<AggregatedSummaryZoneResponse<T>> AggregateSummariesAsync<T>(
        Func<MasterServerTMT, Task<MasterServerResponse<ImmutableArray<SummaryZone<T>>>>> getResponse,
        IProgress<AggregatedSummaryZoneProgress<T>>? progress)
        where T : struct, IComparable<T>
    {
        var tasks = masterServers.ToDictionary(async x => await getResponse(x.Value), x => x.Key);

        var platforms = ImmutableDictionary.CreateBuilder<Platform, AggregatedSummaryInfo>();
        var aggregated = new Dictionary<string, (ImmutableDictionary<Platform, DateTimeOffset>.Builder Timestamps, ImmutableArray<AggregatedRecordUnit<T>>.Builder Scores)>();

        while (tasks.Count > 0)
        {
            var task = await Task.WhenAny(tasks.Keys);
            var platform = tasks[task];
            tasks.Remove(task);

            try
            {
                var response = await task;

                progress?.Report(new AggregatedSummaryZoneProgress<T>(platform, response));

                foreach (var summary in response.Result)
                {
                    if (!aggregated.TryGetValue(summary.Zone, out var existing))
                    {
                        var timestamps = ImmutableDictionary.CreateBuilder<Platform, DateTimeOffset>();
                        var scores = ImmutableArray.CreateBuilder<AggregatedRecordUnit<T>>();
                        aggregated[summary.Zone] = existing = (timestamps, scores);
                    }

                    existing.Timestamps[platform] = summary.Timestamp;
                    existing.Scores.AddRange(summary.Scores.Select(x => new AggregatedRecordUnit<T>(x.Score, x.Count, platform)));

                    SortScores(existing.Scores);
                }

                platforms[platform] = new AggregatedSummaryInfo(response.ExecutionTime, response.XmlParseTime, response.Details, ErrorMessage: null);
            }
            catch (XmlRequestException ex)
            {
                platforms[platform] = new AggregatedSummaryInfo(ExecutionTime: null, ex.XmlParseTime, ex.Details, ex.ErrorMessage);
            }
            catch (Exception ex)
            {
                platforms[platform] = new AggregatedSummaryInfo(ExecutionTime: null, default, null, ex.Message);
            }
        }

        return new AggregatedSummaryZoneResponse<T>(platforms.ToImmutable(), aggregated.Select(x =>
            new AggregatedSummaryZone<T>(x.Key, x.Value.Timestamps.ToImmutable(), x.Value.Scores.ToImmutable()))
            .ToImmutableArray());
    }

    private async Task<AggregatedSummaryResponse<T>> AggregateSummariesAsync<T>(
        Func<MasterServerTMT, Task<MasterServerResponse<Summary<T>>>> getResponse,
        IProgress<AggregatedSummaryProgress<T>>? progress)
        where T : struct, IComparable<T>
    {
        var tasks = masterServers.ToDictionary(async x => await getResponse(x.Value), x => x.Key);

        var platforms = ImmutableDictionary.CreateBuilder<Platform, AggregatedSummaryInfo>();
        var timestamps = ImmutableDictionary.CreateBuilder<Platform, DateTimeOffset>();
        var scores = ImmutableArray.CreateBuilder<AggregatedRecordUnit<T>>();

        while (tasks.Count > 0)
        {
            var task = await Task.WhenAny(tasks.Keys);
            var platform = tasks[task];
            tasks.Remove(task);

            try
            {
                var response = await task;
                progress?.Report(new AggregatedSummaryProgress<T>(platform, response));

                var summary = response.Result;

                timestamps[platform] = summary.Timestamp;
                scores.AddRange(summary.Scores.Select(x => new AggregatedRecordUnit<T>(x.Score, x.Count, platform)));

                SortScores(scores);

                platforms[platform] = new AggregatedSummaryInfo(response.ExecutionTime, response.XmlParseTime, response.Details, ErrorMessage: null);
            }

            catch (XmlRequestException ex)
            {
                platforms[platform] = new AggregatedSummaryInfo(ExecutionTime: null, ex.XmlParseTime, ex.Details, ex.ErrorMessage);
            }
            catch (Exception ex)
            {
                platforms[platform] = new AggregatedSummaryInfo(ExecutionTime: null, default, null, ex.Message);
            }
        }

        return new AggregatedSummaryResponse<T>(platforms.ToImmutable(), new AggregatedSummary<T>(timestamps.ToImmutable(), scores.ToImmutable()));
    }

    private static void SortScores<T>(ImmutableArray<AggregatedRecordUnit<T>>.Builder scores) where T : struct, IComparable<T>
    {
        // sort by score then by platform, in ascending order for TimeInt32 and descending order for int
        if (typeof(T) == typeof(TimeInt32))
        {
            scores.Sort((x, y) =>
            {
                // in case of -1, it should be at the end
                if (x.Score is TimeInt32 timeX && timeX == new TimeInt32(-1))
                {
                    return 1;
                }

                if (y.Score is TimeInt32 timeY && timeY == new TimeInt32(-1))
                {
                    return -1;
                }

                var scoreComparison = x.Score.CompareTo(y.Score);
                return scoreComparison == 0 ? x.Platform.CompareTo(y.Platform) : scoreComparison;
            });
        }
        else
        {
            scores.Sort((x, y) =>
            {
                var scoreComparison = y.Score.CompareTo(x.Score);
                return scoreComparison == 0 ? x.Platform.CompareTo(y.Platform) : scoreComparison;
            });
        }
    }
}
