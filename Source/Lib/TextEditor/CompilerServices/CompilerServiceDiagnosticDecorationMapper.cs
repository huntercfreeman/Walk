using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.TextEditor.RazorLib.CompilerServices;

public class CompilerServiceDiagnosticDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (CompilerServiceDiagnosticDecorationKind)decorationByte;

        return decoration switch
        {
            CompilerServiceDiagnosticDecorationKind.None => string.Empty,
            CompilerServiceDiagnosticDecorationKind.DiagnosticError => "border-bottom: 2px solid var(--di_te_semantic-diagnostic-error-background-color);",
            CompilerServiceDiagnosticDecorationKind.DiagnosticHint => "border-bottom: 2px solid var(--di_te_semantic-diagnostic-hint-background-color);",
            CompilerServiceDiagnosticDecorationKind.DiagnosticSuggestion => "di_te_semantic-diagnostic-suggestion",
            CompilerServiceDiagnosticDecorationKind.DiagnosticWarning => "di_te_semantic-diagnostic-warning",
            CompilerServiceDiagnosticDecorationKind.DiagnosticOther => "di_te_semantic-diagnostic-other",
            _ => string.Empty,
        };
    }
}
