namespace CSharpLearningServer.Tests;

/// <summary>
/// Tests that keywords in different syntactic contexts produce
/// different, context-appropriate explanations.
/// </summary>
public class ContextSensitiveTests
{
  [Fact]
  public void Abstract_OnClass_ExplainsAbstractClass()
  {
    var code = "public abstract class Animal {}";
    var result = Hover.ForToken(code, "abstract");

    Assert.NotNull(result.Markdown);
    Assert.Contains("abstract", result.Markdown);
    Assert.Contains("class", result.Markdown);
    Assert.Contains("cannot be instantiated", result.Markdown);
  }

  [Fact]
  public void Abstract_OnMethod_ExplainsAbstractMember()
  {
    var code = """
            abstract class Shape
            {
                public abstract double Area();
            }
            """;
    // Find "abstract" on the method line (second occurrence)
    var lines = code.Split('\n');
    int methodLine = -1;
    for (int i = 0; i < lines.Length; i++)
    {
      if (lines[i].Contains("double Area"))
      {
        methodLine = i;
        break;
      }
    }

    var col = lines[methodLine].IndexOf("abstract", StringComparison.Ordinal);
    var result = Hover.At(code, methodLine, col);

    Assert.NotNull(result.Markdown);
    Assert.Contains("abstract", result.Markdown);
    Assert.Contains("member", result.Markdown);
  }

  [Fact]
  public void Static_OnClass_ExplainsStaticClass()
  {
    var code = "static class Helper {}";
    var result = Hover.ForToken(code, "static");

    Assert.NotNull(result.Markdown);
    Assert.Contains("static", result.Markdown);
    Assert.Contains("class", result.Markdown);
    Assert.Contains("cannot be instantiated", result.Markdown);
  }

  [Fact]
  public void Static_OnMember_ExplainsStaticMember()
  {
    var code = "class Counter { public static int Count; }";
    var col = code.IndexOf("static", StringComparison.Ordinal);
    var result = Hover.At(code, 0, col);
    Assert.Contains("type itself", result.Markdown);
  }

  [Fact]
  public void Readonly_OnStruct_ExplainsReadonlyStruct()
  {
    var code = "readonly struct Point { public double X; }";
    var result = Hover.ForToken(code, "readonly");

    Assert.NotNull(result.Markdown);
    Assert.Contains("readonly", result.Markdown);
    Assert.Contains("struct", result.Markdown);
  }

  [Fact]
  public void Readonly_OnField_ExplainsReadonlyField()
  {
    var code = """
            class Config
            {
                readonly string _name = "test";
            }
            """;
    var result = Hover.ForToken(code, "readonly");

    Assert.NotNull(result.Markdown);
    Assert.Contains("readonly", result.Markdown);
    Assert.Contains("constructor", result.Markdown);
  }

  [Fact]
  public void Using_AsDirective_ExplainsImport()
  {
    var code = "using System;";
    var result = Hover.ForToken(code, "using");

    Assert.NotNull(result.Markdown);
    Assert.Contains("directive", result.Markdown);
    Assert.Contains("namespace", result.Markdown);
  }

  [Fact]
  public void Using_AsStatement_ExplainsDisposal()
  {
    // Classic using statement (not using declaration) to get UsingStatementSyntax
    var code = """
        class X
        {
            void M()
            {
                using (var s = new System.IO.MemoryStream()) {}
            }
        }
        """;
    var lines = code.Split('\n');
    int usingLine = -1;
    for (int i = 0; i < lines.Length; i++)
    {
      if (lines[i].Contains("using ("))
      {
        usingLine = i;
        break;
      }
    }

    var col = lines[usingLine].IndexOf("using", StringComparison.Ordinal);
    var result = Hover.At(code, usingLine, col);

    Assert.NotNull(result.Markdown);
    Assert.Contains("disposed", result.Markdown);
  }

  [Fact]
  public void Where_OnGenericConstraint_ExplainsConstraint()
  {
    var code = "class Repo<T> where T : class, new() {}";
    // "where" appears after the generic
    var col = code.IndexOf("where", StringComparison.Ordinal);
    var result = Hover.At(code, 0, col);

    Assert.NotNull(result.Markdown);
    Assert.Contains("generic constraint", result.Markdown);
  }

  [Fact]
  public void This_OnExtensionMethod_ExplainsExtension()
  {
    var code = """
            static class Ext
            {
                public static bool IsEmpty(this string s) => s.Length == 0;
            }
            """;
    var lines = code.Split('\n');
    int paramLine = -1;
    for (int i = 0; i < lines.Length; i++)
    {
      if (lines[i].Contains("this string"))
      {
        paramLine = i;
        break;
      }
    }

    var col = lines[paramLine].IndexOf("this", StringComparison.Ordinal);
    var result = Hover.At(code, paramLine, col);

    Assert.NotNull(result.Markdown);
    Assert.Contains("extension method", result.Markdown);
  }

  [Fact]
  public void New_OnObjectCreation_ExplainsInstantiation()
  {
    var code = """
            class X
            {
                void M() { var list = new System.Collections.Generic.List<int>(); }
            }
            """;
    var lines = code.Split('\n');
    int newLine = -1;
    for (int i = 0; i < lines.Length; i++)
    {
      if (lines[i].Contains("new "))
      {
        newLine = i;
        break;
      }
    }

    var col = lines[newLine].IndexOf("new", StringComparison.Ordinal);
    var result = Hover.At(code, newLine, col);

    Assert.NotNull(result.Markdown);
    Assert.Contains("new", result.Markdown);
    Assert.Contains("instance", result.Markdown);
  }
}
