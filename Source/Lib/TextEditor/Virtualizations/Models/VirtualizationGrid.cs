using System.Diagnostics;
using System.Text;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.Cursors.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Displays;
using Walk.TextEditor.RazorLib.Virtualizations.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

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
public class VirtualizationGrid
{
	public static VirtualizationGrid Empty { get; } = new(
        Array.Empty<VirtualizationLine>(),
        new List<VirtualizationSpan>(),
        totalWidth: 0,
        totalHeight: 0,
        resultWidth: 0,
        resultHeight: 0,
        left: 0,
        top: 0,
        componentData: null);

	/// <summary>Measurements are in pixels</summary>
    public VirtualizationGrid(
        VirtualizationLine[] entries,
        List<VirtualizationSpan> virtualizationSpanList,
        int totalWidth,
        int totalHeight,
        int resultWidth,
        int resultHeight,
        double left,
        int top,
        TextEditorComponentData? componentData)
    {
        EntryList = entries;
        VirtualizationSpanList = virtualizationSpanList;
        TotalWidth = totalWidth;
        TotalHeight = totalHeight;
        VirtualWidth = resultWidth;
        VirtualHeight = resultHeight;
        VirtualLeft = left;
        VirtualTop = top;
        ComponentData = componentData;
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
    
    public TextEditorComponentData? ComponentData { get; set; }
    
    // Active is the one given to the UI after the current was validated and found to be valid.
    public TextEditorRenderBatch RenderBatch { get; set; }
    
    public string? InlineUiWidthStyleCssString { get; set; }
	public bool InlineUiWidthStyleCssStringIsOutdated { get; set; }
	
	public bool CursorIsOnHiddenLine { get; set; } = false;
    
    public int ShouldScroll { get; set; }
    
    public int UseLowerBoundInclusiveLineIndex { get; set; }
    public int UseUpperBoundExclusiveLineIndex { get; set; }
    public (int Position_LowerInclusiveIndex, int Position_UpperExclusiveIndex) SelectionBoundsInPositionIndexUnits { get; set; }
    
    public List<(string CssClassString, int StartInclusiveIndex, int EndExclusiveIndex)> FirstPresentationLayerGroupList { get; set; } = new();
	public List<(string PresentationCssClass, string PresentationCssStyle)> FirstPresentationLayerTextSpanList { get; set; } = new();
	
    public List<(string CssClassString, int StartInclusiveIndex, int EndExclusiveIndex)> LastPresentationLayerGroupList { get; set; } = new();
	public List<(string PresentationCssClass, string PresentationCssStyle)> LastPresentationLayerTextSpanList { get; set; } = new();
	
	public List<string> InlineUiStyleList { get; set; } = new();
    
    public List<string> SelectionStyleList { get; set; } = new List<string>();
    
    public List<CollapsePoint> VirtualizedCollapsePointList { get; set; } = new();
    public int VirtualizedCollapsePointListVersion { get; set; }
    
    private List<TextEditorTextSpan> VirtualizedTextSpanList { get; set; } = new();
    private List<TextEditorTextSpan> OutTextSpansList { get; set; } = new();
    
    /// <summary>Pixels (px)</summary>
	public int LineHeight { get; set; }
	
	/// <summary>Pixels (px)</summary>
	public int TextEditor_Width { get; set; }
	/// <summary>Pixels (px)</summary>
	public int TextEditor_Height { get; set; }
	
	/// <summary>Pixels (px)</summary>
	public int Scroll_Width { get; set; }
	/// <summary>Pixels (px)</summary>
	public int Scroll_Height { get; set; }
	/// <summary>Pixels (px)</summary>
	public int Scroll_Left { get; set; }
	/// <summary>Pixels (px)</summary>
	public int Scroll_Top { get; set; }
	
	public bool Scroll_LeftChanged { get; set; }
    public bool Scroll_TopChanged { get; set; }
	
	/// <summary>
    /// Each individual line number is a separate "gutter".
    /// Therefore, the UI in a loop will use a StringBuilder to .Append(...)
    /// - _lineHeightStyleCssString
    /// - _gutterWidthStyleCssString
    /// - _gutterPaddingStyleCssString
    ///
    /// Due to this occuring within a loop, it is presumed that
    /// pre-calculating all 3 strings together would be best.
    ///
    /// Issue with this: anytime any of the variables change that are used
    /// in this calculation, you need to re-calculate this string field,
    /// and it is a D.R.Y. code / omnipotent knowledge
    /// that this needs re-calculated nightmare.
    /// </summary>
    public string Gutter_HeightWidthPaddingCssStyle { get; set; }
    
	public string Gutter_PaddingCssStyle { get; set; }
    public string Gutter_WidthCssStyle { get; set; }
    
    /// <summary>
    /// Pixels (px)
    ///
    /// The initial value cannot be 0 else any text editor without a gutter cannot detect change on the initial render.
    /// Particularly, whatever the double subtraction -- absolute value precision -- check is, it has to be greater a difference than that.
    /// </summary>
    public int ViewModelGutterWidth { get; set; } = -2;
    /// <summary>Pixels (px)</summary>
    private int ViewModelScrollLeft { get; set; }
    
    public string ScrollbarSection_LeftCssStyle { get; set; }
    
    public string LineHeightStyleCssString { get; set; }
    
    public string BodyStyle { get; set; } = $"width: 100%; left: 0;";
    
    /// <summary>Pixels (px)</summary>
    public string ScrollbarSizeCssValue { get; set; }
    
    public bool PreviousIncludeHeader { get; set; }
    public bool PreviousIncludeFooter { get; set; }
    public string PreviousGetHeightCssStyleResult { get; set; } = "height: calc(100%);";
    
    public string VerticalVirtualizationBoundaryStyleCssString { get; set; } = "height: 0px;";
	public string HorizontalVirtualizationBoundaryStyleCssString { get; set; } = "width: 0px;";
	public string BothVirtualizationBoundaryStyleCssString { get; set; } = "width: 0px; height: 0px;";
	
    public string PersonalWrapperCssClass { get; set; }
    public string PersonalWrapperCssStyle { get; set; }
    
    public string CursorCssClassBlinkAnimationOn { get; set; }
    public string CursorCssClassBlinkAnimationOff { get; set; }
	
	// _ = "di_te_text-editor-cursor " + BlinkAnimationCssClass + " " + _activeRenderBatch.Options.Keymap.GetCursorCssClassString();
	public string BlinkAnimationCssClass => ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.ViewModelApi.CursorShouldBlink
        ? CursorCssClassBlinkAnimationOn
        : CursorCssClassBlinkAnimationOff;

	public string WrapperCssClass { get; private set; }
    public string WrapperCssStyle { get; private set; }
    
    /// <summary>
    /// Share this StringBuilder when used for rendering and no other function is currently using it.
    /// (i.e.: only use this for methods that were invoked from the .razor file)
    /// </summary>
    public StringBuilder UiStringBuilder { get; set; } = new();
    
    private Key<TextEditorViewModel> _seenViewModelKey = Key<TextEditorViewModel>.Empty;
    
    
    
    public string GutterCssStyle { get; set; }
    public string GutterSectionCssStyle { get; set; }
    // string RowSection_GetRowStyleCss(int lineIndex)
    public string CursorCssStyle { get; set; } = string.Empty;
    public string CaretRowCssStyle { get; set; } = string.Empty;
    public string VERTICAL_SliderCssStyle { get; set; }
    public string HORIZONTAL_SliderCssStyle { get; set; }
    public string HORIZONTAL_ScrollbarCssStyle { get; set; }
    // string PresentationGetCssStyleString(int position_LowerInclusiveIndex, int position_UpperExclusiveIndex, int lineIndex)

    // IReadOnlyList<TextEditorTextSpan> PresentationVirtualizeAndShiftTextSpans(IReadOnlyList<TextEditorTextModification> textModifications, IReadOnlyList<TextEditorTextSpan> inTextSpanList)

    // string GetTextSelectionStyleCss(int position_LowerInclusiveIndex, int position_UpperExclusiveIndex, int lineIndex)

    // public void GetSelection() SelectionStyleList

    // public void SetWrapperCssAndStyle()
    // WrapperCssClass
    // PersonalWrapperCssClass
    // WrapperCssStyle
    // PersonalWrapperCssStyle
    // SetRenderBatchConstants();
    // InvokeTextEditorWrapperCssStateChanged();
    
    // ConstructVirtualizationStyleCssStrings()
    // HorizontalVirtualizationBoundaryStyleCssString
    // VerticalVirtualizationBoundaryStyleCssString
    // BothVirtualizationBoundaryStyleCssString
    
    /* private void GetPresentationLayer(
    	List<Key<TextEditorPresentationModel>> presentationLayerKeysList,
    	List<(string CssClassString, int StartInclusiveIndex, int EndExclusiveIndex)> presentationLayerGroupList,
    	List<(string PresentationCssClass, string PresentationCssStyle)> presentationLayerTextSpanList) */
    
    // private void GetInlineUiStyleList()
    // InlineUiWidthStyleCssString
    // InlineUiWidthStyleCssStringIsOutdated
    // InlineUiStyleList
}
