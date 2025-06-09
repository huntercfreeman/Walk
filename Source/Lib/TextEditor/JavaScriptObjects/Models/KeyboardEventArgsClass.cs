namespace Walk.TextEditor.RazorLib.JavaScriptObjects.Models;

public class KeyboardEventArgsClass
{
	public KeyboardEventArgsClass(
	    string key,
        string code,
        bool ctrlKey,
        bool shiftKey,
        bool altKey,
        bool metaKey)
	{
	    Key = key;
        Code = code;
        CtrlKey = ctrlKey;
        ShiftKey = shiftKey;
        AltKey = altKey;
        MetaKey = metaKey;
	}
	
	public string Key { get; set; }
    public string Code { get; set; }
    public bool CtrlKey { get; set; }
    public bool ShiftKey { get; set; }
    public bool AltKey { get; set; }
    public bool MetaKey { get; set; }
}
