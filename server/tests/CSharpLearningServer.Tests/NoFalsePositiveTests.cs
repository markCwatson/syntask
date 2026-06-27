namespace CSharpLearningServer.Tests;

/// <summary>
/// Tests that identifiers, literals, and non-keyword tokens
/// do NOT produce a learning hover. This prevents noise.
/// </summary>
public class NoFalsePositiveTests
{
  [Theory]
  [InlineData("class Customer { }", "Customer")]
  [InlineData("int count = 42;", "count")]
  [InlineData("string name = \"hello\";", "name")]
  [InlineData("var x = Math.PI;", "Math")]
  [InlineData("var x = Math.PI;", "PI")]
  [InlineData("Console.WriteLine(\"hi\");", "Console")]
  [InlineData("Console.WriteLine(\"hi\");", "WriteLine")]
  public void Identifiers_ReturnNoHover(string code, string identifier)
  {
    var col = code.IndexOf(identifier, StringComparison.Ordinal);
    var result = Hover.At(code, 0, col);

    Assert.Null(result.Markdown);
  }

  [Theory]
  [InlineData("int x = 42;", "42")]
  [InlineData("string s = \"hello\";", "hello")]
  [InlineData("double d = 3.14;", "3")]
  public void Literals_ReturnNoHover(string code, string literal)
  {
    var col = code.IndexOf(literal, StringComparison.Ordinal);
    var result = Hover.At(code, 0, col);

    Assert.Null(result.Markdown);
  }

  [Theory]
  [InlineData("var x = 1 + 2;", "+")]
  [InlineData("if (x == 1) {}", "==")]
  [InlineData("var arr = new int[] { 1, 2 };", ",")]
  public void Operators_ReturnNoHover(string code, string op)
  {
    var col = code.IndexOf(op, StringComparison.Ordinal);
    var result = Hover.At(code, 0, col);

    Assert.Null(result.Markdown);
  }

  [Fact]
  public void TypeName_AfterNew_NoHover()
  {
    // "List" is an identifier, not a keyword
    var code = """
            class X
            {
                void M() { var x = new List<int>(); }
            }
            """;
    var lines = code.Split('\n');
    int line = -1;
    for (int i = 0; i < lines.Length; i++)
    {
      if (lines[i].Contains("List"))
      {
        line = i;
        break;
      }
    }

    var col = lines[line].IndexOf("List", StringComparison.Ordinal);
    var result = Hover.At(code, line, col);

    Assert.Null(result.Markdown);
  }
}
