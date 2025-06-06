using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.Extensions.DotNet.CompilerServices.Models;

public class CompilerServiceEditorService : ICompilerServiceEditorService
{
	private CompilerServiceEditorState _compilerServiceEditorState = new();
	
	public event Action? CompilerServiceEditorStateChanged;
	
	public CompilerServiceEditorState GetCompilerServiceEditorState() => _compilerServiceEditorState;

    public void ReduceSetTextEditorViewModelKeyAction(Key<TextEditorViewModel> textEditorViewModelKey)
    {
    	var inState = GetCompilerServiceEditorState();
    
        _compilerServiceEditorState = inState with
        { 
            TextEditorViewModelKey = textEditorViewModelKey
        };
        
        CompilerServiceEditorStateChanged?.Invoke();
        return;
    }
}
