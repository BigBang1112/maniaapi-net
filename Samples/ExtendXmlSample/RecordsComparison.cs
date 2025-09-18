using System.Collections.Immutable;

namespace ExtendXmlSample;

public sealed record RecordsComparison(DateTimeOffset Timestamp, string Login, string OpponentLogin, ImmutableList<RecordComparison> Records);
