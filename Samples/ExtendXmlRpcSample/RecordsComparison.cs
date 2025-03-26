using System.Collections.Immutable;

namespace ExtendXmlRpcSample;

public sealed record RecordsComparison(DateTimeOffset Timestamp, string Login, string OpponentLogin, ImmutableArray<RecordComparison> Records);
