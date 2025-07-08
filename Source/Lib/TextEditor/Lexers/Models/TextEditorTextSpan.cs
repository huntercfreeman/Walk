namespace Walk.TextEditor.RazorLib.Lexers.Models;

public record struct TextEditorTextSpan
{
	public TextEditorTextSpan(
		int StartInclusiveIndex,
	    int EndExclusiveIndex,
	    byte DecorationByte,
	    string SourceText)
	{
		this.StartInclusiveIndex = StartInclusiveIndex;
		this.EndExclusiveIndex = EndExclusiveIndex;
		this.DecorationByte = DecorationByte;
		
		// !!! WARNING THIS CODE IS DUPLICATED IN OTHER CONSTRUCTORS. !!!
		if (Text is null && StartInclusiveIndex < SourceText.Length && EndExclusiveIndex <= SourceText.Length && EndExclusiveIndex >= StartInclusiveIndex)
		{
			Text = SourceText.Substring(StartInclusiveIndex, EndExclusiveIndex - StartInclusiveIndex);
			Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.StringAllocation++;
		}
	}

    /// <summary>
    /// This constructor is used for text spans where their
    /// <see cref="EndExclusiveIndex"/> is the current position
    /// of a <see cref="StringWalker"/>.
    /// </summary>
    public TextEditorTextSpan(
            int StartInclusiveIndex,
            StringWalker stringWalker,
            byte decorationByte)
        : this(
              StartInclusiveIndex,
              stringWalker.PositionIndex,
              decorationByte,
              stringWalker.SourceText)
    {
		
    }
    
    /// <summary>
    /// This constructor is being used to
    /// experiment with not holding a reference
    /// to the <see cref="SourceText"/> (2024-07-27)
    ///
    /// It is a bit clumsy since I'm still taking in
    /// the source text as an argument.
    ///
    /// But the source text and the 'getTextPrecalculatedResult'
    /// share the same datatype and position in the constructor
    /// otherwise.
    /// </summary>
    public TextEditorTextSpan(
	    int startInclusiveIndex,
	    int endExclusiveIndex,
	    byte decorationByte,
	    string sourceText,
	    string getTextPrecalculatedResult)
    {
    	StartInclusiveIndex = startInclusiveIndex;
		EndExclusiveIndex = endExclusiveIndex;
		DecorationByte = decorationByte;
		Text = getTextPrecalculatedResult;
    }
    
    public TextEditorTextSpan(
        int startInclusiveIndex,
	    int endExclusiveIndex,
	    byte decorationByte,
	    string sourceText,
	    TextEditorService textEditorService)
    {
    	StartInclusiveIndex = startInclusiveIndex;
		EndExclusiveIndex = endExclusiveIndex;
		DecorationByte = decorationByte;
		
		if (textEditorService is null)
		{
			// !!! WARNING THIS CODE IS DUPLICATED IN OTHER CONSTRUCTORS. !!!
			if (Text is null && StartInclusiveIndex < sourceText.Length && EndExclusiveIndex <= sourceText.Length && EndExclusiveIndex >= StartInclusiveIndex)
			{
				Text = sourceText.Substring(StartInclusiveIndex, EndExclusiveIndex - StartInclusiveIndex);
				Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.StringAllocation++;
			}
		}
		else
		{
			Text = textEditorService.EditContext_GetText(
	        	sourceText.AsSpan(StartInclusiveIndex, EndExclusiveIndex - StartInclusiveIndex));
		}
    }
	
	public int StartInclusiveIndex { get; set; }
    public int EndExclusiveIndex { get; set; }
    public byte DecorationByte { get; set; }
    public string? Text { get; set; }
	
    public int Length => EndExclusiveIndex - StartInclusiveIndex;
    public bool ConstructorWasInvoked => Text is not null;
}