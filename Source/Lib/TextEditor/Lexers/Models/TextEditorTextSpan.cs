namespace Walk.TextEditor.RazorLib.Lexers.Models;

public record struct TextEditorTextSpan
{
	public TextEditorTextSpan(
		int StartInclusiveIndex,
	    int EndExclusiveIndex,
	    byte DecorationByte,
	    ResourceUri ResourceUri,
	    string SourceText)
	{
		this.StartInclusiveIndex = StartInclusiveIndex;
		this.EndExclusiveIndex = EndExclusiveIndex;
		this.DecorationByte = DecorationByte;
		this.ResourceUri = ResourceUri;
		
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
              stringWalker.ResourceUri,
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
		    ResourceUri resourceUri,
		    string sourceText,
		    string getTextPrecalculatedResult)
    {
    	StartInclusiveIndex = startInclusiveIndex;
		EndExclusiveIndex = endExclusiveIndex;
		DecorationByte = decorationByte;
		ResourceUri = resourceUri;
		Text = getTextPrecalculatedResult;
    }
	
	public int StartInclusiveIndex { get; set; }
    public int EndExclusiveIndex { get; set; }
    public byte DecorationByte { get; set; }
    public ResourceUri ResourceUri { get; set; }
    public string? Text { get; set; }
	
    public int Length => EndExclusiveIndex - StartInclusiveIndex;
    public bool ConstructorWasInvoked => ResourceUri.Value is not null;

    /// <summary>
    /// When using the record 'with' contextual keyword the <see cref="_text"/>
    /// might hold the cached value prior to the 'with' result.
    /// </summary>
    public string ClearTextCache()
    {
        return Text = null;
    }
    
    /// <summary>
    /// The method 'GetText()' will be invoked and cached prior to
    /// setting the 'SourceText' to null.
    ///
    /// This allows one to still get the text from the text span,
    /// but without holding a reference to the original text.
    /// </summary>
    public TextEditorTextSpan SetNullSourceText()
    {
    	// SourceText = null;
    	return this;
    }
    
    /// <summary>
    /// Argument 'text': The pre-calculated text to return when one invokes 'GetText()'
    /// instead of returning a null reference exception.
    /// </summary>
    public TextEditorTextSpan SetNullSourceText(string? text = null)
    {
    	Text = text;
    	// SourceText = null;
    	return this;
    }
}