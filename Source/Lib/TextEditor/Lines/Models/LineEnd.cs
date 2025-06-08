namespace Walk.TextEditor.RazorLib.Lines.Models;

public record struct LineEnd
{
    public static readonly LineEnd StartOfFile = new(0, 0, LineEndKind.StartOfFile);

    public LineEnd(
        int position_StartInclusiveIndex,
        int position_EndExclusiveIndex,
        LineEndKind lineEndKind,
        LineEndKind lineEndKindOriginal)
    {
        Position_StartInclusiveIndex = position_StartInclusiveIndex;
        Position_EndExclusiveIndex = position_EndExclusiveIndex;
        LineEndKind = lineEndKind;
        LineEndKindOriginal = lineEndKindOriginal;
    }
    
    public LineEnd(
        int position_StartInclusiveIndex,
        int position_EndExclusiveIndex,
        LineEndKind lineEndKind)
    {
        Position_StartInclusiveIndex = position_StartInclusiveIndex;
        Position_EndExclusiveIndex = position_EndExclusiveIndex;
        LineEndKind = lineEndKind;
        LineEndKindOriginal = LineEndKind.Unset;
    }

    /// <summary>
    /// Given: "Hello World!\nAbc123"<br/>
    /// Then: \n starts inclusively at position index 12
    /// </summary>
    public int Position_StartInclusiveIndex { get; init; }
    /// <summary>
    /// Given: "Hello World!\nAbc123"<br/>
    /// Then: \n ends exclusively at position index 13
    /// </summary>
    public int Position_EndExclusiveIndex { get; init; }
    /// <summary>
    /// Given: "Hello World!\nAbc123"<br/>
    /// Then: \n is <see cref="LineEndKind.LineFeed"/>
    /// </summary>
    public LineEndKind LineEndKind { get; init; }
    /// <summary>
    /// Mixed line endings support:
    /// The text editor will always standardize the line endings when opening a file.
    /// This property stores the original line ending so it can be swapped in when saving the file
    /// to avoid messing with files that have mixed line endings.
    /// </summary>
    public LineEndKind LineEndKindOriginal { get; init; }
}
