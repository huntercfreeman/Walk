using Walk.TextEditor.RazorLib.Virtualizations.Models;

namespace Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

public struct TextEditorLineIndexCacheEntry
{
	public TextEditorLineIndexCacheEntry(
		string topCssValue,
		string leftCssValue,
		string lineNumberString,
		int hiddenLineCount,
		VirtualizationLine virtualizationLine)
	{
		TopCssValue = topCssValue;
		LeftCssValue = leftCssValue;
		LineNumberString = lineNumberString;
		HiddenLineCount = hiddenLineCount;
		VirtualizationLine = virtualizationLine;
	}

    public string TopCssValue { get; set; }
    public string LeftCssValue { get; set; }
    public string LineNumberString { get; set; }
    public int HiddenLineCount { get; set; }
    public VirtualizationLine VirtualizationLine { get; set; }
}
