using System.Collections.Immutable;

namespace ExtendXmlSample;

public sealed record RecordsComparison(DateTimeOffset Timestamp, string Login, string OpponentLogin, ImmutableArray<RecordComparison> Records);
