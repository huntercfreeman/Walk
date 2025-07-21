using Microsoft.AspNetCore.Components;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.Extensions.CompilerServices.Displays.Internals;

public partial class FunctionSyntaxDisplay : ComponentBase
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;
    
    [Parameter, EditorRequired]
    public SyntaxViewModel SyntaxViewModel { get; set; } = default!;
    
    private string GetIdentifierText(ISyntaxNode node)
    {
        return SyntaxViewModel.GetIdentifierText(node);
    }
    
    private string GetTextFromTextSpan(TextEditorTextSpan textSpan)
    {
        return SyntaxViewModel.GetTextFromTextSpan(textSpan);
    }
}
