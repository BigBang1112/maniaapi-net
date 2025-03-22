namespace ManiaAPI.TMX.Attributes;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class QueryAttribute(string name) : Attribute
{
    public string Name { get; } = name;
    public object? Default { get; init; }
}
