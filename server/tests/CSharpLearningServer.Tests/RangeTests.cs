namespace CSharpLearningServer.Tests;

/// <summary>
/// Tests that the reported token range is accurate.
/// A correct range means the hover highlights exactly the keyword.
/// </summary>
public class RangeTests
{
  [Fact]
  public void Range_MatchesTokenPosition_Abstract()
  {
    var code = "public abstract class Animal {}";
    // "abstract" starts at col 7, ends at col 15
    var result = Hover.At(code, 0, 7);

    Assert.NotNull(result.Range);
    Assert.Equal(0, result.Range.StartLine);
    Assert.Equal(7, result.Range.StartCharacter);
    Assert.Equal(0, result.Range.EndLine);
    Assert.Equal(15, result.Range.EndCharacter);
  }

  [Fact]
  public void Range_MatchesTokenPosition_Sealed()
  {
    var code = "public sealed class Dog {}";
    // "sealed" starts at col 7, ends at col 13
    var result = Hover.At(code, 0, 7);

    Assert.NotNull(result.Range);
    Assert.Equal(0, result.Range.StartLine);
    Assert.Equal(7, result.Range.StartCharacter);
    Assert.Equal(0, result.Range.EndLine);
    Assert.Equal(13, result.Range.EndCharacter);
  }

  [Fact]
  public void Range_OnMultilineCode_ReportsCorrectLine()
  {
    var code = """
            class Foo
            {
                public virtual void Bar() {}
            }
            """;
    var lines = code.Split('\n');
    int virtualLine = -1;
    for (int i = 0; i < lines.Length; i++)
    {
      if (lines[i].Contains("virtual"))
      {
        virtualLine = i;
        break;
      }
    }

    var col = lines[virtualLine].IndexOf("virtual", StringComparison.Ordinal);
    var result = Hover.At(code, virtualLine, col);

    Assert.NotNull(result.Range);
    Assert.Equal(virtualLine, result.Range.StartLine);
    Assert.Equal(col, result.Range.StartCharacter);
    Assert.Equal(virtualLine, result.Range.EndLine);
    Assert.Equal(col + "virtual".Length, result.Range.EndCharacter);
  }

  [Fact]
  public void NullRange_WhenNoHover()
  {
    var code = "int x = 42;";
    var result = Hover.At(code, 0, 4); // 'x' identifier

    Assert.Null(result.Range);
  }
}
