using System.Text.Json.Nodes;
using ManiaAPI.MasterServer.TMUF;

namespace ManiaAPI.MCP.Catalog;

public static class ScoreUrlOperations
{
    public static IEnumerable<Operation> Create()
    {
        foreach (var game in new[] { "tmuf", "tmnf" })
        {
            foreach (var kind in new[] { "general", "ladder", "scores" })
            {
                var parameters = new List<(string, JsonNode, bool)>
                {
                    ("num", CatalogMetadata.Schema<ScoresNumber>(), true),
                    ("leagueId", CatalogMetadata.Schema<int>(), true)
                };

                if (kind == "scores")
                {
                    parameters.Add(("scoresName", CatalogMetadata.Schema<string>(), true));
                }

                var name = kind == "scores" ? "scores_url" : kind + "_scores_url";

                yield return new Operation("masterserver." + game + "." + name,
                    game, "Get the upstream URL of a TM Forever scores file.", "none", null, false, false,
                [
                    new OperationOverload(CatalogMetadata.Input(parameters.ToArray()),
                        "num: ScoresNumber, leagueId: int" + (kind == "scores" ? ", scoresName: string" : ""),
                        CatalogMetadata.Schema<string>(), (context, arguments, token) =>
                        {
                            var number = CatalogMetadata.Argument<ScoresNumber>(arguments, "num");
                            var league = arguments.GetProperty("leagueId").GetInt32();
                            var url = kind switch
                            {
                                "general" => MasterServerTMUF.GetGeneralScoresUrl(number, league),
                                "ladder" => MasterServerTMUF.GetLadderScoresUrl(number, league),
                                _ => MasterServerTMUF.GetScoresUrl(number, arguments.GetProperty("scoresName").GetString()!, league)
                            };

                            return Task.FromResult<JsonNode?>(JsonValue.Create(url));
                        })
                ]);
            }
        }
    }
}
