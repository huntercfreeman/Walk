using Walk.TextEditor.RazorLib.CompilerServices;
using Microsoft.AspNetCore.Components;

namespace Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;

public partial class DiagnosticDisplay : ComponentBase
{
    [Parameter, EditorRequired]
    public TextEditorDiagnostic Diagnostic { get; set; } = default!;
}