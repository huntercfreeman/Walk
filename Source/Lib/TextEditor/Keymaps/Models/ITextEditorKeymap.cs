using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.JavaScriptObjects.Models;

namespace Walk.TextEditor.RazorLib.Keymaps.Models;

/// <summary>
/// Are you not just writing the name of the keymap?
/// (or some unique identifier).
/// into local storage?
/// </summary>
public interface ITextEditorKeymap
{
	public string DisplayName { get; }

	public Key<KeymapLayer> GetLayer(bool hasSelection);

    public string GetCursorCssClassString();

    public string GetCursorCssStyleString(
        TextEditorModel textEditorModel,
        TextEditorViewModel textEditorViewModel,
        TextEditorOptions textEditorOptions);
	
	public ValueTask HandleEvent(
    	TextEditorComponentData componentData,
	    Key<TextEditorViewModel> viewModelKey,
	    string key,
        string code,
        bool ctrlKey,
        bool shiftKey,
        bool altKey,
        bool metaKey);
}