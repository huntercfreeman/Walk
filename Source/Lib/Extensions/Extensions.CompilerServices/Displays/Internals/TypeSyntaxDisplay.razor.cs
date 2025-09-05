using Microsoft.AspNetCore.Components;
using Walk.TextEditor.RazorLib;
using Walk.Extensions.CompilerServices.Syntax.Values;

namespace Walk.Extensions.CompilerServices.Displays.Internals;

public partial class TypeSyntaxDisplay : ComponentBase
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;
    
    [Parameter, EditorRequired]
    public SyntaxViewModel SyntaxViewModel { get; set; } = default!;
    
    [Parameter]
    public TypeReference TypeReference { get; set; } = default;
}
