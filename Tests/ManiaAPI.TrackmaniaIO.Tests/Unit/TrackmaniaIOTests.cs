using System.Net.Http;
using Xunit;

namespace ManiaAPI.TrackmaniaIO.Tests.Unit;

public class TrackmaniaIOTests
{
    // test AddUserAgent
    [Fact]
    public void AddUserAgent_Adds_UserAgent()
    {
        // Arrange
        var userAgent = "Test User Agent";
        var expected = $"TrackmaniaIO.NET by BigBang1112 " + userAgent;

        // Act
        TrackmaniaIO.AddUserAgent(userAgent);

        // Assert
        Assert.Equal(expected, TrackmaniaIO.Client.DefaultRequestHeaders.UserAgent.ToString());
    }

    // test GetHeaderNumberValue
    [Fact]
    public void GetHeaderNumberValue_ReturnsLong()
    {
        // Arrange
        var headerName = "X-Test-Header";
        var headerValue = "123";
        var expected = 123;

        var response = new HttpResponseMessage();
        response.Headers.Add(headerName, headerValue);

        // Act
        var actual = TrackmaniaIO.GetHeaderNumberValue(headerName, response);

        // Assert
        Assert.Equal(expected, actual);
    }

    // test GetHeaderNumberValue if header not found
    [Fact]
    public void GetHeaderNumberValue_NoHeader_ReturnsNull()
    {
        // Arrange
        var expected = default(long?);

        var response = new HttpResponseMessage();

        // Act
        var actual = TrackmaniaIO.GetHeaderNumberValue("X-Test-Header", response);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetHeaderNumberValue_WrongTypeOfValue_ReturnsNull()
    {
        // Arrange
        var headerName = "X-Test-Header";
        var headerValue = "value";
        var expected = default(long?);

        var response = new HttpResponseMessage();
        response.Headers.Add(headerName, headerValue);

        // Act
        var actual = TrackmaniaIO.GetHeaderNumberValue("X-Test-Header", response);

        // Assert
        Assert.Equal(expected, actual);
    }
}
