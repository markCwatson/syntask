using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

/// <summary>
/// Creates a minimal CSharpCompilation from document text with enough
/// BCL references to resolve common types (int, string, List, Task, etc.).
/// This enables SemanticModel queries for var inference, default values, etc.
/// </summary>
public static class SemanticAnalyzer
{
  private static readonly MetadataReference[] CoreReferences = BuildCoreReferences();

  private static MetadataReference[] BuildCoreReferences()
  {
    var references = new List<MetadataReference>();

    // Get the runtime directory where core assemblies live
    var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
    if (runtimeDir is null)
    {
      return references.ToArray();
    }

    // Core assemblies needed for basic type resolution
    var assemblyNames = new[]
    {
      "System.Runtime.dll",
      "System.Collections.dll",
      "System.Linq.dll",
      "System.Threading.Tasks.dll",
      "System.Console.dll",
      "System.IO.dll",
      "System.Net.Http.dll",
      "netstandard.dll",
      "mscorlib.dll",
      "System.Private.CoreLib.dll",
    };

    foreach (var name in assemblyNames)
    {
      var path = Path.Combine(runtimeDir, name);
      if (File.Exists(path))
      {
        references.Add(MetadataReference.CreateFromFile(path));
      }
    }

    return references.ToArray();
  }

  public static SemanticModel? GetSemanticModel(SyntaxTree tree)
  {
    if (CoreReferences.Length == 0)
    {
      return null;
    }

    var compilation = CSharpCompilation.Create(
      "HoverAnalysis",
      syntaxTrees: [tree],
      references: CoreReferences,
      options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        .WithNullableContextOptions(NullableContextOptions.Enable)
    );

    return compilation.GetSemanticModel(tree);
  }
}
