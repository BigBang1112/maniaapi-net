﻿using TmEssentials;

namespace ExtendXmlSample;

public sealed record class RecordComparison(string MapUid, TimeInt32 Score, DateTimeOffset Timestamp, TimeInt32 OpponentScore, DateTimeOffset OpponentTimestamp, string OpponentDownloadUrl);
