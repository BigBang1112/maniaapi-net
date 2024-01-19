using NSubstitute;
using System.Net.Http.Json;
using ManiaAPI.NadeoAPI.JsonContexts;
using System.Text.Json;

namespace ManiaAPI.NadeoAPI.Extensions.Gbx.Tests;

public class NadeoServicesExtensionsTests
{
    private readonly INadeoServices nadeoServices;
    private readonly Stream stream;
    private readonly CancellationToken cancellationToken;

    public NadeoServicesExtensionsTests()
    {
        nadeoServices = Substitute.For<INadeoServices>();
        stream = new MemoryStream(File.ReadAllBytes("ManiaAPI-NET Test Map.Map.Gbx"));
        cancellationToken = new CancellationToken();
    }

    [Fact]
    public async Task UploadMapAsync_SuccessfulUploadReturningJson()
    {
        // Arrange
        var expectedMapInfo = new MapInfo(
            Author: Guid.NewGuid(),
            AuthorScore: new(3572),
            BronzeScore: new(6000),
            CollectionName: "Stadium",
            FileName: "ManiaAPI-NET Test Map.Map.Gbx",
            GoldScore: new(4000),
            IsPlayable: true,
            MapId: Guid.NewGuid(),
            MapStyle: "",
            MapType: "",
            MapUid: "",
            Name: "",
            SilverScore: new(5000),
            Submitter: Guid.NewGuid(),
            Timestamp: DateTimeOffset.UtcNow,
            FileUrl: "https://test.com/ManiaAPI-NET Test Map.Map.Gbx",
            ThumbnailUrl: "https://test.com/ManiaAPI-NET Test Map.Map.jpg");

        nadeoServices.SendAsync(Arg.Any<HttpMethod>(), Arg.Any<string>(), Arg.Any<MultipartFormDataContent>(), Arg.Any<CancellationToken>())
                     .Returns(Task.FromResult(new HttpResponseMessage
                     {
#if NET8_0_OR_GREATER
                         Content = JsonContent.Create(expectedMapInfo, NadeoAPIJsonContext.Default.MapInfo)
#else
                         Content = JsonContent.Create(expectedMapInfo, NadeoAPIJsonContext.Default.MapInfo.Type)
#endif
                     }));

        // Act
        var result = await NadeoServicesExtensions.UploadMapAsync(nadeoServices, stream, expectedMapInfo.FileName, cancellationToken);

        // Assert
        Assert.Equal(
            expected: JsonSerializer.Serialize(expectedMapInfo, NadeoAPIJsonContext.Default.MapInfo),
            actual: JsonSerializer.Serialize(result, NadeoAPIJsonContext.Default.MapInfo));
    }

    [Fact]
    public async Task UpdateMapAsync_SuccessfulUpdateReturningJson()
    {
        // Arrange
        var expectedMapInfo = new MapInfo(
            Author: Guid.NewGuid(),
            AuthorScore: new(3572),
            BronzeScore: new(6000),
            CollectionName: "Stadium",
            FileName: "ManiaAPI-NET Test Map.Map.Gbx",
            GoldScore: new(4000),
            IsPlayable: true,
            MapId: Guid.NewGuid(),
            MapStyle: "",
            MapType: "",
            MapUid: "",
            Name: "",
            SilverScore: new(5000),
            Submitter: Guid.NewGuid(),
            Timestamp: DateTimeOffset.UtcNow,
            FileUrl: "https://test.com/ManiaAPI-NET Test Map.Map.Gbx",
            ThumbnailUrl: "https://test.com/ManiaAPI-NET Test Map.Map.jpg");

        nadeoServices.SendAsync(Arg.Any<HttpMethod>(), $"maps/{expectedMapInfo.MapId}", Arg.Any<MultipartFormDataContent>(), Arg.Any<CancellationToken>())
                     .Returns(Task.FromResult(new HttpResponseMessage
                     {
#if NET8_0_OR_GREATER
                         Content = JsonContent.Create(expectedMapInfo, NadeoAPIJsonContext.Default.MapInfo)
#else
                         Content = JsonContent.Create(expectedMapInfo, NadeoAPIJsonContext.Default.MapInfo.Type)
#endif
                     }));

        // Act
        var result = await NadeoServicesExtensions.UpdateMapAsync(nadeoServices, expectedMapInfo.MapId, stream, expectedMapInfo.FileName, cancellationToken);

        // Assert
        Assert.Equal(
            expected: JsonSerializer.Serialize(expectedMapInfo, NadeoAPIJsonContext.Default.MapInfo),
            actual: JsonSerializer.Serialize(result, NadeoAPIJsonContext.Default.MapInfo));
    }
}