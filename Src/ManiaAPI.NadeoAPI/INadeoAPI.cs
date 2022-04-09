namespace ManiaAPI.NadeoAPI;

public interface INadeoAPI
{
    ValueTask<bool> RefreshAsync(CancellationToken cancellationToken = default);
}
