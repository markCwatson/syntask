
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Server.Models;
using Server.Utils;

namespace Server.Services;

public static class DiagnosticExplanationService
{
    public static DiagnosticExplanationResponse GetDiagnosticExplanation(DiagnosticExplanationRequest request)
    {
        var sourceText = SourceText.From(request.Text);

        if (request.Line < 0 || request.Line >= sourceText.Lines.Count)
        {
            return NoDiagnosticResponse();
        }

        var line = sourceText.Lines[request.Line];
        var cursorPosition = Math.Min(
            line.Start + request.Character,
            Math.Max(0, sourceText.Length - 1)
        );

        var tree = CSharpSyntaxTree.ParseText(sourceText);
        var semanticModel = SemanticAnalyzer.GetSemanticModel(tree);

        // Collect both syntax and semantic diagnostics
        var diagnostics = new List<Diagnostic>();

        // Syntax diagnostics are always available from the tree
        diagnostics.AddRange(tree.GetDiagnostics());

        // Semantic diagnostics require a successful compilation
        if (semanticModel is not null)
        {
            diagnostics.AddRange(semanticModel.GetDiagnostics());
        }

        // Filter to errors and warnings only (ignore hidden/info)
        var relevant = diagnostics
            .Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning)
            .Where(d => d.Location.IsInSource)
            .ToList();

        if (relevant.Count == 0)
        {
            return NoDiagnosticResponse();
        }

        // Find the diagnostic closest to the cursor position
        var best = SelectBestDiagnostic(relevant, cursorPosition);

        if (best is null)
        {
            return NoDiagnosticResponse();
        }

        var markdown = DiagnosticExplanations.GetExplanation(best, request);
        var range = ToRange(best.Location, sourceText);

        return new DiagnosticExplanationResponse(markdown, range);
    }

    /// <summary>
    /// Selects the best diagnostic at or nearest the cursor.
    /// Prefers diagnostics whose span contains the cursor, then the closest one.
    /// </summary>
    private static Diagnostic? SelectBestDiagnostic(List<Diagnostic> diagnostics, int cursorPosition)
    {
        // First: prefer diagnostics whose span contains the cursor
        var containing = diagnostics
            .Where(d => d.Location.SourceSpan.Contains(cursorPosition) ||
                        d.Location.SourceSpan.Start == cursorPosition)
            .OrderBy(d => d.Location.SourceSpan.Length) // prefer tightest span
            .ThenByDescending(d => d.Severity)           // prefer errors over warnings
            .FirstOrDefault();

        if (containing is not null)
        {
            return containing;
        }

        // Fallback: nearest diagnostic by distance to cursor
        return diagnostics
            .OrderBy(d =>
            {
                var span = d.Location.SourceSpan;
                if (cursorPosition < span.Start)
                    return span.Start - cursorPosition;
                if (cursorPosition > span.End)
                    return cursorPosition - span.End;
                return 0;
            })
            .ThenByDescending(d => d.Severity)
            .First();
    }

    private static HoverRange ToRange(Location location, SourceText sourceText)
    {
        var span = location.SourceSpan;
        var start = sourceText.Lines.GetLinePosition(span.Start);
        var end = sourceText.Lines.GetLinePosition(span.End);

        return new HoverRange(
            start.Line,
            start.Character,
            end.Line,
            end.Character
        );
    }

    private static DiagnosticExplanationResponse NoDiagnosticResponse()
    {
        return new DiagnosticExplanationResponse(
            "No compiler diagnostic found at this position.",
            null
        );
    }
}
