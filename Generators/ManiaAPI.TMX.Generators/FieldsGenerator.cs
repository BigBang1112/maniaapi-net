using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Text;

namespace ManiaAPI.TMX.Generators;

[Generator]
public class FieldsGenerator : ISourceGenerator
{
    private const bool Debug = false;

    public void Initialize(GeneratorInitializationContext context)
    {
        if (Debug && !Debugger.IsAttached)
        {
            Debugger.Launch();
        }
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var maniaApiTmxNamespace = context.Compilation
            .GlobalNamespace
            .GetNamespaceMembers()
            .FirstOrDefault(x => x.Name == "ManiaAPI")
            .GetNamespaceMembers()
            .FirstOrDefault(x => x.Name == "TMX");

        var symbolsWithFieldsAtt = maniaApiTmxNamespace.GetTypeMembers()
            .Where(x => x.GetAttributes().Any(x => x.AttributeClass?.Name == "FieldsAttribute"));

        foreach (var symbol in symbolsWithFieldsAtt)
        {
            context.AddSource($"{symbol.Name}Fields.cs", GenerateFields(symbol));
        }
    }

    private string GenerateFields(INamedTypeSymbol symbol)
    {
        var sb = new StringBuilder("using System.Text;\n\n");
        sb.AppendLine("namespace ManiaAPI.TMX;");
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

            var isFields = propSymbol.Type.GetAttributes()
                .Any(x => x.AttributeClass?.Name == "FieldsAttribute");

            propSb.Append("    public ");

            if (isFields)
            {
                propSb.Append(propSymbol.Type.Name);
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
                allSb.Append(propSymbol.Type.Name);
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

        return sb.ToString();
    }

    private static void AppendFieldCheck(StringBuilder sb, IPropertySymbol propSymbol, bool isFields, IPropertySymbol[]? accessToProperty = null)
    {
        if (isFields)
        {
            if (accessToProperty is null)
            {
                accessToProperty = new[] { propSymbol };
            }
            else
            {
                accessToProperty = accessToProperty.Append(propSymbol).ToArray();
            }

            foreach (var symbol in propSymbol.Type.GetMembers().OfType<IPropertySymbol>())
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
                sb.Append(accessProp.ContainingType.Name);
                sb.Append('.');
                sb.Append(accessProp.Name);
                sb.AppendLine("));");
                sb.AppendLine("            sb.Append('.');");
            }
        }

        sb.Append("            sb.Append(nameof(");
        sb.Append(propSymbol.ContainingType.Name);
        sb.Append('.');
        sb.Append(propSymbol.Name);
        sb.AppendLine("));");

        sb.AppendLine("            first = false;");
        sb.AppendLine("        }");
    }
}
