using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.Extensions.DotNet.CompilerServices.Models;

public record struct CompilerServiceEditorState(Key<TextEditorViewModel> TextEditorViewModelKey)
{
    public CompilerServiceEditorState() : this(Key<TextEditorViewModel>.Empty)
    {
        
    }
}
