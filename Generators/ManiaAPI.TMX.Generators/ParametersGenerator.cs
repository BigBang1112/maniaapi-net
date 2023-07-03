using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Text;

namespace ManiaAPI.TMX.Generators;

[Generator]
public class ParametersGenerator : ISourceGenerator
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

        var clientSymbols = maniaApiTmxNamespace.GetTypeMembers()
            .Where(x => x.Interfaces.Any(x => x.Name == "IClient"));

        foreach (var clientSymbol in clientSymbols)
        {
            foreach (var symbol in clientSymbol.GetTypeMembers())
            {
                var att = symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.Name == "ParametersAttribute")?.AttributeClass;

                if (att is null)
                {
                    continue;
                }

                context.AddSource($"{clientSymbol.Name}.{symbol.Name}.cs", GeneratePartialParameters(clientSymbol, symbol, att.TypeArguments[0]));
            }
        }
    }

    private string GeneratePartialParameters(INamedTypeSymbol clientSymbol, INamedTypeSymbol parametersSymbol, ITypeSymbol resultSymbol)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using System.Runtime.InteropServices;");
        sb.AppendLine("using System.Text;");
        sb.AppendLine();
        sb.AppendLine("namespace ManiaAPI.TMX;");
        sb.AppendLine();
        sb.Append("public partial class ");
        sb.AppendLine(clientSymbol.Name);
        sb.AppendLine("{");
        sb.AppendLine("    [StructLayout(LayoutKind.Auto)]");
        sb.Append("    public readonly partial record struct ");
        sb.AppendLine(parametersSymbol.Name);
        sb.AppendLine("    {");

        sb.Append("        public ");
        sb.Append(resultSymbol.Name);
        sb.AppendLine("Fields Fields { get; init; }");
        sb.AppendLine();
        sb.Append("        public ");
        sb.Append(parametersSymbol.Name);
        sb.AppendLine("()");
        sb.AppendLine("        {");
        sb.Append("            Fields = ");
        sb.Append(resultSymbol.Name);
        sb.AppendLine("Fields.All;");
        sb.AppendLine("        }");

        sb.AppendLine();

        sb.AppendLine("        internal bool AppendQueryString(StringBuilder sb)");
        sb.AppendLine("        {");
        sb.AppendLine("            var first = true;");

        foreach (var propSymbol in parametersSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            var atts = propSymbol.GetAttributes();

            sb.AppendLine();

            // this generation can be done better, but it works for now
            if (propSymbol.Type.Name == "Nullable")
            {
                var actualType = ((INamedTypeSymbol)propSymbol.Type).TypeArguments[0];
                var asNumber = actualType.Name == "Boolean" && atts.Any(x => x.AttributeClass?.Name == "AsNumberAttribute");

                sb.Append("            if (");
                sb.Append(propSymbol.Name);
                sb.AppendLine(".HasValue)");
                sb.AppendLine("            {");
                sb.AppendLine("                if (first) sb.Append('?');");
                sb.AppendLine("                else sb.Append('&');");
                sb.Append("                sb.Append(\"");
                sb.Append(propSymbol.Name.ToLowerInvariant());
                sb.AppendLine("=\");");
                sb.Append("                sb.Append(");

                if (actualType.TypeKind == TypeKind.Enum)
                {
                    sb.Append("(int)");
                }

                sb.Append(propSymbol.Name);

                if (asNumber)
                {
                    sb.AppendLine(".Value ? '1' : '0');");
                }
                else
                {
                    sb.AppendLine(".Value);");
                }

                sb.AppendLine("                first = false;");
                sb.AppendLine("            }");
            }
            else if (propSymbol.Type.IsValueType)
            {
                var asNumber = propSymbol.Type.Name == "Boolean" && atts.Any(x => x.AttributeClass?.Name == "AsNumberAttribute");

                sb.AppendLine("            if (first) sb.Append('?');");
                sb.AppendLine("            else sb.Append('&');");
                sb.Append("            sb.Append(\"");
                sb.Append(propSymbol.Name.ToLowerInvariant());
                sb.AppendLine("=\");");
                sb.Append("            sb.Append(");
                sb.Append(propSymbol.Name);

                if (asNumber)
                {
                    sb.AppendLine(" ? '1' : '0');");
                }
                else
                {
                    sb.AppendLine(");");
                }

                sb.AppendLine("            first = false;");
            }
            else
            {
                // TODO
            }
        }

        sb.AppendLine();
        sb.AppendLine("            if (first) sb.Append(\"?fields=\");");
        sb.AppendLine("            else sb.Append(\"&fields=\");");
        sb.AppendLine("            Fields.Append(sb);");
        sb.AppendLine();
        sb.AppendLine("            return !first;");

        sb.AppendLine("        }");

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}
