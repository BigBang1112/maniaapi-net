using ExtendXmlRpcSample;
using ManiaAPI.XmlRpc;
using ManiaAPI.XmlRpc.TMT;
using MinimalXmlReader;
using System.Collections.Immutable;
using TmEssentials;

var yourLogin = "yourLogin";
var yourSessionId = 999999;
var opponentLogin = "opponentLogin";
var platform = Platform.PC;

var initServer = new InitServerTMT(platform);

var waitingParams = await initServer.GetWaitingParamsAsync();

var masterServer = new ExtendedMasterServerTMT(waitingParams.MasterServers.First());

var opponentRecords = await masterServer.GetChallengeRecordsComparisonAsync(opponentLogin, yourLogin, yourSessionId);

foreach (var rec in opponentRecords.Records)
{
    Console.WriteLine(rec.OpponentDownloadUrl);
}

Console.WriteLine();

internal class ExtendedMasterServerTMT : MasterServerTMT
{
    public ExtendedMasterServerTMT(MasterServerInfo info) : base(info)
    {
    }

    public async Task<MasterServerResponse<RecordsComparison>> GetChallengeRecordsComparisonResponseAsync(string login, string authorLogin, int sessionId, CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetChallengeRecordsComparison";
        return await RequestAsync($"<author><login>{authorLogin}</login><session>{sessionId}</session></author>",
            RequestName, $"<ol>{login}</ol>", (ref MiniXmlReader xml) =>
            {
                var mainTimestmap = default(DateTimeOffset);
                var login = string.Empty;
                var opponentLogin = string.Empty;
                var records = ImmutableArray.CreateBuilder<RecordComparison>();

                while (xml.TryReadStartElement(out var element))
                {
                    switch (element)
                    {
                        case "t":
                            mainTimestmap = DateTimeOffset.FromUnixTimeSeconds(long.Parse(xml.ReadContent()));
                            break;
                        case "l":
                            login = xml.ReadContentAsString();
                            break;
                        case "ol":
                            opponentLogin = xml.ReadContentAsString();
                            break;
                        case "v":
                            var mapUid = string.Empty;
                            var score = default(TimeInt32);
                            var timestamp = default(DateTimeOffset);
                            var opponentScore = default(TimeInt32);
                            var opponentTimestamp = default(DateTimeOffset);
                            var opponentDownloadUrl = string.Empty;

                            while (xml.TryReadStartElement(out var subElement))
                            {
                                switch (subElement)
                                {
                                    case "c":
                                        mapUid = xml.ReadContentAsString();
                                        break;
                                    case "r":
                                        score = new TimeInt32((int)uint.Parse(xml.ReadContent()));
                                        break;
                                    case "d":
                                        timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(xml.ReadContent()));
                                        break;
                                    case "or":
                                        opponentScore = new TimeInt32((int)uint.Parse(xml.ReadContent()));
                                        break;
                                    case "od":
                                        opponentTimestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(xml.ReadContent()));
                                        break;
                                    case "ou":
                                        opponentDownloadUrl = xml.ReadContentAsString();
                                        break;
                                    default:
                                        xml.ReadContent();
                                        break;
                                }
                                _ = xml.SkipEndElement();
                            }

                            records.Add(new RecordComparison(mapUid, score, timestamp, opponentScore, opponentTimestamp, opponentDownloadUrl));
                            break;
                        default:
                            xml.ReadContent();
                            break;
                    }

                    _ = xml.SkipEndElement();
                }

                return new RecordsComparison(mainTimestmap, login, opponentLogin, records.ToImmutable());
            }, cancellationToken);
    }

    public async Task<RecordsComparison> GetChallengeRecordsComparisonAsync(string login, string authorLogin, int sessionId, CancellationToken cancellationToken = default)
    {
        return (await GetChallengeRecordsComparisonResponseAsync(login, authorLogin, sessionId, cancellationToken)).Result;
    }
}