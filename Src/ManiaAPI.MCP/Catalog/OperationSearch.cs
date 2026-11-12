using System.Text.RegularExpressions;

namespace ManiaAPI.MCP.Catalog;

public sealed class OperationSearch
{
    private readonly Dictionary<string, Dictionary<string, int>> documents;
    private readonly Dictionary<string, int> frequencies;
    private readonly double averageLength;

    public OperationSearch(IEnumerable<Operation> operations)
    {
        documents = operations.ToDictionary(operation => operation.Id, operation =>
        {
            var tags = operation.Toolset switch
            {
                "exchange" => "community maps tracks exchange TM2 Trackmania 2 ShootMania",
                "maniaplanet" => "ManiaPlanet Trackmania 2 TM2 TrackMania² ShootMania SM",
                "tmuf" => "Trackmania United Forever TMUF",
                "tmnf" => "Trackmania Nations Forever TMNF",
                "trackmaniaturbo" => "Trackmania Turbo TMT PC XB1 PS4",
                _ => "Trackmania 2020 TM2020"
            };

            return Tokens(operation.Id + " " + operation.Summary + " " + tags + " " +
                string.Join(" ", operation.Overloads.Select(x => x.Signature)))
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());
        });

        frequencies = documents.Values
            .SelectMany(x => x.Keys)
            .GroupBy(x => x)
            .ToDictionary(x => x.Key, x => x.Count());

        averageLength = documents.Count == 0 ? 1 : documents.Values.Average(x => x.Values.Sum());
    }

    public IEnumerable<Operation> Search(IEnumerable<Operation> candidates, string query, int limit)
    {
        if (query.Length > 1000 || limit is < 1 or > 50)
        {
            throw new ArgumentException("query must be at most 1000 characters; limit must be between 1 and 50.");
        }

        var terms = Tokens(query).Distinct().ToArray();

        return candidates
            .Select(operation => (Operation: operation, Score: Score(operation.Id, terms)))
            .Where(x => terms.Length == 0 || x.Score > 0)
            .OrderBy(x => x.Operation.Obsolete)
            .ThenByDescending(x => x.Score)
            .ThenBy(x => x.Operation.Id, StringComparer.Ordinal)
            .Take(limit)
            .Select(x => x.Operation);
    }

    private double Score(string id, string[] terms)
    {
        var document = documents[id];
        var length = document.Values.Sum();
        double score = 0;

        foreach (var term in terms)
        {
            if (!document.TryGetValue(term, out var frequency))
            {
                continue;
            }

            var inverseFrequency = Math.Log(1 + (documents.Count - frequencies[term] + 0.5) / (frequencies[term] + 0.5));
            score += inverseFrequency * frequency * 2.2 / (frequency + 1.2 * (0.25 + 0.75 * length / averageLength));
        }

        return score;
    }

    private static IEnumerable<string> Tokens(string value)
    {
        return Regex.Matches(value.ToLowerInvariant(), @"[\p{L}\p{N}]+")
            .Select(x => x.Value);
    }
}
