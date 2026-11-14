namespace ManiaAPI.TMX.Attributes;

/// <summary>
/// Marks a field that only MX sites have. It's left out of the default field selection of TMX searches,
/// because requesting a field the site doesn't have fails the whole request.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal sealed class MxOnlyAttribute : Attribute
{
}
