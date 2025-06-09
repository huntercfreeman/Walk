namespace Walk.TextEditor.RazorLib.JavaScriptObjects.Models;

/// <summary>
/// If this does work make sure you check if you at any point
/// were setting the property while it was a class cause it won't propagate anymore.
///
/// Instead of nullable, buttons == -1 will be used to indicate null,
/// since -1 isn't a natural buttons value according the web docs.
/// </summary>
public struct MouseEventArgsStruct
{
    public MouseEventArgsStruct(
        double clientX,
        double clientY,
        long buttons)
    {
        ClientX = clientX;
        ClientY = clientY;
        Buttons = buttons;
    }

	public double ClientX { get; set; }
	public double ClientY { get; set; }
	public long Buttons { get; set; }
}

public class MouseEventArgsClass
{
    public MouseEventArgsClass()
    {
    }

    public MouseEventArgsClass(
        double clientX,
        double clientY,
        long buttons,
        bool shiftKey)
    {
        ClientX = clientX;
        ClientY = clientY;
        Buttons = buttons;
        ShiftKey = shiftKey;
    }

	public double ClientX { get; set; }
	public double ClientY { get; set; }
	public long Buttons { get; set; }
	public bool ShiftKey { get; set; }
}
