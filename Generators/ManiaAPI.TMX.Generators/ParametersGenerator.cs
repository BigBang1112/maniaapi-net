using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Text;

namespace ManiaAPI.TMX.Generators;

[Generator]
public class ParametersGenerator : IIncrementalGenerator
{
    private const bool Debug = false;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        if (Debug && !Debugger.IsAttached)
        {
            Debugger.Launch();
        }

        var types = context.CompilationProvider.SelectMany((compilation, token) =>
        {
            var maniaApiTmxNamespace = compilation.GlobalNamespace
                .GetNamespaceMembers()
                .FirstOrDefault(x => x.Name == "ManiaAPI")
                .GetNamespaceMembers()
                .FirstOrDefault(x => x.Name == "TMX");

            return maniaApiTmxNamespace.GetTypeMembers()
                .Where(x => x.Interfaces.Any(x => x.Name == "IClient"))
                .SelectMany(clientSymbol => clientSymbol.GetTypeMembers()
                    .Where(typeSymbol => typeSymbol.GetAttributes().Any(x => x.AttributeClass?.Name == "ParametersAttribute")));
        });

        context.RegisterSourceOutput(types, GeneratePartialParameters);
    }

    private void GeneratePartialParameters(SourceProductionContext context, INamedTypeSymbol parametersSymbol)
    {
        var clientSymbol = parametersSymbol.ContainingType;
        var resultSymbol = parametersSymbol.GetAttributes()
            .FirstOrDefault(x => x.AttributeClass?.Name == "ParametersAttribute")?.AttributeClass?.TypeArguments[0] as INamedTypeSymbol
            ?? throw new Exception("Result is null");

        var sb = new StringBuilder();

        sb.AppendLine("using System.Runtime.InteropServices;");
        sb.AppendLine("using System.Text;");
        sb.AppendLine("using System.Net;");
        sb.AppendLine();
        sb.AppendLine("namespace ManiaAPI.TMX;");
        sb.AppendLine();
        sb.Append("public partial class ");
        sb.AppendLine(clientSymbol.Name);
        sb.AppendLine("{");
        sb.AppendLine("    /// <remarks>Be careful using <c>default</c> value. It also disables all fields.</remarks>");
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
            else if (propSymbol.Type.Name == "String")
            {
                sb.Append("            if (!string.IsNullOrEmpty(");
                sb.Append(propSymbol.Name);
                sb.AppendLine("))");
                sb.AppendLine("            {");
                sb.AppendLine("                if (first) sb.Append('?');");
                sb.AppendLine("                else sb.Append('&');");
                sb.Append("                sb.Append(\"");
                sb.Append(propSymbol.Name.ToLowerInvariant());
                sb.AppendLine("=\");");
                sb.Append("                sb.Append(WebUtility.UrlEncode(");
                sb.Append(propSymbol.Name);
                sb.AppendLine("));");
                sb.AppendLine("                first = false;");
                sb.AppendLine("            }");
            }
            else if (propSymbol.Type.Kind == SymbolKind.ArrayType)
            {
                sb.Append("            if (");
                sb.Append(propSymbol.Name);
                sb.AppendLine(" is not null)");
                sb.AppendLine("            {");
                sb.AppendLine("                if (first) sb.Append('?');");
                sb.AppendLine("                else sb.Append('&');");
                sb.Append("                sb.Append(\"");
                sb.Append(propSymbol.Name.ToLowerInvariant());
                sb.AppendLine("=\");");
                sb.Append("                sb.Append(string.Join(\"%2C\", ");
                sb.Append(propSymbol.Name);
                sb.AppendLine("));");
                sb.AppendLine("                first = false;");
                sb.AppendLine("            }");
            }
            else
            {
                sb.Append("            // TODO: ");
                sb.Append(propSymbol.Name);
                sb.AppendLine(" is not supported yet");
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

        context.AddSource($"{clientSymbol.Name}.{parametersSymbol.Name}.cs", sb.ToString());
    }
}
