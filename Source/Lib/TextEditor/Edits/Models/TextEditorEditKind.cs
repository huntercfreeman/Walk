namespace Walk.TextEditor.RazorLib.Edits.Models;

public enum TextEditorEditKind
{
	Constructor,
	Insert,
	Backspace,
	Delete,
	DeleteSelection,
	OtherOpen,
	OtherClose,
}
