using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using TmEssentials;

namespace ManiaAPI.Dedimania;

public interface IDedimania : IDisposable
{
    /// <summary>
    /// HTTP client.
    /// </summary>
    HttpClient Client { get; }

    /// <summary>
    /// Gets a map with its best records, at most 100.
    /// </summary>
    /// <param name="uid">UID of the map.</param>
    /// <param name="mode">Game mode of the records, or null for the best records of every mode together.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The map, or null when Dedimania doesn't know it.</returns>
    Task<Map?> GetMapAsync(string uid, GameMode? mode = null, CancellationToken cancellationToken = default);
}

public class Dedimania : IDedimania
{
    public const string BaseAddress = "http://dedimania.net:8080";

    public HttpClient Client { get; }

    public Dedimania(HttpClient client)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));

        var headers = Client.DefaultRequestHeaders;

        const string product = "ManiaAPI.NET";
        const string version = PackageInfo.Version;

        var libraryExists = headers.UserAgent.Any(h => h.Product?.Name == product && h.Product?.Version == version);

        if (!libraryExists)
        {
            headers.UserAgent.Add(new ProductInfoHeaderValue(product, version));
            headers.UserAgent.Add(new ProductInfoHeaderValue("(Dedimania; Discord=bigbang1112)"));
        }
    }

    public Dedimania() : this(new HttpClient())
    {

    }

    public virtual async Task<Map?> GetMapAsync(string uid, GameMode? mode = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uid);

        // "more" adds the date and the server login to each record
        var endpoint = mode is null
            ? $"MAP?uid={Uri.EscapeDataString(uid)}&more"
            : $"MAP?uid={Uri.EscapeDataString(uid)}&mode={(int)mode.Value}&more";

        var response = await GetStringAsync(endpoint, cancellationToken);

        return ParseMap(response);
    }

    protected internal async Task<string> GetStringAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseAddress}/{endpoint}");

        using var response = await Client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        Debug.WriteLine($"Route: {endpoint}{Environment.NewLine}Response: {content}");

        return content;
    }

    /// <summary>
    /// Parses the reply of the <c>MAP</c> endpoint. Its first line is <c>uid,envir,author,nb_time_played,name</c>
    /// and each other line is <c>time,mode,date,login,serverlogin,nickname,servername</c>. Names have no commas,
    /// Dedimania replaces them with dots. An unknown map is <c>-1</c>.
    /// </summary>
    internal static Map? ParseMap(string response)
    {
        using var reader = new StringReader(response);

        var header = reader.ReadLine();

        if (header is null || header.Trim() == "-1")
        {
            return null;
        }

        var fields = header.Split(',', 5);

        if (fields.Length != 5)
        {
            throw new FormatException($"Unexpected Dedimania response: {header}");
        }

        var records = ImmutableList.CreateBuilder<MapRecord>();

        while (reader.ReadLine() is { } line)
        {
            if (line.Length > 0)
            {
                records.Add(ParseRecord(line));
            }
        }

        return new Map(
            Uid: fields[0],
            Environment: fields[1],
            AuthorLogin: fields[2],
            TimesPlayed: int.Parse(fields[3], CultureInfo.InvariantCulture),
            Name: fields[4],
            Records: records.ToImmutable());
    }

    private static MapRecord ParseRecord(string line)
    {
        var fields = line.Split(',', 7);

        if (fields.Length != 7)
        {
            throw new FormatException($"Unexpected Dedimania record: {line}");
        }

        return new MapRecord(
            Time: new TimeInt32(int.Parse(fields[0], CultureInfo.InvariantCulture)),
            Mode: (GameMode)int.Parse(fields[1], CultureInfo.InvariantCulture),
            Timestamp: fields[2].Length == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(long.Parse(fields[2], CultureInfo.InvariantCulture)),
            Login: fields[3],
            ServerLogin: fields[4].Length == 0 ? null : fields[4],
            Nickname: fields[5],
            ServerName: fields[6]);
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
