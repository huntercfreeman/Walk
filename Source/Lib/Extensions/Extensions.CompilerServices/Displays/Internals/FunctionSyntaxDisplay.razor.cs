using Microsoft.AspNetCore.Components;
using Walk.TextEditor.RazorLib;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.Extensions.CompilerServices.Displays.Internals;

public partial class FunctionSyntaxDisplay : ComponentBase
{
	[Inject]
	private TextEditorService TextEditorService { get; set; } = null!;
	
	[Parameter, EditorRequired]
	public SyntaxViewModel SyntaxViewModel { get; set; } = default!;
	
	private string GetTextFromToken(SyntaxToken token)
	{
	    var model = TextEditorService.ModelApi.GetOrDefault(SyntaxViewModel.ResourceUri);
	    
	    if (model.PersistentState.CompilerService is IExtendedCompilerService extendedCompilerService)
	        return extendedCompilerService.GetTextFromToken(token, SyntaxViewModel.ResourceUri);
	    
	    return token.TextSpan.Text(model.GetAllText(), TextEditorService);
	}
}
