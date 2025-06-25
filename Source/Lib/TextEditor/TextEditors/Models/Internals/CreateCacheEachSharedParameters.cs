namespace Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

/// <summary>
/// Only one "TextEditorEditContext" runs at a time so having this object is thread safe.
///
/// This type avoids passing many parameters to a function `CreateCacheEach(...)`.
/// Which is invoked inside of a loop.
/// </summary>
public class CreateCacheEachSharedParameters
{
	public TextEditorModel Model { get; set; }
	public TextEditorViewModel ViewModel { get; set; }
	public TextEditorComponentData ComponentData { get; set; }
	public string TabKeyOutput { get; set; }
	public string SpaceKeyOutput { get; set; }
}
