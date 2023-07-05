using ManiaAPI.TMX.Converters;
using System.Text.Json;

namespace ManiaAPI.TMX;

static class TmxJsonSerializerOptions
{
    public static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new TimeInt32Converter());
        return options;
    }
}
