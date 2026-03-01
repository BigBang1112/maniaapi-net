using Microsoft.CodeAnalysis;
using System.Text;

namespace ManiaAPI.TMX.Generators;

[Generator]
public class FieldsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var fields = context.CompilationProvider.SelectMany((compilation, token) =>
        {
            var maniaApiTmxNamespace = compilation.GlobalNamespace
                .GetNamespaceMembers()
                .FirstOrDefault(x => x.Name == "ManiaAPI")
                ?.GetNamespaceMembers()
                .FirstOrDefault(x => x.Name == "TMX");

            if (maniaApiTmxNamespace is null)
            {
                return [];
            }

            return Utils.GetAllTypes(maniaApiTmxNamespace)
                .Where(x => x.GetAttributes()
                .Any(x => x.AttributeClass?.Name == "FieldsAttribute"));
        });

        context.RegisterSourceOutput(fields, GenerateFields);
    }

    private void GenerateFields(SourceProductionContext context, INamedTypeSymbol symbol)
    {
        var sb = new StringBuilder("using System.Text;\n\n");
        sb.Append("namespace ");
        sb.Append(symbol.ContainingNamespace.ToDisplayString());
        sb.AppendLine(";");
        sb.AppendLine();
        sb.Append("public readonly record struct ");
        sb.Append(symbol.Name);
        sb.AppendLine("Fields");
        sb.AppendLine("{");

        var propSb = new StringBuilder();

        var allSb = new StringBuilder("    public static readonly ");
        allSb.Append(symbol.Name);
        allSb.AppendLine("Fields All = new()");
        allSb.AppendLine("    {");

        var appendSb = new StringBuilder("    internal bool Append(StringBuilder sb)\n");
        appendSb.AppendLine("    {");
        appendSb.AppendLine("        var first = true;");

        foreach (var propSymbol in symbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (propSymbol.Name == "EqualityContract")
            {
                continue;
            }

            var propType = propSymbol.Type;
            if (propType is INamedTypeSymbol namedType && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                propType = namedType.TypeArguments[0];
            }

            var isFields = propType.GetAttributes()
                .Any(x => x.AttributeClass?.Name == "FieldsAttribute");

            propSb.Append("    public ");

            if (isFields)
            {
                propSb.Append(propType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                propSb.Append("Fields ");
            }
            else
            {
                propSb.Append("bool ");
            }

            propSb.Append(propSymbol.Name);
            propSb.AppendLine(" { get; init; }");

            allSb.Append("        ");
            allSb.Append(propSymbol.Name);
            allSb.Append(" = ");

            if (isFields)
            {
                allSb.Append(propType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                allSb.AppendLine("Fields.All,");
            }
            else
            {
                allSb.AppendLine("true,");
            }

            AppendFieldCheck(appendSb, propSymbol, isFields);
        }

        allSb.AppendLine("    };");

        appendSb.AppendLine();
        appendSb.AppendLine("        return !first;");
        appendSb.AppendLine("    }");

        sb.Append(propSb);
        sb.AppendLine();
        sb.Append(allSb);
        sb.AppendLine();
        sb.Append(appendSb);
        sb.AppendLine("}");

        context.AddSource($"{symbol.ContainingNamespace.ToDisplayString()}.{symbol.Name}Fields.cs", sb.ToString());
    }

    private static void AppendFieldCheck(StringBuilder sb, IPropertySymbol propSymbol, bool isFields, IPropertySymbol[]? accessToProperty = null)
    {
        if (isFields)
        {
            if (accessToProperty is null)
            {
                accessToProperty = [propSymbol];
            }
            else
            {
                accessToProperty = accessToProperty.Append(propSymbol).ToArray();
            }

            var memberType = propSymbol.Type;
            if (memberType is INamedTypeSymbol nt && nt.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                memberType = nt.TypeArguments[0];
            }

            foreach (var symbol in memberType.GetMembers().OfType<IPropertySymbol>())
            {
                if (symbol.Name == "EqualityContract")
                {
                    continue;
                }

                isFields = symbol.Type.GetAttributes()
                    .Any(x => x.AttributeClass?.Name == "FieldsAttribute");

                AppendFieldCheck(sb, symbol, isFields, accessToProperty);
            }

            return;
        }

        sb.AppendLine();
        sb.Append("        if (");

        if (accessToProperty is not null)
        {
            foreach (var access in accessToProperty)
            {
                sb.Append(access.Name);
                sb.Append('.');
            }
        }

        sb.Append(propSymbol.Name);
        sb.AppendLine(")");
        sb.AppendLine("        {");
        sb.AppendLine("            if (!first) sb.Append(\"%2C\");");

        if (accessToProperty is not null)
        {
            foreach (var accessProp in accessToProperty)
            {
                sb.Append("            sb.Append(nameof(");
                sb.Append(accessProp.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                sb.Append('.');
                sb.Append(accessProp.Name);
                sb.AppendLine("));");
                sb.AppendLine("            sb.Append('.');");
            }
        }

        sb.Append("            sb.Append(nameof(");
        sb.Append(propSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        sb.Append('.');
        sb.Append(propSymbol.Name);
        sb.AppendLine("));");

        sb.AppendLine("            first = false;");
        sb.AppendLine("        }");
    }
}
