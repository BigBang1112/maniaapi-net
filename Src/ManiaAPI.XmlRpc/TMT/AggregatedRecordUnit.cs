namespace ManiaAPI.XmlRpc.TMT;

public readonly record struct AggregatedRecordUnit<T>(T Score, int Count, Platform Platform) where T : struct;
