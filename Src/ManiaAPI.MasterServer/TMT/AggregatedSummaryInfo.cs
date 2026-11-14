namespace ManiaAPI.MasterServer.TMT;

/// <param name="ExecutionTime">Execution time reported by the master server.</param>
/// <param name="XmlParseTime">Time it took to parse the response.</param>
/// <param name="Details">Details of the request, or <see langword="null"/> if it did not complete.</param>
/// <param name="ErrorMessage">Error message if the request failed, otherwise <see langword="null"/>.</param>
/// <param name="Error">Error code if the master server returned an error, otherwise <see langword="null"/>.</param>
public sealed record AggregatedSummaryInfo(TimeSpan? ExecutionTime, TimeSpan XmlParseTime, MasterServerResponseDetails? Details, string? ErrorMessage, MasterServerError? Error = null);
