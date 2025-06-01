namespace Walk.TextEditor.RazorLib.Lexers.Models;

public record struct TextEditorTextSpan
{
	public TextEditorTextSpan(
	    int startInclusiveIndex,
	    int endExclusiveIndex,
	    byte decorationByte,
	    ResourceUri resourceUri)
	{
		StartInclusiveIndex = startInclusiveIndex;
	    EndExclusiveIndex = endExclusiveIndex;
	    DecorationByte = decorationByte;
	    ResourceUri = resourceUri;
	}
	
	public TextEditorTextSpan(
	    int startInclusiveIndex,
	    int endExclusiveIndex,
	    byte decorationByte,
	    ResourceUri resourceUri,
	    string text)
	{
		StartInclusiveIndex = startInclusiveIndex;
	    EndExclusiveIndex = endExclusiveIndex;
	    DecorationByte = decorationByte;
	    ResourceUri = resourceUri;
		Text = text
	}

	public int StartInclusiveIndex { get; set; }
    public int EndExclusiveIndex { get; set; }
    public byte DecorationByte { get; set; }
    public ResourceUri ResourceUri { get; set; }
	public string? Text { get; set; }
	
    public int Length => EndExclusiveIndex - StartInclusiveIndex;
    public bool ConstructorWasInvoked => ResourceUri.Value is not null;
}
