using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Server.Models;
using Server.Utils;

namespace Server.Services;

public static class HoverService
{
  public static HoverResponse GetHover(HoverRequest request)
  {
    var sourceText = SourceText.From(request.Text);

    if (request.Line < 0 || request.Line >= sourceText.Lines.Count)
    {
      return new HoverResponse(null, null);
    }

    var line = sourceText.Lines[request.Line];
    var absolutePosition = Math.Min(
        line.Start + request.Character,
        Math.Max(0, sourceText.Length - 1)
    );

    var tree = CSharpSyntaxTree.ParseText(sourceText);
    var root = tree.GetRoot();

    var token = root.FindToken(absolutePosition, findInsideTrivia: true);

    // Try syntax-only classification first (fast path)
    var result = CSharpFeatureClassifier.GetHover(token, sourceText, request);
    if (result.Markdown is not null)
    {
      return result;
    }

    // Try semantic analysis for tokens that need type resolution
    return SemanticHoverProvider.GetHover(token, tree, sourceText, request);
  }
}
