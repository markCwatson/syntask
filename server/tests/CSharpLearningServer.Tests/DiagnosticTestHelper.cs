using Server.Models;
using Server.Services;

namespace CSharpLearningServer.Tests;

/// <summary>
/// Helper to call DiagnosticExplanationService with minimal boilerplate.
/// </summary>
public static class Diag
{
  public static DiagnosticExplanationResponse At(string code, int line, int character,
      string detailLevel = "beginner", bool includeExamples = true)
  {
    return DiagnosticExplanationService.GetDiagnosticExplanation(
        new DiagnosticExplanationRequest(
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
  /// and returns the diagnostic explanation at that position.
  /// </summary>
  public static DiagnosticExplanationResponse ForToken(string code, string token)
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
