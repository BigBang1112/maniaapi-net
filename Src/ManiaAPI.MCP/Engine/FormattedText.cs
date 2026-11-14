using System.Text.Json.Nodes;
using TmEssentials;

namespace ManiaAPI.MCP.Engine;

/// <summary>
/// Adds a deformatted <c>&lt;field&gt;Plain</c> sibling next to name-like fields that contain Trackmania/ManiaPlanet
/// formatting codes (<c>$o</c>, <c>$fff</c>, <c>$h[login]</c>, ...). The original value is never changed.
/// </summary>
public static class FormattedText
{
    public const string PlainSuffix = "Plain";

    // Only fields that hold display names; deformatting other strings (URLs, descriptions, scripts) could mangle them.
    private static readonly HashSet<string> Fields = new(StringComparer.OrdinalIgnoreCase)
    {
        "nickname",
        "userNickname",
        "name",
        "displayName",
        "mapName",
        "gbxMapName",
        "clubName",
        "campaignName",
        "titleName",
        "serverName",
        "tag"
    };

    public static void AddPlain(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            foreach (var (key, child) in obj.ToArray())
            {
                if (child is JsonValue value && Fields.Contains(key) && value.TryGetValue<string>(out var text) &&
                    text.Contains('$') && !obj.ContainsKey(key + PlainSuffix))
                {
                    var plain = TextFormatter.Deformat(text);

                    if (plain != text)
                    {
                        obj[key + PlainSuffix] = plain;
                    }
                }
                else
                {
                    AddPlain(child);
                }
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var child in array)
            {
                AddPlain(child);
            }
        }
    }
}
