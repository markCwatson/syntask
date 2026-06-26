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
    // Assembly.Location is empty in single-file apps, so fall back to
    // the trusted assemblies list provided by the runtime.
#pragma warning disable IL3000 // handled by fallback below
    var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
#pragma warning restore IL3000
    if (string.IsNullOrEmpty(runtimeDir))
    {
      // Single-file publish: use trusted platform assemblies to find the runtime dir
      var trustedAssemblies = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string);
      if (trustedAssemblies is not null)
      {
        var first = trustedAssemblies.Split(Path.PathSeparator).FirstOrDefault();
        if (first is not null)
        {
          runtimeDir = Path.GetDirectoryName(first);
        }
      }
    }

    if (string.IsNullOrEmpty(runtimeDir))
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
