using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.TextEditor.RazorLib.ComponentRenderers.Models;

public interface ITextEditorDiagnosticRenderer
{
    public TextEditorDiagnostic Diagnostic { get; set; }
}