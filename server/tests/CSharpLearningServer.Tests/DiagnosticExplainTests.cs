namespace CSharpLearningServer.Tests;

/// <summary>
/// Tests for the diagnostic explanation pipeline: fallback behavior,
/// no-diagnostic scenarios, edge cases, range accuracy, and selection logic.
/// </summary>
public class DiagnosticExplainTests
{
  // ── No diagnostic scenarios ──

  [Fact]
  public void ValidCode_ReturnsNoDiagnosticMessage()
  {
    var code = "class X { }";
    var result = Diag.At(code, 0, 6); // cursor on 'X'

    Assert.NotNull(result.Markdown);
    Assert.Contains("No compiler diagnostic found", result.Markdown);
    Assert.Null(result.Range);
  }

  [Fact]
  public void ValidCode_WithMethod_ReturnsNoDiagnosticMessage()
  {
    var code = """
        class X
        {
            void M() { System.Console.WriteLine(1); }
        }
        """;
    var result = Diag.ForToken(code, "WriteLine");

    Assert.Contains("No compiler diagnostic found", result.Markdown);
    Assert.Null(result.Range);
  }

  // ── Unknown diagnostic ID fallback ──

  [Fact]
  public void UnknownDiagnosticId_ReturnsFallbackTemplate()
  {
    // Use a diagnostic that is unlikely to be in the curated set
    // CS0159: goto to non-existent label
    var code = """
        class X
        {
            void M()
            {
                goto nowhere;
            }
        }
        """;
    var result = Diag.ForToken(code, "nowhere");

    Assert.NotNull(result.Markdown);
    // Fallback should contain the diagnostic ID and guidance
    Assert.Contains("CS0159", result.Markdown);
    Assert.Contains("Compiler", result.Markdown);
    Assert.Contains("Microsoft", result.Markdown); // doc link reference
  }

  // ── Edge cases ──

  [Fact]
  public void EmptyText_ReturnsNoDiagnostic()
  {
    var result = Diag.At("", 0, 0);

    Assert.NotNull(result.Markdown);
    Assert.Contains("No compiler diagnostic found", result.Markdown);
  }

  [Fact]
  public void NegativeLine_ReturnsNoDiagnostic()
  {
    var result = Diag.At("class X {}", -1, 0);

    Assert.Contains("No compiler diagnostic found", result.Markdown);
  }

  [Fact]
  public void LineBeyondEnd_ReturnsNoDiagnostic()
  {
    var result = Diag.At("class X {}", 99, 0);

    Assert.Contains("No compiler diagnostic found", result.Markdown);
  }

  [Fact]
  public void CharacterBeyondLineEnd_DoesNotCrash()
  {
    // Code with an error, cursor way past end of line
    var code = """
        class X
        {
            void M() { int y = foo; }
        }
        """;
    var result = Diag.At(code, 2, 999);

    // Should not throw; may find nearby diagnostic or return no-diagnostic
    Assert.NotNull(result.Markdown);
  }

  [Fact]
  public void SeverelyMalformedCode_DoesNotCrash()
  {
    var code = "class { {{ }} }} void (( int ;;; ===";
    var result = Diag.At(code, 0, 0);

    // Should not throw; should find at least one syntax error
    Assert.NotNull(result.Markdown);
  }

  [Fact]
  public void WhitespaceOnly_ReturnsNoDiagnostic()
  {
    var result = Diag.At("    \n    \n", 0, 2);

    Assert.Contains("No compiler diagnostic found", result.Markdown);
  }

  // ── Range correctness ──

  [Fact]
  public void Range_PointsToErrorLocation()
  {
    // 'Foobar' is the undefined type — diagnostic should point to it
    var code = """
        class X
        {
            void M() { Foobar x = null; }
        }
        """;
    var result = Diag.ForToken(code, "Foobar");

    Assert.NotNull(result.Range);
    // The range should be on the same line as 'Foobar'
    var lines = code.Split('\n');
    int foobarLine = -1;
    int foobarCol = -1;
    for (int i = 0; i < lines.Length; i++)
    {
      var idx = lines[i].IndexOf("Foobar", StringComparison.Ordinal);
      if (idx >= 0)
      {
        foobarLine = i;
        foobarCol = idx;
        break;
      }
    }

    Assert.Equal(foobarLine, result.Range.StartLine);
    Assert.Equal(foobarCol, result.Range.StartCharacter);
    Assert.Equal(foobarCol + "Foobar".Length, result.Range.EndCharacter);
  }

  [Fact]
  public void Range_HasNonZeroWidth()
  {
    var code = """
        class X
        {
            void M() { int y = foo; }
        }
        """;
    var result = Diag.ForToken(code, "foo");

    Assert.NotNull(result.Range);
    Assert.True(
        result.Range.EndCharacter > result.Range.StartCharacter ||
        result.Range.EndLine > result.Range.StartLine,
        "Diagnostic range should have non-zero width.");
  }

  // ── Diagnostic selection logic ──

  [Fact]
  public void CursorOnDiagnostic_SelectsThatDiagnostic()
  {
    // Two errors: 'aaa' and 'bbb' are both undefined
    var code = """
        class X
        {
            void M()
            {
                int x = aaa;
                int y = bbb;
            }
        }
        """;
    // Cursor on 'bbb' should select the CS0103 for 'bbb', not 'aaa'
    var result = Diag.ForToken(code, "bbb");

    Assert.NotNull(result.Markdown);
    Assert.Contains("CS0103", result.Markdown);

    // The range should point to 'bbb', not 'aaa'
    var lines = code.Split('\n');
    int bbbLine = -1;
    for (int i = 0; i < lines.Length; i++)
    {
      if (lines[i].Contains("bbb"))
      {
        bbbLine = i;
        break;
      }
    }
    Assert.NotNull(result.Range);
    Assert.Equal(bbbLine, result.Range.StartLine);
  }

  [Fact]
  public void CursorBetweenDiagnostics_SelectsNearest()
  {
    // Two errors on different lines, cursor in between on a blank line
    var code = """
        class X
        {
            void M()
            {
                int x = aaa;

                int y = bbb;
            }
        }
        """;
    // Cursor on the blank line between the two errors (line 5, col 0)
    var result = Diag.At(code, 5, 0);

    Assert.NotNull(result.Markdown);
    // Should find one of the diagnostics (closest one)
    Assert.Contains("CS0103", result.Markdown);
  }

  [Fact]
  public void PrefersErrorOverWarning_AtSameLocation()
  {
    // This code triggers CS0029 (error: can't convert string to int)
    // which is an error, not a warning
    var code = """
        class X
        {
            void M() { int x = "hello"; }
        }
        """;
    var result = Diag.ForToken(code, "hello");

    Assert.NotNull(result.Markdown);
    // Should get an error-level diagnostic, not a warning
    Assert.Contains("CS0029", result.Markdown);
  }

  // ── IncludeExamples toggle ──

  [Fact]
  public void IncludeExamples_True_ContainsExampleSection()
  {
    var code = """
        class X
        {
            void M() { int y = foo; }
        }
        """;
    var lines = code.Split('\n');
    int fooLine = -1;
    int fooCol = -1;
    for (int i = 0; i < lines.Length; i++)
    {
      var idx = lines[i].IndexOf("foo", StringComparison.Ordinal);
      if (idx >= 0)
      {
        fooLine = i;
        fooCol = idx;
        break;
      }
    }

    var result = Diag.At(code, fooLine, fooCol, includeExamples: true);

    Assert.NotNull(result.Markdown);
    Assert.Contains("Example", result.Markdown);
  }

  [Fact]
  public void IncludeExamples_False_OmitsExampleSection()
  {
    var code = """
        class X
        {
            void M() { int y = foo; }
        }
        """;
    var lines = code.Split('\n');
    int fooLine = -1;
    int fooCol = -1;
    for (int i = 0; i < lines.Length; i++)
    {
      var idx = lines[i].IndexOf("foo", StringComparison.Ordinal);
      if (idx >= 0)
      {
        fooLine = i;
        fooCol = idx;
        break;
      }
    }

    var result = Diag.At(code, fooLine, fooCol, includeExamples: false);

    Assert.NotNull(result.Markdown);
    Assert.DoesNotContain("Example", result.Markdown);
  }

  // ── Related concept integration ──

  [Fact]
  public void CS0535_IncludesInterfaceRelatedConcept()
  {
    var code = """
        interface IShape { double Area(); }
        class Circle : IShape { }
        """;
    var result = Diag.ForToken(code, "Circle");

    Assert.NotNull(result.Markdown);
    Assert.Contains("Related concept", result.Markdown);
    Assert.Contains("interface", result.Markdown);
  }

  [Fact]
  public void CS0120_IncludesStaticRelatedConcept()
  {
    var code = """
        class X
        {
            int count = 0;
            static void Main()
            {
                System.Console.WriteLine(count);
            }
        }
        """;
    var result = Diag.ForToken(code, "count);");

    Assert.NotNull(result.Markdown);
    Assert.Contains("Related concept", result.Markdown);
    Assert.Contains("static", result.Markdown);
  }

  [Fact]
  public void CS0534_IncludesAbstractAndOverrideRelatedConcepts()
  {
    var code = """
        abstract class Shape { public abstract double Area(); }
        class Circle : Shape { }
        """;
    var result = Diag.ForToken(code, "Circle");

    Assert.NotNull(result.Markdown);
    Assert.Contains("Related concept", result.Markdown);
    Assert.Contains("abstract", result.Markdown);
    Assert.Contains("override", result.Markdown);
  }

  // ── Multiple diagnostics on same line ──

  [Fact]
  public void MultipleDiagnosticsOnSameLine_CursorSelectsCorrectOne()
  {
    // Two undefined names on the same line
    var code = """
        class X
        {
            void M() { int a = xxx; int b = yyy; }
        }
        """;
    var result = Diag.ForToken(code, "yyy");

    Assert.NotNull(result.Markdown);
    Assert.NotNull(result.Range);

    // Range should point to 'yyy', not 'xxx'
    var lines = code.Split('\n');
    for (int i = 0; i < lines.Length; i++)
    {
      var idx = lines[i].IndexOf("yyy", StringComparison.Ordinal);
      if (idx >= 0)
      {
        Assert.Equal(idx, result.Range.StartCharacter);
        break;
      }
    }
  }

  // ── Large file stress test ──

  [Fact]
  public void LargeFile_WithErrorAtEnd_DoesNotCrash()
  {
    var lines = new List<string> { "namespace Big;" };
    for (int i = 0; i < 500; i++)
    {
      lines.Add($"class C{i} {{ public void M() {{ }} }}");
    }
    // Add an error at the end
    lines.Add("class Broken { void M() { int x = foo; } }");
    var code = string.Join("\n", lines);

    var lastLine = lines.Count - 1;
    var fooCol = lines[lastLine].IndexOf("foo", StringComparison.Ordinal);
    var result = Diag.At(code, lastLine, fooCol);

    Assert.NotNull(result.Markdown);
    Assert.Contains("CS0103", result.Markdown);
  }
}
