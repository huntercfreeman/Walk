namespace Walk.TextEditor.RazorLib.Virtualizations.Models;

/// <summary>
/// This type is used to render the text editor's text on the UI.
///
/// A single List of 'VirtualizationEntry' is allocated each time
/// the UI is calculated.
///
/// Each 'VirtualizationEntry' is a representation of each line of text to display.
///
/// The text to display for each line is not stored in the 'VirtualizationEntry'.
///
/// The 'VirtualizationEntry' is only describing the position index span
/// of characters that should be rendered by mapping from the 'TextEditorModel'.
///
/// The 'VirtualizationBoundary's will ensure that the horizontal
/// and vertical scrollbars stay consistent, regardless of how much
/// text is "virtually" not being rendered.
///
/// ===============================================================================
/// 
/// Idea: look in the previous VirtualizationGrid to see if a line of text has already been calculated.
///       If it was already calculated then re-use the previous calculation.
///       Otherwise calculate it.
/// 
/// Remarks: this idea is in reference to a 'partially' changed virtualization grid.
///          Not to be confused with how the code currently will 're-use' the previous virtualization grid
///          if a new virtualization grid was not deemed necessary.
///          |
///          This is re-using the virtualization lines that are common between
///          two separate virtualization grids, that have some overlapping lines.
///
/// i.e.:
/// - Use for scrolling vertically, only calculate the lines that were not previously visible.
/// - Use for editing text, in the case that only 1 line of text is being edited,
///   	this would permit re-use of the other 29 lines (relative to the height of the text editor and font-size;
///       30 lines is what I see currently in mine.).
/// - Given how the UI is written, all 30 lines have to re-drawn yes.
///   	But, we still get to avoid the overhead of 'calculating what is to be drawn'.
///       i.e.: contiguous decoration bytes being grouped in the same '<span>'.
/// 
/// </summary>
public struct VirtualizationGrid
{
	public static VirtualizationGrid Empty { get; } = new(
        Array.Empty<VirtualizationLine>(),
        new List<VirtualizationSpan>(),
        totalWidth: 0,
        totalHeight: 0,
        resultWidth: 0,
        resultHeight: 0,
        left: 0,
        top: 0);

	/// <summary>Measurements are in pixels</summary>
    public VirtualizationGrid(
        VirtualizationLine[] entries,
        List<VirtualizationSpan> virtualizationSpanList,
        int totalWidth,
        int totalHeight,
        int resultWidth,
        int resultHeight,
        double left,
        int top)
    {
        EntryList = entries;
        VirtualizationSpanList = virtualizationSpanList;
        TotalWidth = totalWidth;
        TotalHeight = totalHeight;
        VirtualWidth = resultWidth;
        VirtualHeight = resultHeight;
        VirtualLeft = left;
        VirtualTop = top;
    }

    /// <summary>
    /// Do NOT use EntryList.Length.
    /// Use TextEditorViewModel.VirtualizationResultCount,
    /// because this array is allocated at a predicted size
    /// but it is possible that the count does not reach capacity.
    /// </summary>
    public VirtualizationLine[] EntryList { get; init; }
    public List<VirtualizationSpan> VirtualizationSpanList { get; init; }
    
    /// <summary>
    /// Measurements are in pixels.
    ///
    /// Width (including non-rendered elements).
    /// </summary>
    public int TotalWidth { get; init; }
    /// <summary>
    /// Measurements are in pixels
    ///
    /// Height (including non-rendered elements).
    /// </summary>
    public int TotalHeight { get; init; }
    /// <summary>
    /// Measurements are in pixels
    ///
    /// Width (only rendered elements).
    /// </summary>
    public int VirtualWidth { get; init; }
    /// <summary>
    /// Measurements are in pixels
    ///
    /// Height (only rendered elements).
    /// </summary>
    public int VirtualHeight { get; init; }
    /// <summary>
    /// Measurements are in pixels
    ///
    /// Lowest 'left' point where a rendered element is displayed.
    /// </summary>
    public double VirtualLeft { get; init; }
    /// <summary>
    /// Measurements are in pixels
    ///
    /// Lowest 'top' point where a rendered element is displayed.
    /// </summary>
    public int VirtualTop { get; init; }
}