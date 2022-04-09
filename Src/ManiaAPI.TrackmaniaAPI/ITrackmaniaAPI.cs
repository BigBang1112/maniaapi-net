
namespace ManiaAPI.TrackmaniaAPI;

public interface ITrackmaniaAPI
{
    Task<Dictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<User> GetUserAsync(CancellationToken cancellationToken = default);
}
