using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.Extensions.DotNet.CompilerServices.Models;

public interface ICompilerServiceEditorService
{
	public event Action? CompilerServiceEditorStateChanged;
	
	public CompilerServiceEditorState GetCompilerServiceEditorState();

    public void ReduceSetTextEditorViewModelKeyAction(Key<TextEditorViewModel> textEditorViewModelKey);
}
