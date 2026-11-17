using ManiaAPI.ManiaPlanetAPI.Converters;
using System.Text.Json;

namespace ManiaAPI.ManiaPlanetAPI.Tests;

public class DateTimeOffsetUnixConverterTests
{
    private static readonly JsonSerializerOptions options = new() { Converters = { new DateTimeOffsetUnixConverter() } };

    [Test]
    public async Task Read_WholeSeconds()
    {
        var value = JsonSerializer.Deserialize<DateTimeOffset>("1794943794", options);

        await Assert.That(value).IsEqualTo(DateTimeOffset.FromUnixTimeSeconds(1794943794));
    }

    [Test]
    public async Task Read_FractionalSeconds_KeepsMicroseconds()
    {
        var value = JsonSerializer.Deserialize<DateTimeOffset>("1794943794.948835", options);

        await Assert.That(value).IsEqualTo(DateTimeOffset.FromUnixTimeSeconds(1794943794).AddTicks(9_488_350));
    }

    [Test]
    [Arguments("1794943794")]
    [Arguments("1794943794.948835")]
    public async Task Write_RoundTrips(string json)
    {
        var value = JsonSerializer.Deserialize<DateTimeOffset>(json, options);

        await Assert.That(JsonSerializer.Serialize(value, options)).IsEqualTo(json);
    }

    [Test]
    public async Task Read_String_Throws()
    {
        await Assert.ThrowsAsync<JsonException>(() => Task.FromResult(JsonSerializer.Deserialize<DateTimeOffset>("\"1794943794\"", options)));
    }
}
