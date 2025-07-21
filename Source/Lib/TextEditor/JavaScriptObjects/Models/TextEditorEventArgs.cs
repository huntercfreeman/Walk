namespace Walk.TextEditor.RazorLib.JavaScriptObjects.Models;

/// <summary>
/// Buttons == -1 will signify null since that isn't a naturally occuring Buttons value.
///
/// This is an all in one event type for the text editor from JavaScript to C#.
///
/// The struct is quite large so be wary of minimizing copying.
/// The overhead of the garbage collection on text editor events though is massive
/// so even with the size of this struct, it is well worth it.
/// </summary>
public struct TextEditorEventArgs
{
    public string Key { get; set; }
    public string Code { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public long Buttons { get; set; }
    public bool ShiftKey { get; set; }
    public bool CtrlKey { get; set; }
    public bool AltKey { get; set; }
    public bool MetaKey { get; set; }
}
