namespace ManiaAPI.MasterServer;

public readonly record struct RecordUnit<T>(T Score, int Count) where T : struct;
