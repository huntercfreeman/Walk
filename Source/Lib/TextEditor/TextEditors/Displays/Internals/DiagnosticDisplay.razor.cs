using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.ComponentRenderers.Models;
using Microsoft.AspNetCore.Components;

namespace Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;

public partial class DiagnosticDisplay : ComponentBase, ITextEditorDiagnosticRenderer
{
    [Parameter, EditorRequired]
    public TextEditorDiagnostic Diagnostic { get; set; } = default!;
}