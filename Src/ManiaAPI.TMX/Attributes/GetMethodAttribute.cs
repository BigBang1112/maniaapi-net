namespace ManiaAPI.TMX.Attributes;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class GetMethodAttribute(string route) : Attribute
{
    public string Route { get; } = route;
}
