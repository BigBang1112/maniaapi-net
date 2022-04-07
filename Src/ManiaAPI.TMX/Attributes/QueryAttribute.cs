namespace ManiaAPI.TMX.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class QueryAttribute : Attribute
{
    public string Name { get; }
    public object? Default { get; init; }

    public QueryAttribute(string name)
    {
        Name = name;
    }
}
