using ManiaAPI.MCP.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ManiaAPI.MCP.Tests;

public class GeneratorTests
{
    [Test]
    public async Task GeneratorEmitsTypedReadInvokersAndSuppliesCancellation()
    {
        var syntax = CSharpSyntaxTree.ParseText("""
            namespace ManiaAPI.NadeoAPI
            {
                public interface INadeoLiveServices
                {
                    System.Threading.Tasks.Task<string> GetMapAsync(string mapUid, System.Threading.CancellationToken cancellationToken = default);
                    System.Threading.Tasks.Task DeleteMapAsync(string mapUid);
                }
            }
            """);

        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));

        var compilation = CSharpCompilation.Create("GeneratorTest", [syntax], references);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new CatalogGenerator().AsSourceGenerator());
        driver = driver.RunGenerators(compilation);

        var result = driver.GetRunResult();
        var source = result.GeneratedTrees.Single().ToString();

        await Assert.That(result.Diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error)).IsFalse();
        await Assert.That(source).Contains("nadeo.live.map");
        await Assert.That(source).Contains("context.ClientAsync<global::ManiaAPI.NadeoAPI.INadeoLiveServices>");
        await Assert.That(source).Contains("cancellationToken");
        await Assert.That(source).Contains("ReadOnly = false");
    }
}
