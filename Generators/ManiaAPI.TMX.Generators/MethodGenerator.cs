using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Text;

namespace ManiaAPI.TMX.Generators;

[Generator]
public class MethodGenerator : ISourceGenerator
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
            foreach (var symbol in clientSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                var att = symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.Name == "GetMethodAttribute");

                if (att?.AttributeClass is null)
                {
                    continue;
                }

                var jsonContextSymbol = att.AttributeClass.TypeArguments[0];
                var route = att.ConstructorArguments[0].Value as string ?? throw new Exception("Route is null");

                context.AddSource($"{clientSymbol.Name}.{symbol.Name}.cs", GeneratePartialMethods(clientSymbol, symbol, jsonContextSymbol, route));
            }
        }
    }

    private string GeneratePartialMethods(INamedTypeSymbol clientSymbol, IMethodSymbol symbol, ITypeSymbol jsonContextSymbol, string route)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using System.Net.Http.Json;");
        sb.AppendLine("using System.Text;");
        sb.AppendLine();
        sb.AppendLine("namespace ManiaAPI.TMX;");
        sb.AppendLine();
        sb.Append("public partial class ");
        sb.AppendLine(clientSymbol.Name);
        sb.AppendLine("{");
        sb.Append("    public ");

        if (symbol.IsVirtual)
        {
            sb.Append("virtual ");
        }

        sb.Append("partial async ");
        sb.Append(symbol.ReturnType);
        sb.Append(" ");
        sb.Append(symbol.Name);
        sb.Append("(");

        var firstParam = true;

        foreach (var parameter in symbol.Parameters)
        {
            if (!firstParam)
            {
                sb.Append(", ");
            }

            firstParam = false;

            sb.Append(parameter.Type);
            sb.Append(" ");
            sb.Append(parameter.Name);
        }

        sb.AppendLine(")");
        sb.AppendLine("    {");

        sb.AppendLine("        cancellationToken.ThrowIfCancellationRequested();");
        sb.AppendLine();
        sb.Append("        var sb = new StringBuilder(\"");
        sb.Append(route);
        sb.AppendLine("\");");
        sb.Append("        ");
        sb.Append(symbol.Parameters[0].Name);
        sb.AppendLine(".AppendQueryString(sb);");
        sb.AppendLine();
        sb.AppendLine("        using var response = await Client.GetAsync(sb.ToString(), cancellationToken);");
        sb.AppendLine();
        sb.AppendLine("        response.EnsureSuccessStatusCode();");
        sb.AppendLine();
        sb.Append("        return await response.Content.ReadFromJsonAsync(");

        if (jsonContextSymbol is not null)
        {
            sb.Append(jsonContextSymbol);
            sb.AppendLine(".TypeInfo, cancellationToken) ?? new();");
        }
        else
        {
            sb.AppendLine("cancellationToken: cancellationToken) ?? new();");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}
