namespace ManiaAPI.TMX.Attributes;

[AttributeUsage(AttributeTargets.Property)]
sealed class QueryAttribute : Attribute
{
    public string Name { get; }
    public object? Default { get; init; }

    public QueryAttribute(string name)
    {
        Name = name;
    }
}
