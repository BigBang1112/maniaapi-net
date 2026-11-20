namespace ManiaAPI.MasterServer.Extensions.Hosting;

/// <summary>Options shared by registered init and master server clients.</summary>
public sealed class MasterServerOptions
{
    public string? UserAgent { get; set; }
}
