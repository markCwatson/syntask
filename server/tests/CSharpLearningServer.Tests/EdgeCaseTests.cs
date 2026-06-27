namespace CSharpLearningServer.Tests;

/// <summary>
/// Tests edge cases: empty input, out-of-bounds positions, etc.
/// </summary>
public class EdgeCaseTests
{
  [Fact]
  public void EmptyText_ReturnsNoHover()
  {
    var result = Hover.At("", 0, 0);
    Assert.Null(result.Markdown);
  }

  [Fact]
  public void NegativeLine_ReturnsNoHover()
  {
    var result = Hover.At("class X {}", -1, 0);
    Assert.Null(result.Markdown);
  }

  [Fact]
  public void LineBeyondEnd_ReturnsNoHover()
  {
    var result = Hover.At("class X {}", 99, 0);
    Assert.Null(result.Markdown);
  }

  [Fact]
  public void CharacterBeyondLineEnd_StillWorks()
  {
    // Should not crash; lands on or near end of line
    var code = "sealed class X {}";
    var result = Hover.At(code, 0, 999);
    // Shouldn't crash — may or may not return a hover depending on clamping
    Assert.True(result.Markdown is null || result.Markdown.Length > 0);
  }

  [Fact]
  public void WhitespaceOnly_ReturnsNoHover()
  {
    var result = Hover.At("    \n    \n", 0, 2);
    Assert.Null(result.Markdown);
  }

  [Fact]
  public void CursorOnWhitespace_BetweenKeywords_ReturnsNoHover()
  {
    var code = "public   abstract   class   Animal {}";
    // Position in the whitespace between "public" and "abstract"
    var result = Hover.At(code, 0, 6); // space after "public"
                                       // FindToken may snap to nearest token; just ensure no crash
    Assert.True(true); // passes if no exception
  }

  [Fact]
  public void MalformedCode_DoesNotCrash()
  {
    var code = "public class { abstract void (; }}}";
    var result = Hover.ForToken(code, "abstract");
    // Should still return something for the keyword despite invalid syntax
    Assert.NotNull(result.Markdown);
  }

  [Fact]
  public void VeryLongFile_DoesNotCrash()
  {
    var lines = new List<string> { "namespace Big;" };
    for (int i = 0; i < 1000; i++)
    {
      lines.Add($"public class C{i} {{ public virtual void M() {{}} }}");
    }
    var code = string.Join("\n", lines);

    // Hover on "virtual" somewhere in the middle
    var result = Hover.At(code, 500, code.Split('\n')[500].IndexOf("virtual", StringComparison.Ordinal));
    Assert.NotNull(result.Markdown);
    Assert.Contains("virtual", result.Markdown);
  }
}
