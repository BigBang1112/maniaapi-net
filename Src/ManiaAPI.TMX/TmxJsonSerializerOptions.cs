using ManiaAPI.TMX.Converters;
using System.Text.Json;

namespace ManiaAPI.TMX;

static class TmxJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Default = new();

    static TmxJsonSerializerOptions()
    {
        Default.Converters.Add(new TimeInt32Converter());
    }
}
