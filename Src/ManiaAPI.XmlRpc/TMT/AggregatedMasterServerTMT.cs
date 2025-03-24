using System.Collections.Immutable;
using TmEssentials;

namespace ManiaAPI.XmlRpc.TMT;

/// <summary>
/// Interface for the <see cref="AggregatedMasterServerTMT"/> class.
/// </summary>
public interface IAggregatedMasterServerTMT
{
    Task<ImmutableArray<AggregatedSummary<int>>> GetCampaignLeaderBoardSummariesAsync(IEnumerable<string> zones, IProgress<AggregatedSummaryProgress<int>>? progress = null, CancellationToken cancellationToken = default);
    Task<ImmutableArray<AggregatedSummary<int>>> GetCampaignLeaderBoardSummariesAsync(string zone = "World", IProgress<AggregatedSummaryProgress<int>>? progress = null, CancellationToken cancellationToken = default);
    Task<ImmutableArray<AggregatedSummary<int>>> GetCampaignLeaderBoardSummariesAsync(params IEnumerable<string> zones);
    Task<ImmutableArray<AggregatedSummary<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, IEnumerable<string> zones, IProgress<AggregatedSummaryProgress<TimeInt32>>? progress = null, CancellationToken cancellationToken = default);
    Task<ImmutableArray<AggregatedSummary<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, string zone = "World", IProgress<AggregatedSummaryProgress<TimeInt32>>? progress = null, CancellationToken cancellationToken = default);
    Task<ImmutableArray<AggregatedSummary<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, params IEnumerable<string> zones);
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

    public async Task<ImmutableArray<AggregatedSummary<int>>> GetCampaignLeaderBoardSummariesAsync(IEnumerable<string> zones, IProgress<AggregatedSummaryProgress<int>>? progress = null, CancellationToken cancellationToken = default)
    {
        return await AggregateSummaries(async x => await x.GetCampaignLeaderBoardSummariesResponseAsync(zones, cancellationToken), progress);
    }

    public async Task<ImmutableArray<AggregatedSummary<int>>> GetCampaignLeaderBoardSummariesAsync(string zone = "World", IProgress<AggregatedSummaryProgress<int>>? progress = null, CancellationToken cancellationToken = default)
    {
        return await GetCampaignLeaderBoardSummariesAsync([zone], progress, cancellationToken);
    }

    public async Task<ImmutableArray<AggregatedSummary<int>>> GetCampaignLeaderBoardSummariesAsync(params IEnumerable<string> zones)
    {
        return await GetCampaignLeaderBoardSummariesAsync(zones, default);
    }

    public async Task<ImmutableArray<AggregatedSummary<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, IEnumerable<string> zones, IProgress<AggregatedSummaryProgress<TimeInt32>>? progress = null, CancellationToken cancellationToken = default)
    {
        return await AggregateSummaries(async x => await x.GetMapLeaderBoardSummariesResponseAsync(mapUid, zones, cancellationToken), progress);
    }

    public async Task<ImmutableArray<AggregatedSummary<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, string zone = "World", IProgress<AggregatedSummaryProgress<TimeInt32>>? progress = null, CancellationToken cancellationToken = default)
    {
        return await GetMapLeaderBoardSummariesAsync(mapUid, [zone], progress, cancellationToken);
    }

    public async Task<ImmutableArray<AggregatedSummary<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, params IEnumerable<string> zones)
    {
        return await GetMapLeaderBoardSummariesAsync(mapUid, zones, default);
    }

    private async Task<ImmutableArray<AggregatedSummary<T>>> AggregateSummaries<T>(
        Func<MasterServerTMT, Task<MasterServerResponse<ImmutableArray<Summary<T>>>>> getResponse,
        IProgress<AggregatedSummaryProgress<T>>? progress)
        where T : struct, IComparable // Change this to IComparable<T> once TimeInt32 is fixed
    {
        var tasks = masterServers.ToDictionary(async x => await getResponse(x.Value), x => x.Key);

        var aggregated = new Dictionary<string, (ImmutableDictionary<Platform, DateTimeOffset>.Builder Timestamps, ImmutableArray<AggregatedRecordUnit<T>>.Builder Scores)>();

        while (tasks.Count > 0)
        {
            var task = await Task.WhenAny(tasks.Keys);
            var platform = tasks[task];
            tasks.Remove(task);

            var response = await task;

            progress?.Report(new AggregatedSummaryProgress<T>(platform, response));

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

                // sort by score then by platform, in ascending order for TimeInt32 and descending order for int
                if (typeof(T) == typeof(TimeInt32))
                {
                    existing.Scores.Sort((x, y) =>
                    {
                        var scoreComparison = x.Score.CompareTo(y.Score);
                        return scoreComparison == 0 ? x.Platform.CompareTo(y.Platform) : scoreComparison;
                    });
                }
                else
                {
                    existing.Scores.Sort((x, y) =>
                    {
                        var scoreComparison = y.Score.CompareTo(x.Score);
                        return scoreComparison == 0 ? x.Platform.CompareTo(y.Platform) : scoreComparison;
                    });
                }
            }
        }

        return aggregated.Select(x =>
            new AggregatedSummary<T>(x.Key, x.Value.Timestamps.ToImmutable(), x.Value.Scores.ToImmutable()))
            .ToImmutableArray();
    }
}
