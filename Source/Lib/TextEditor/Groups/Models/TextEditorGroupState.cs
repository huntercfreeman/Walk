namespace Walk.TextEditor.RazorLib.Groups.Models;

public record struct TextEditorGroupState
{
    public TextEditorGroupState()
    {
        GroupList = new();
    }

    public List<TextEditorGroup> GroupList { get; init; }
}