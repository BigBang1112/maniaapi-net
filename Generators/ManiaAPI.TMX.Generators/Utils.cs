using Microsoft.CodeAnalysis;

namespace ManiaAPI.TMX.Generators;

public static class Utils
{
    public static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol namespaceSymbol)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            yield return type;
        }

        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var type in GetAllTypes(nestedNamespace))
            {
                yield return type;
            }
        }
    }

    /// <summary>
    /// Gets the property name used in TMXJsonContext for a given return type symbol.
    /// Handles arrays (e.g. string[] -> StringArray) and generic types (e.g. ItemCollection&lt;MapItem&gt; -> ItemCollectionMapItem).
    /// </summary>
    public static string GetJsonContextPropertyName(ITypeSymbol symbol)
    {
        if (symbol is IArrayTypeSymbol arraySymbol)
        {
            return arraySymbol.ElementType.Name + "Array";
        }

        var name = symbol.Name;

        if (symbol is INamedTypeSymbol namedSymbol)
        {
            foreach (var typeArg in namedSymbol.TypeArguments)
            {
                name += typeArg.Name;
            }
        }

        return name;
    }
}