using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.TextEditor.RazorLib.CompilerServices;

/// <summary>
/// Use the <see cref="Id"/> to determine if two diagnostics
/// are reporting the same diagnostic but perhaps with differing
/// messages due to variable interpolation into the string.
/// </summary>
public record struct TextEditorDiagnostic(
    TextEditorDiagnosticLevel DiagnosticLevel,
    string Message,
    TextEditorTextSpan TextSpan,
    Guid Id);
