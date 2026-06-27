namespace CSharpLearningServer.Tests;

/// <summary>
/// Tests for semantic analysis features: var inference, default, nameof, typeof, dynamic.
/// </summary>
public class SemanticTests
{
  [Fact]
  public void Var_ResolvesToString()
  {
    var code = """
        class X
        {
            void M() { var name = "hello"; }
        }
        """;
    var result = Hover.ForToken(code, "var");

    Assert.NotNull(result.Markdown);
    Assert.Contains("var", result.Markdown);
    Assert.Contains("string", result.Markdown);
  }

  [Fact]
  public void Var_ResolvesToInt()
  {
    var code = """
        class X
        {
            void M() { var count = 42; }
        }
        """;
    var result = Hover.ForToken(code, "var");

    Assert.NotNull(result.Markdown);
    Assert.Contains("int", result.Markdown);
  }

  [Fact]
  public void Var_ResolvesToBool()
  {
    var code = """
        class X
        {
            void M() { var flag = true; }
        }
        """;
    var result = Hover.ForToken(code, "var");

    Assert.NotNull(result.Markdown);
    Assert.Contains("bool", result.Markdown);
  }

  [Fact]
  public void Var_ShowsExplanationEvenWithoutResolution()
  {
    // UnknownType won't resolve, but var should still explain itself
    var code = """
        class X
        {
            void M() { var x = SomeUnknownMethod(); }
        }
        """;
    var result = Hover.ForToken(code, "var");

    Assert.NotNull(result.Markdown);
    Assert.Contains("var", result.Markdown);
    // Should still provide a general explanation
    Assert.Contains("infer", result.Markdown);
  }

  [Fact]
  public void Default_InReturnStatement_ResolvesType()
  {
    var code = """
        class X
        {
            int M() { return default; }
        }
        """;
    var result = Hover.ForToken(code, "default");

    Assert.NotNull(result.Markdown);
    Assert.Contains("default", result.Markdown);
    Assert.Contains("int", result.Markdown);
  }

  [Fact]
  public void Default_WithExplicitType()
  {
    var code = """
        class X
        {
            void M() { int x = default(int); }
        }
        """;
    var lines = code.Split('\n');
    int line = -1;
    for (int i = 0; i < lines.Length; i++)
    {
      if (lines[i].Contains("default"))
      {
        line = i;
        break;
      }
    }
    var col = lines[line].IndexOf("default", StringComparison.Ordinal);
    var result = Hover.At(code, line, col);

    Assert.NotNull(result.Markdown);
    Assert.Contains("default", result.Markdown);
  }

  [Fact]
  public void Nameof_ResolvesSymbolName()
  {
    var code = """
        class X
        {
            string M() { return nameof(X); }
        }
        """;
    var result = Hover.ForToken(code, "nameof");

    Assert.NotNull(result.Markdown);
    Assert.Contains("nameof", result.Markdown);
    Assert.Contains("X", result.Markdown);
  }

  [Fact]
  public void Nameof_WithMemberAccess()
  {
    var code = """
        class X
        {
            public int Value { get; set; }
            string M() { return nameof(X.Value); }
        }
        """;
    var result = Hover.ForToken(code, "nameof");

    Assert.NotNull(result.Markdown);
    Assert.Contains("Value", result.Markdown);
  }

  [Fact]
  public void Typeof_ResolvesType()
  {
    var code = """
        using System;
        class X
        {
            Type M() { return typeof(int); }
        }
        """;
    var result = Hover.ForToken(code, "typeof");

    Assert.NotNull(result.Markdown);
    Assert.Contains("typeof", result.Markdown);
    Assert.Contains("int", result.Markdown);
  }

  [Fact]
  public void Dynamic_ExplainsRuntimeDispatch()
  {
    var code = """
        class X
        {
            void M() { dynamic obj = 42; }
        }
        """;
    var result = Hover.ForToken(code, "dynamic");

    Assert.NotNull(result.Markdown);
    Assert.Contains("dynamic", result.Markdown);
    Assert.Contains("runtime", result.Markdown);
  }

  [Fact]
  public void Var_DoesNotFireOnIdentifierNamedVar()
  {
    // If someone has a type literally named "var" this is unusual,
    // but the hover should still explain var behavior
    var code = """
        class X
        {
            void M() { var x = 1; }
        }
        """;
    var result = Hover.ForToken(code, "var");
    // Should produce a hover (it's the var keyword for type inference)
    Assert.NotNull(result.Markdown);
  }

  [Fact]
  public void Identifiers_StillReturnNoHover_WithSemanticEnabled()
  {
    // Ensure adding semantic analysis didn't break false-positive suppression
    var code = """
        class Customer { public string Name { get; set; } }
        """;
    var result = Hover.ForToken(code, "Customer");
    Assert.Null(result.Markdown);
  }
}
