using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Xunit;

namespace HandyCommandy.Generator.Test;

public class CommandLineParserSourceGeneratorTest
{
    [Fact]
    public void GeneratedCodeCompiles()
    {
        // Create the 'input' compilation that the generator will act on
        Compilation inputCompilation = CreateCompilation(@"
#nullable enable

HandyCommandy.Args cmdArgs = new HandyCommandy.ArgBuilder()
    .Option(""--episode <number>"", ""Download episode No. <number>"")
    .Option(""--keep"", ""Keeps temporary files"")
    .Option(""--ratio [ratio]"", ""Either 16:9, or a custom ratio"")
    .Run(args);
string episode = cmdArgs.Episode;
bool keep = cmdArgs.Keep;
string? ratio = cmdArgs.Ratio;
");

        // directly create an instance of the generator
        // (Note: in the compiler this is loaded from an assembly, and created via reflection at runtime)
        CommandLineParserSourceGenerator generator = new();

        // Create the driver that will control the generation, passing in our generator
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the generation pass
        // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

        // We can now assert things about the resulting compilation:
        Assert.Empty(diagnostics); // there were no diagnostics created by the generators
        Assert.Equal(2, outputCompilation.SyntaxTrees.Count()); // we have two syntax trees, the original 'user' provided one, and the one added by the generator
        var generatedCodeDiagnostics = outputCompilation.GetDiagnostics().ToList();
        Assert.Empty(generatedCodeDiagnostics); // verify the compilation with the added source has no diagnostics

        // Or we can look at the results directly:
        GeneratorDriverRunResult runResult = driver.GetRunResult();

        // The runResult contains the combined results of all generators passed to the driver
        Assert.Single(runResult.GeneratedTrees);
        Assert.Empty(runResult.Diagnostics);

        // Or you can access the individual results on a by-generator basis
        GeneratorRunResult generatorResult = runResult.Results[0];
        Assert.Equal(generator, generatorResult.Generator);
        Assert.Empty(generatorResult.Diagnostics);
        Assert.Single(generatorResult.GeneratedSources);
        Assert.Null(generatorResult.Exception);
    }

    private static Compilation CreateCompilation(string source)
    {
        return CSharpCompilation.Create(
            "compilation",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                Basic.Reference.Assemblies.Net60.SystemRuntime,
                Basic.Reference.Assemblies.Net60.SystemLinq,
            },
            new CSharpCompilationOptions(OutputKind.ConsoleApplication)
        );
    }
}
