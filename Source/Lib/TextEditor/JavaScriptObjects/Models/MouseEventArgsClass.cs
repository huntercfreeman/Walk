namespace Walk.TextEditor.RazorLib.JavaScriptObjects.Models;

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
