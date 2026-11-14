using System.Collections.Immutable;

namespace ExtendMasterServerSample;

public sealed record RecordsComparison(DateTimeOffset Timestamp, string Login, string OpponentLogin, ImmutableList<RecordComparison> Records);
