using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.TMX.Extensions.Hosting;

public sealed class TmxOptions
{
    /// <summary>
    /// User-Agent appended to all TMX clients.
    /// </summary>
    public string? UserAgent { get; set; }

    public Action<IHttpClientBuilder>? ConfigureHttpClient { get; set; }
}
