namespace Walk.TextEditor.RazorLib.TextEditors.Models;

/// <summary>
/// This type is intended to represent a line within a flat list.
/// The 'LineIndex' is just a marker for the offset within the flat list, not actually multi-dimensional list.
/// </summary>
public record struct TextEditorVirtualizationLine
{
    /// <summary>
    /// TODO: This constructor is massive and likely doesn't need to copy so much data around.
    /// </summary>
	public TextEditorVirtualizationLine(
	    int lineIndex,
	    int position_StartInclusiveIndex,
	    int position_EndExclusiveIndex,
	    int virtualizationSpan_StartInclusiveIndex,
	    int virtualizationSpan_EndExclusiveIndex,
	    double leftInPixels,
	    string topCssValue,
	    string gutterCssStyle,
        string lineCssStyle,
        string lineNumberString)
	{
		LineIndex = lineIndex;
	    Position_StartInclusiveIndex = position_StartInclusiveIndex;
	    Position_EndExclusiveIndex = position_EndExclusiveIndex;
	    VirtualizationSpan_StartInclusiveIndex = virtualizationSpan_StartInclusiveIndex;
	    VirtualizationSpan_EndExclusiveIndex = virtualizationSpan_EndExclusiveIndex;
	    LeftInPixels = leftInPixels;
	    TopCssValue = topCssValue;
	    GutterCssStyle = gutterCssStyle;
        LineCssStyle = lineCssStyle;
        LineNumberString = lineNumberString;
	}
	
	public TextEditorVirtualizationLine(
	    int lineIndex,
	    int position_StartInclusiveIndex,
	    int position_EndExclusiveIndex,
	    int virtualizationSpan_StartInclusiveIndex,
	    int virtualizationSpan_EndExclusiveIndex,
	    double leftInPixels,
	    string topCssValue)
	{
		LineIndex = lineIndex;
	    Position_StartInclusiveIndex = position_StartInclusiveIndex;
	    Position_EndExclusiveIndex = position_EndExclusiveIndex;
	    VirtualizationSpan_StartInclusiveIndex = virtualizationSpan_StartInclusiveIndex;
	    VirtualizationSpan_EndExclusiveIndex = virtualizationSpan_EndExclusiveIndex;
	    LeftInPixels = leftInPixels;
	    TopCssValue = topCssValue;
	}
	
	public int LineIndex { get; }
    public int Position_StartInclusiveIndex { get; }
    public int Position_EndExclusiveIndex { get; }
    public int VirtualizationSpan_StartInclusiveIndex { get; set; }
    public int VirtualizationSpan_EndExclusiveIndex { get; set; }
    public double LeftInPixels { get; }
    public string TopCssValue { get; }
    public string GutterCssStyle { get; set; }
    public string LineCssStyle { get; set; }
    public string LineNumberString { get; set; }
}