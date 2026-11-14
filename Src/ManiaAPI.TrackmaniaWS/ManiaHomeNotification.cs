namespace ManiaAPI.TrackmaniaWS;

internal sealed record ManiaHomeNotification(string SenderName, string? ReceiverName, string Message, string? Link, string? IconStyle, string? IconSubStyle);
