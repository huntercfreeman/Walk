
namespace Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

public struct TextEditorLineIndexCacheEntry
{
	public TextEditorLineIndexCacheEntry(
		string topCssValue,
		string leftCssValue,
		string lineNumberString,
		int hiddenLineCount,
		int lineIndex,
	    int position_StartInclusiveIndex,
	    int position_EndExclusiveIndex,
	    int virtualizationSpan_StartInclusiveIndex,
	    int virtualizationSpan_EndExclusiveIndex,
	    int widthInPixels,
	    int heightInPixels,
	    double leftInPixels,
	    int topInPixels,
	    string gutterCssStyle,
	    string lineCssStyle)
	{
		TopCssValue = topCssValue;
		LeftCssValue = leftCssValue;
		LineNumberString = lineNumberString;
		HiddenLineCount = hiddenLineCount;
		LineIndex = lineIndex;
	    Position_StartInclusiveIndex = position_StartInclusiveIndex;
	    Position_EndExclusiveIndex = position_EndExclusiveIndex;
	    VirtualizationSpan_StartInclusiveIndex = virtualizationSpan_StartInclusiveIndex;
	    VirtualizationSpan_EndExclusiveIndex = virtualizationSpan_EndExclusiveIndex;
	    WidthInPixels = widthInPixels;
	    HeightInPixels = heightInPixels;
	    LeftInPixels = leftInPixels;
	    TopInPixels = topInPixels;
	    GutterCssStyle = gutterCssStyle;
	    LineCssStyle = lineCssStyle;
	}

    public string TopCssValue { get; set; }
    public string LeftCssValue { get; set; }
    public string LineNumberString { get; set; }
    public int HiddenLineCount { get; set; }
    public int LineIndex { get; }
    public int Position_StartInclusiveIndex { get; }
    public int Position_EndExclusiveIndex { get; }
    public int VirtualizationSpan_StartInclusiveIndex { get; set; }
    public int VirtualizationSpan_EndExclusiveIndex { get; set; }
    public int WidthInPixels { get; }
    public int HeightInPixels { get; }
    public double LeftInPixels { get; }
    public int TopInPixels { get; }
    public string GutterCssStyle { get; }
    public string LineCssStyle { get; }
}
