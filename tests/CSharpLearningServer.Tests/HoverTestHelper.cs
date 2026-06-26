namespace CSharpLearningServer.Tests;

/// <summary>
/// Helper to call HoverService with minimal boilerplate.
/// </summary>
public static class Hover
{
  public static HoverResponse At(string code, int line, int character,
      string detailLevel = "beginner", bool includeExamples = true)
  {
    return HoverService.GetHover(new HoverRequest(
        Text: code,
        FilePath: null,
        Line: line,
        Character: character,
        DetailLevel: detailLevel,
        IncludeExamples: includeExamples
    ));
  }

  /// <summary>
  /// Finds the first occurrence of <paramref name="token"/> in the code
  /// and returns the hover at that position.
  /// </summary>
  public static HoverResponse ForToken(string code, string token)
  {
    var lines = code.Split('\n');
    for (int line = 0; line < lines.Length; line++)
    {
      var col = lines[line].IndexOf(token, StringComparison.Ordinal);
      if (col >= 0)
      {
        return At(code, line, col);
      }
    }

    throw new ArgumentException($"Token '{token}' not found in code.");
  }
}
