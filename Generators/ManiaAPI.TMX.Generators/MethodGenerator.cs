using Microsoft.CodeAnalysis;
using System.Text;

namespace ManiaAPI.TMX.Generators;

[Generator]
public class MethodGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methods = context.CompilationProvider.SelectMany((compilation, token) =>
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
                .Where(x => x.Interfaces.Any(i => i.Name == "ITMX" || i.Name == "IMX"))
                .SelectMany(clientSymbol => clientSymbol.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(methodSymbol => methodSymbol.GetAttributes().Any(x => x.AttributeClass?.Name == "GetMethodAttribute")));
        });

        context.RegisterSourceOutput(methods, GeneratePartialMethods);
    }

    private void GeneratePartialMethods(SourceProductionContext context, IMethodSymbol symbol)
    {
        var clientSymbol = symbol.ContainingType;
        var route = symbol.GetAttributes()
            .FirstOrDefault(x => x.AttributeClass?.Name == "GetMethodAttribute")?.ConstructorArguments[0].Value as string
            ?? throw new Exception("Route is null");

        var sb = new StringBuilder();

        sb.AppendLine("using System.Net.Http.Json;");
        sb.AppendLine("using System.Text;");
        sb.AppendLine("using System.Diagnostics;");
        sb.AppendLine();
        sb.Append("namespace ");
        sb.Append(clientSymbol.ContainingNamespace.ToDisplayString());
        sb.AppendLine(";");
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

        var hasParameters = symbol.Parameters.Length > 0 && symbol.Parameters[0].Type.Name != "CancellationToken";

        if (hasParameters)
        {
            sb.Append("        var sb = new StringBuilder(\"");
            sb.Append(route);
            sb.AppendLine("\");");
            sb.Append("        ");
            sb.Append(symbol.Parameters[0].Name);
            sb.AppendLine(".AppendQueryString(sb);");
            sb.AppendLine();
            sb.AppendLine("        using var response = await Client.GetAsync(sb.ToString(), cancellationToken);");
        }
        else
        {
            sb.Append("        using var response = await Client.GetAsync(\"");
            sb.Append(route);
            sb.AppendLine("\", cancellationToken);");
        }

        sb.AppendLine();
        sb.AppendLine("        response.EnsureSuccessStatusCode();");
        sb.AppendLine();
        sb.AppendLine("        Debug.WriteLine($\"{route}\n{await response.Content.ReadAsStringAsync(cancellationToken)}\");");
        sb.AppendLine();
        sb.Append("        return await response.Content.ReadFromJsonAsync(TMXJsonContext.Default.");

        var returnSymbol = symbol.ReturnType;

        if (returnSymbol.Name == "Task")
        {
            returnSymbol = (returnSymbol as INamedTypeSymbol)?.TypeArguments[0] ?? throw new Exception("This should not happen");
        }

        sb.Append(Utils.GetJsonContextPropertyName(returnSymbol));

        var defaultValue = returnSymbol is IArrayTypeSymbol ? "[]" : "new()";
        sb.Append(", cancellationToken) ?? ");
        sb.Append(defaultValue);
        sb.AppendLine(";");

        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"{clientSymbol.Name}.{symbol.Name}.cs", sb.ToString());
    }
}
