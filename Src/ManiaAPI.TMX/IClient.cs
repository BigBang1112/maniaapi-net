namespace ManiaAPI.TMX;

public interface IClient : IDisposable
{
    HttpClient Client { get; }
}
