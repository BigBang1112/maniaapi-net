using System.Threading.Tasks;
using Xunit;

namespace ManiaAPI.TrackmaniaAPI.Tests.Integration;

public class TrackmaniaAPITests
{
    // test authorize
    [Fact]
    public async Task RequestManagement()
    {
        var tmio = new TrackmaniaAPI();
        await tmio.AuthorizeAsync("", "");
    }
}
