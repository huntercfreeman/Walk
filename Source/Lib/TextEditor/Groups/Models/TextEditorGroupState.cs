namespace Walk.TextEditor.RazorLib.Groups.Models;

public record struct TextEditorGroupState
{
    public TextEditorGroupState()
    {
        GroupList = new();
    }

    /// <summary>
    /// This somewhat arbitrarily represents the "main" text editor group
    /// so that the most often seen text editor group doesn't require a search
    /// but is instead always known to not be null and available.
    /// </summary>
    public TextEditorGroup EditorTextEditorGroup { get; set; }
    public List<TextEditorGroup> GroupList { get; init; }
}
