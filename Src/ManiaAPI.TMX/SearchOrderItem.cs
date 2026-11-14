namespace ManiaAPI.TMX;

/// <summary>
/// A search order usable in the <c>order1</c>/<c>order2</c> parameters. <see cref="Name"/> is null for unnamed orders, such as key 0 on TMX sites.
/// </summary>
public record struct SearchOrderItem(int Key, string? Name);