using System.Threading.Tasks;
using Xunit;

namespace ManiaAPI.TMX.Tests;

public class TMXTests
{
    [Fact]
    public async Task GetReplaysAsync_Test()
    {
        var tmx = new TMX(TmxSite.TMNF);
        var replays = await tmx.GetReplaysAsync(new()
        {
            TrackId = 2233
        });
    }
}
