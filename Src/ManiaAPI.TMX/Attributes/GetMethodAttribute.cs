namespace ManiaAPI.TMX.Attributes;

[AttributeUsage(AttributeTargets.Method)]
sealed class GetMethodAttribute : Attribute
{
    public string Route { get; }

    public GetMethodAttribute(string route)
    {
        Route = route;
    }
}
