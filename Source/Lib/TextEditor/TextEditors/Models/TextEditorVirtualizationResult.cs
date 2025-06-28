using System.Diagnostics;
using System.Text;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.JavaScriptObjects.Models;
using Walk.TextEditor.RazorLib.Cursors.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Displays;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

namespace Walk.TextEditor.RazorLib.TextEditors.Models;

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
public class TextEditorVirtualizationResult
{
    public const string DEFAULT_FONT_FAMILY = "monospace";

	public static TextEditorVirtualizationResult Empty { get; } = new(
        Array.Empty<TextEditorVirtualizationLine>(),
        new List<TextEditorVirtualizationSpan>(),
        totalWidth: 0,
        totalHeight: 0,
        resultWidth: 0,
        resultHeight: 0,
        left: 0,
        top: 0,
        componentData: null,
        model: null,
	    viewModel: null,
	    renderBatchPersistentState: null,
	    count: 0,
	    previousState: null);

	/// <summary>Measurements are in pixels</summary>
    public TextEditorVirtualizationResult(
        TextEditorVirtualizationLine[] entries,
        List<TextEditorVirtualizationSpan> virtualizationSpanList,
        int totalWidth,
        int totalHeight,
        int resultWidth,
        int resultHeight,
        double left,
        int top,
        TextEditorComponentData? componentData,
        TextEditorModel? model,
	    TextEditorViewModel? viewModel,
	    TextEditorRenderBatchPersistentState? renderBatchPersistentState,
	    int count,
	    TextEditorVirtualizationResult previousState)
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
        Model = model;
	    ViewModel = viewModel;
	    TextEditorRenderBatchPersistentState = renderBatchPersistentState;
	    Count = count;
	    _previousState = previousState;
	    
	    IsValid = Model is not null &&
			      ViewModel is not null &&
			      TextEditorRenderBatchPersistentState?.TextEditorOptions is not null;
    }
    
    private TextEditorVirtualizationResult _previousState;

    /// <summary>
    /// Do NOT use EntryList.Length.
    /// Use Count,
    /// because this array is allocated at a predicted size
    /// but it is possible that the count does not reach capacity.
    /// </summary>
    public TextEditorVirtualizationLine[] EntryList { get; init; }
    
    /// <summary>
    /// Do NOT use EntryList.Length.
    /// Use this property instead,
    /// because the array is allocated at a predicted size
    /// but it is possible that the count does not reach capacity.
    /// </summary>
    public int Count { get; set; }
    
    public List<TextEditorVirtualizationSpan> VirtualizationSpanList { get; init; }
    
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
	
	public TextEditorDimensions TextEditorDimensions { get; set; }
	
    public TextEditorModel? Model { get; set; }
    public TextEditorViewModel? ViewModel { get; set; }
    public TextEditorRenderBatchPersistentState? TextEditorRenderBatchPersistentState { get; set; }
    
    public bool IsValid { get; private set; }
        
    public TextEditorComponentData? ComponentData { get; set; }
    
    public string? InlineUiWidthStyleCssString { get; set; }
	
	
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
    
    public List<TextEditorTextSpan> VirtualizedTextSpanList { get; set; } = new();
    public List<TextEditorTextSpan> OutTextSpansList { get; set; } = new();
    
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
    
	
    public string Gutter_WidthCssStyle { get; set; }
    
    /// <summary>
    /// Pixels (px)
    ///
    /// The initial value cannot be 0 else any text editor without a gutter cannot detect change on the initial render.
    /// Particularly, whatever the double subtraction -- absolute value precision -- check is, it has to be greater a difference than that.
    /// </summary>
    public int GutterWidth { get; set; } = -2;
    
    public string LineHeightStyleCssString { get; set; }
    
    public string BodyStyle { get; set; } = $"width: 100%; left: 0;";
    
    public Key<TextEditorViewModel> _seenViewModelKey = Key<TextEditorViewModel>.Empty;
    
    public string GutterCssStyle { get; set; }
    public string GutterSectionCssStyle { get; set; }
    public string GutterColumnTopCss { get; set; }
    
    // string RowSection_GetRowStyleCss(int lineIndex)
    public string CursorCssStyle { get; set; } = string.Empty;
    public string CaretRowCssStyle { get; set; } = string.Empty;
    public string VERTICAL_SliderCssStyle { get; set; }
    public string HORIZONTAL_SliderCssStyle { get; set; }
    public string HORIZONTAL_ScrollbarCssStyle { get; set; }
    
    public int ScrollTop { get; set; }
    public int ScrollWidth { get; set; }
    public int ScrollHeight { get; set; }
    public int MarginScrollHeight { get; set; }
    
    /// <summary>Pixels (px)</summary>
    public int ScrollLeft { get; set; }
    
    public string ScrollbarSection_LeftCssStyle { get; set; }
	
	/// <summary>
	/// TODO: Rename 'CharAndLineMeasurements' to 'CharAndLineDimensions'...
	///       ...as to bring it inline with 'TextEditorDimensions' and 'ScrollbarDimensions'.
	/// </summary>
    public CharAndLineMeasurements CharAndLineMeasurements { get; set; }
    
    /// <summary>
	/// This property decides whether or not to re-calculate the virtualization result that gets displayed on the UI.
	/// </summary>
    public bool ShouldCalculateVirtualizationResult { get; set; }
    
    private static int _stopDebugConsoleWriteCount = 0;
    
    public void LineIndexCache_Create()
    {
        /*
    	if (_previousState.GutterWidth != ViewModel.GutterWidthInPixels)
    	{
    		GutterWidth = ViewModel.GutterWidthInPixels;
    		ComponentData.LineIndexCache.Clear();
    		
    		var widthInPixelsInvariantCulture = GutterWidth.ToString();
    		
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("width: ");
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(widthInPixelsInvariantCulture);
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
    		Gutter_WidthCssStyle = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
    		
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(LineHeightStyleCssString);
	        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(Gutter_WidthCssStyle);
	        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(ComponentData.Gutter_PaddingCssStyle);
    		Gutter_HeightWidthPaddingCssStyle = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
    		
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("width: calc(100% - ");
	        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(widthInPixelsInvariantCulture);
	        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px); left: ");
	        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(widthInPixelsInvariantCulture);
	        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
    		BodyStyle = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();

    		ViewModel.PersistentState.PostScrollAndRemeasure();
    		
    		HORIZONTAL_GetScrollbarHorizontalStyleCss();
    		HORIZONTAL_GetSliderHorizontalStyleCss();
    		
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("left: ");
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(widthInPixelsInvariantCulture);
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
    		ScrollbarSection_LeftCssStyle = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
    	}
    	else if (ScrollLeft != ViewModel.ScrollLeft)
    	{
    		ScrollLeft = ViewModel.ScrollLeft;
    		ComponentData.LineIndexCache.Clear();
    	}
    	else
    	{
    	    GutterWidth = _previousState.GutterWidth;
    		Gutter_WidthCssStyle = _previousState.Gutter_WidthCssStyle;
    		Gutter_HeightWidthPaddingCssStyle = _previousState.Gutter_HeightWidthPaddingCssStyle;
    		BodyStyle = _previousState.BodyStyle;
            HORIZONTAL_ScrollbarCssStyle = _previousState.HORIZONTAL_ScrollbarCssStyle;
            HORIZONTAL_SliderCssStyle = _previousState.HORIZONTAL_SliderCssStyle;
    		ScrollbarSection_LeftCssStyle = _previousState.ScrollbarSection_LeftCssStyle;
    	}
    	*/
    }
    
    /// <summary>
    /// Non loop related UI:
    /// --------------------
    /// - GutterColumnTopCss (specifically the column that provides background-color not the individual line numbers).
    /// - CursorIsOnHiddenLine
    /// - LineHeight
    /// - LineHeightStyleCssString
    /// - Gutter_HeightWidthPaddingCssStyle
    /// - TextEditor_Height
    /// - Scroll_Height
    /// - Scroll_Top
    /// - TextEditor_Width
    /// - Scroll_Width
    /// - Scroll_Left
    /// - VERTICAL_GetSliderVerticalStyleCss
    /// - HORIZONTAL_GetSliderHorizontalStyleCss
    /// - HORIZONTAL_GetScrollbarHorizontalStyleCss
    /// - ConstructVirtualizationStyleCssStrings
    ///
    /// </summary>
    public void CreateUi_NotCacheRelated()
    {
        if (!IsValid)
        {
        	DiagnoseIssues();
        	return;
        }
    
        string gutterColumnTopCssValue;
    	
	    if (ViewModel.Virtualization.Count > 0)
        	gutterColumnTopCssValue = ComponentData.LineIndexCache.Map[ViewModel.Virtualization.EntryList[0].LineIndex].TopCssValue;
        else
            gutterColumnTopCssValue = "0";
        
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("top: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(gutterColumnTopCssValue);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
        
        GutterColumnTopCss = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();

    	if (ViewModel.PersistentState.HiddenLineIndexHashSet.Contains(ViewModel.LineIndex))
	    	CursorIsOnHiddenLine = true;
    	else
    	    CursorIsOnHiddenLine = false;
    	
    	if (_previousState.LineHeight != ViewModel.Virtualization.CharAndLineMeasurements.LineHeight)
    	{
    		LineHeight = ViewModel.Virtualization.CharAndLineMeasurements.LineHeight;
			
			ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("height: ");
	        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(ViewModel.Virtualization.CharAndLineMeasurements.LineHeight.ToString());
	        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
	        LineHeightStyleCssString = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
	        
	        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(LineHeightStyleCssString);
	        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(Gutter_WidthCssStyle);
	        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(ComponentData.Gutter_PaddingCssStyle);
    		Gutter_HeightWidthPaddingCssStyle = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
		}
		else
		{
		    LineHeight = _previousState.LineHeight;
		    LineHeightStyleCssString = _previousState.LineHeightStyleCssString;
		    Gutter_HeightWidthPaddingCssStyle = _previousState.Gutter_HeightWidthPaddingCssStyle;
		}
		
		bool shouldCalculateVerticalSlider = false;
		bool shouldCalculateHorizontalSlider = false;
		bool shouldCalculateHorizontalScrollbar = false;
		
    	if (TextEditor_Height != ViewModel.Virtualization.TextEditorDimensions.Height)
    	{
    		TextEditor_Height = ViewModel.Virtualization.TextEditorDimensions.Height;
    		shouldCalculateVerticalSlider = true;
	    }
		
    	if (Scroll_Height != ViewModel.Virtualization.ScrollHeight)
    	{
    		Scroll_Height = ViewModel.Virtualization.ScrollHeight;
    		shouldCalculateVerticalSlider = true;
	    }
		
    	if (Scroll_Top != ViewModel.Virtualization.ScrollTop)
    	{
    		Scroll_Top = ViewModel.Virtualization.ScrollTop;
    		shouldCalculateVerticalSlider = true;
	    }
		
    	if (TextEditor_Width != ViewModel.Virtualization.TextEditorDimensions.Width)
    	{
    		TextEditor_Width = ViewModel.Virtualization.TextEditorDimensions.Width;
    		shouldCalculateHorizontalSlider = true;
    		shouldCalculateHorizontalScrollbar = true;
	    }
		
    	if (Scroll_Width != ViewModel.Virtualization.ScrollWidth)
    	{
    		Scroll_Width = ViewModel.Virtualization.ScrollWidth;
    		shouldCalculateHorizontalSlider = true;
	    }
		
    	if (Scroll_Left != ViewModel.Virtualization.ScrollLeft)
    	{
    		Scroll_Left = ViewModel.Virtualization.ScrollLeft;
    		shouldCalculateHorizontalSlider = true;
	    }

		if (shouldCalculateVerticalSlider)
			VERTICAL_GetSliderVerticalStyleCss();
		
		if (shouldCalculateHorizontalSlider)
			HORIZONTAL_GetSliderHorizontalStyleCss();
		
		if (shouldCalculateHorizontalScrollbar)
			HORIZONTAL_GetScrollbarHorizontalStyleCss();
    
    	ConstructVirtualizationStyleCssStrings();
    }
    
    public void CreateUi_IsCacheRelated()
    {
        GetCursorAndCaretRowStyleCss();
        GetSelection();
        
        GetPresentationLayer(
        	ViewModel.PersistentState.FirstPresentationLayerKeysList,
        	FirstPresentationLayerGroupList,
        	FirstPresentationLayerTextSpanList);
        	
        GetPresentationLayer(
        	ViewModel.PersistentState.LastPresentationLayerKeysList,
        	LastPresentationLayerGroupList,
        	LastPresentationLayerTextSpanList);
        
        if (VirtualizedCollapsePointListVersion != ViewModel.PersistentState.VirtualizedCollapsePointListVersion ||
        	_seenViewModelKey != ViewModel.PersistentState.ViewModelKey)
        {
        	VirtualizedCollapsePointList.Clear();
        
        	for (int i = 0; i < ViewModel.PersistentState.VirtualizedCollapsePointList.Count; i++)
        	{
        		VirtualizedCollapsePointList.Add(ViewModel.PersistentState.VirtualizedCollapsePointList[i]);
        	}
        	
        	GetInlineUiStyleList();
        	
        	_seenViewModelKey = ViewModel.PersistentState.ViewModelKey;
        	VirtualizedCollapsePointListVersion = ViewModel.PersistentState.VirtualizedCollapsePointListVersion;
        }
    }
    
    public string GetGutterStyleCss(string topCssValue)
    {
    	ComponentData.UiStringBuilder.Clear();
    
    	ComponentData.UiStringBuilder.Append("top: ");
        ComponentData.UiStringBuilder.Append(topCssValue);
        ComponentData.UiStringBuilder.Append("px;");
    
        ComponentData.UiStringBuilder.Append(Gutter_HeightWidthPaddingCssStyle);

        return ComponentData.UiStringBuilder.ToString();
    }
    
    public string RowSection_GetRowStyleCss(int lineIndex)
    {
    	ComponentData.UiStringBuilder.Clear();
    
        ComponentData.UiStringBuilder.Append("top: ");
        ComponentData.UiStringBuilder.Append(ComponentData.LineIndexCache.Map[lineIndex].TopCssValue);
        ComponentData.UiStringBuilder.Append("px;");

        ComponentData.UiStringBuilder.Append(LineHeightStyleCssString);

        ComponentData.UiStringBuilder.Append("left: ");
        ComponentData.UiStringBuilder.Append(ComponentData.LineIndexCache.Map[lineIndex].LeftCssValue);
        ComponentData.UiStringBuilder.Append("px;");

        return ComponentData.UiStringBuilder.ToString();
    }
    
    /// <summary>TODO: Determine if total width changed?</summary>
    public void ConstructVirtualizationStyleCssStrings()
    {
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
		
    	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("width: ");
    	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(ViewModel.Virtualization.TotalWidth.ToString());
    	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px; ");
    	
    	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("height: ");
    	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(ViewModel.Virtualization.TotalHeight.ToString());
    	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
    	
    	ComponentData.BothVirtualizationBoundaryStyleCssString = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
    }
    
    public void VERTICAL_GetSliderVerticalStyleCss()
    {
    	// Divide by zero exception
    	if (ViewModel.Virtualization.ScrollHeight == 0)
    		return;
    
        var scrollbarHeightInPixels = ViewModel.Virtualization.TextEditorDimensions.Height - ScrollbarFacts.SCROLLBAR_SIZE_IN_PIXELS;

        // Proportional Top
        var sliderProportionalTopInPixels = ViewModel.Virtualization.ScrollTop *
            scrollbarHeightInPixels /
            ViewModel.Virtualization.ScrollHeight;

		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
		
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("left: 0; width: ");
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(ComponentData.ScrollbarSizeCssValue);
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px; ");
		
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("top: ");
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(sliderProportionalTopInPixels.ToString());
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

        // Proportional Height
        var pageHeight = ViewModel.Virtualization.TextEditorDimensions.Height;

        var sliderProportionalHeightInPixels = pageHeight *
            scrollbarHeightInPixels /
            ViewModel.Virtualization.ScrollHeight;

        var sliderProportionalHeightInPixelsInvariantCulture = sliderProportionalHeightInPixels.ToString();

		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("height: ");
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(sliderProportionalHeightInPixelsInvariantCulture);
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

        VERTICAL_SliderCssStyle = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
    }
    
    private void GetInlineUiStyleList()
    {
    	if (InlineUiWidthStyleCssString is null || ComponentData.InlineUiWidthStyleCssStringIsOutdated)
    	{
	    	var widthPixels = ViewModel.Virtualization.CharAndLineMeasurements.CharacterWidth * 3;
			var widthCssValue = widthPixels.ToString(System.Globalization.CultureInfo.InvariantCulture);
			InlineUiWidthStyleCssString = $"width: {widthCssValue}px;";
			// width: @(widthCssValue)px;
		}
    
    	InlineUiStyleList.Clear();
        var tabWidth = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.OptionsApi.GetOptions().TabWidth;
    	
    	for (int inlineUiIndex = 0; inlineUiIndex < ViewModel.PersistentState.InlineUiList.Count; inlineUiIndex++)
    	{
    		var entry = ViewModel.PersistentState.InlineUiList[inlineUiIndex];
    		
    		var lineAndColumnIndices = Model.GetLineAndColumnIndicesFromPositionIndex(entry.InlineUi.PositionIndex);
    		
    		if (!ComponentData.LineIndexCache.Map.ContainsKey(lineAndColumnIndices.lineIndex))
    			continue;
    		
    		var leftInPixels = ViewModel.Virtualization.GutterWidth + lineAndColumnIndices.columnIndex * ViewModel.Virtualization.CharAndLineMeasurements.CharacterWidth;
    		
    		// Tab key column offset
    		{
	    		var tabsOnSameLineBeforeCursor = Model.GetTabCountOnSameLineBeforeCursor(
				    lineAndColumnIndices.lineIndex,
				    lineAndColumnIndices.columnIndex);
				
				// 1 of the character width is already accounted for
				var extraWidthPerTabKey = tabWidth - 1;
				
				leftInPixels += extraWidthPerTabKey *
				    tabsOnSameLineBeforeCursor *
				    ViewModel.Virtualization.CharAndLineMeasurements.CharacterWidth;
			}
    		
    		var topCssValue = ComponentData.LineIndexCache.Map[lineAndColumnIndices.lineIndex].TopCssValue;

    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
    		
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("position: absolute;");
    		
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("left: ");
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(leftInPixels.ToString(System.Globalization.CultureInfo.InvariantCulture));
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
    		
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("top: ");
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(topCssValue);
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
    		
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(InlineUiWidthStyleCssString);
    		
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(LineHeightStyleCssString);
    		
    		InlineUiStyleList.Add(ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString());
    	}
    }
    
    public void GetCursorAndCaretRowStyleCss()
    {
    	var shouldAppearAfterCollapsePoint = CursorIsOnHiddenLine;
    	var tabWidth = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.OptionsApi.GetOptions().TabWidth;
    	
    	double leftInPixels = ViewModel.Virtualization.GutterWidth;
    	var topInPixelsInvariantCulture = string.Empty;
	
		if (CursorIsOnHiddenLine)
		{
			for (int collapsePointIndex = 0; collapsePointIndex < ViewModel.PersistentState.AllCollapsePointList.Count; collapsePointIndex++)
			{
				var collapsePoint = ViewModel.PersistentState.AllCollapsePointList[collapsePointIndex];
				
				if (!collapsePoint.IsCollapsed)
					continue;
			
				var lastLineIndex = collapsePoint.EndExclusiveLineIndex - 1;
				
				if (lastLineIndex == ViewModel.LineIndex)
				{
					var lastLineInformation = Model.GetLineInformation(lastLineIndex);
					
					var appendToLineInformation = Model.GetLineInformation(collapsePoint.AppendToLineIndex);
					
					// Tab key column offset
			        {
			            var tabsOnSameLineBeforeCursor = Model.GetTabCountOnSameLineBeforeCursor(
			                collapsePoint.AppendToLineIndex,
			                appendToLineInformation.LastValidColumnIndex);
			
			            // 1 of the character width is already accounted for
			
			            var extraWidthPerTabKey = tabWidth - 1;
			
			            leftInPixels += extraWidthPerTabKey *
			                tabsOnSameLineBeforeCursor *
			                ViewModel.Virtualization.CharAndLineMeasurements.CharacterWidth;
			        }
			        
			        // +3 for the 3 dots: '[...]'
			        leftInPixels += ViewModel.Virtualization.CharAndLineMeasurements.CharacterWidth * (appendToLineInformation.LastValidColumnIndex + 3);
			        
			        if (ComponentData.LineIndexCache.Map.ContainsKey(collapsePoint.AppendToLineIndex))
			        {
			        	topInPixelsInvariantCulture = ComponentData.LineIndexCache.Map[collapsePoint.AppendToLineIndex].TopCssValue;
			        }
			        else
			        {
			        	if (ViewModel.Virtualization.Count > 0)
			        	{
			        		var firstEntry = ViewModel.Virtualization.EntryList[0];
			        		topInPixelsInvariantCulture = ComponentData.LineIndexCache.Map[firstEntry.LineIndex].TopCssValue;
			        	}
			        	else
			        	{
			        		topInPixelsInvariantCulture = 0.ToString();
			        	}
			        }
			        
			        break;
				}
			}
		}

		if (!shouldAppearAfterCollapsePoint)
		{
	        // Tab key column offset
	        {
	            var tabsOnSameLineBeforeCursor = Model.GetTabCountOnSameLineBeforeCursor(
	                ViewModel.LineIndex,
	                ViewModel.ColumnIndex);
	
	            // 1 of the character width is already accounted for
	
	            var extraWidthPerTabKey = tabWidth - 1;
	
	            leftInPixels += extraWidthPerTabKey *
	                tabsOnSameLineBeforeCursor *
	                ViewModel.Virtualization.CharAndLineMeasurements.CharacterWidth;
	        }
	        
	        leftInPixels += ViewModel.Virtualization.CharAndLineMeasurements.CharacterWidth * ViewModel.ColumnIndex;
	        
	        for (int inlineUiTupleIndex = 0; inlineUiTupleIndex < ViewModel.PersistentState.InlineUiList.Count; inlineUiTupleIndex++)
			{
				var inlineUiTuple = ViewModel.PersistentState.InlineUiList[inlineUiTupleIndex];
				
				var lineAndColumnIndices = Model.GetLineAndColumnIndicesFromPositionIndex(inlineUiTuple.InlineUi.PositionIndex);
				
				if (lineAndColumnIndices.lineIndex == ViewModel.LineIndex)
				{
					if (lineAndColumnIndices.columnIndex == ViewModel.ColumnIndex)
					{
						if (ViewModel.PersistentState.VirtualAssociativityKind == VirtualAssociativityKind.Right)
						{
							leftInPixels += ViewModel.Virtualization.CharAndLineMeasurements.CharacterWidth * 3;
						}
					}
					else if (lineAndColumnIndices.columnIndex <= ViewModel.ColumnIndex)
					{
						leftInPixels += ViewModel.Virtualization.CharAndLineMeasurements.CharacterWidth * 3;
					}
				}
			}
	    }
        
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();

        var leftInPixelsInvariantCulture = leftInPixels.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("left: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(leftInPixelsInvariantCulture);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

		if (!shouldAppearAfterCollapsePoint)
			topInPixelsInvariantCulture = ComponentData.LineIndexCache.Map[ViewModel.LineIndex].TopCssValue;

		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("top: ");
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(topInPixelsInvariantCulture);
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(LineHeightStyleCssString);

        var widthInPixelsInvariantCulture = TextEditorRenderBatchPersistentState.TextEditorOptions.CursorWidthInPixels.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("width: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(widthInPixelsInvariantCulture);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(((ITextEditorKeymap)TextEditorRenderBatchPersistentState.TextEditorOptions.Keymap).GetCursorCssStyleString(
            Model,
            ViewModel,
            TextEditorRenderBatchPersistentState.TextEditorOptions));
        
        // This feels a bit hacky, exceptions are happening because the UI isn't accessing
        // the text editor in a thread safe way.
        //
        // When an exception does occur though, the cursor should receive a 'text editor changed'
        // event and re-render anyhow however.
        // 
        // So store the result of this method incase an exception occurs in future invocations,
        // to keep the cursor on screen while the state works itself out.
        CursorCssStyle = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
    
    	/////////////////////
    	/////////////////////
    	
    	// CaretRow starts here
    	
    	/////////////////////
    	/////////////////////
		
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
		
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("top: ");
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(topInPixelsInvariantCulture);
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(LineHeightStyleCssString);

        var widthOfBodyInPixelsInvariantCulture =
            (Model.MostCharactersOnASingleLineTuple.lineLength * ViewModel.Virtualization.CharAndLineMeasurements.CharacterWidth)
            .ToString(System.Globalization.CultureInfo.InvariantCulture);

		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("width: ");
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(widthOfBodyInPixelsInvariantCulture);
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

        // This feels a bit hacky, exceptions are happening because the UI isn't accessing
        // the text editor in a thread safe way.
        //
        // When an exception does occur though, the cursor should receive a 'text editor changed'
        // event and re-render anyhow however.
        // 
        // So store the result of this method incase an exception occurs in future invocations,
        // to keep the cursor on screen while the state works itself out.
        CaretRowCssStyle = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
    }
    
    public void GetSelection()
    {
    	SelectionStyleList.Clear();
    
    	if (TextEditorSelectionHelper.HasSelectedText(ViewModel) &&
	         ViewModel.Virtualization.Count > 0)
	    {
	        SelectionBoundsInPositionIndexUnits = TextEditorSelectionHelper.GetSelectionBounds(
	            ViewModel);
	
	        var selectionBoundsInLineIndexUnits = TextEditorSelectionHelper.ConvertSelectionOfPositionIndexUnitsToLineIndexUnits(
                Model,
                SelectionBoundsInPositionIndexUnits);
	
	        var virtualLowerBoundInclusiveLineIndex = ViewModel.Virtualization.EntryList[0].LineIndex;
	        var virtualUpperBoundExclusiveLineIndex = 1 + ViewModel.Virtualization.EntryList[ViewModel.Virtualization.Count - 1].LineIndex;
	
	        UseLowerBoundInclusiveLineIndex = virtualLowerBoundInclusiveLineIndex >= selectionBoundsInLineIndexUnits.Line_LowerInclusiveIndex
	            ? virtualLowerBoundInclusiveLineIndex
	            : selectionBoundsInLineIndexUnits.Line_LowerInclusiveIndex;
	
	        UseUpperBoundExclusiveLineIndex = virtualUpperBoundExclusiveLineIndex <= selectionBoundsInLineIndexUnits.Line_UpperExclusiveIndex
	            ? virtualUpperBoundExclusiveLineIndex
            	: selectionBoundsInLineIndexUnits.Line_UpperExclusiveIndex;
            
            var hiddenLineCount = 0;
			var checkHiddenLineIndex = 0;
            
            for (; checkHiddenLineIndex < UseLowerBoundInclusiveLineIndex; checkHiddenLineIndex++)
            {
            	if (ViewModel.PersistentState.HiddenLineIndexHashSet.Contains(checkHiddenLineIndex))
            		hiddenLineCount++;
            }
            
            for (var i = UseLowerBoundInclusiveLineIndex; i < UseUpperBoundExclusiveLineIndex; i++)
	        {
	        	checkHiddenLineIndex++;
	        
	        	if (ViewModel.PersistentState.HiddenLineIndexHashSet.Contains(i))
	        	{
	        		hiddenLineCount++;
	        		continue;
	        	}
	        	
	        	SelectionStyleList.Add(GetTextSelectionStyleCss(
		     	   SelectionBoundsInPositionIndexUnits.Position_LowerInclusiveIndex,
		     	   SelectionBoundsInPositionIndexUnits.Position_UpperExclusiveIndex,
		     	   lineIndex: i));
	        }
	    }
    }
    
    public string GetTextSelectionStyleCss(
        int position_LowerInclusiveIndex,
        int position_UpperExclusiveIndex,
        int lineIndex)
    {
        if (lineIndex >= Model.LineEndList.Count)
            return string.Empty;

        var line = Model.GetLineInformation(lineIndex);
        var tabWidth = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.OptionsApi.GetOptions().TabWidth;

        var selectionStartingColumnIndex = 0;
        var selectionEndingColumnIndex = line.Position_EndExclusiveIndex - 1;

        var fullWidthOfLineIsSelected = true;

        if (position_LowerInclusiveIndex > line.Position_StartInclusiveIndex)
        {
            selectionStartingColumnIndex = position_LowerInclusiveIndex - line.Position_StartInclusiveIndex;
            fullWidthOfLineIsSelected = false;
        }

        if (position_UpperExclusiveIndex < line.Position_EndExclusiveIndex)
        {
            selectionEndingColumnIndex = position_UpperExclusiveIndex - line.Position_StartInclusiveIndex;
            fullWidthOfLineIsSelected = false;
        }

        var charMeasurements = ViewModel.Virtualization.CharAndLineMeasurements;

        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
        
        var topInPixelsInvariantCulture = ComponentData.LineIndexCache.Map[lineIndex].TopCssValue;
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("top: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(topInPixelsInvariantCulture);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(LineHeightStyleCssString);

        var selectionStartInPixels = ViewModel.Virtualization.GutterWidth + selectionStartingColumnIndex * charMeasurements.CharacterWidth;

        // selectionStartInPixels offset from Tab keys a width of many characters
        {
            var tabsOnSameLineBeforeCursor = Model.GetTabCountOnSameLineBeforeCursor(
                lineIndex,
                selectionStartingColumnIndex);

            // 1 of the character width is already accounted for
            var extraWidthPerTabKey = tabWidth - 1;

            selectionStartInPixels += 
                extraWidthPerTabKey * tabsOnSameLineBeforeCursor * charMeasurements.CharacterWidth;
        }

        var selectionStartInPixelsInvariantCulture = selectionStartInPixels.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("left: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(selectionStartInPixelsInvariantCulture);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

        var selectionWidthInPixels = 
            selectionEndingColumnIndex * charMeasurements.CharacterWidth - selectionStartInPixels + ViewModel.Virtualization.GutterWidth;

        // Tab keys a width of many characters
        {
            var lineInformation = Model.GetLineInformation(lineIndex);

            selectionEndingColumnIndex = Math.Min(
                selectionEndingColumnIndex,
                lineInformation.LastValidColumnIndex);

            var tabsOnSameLineBeforeCursor = Model.GetTabCountOnSameLineBeforeCursor(
                lineIndex,
                selectionEndingColumnIndex);

            // 1 of the character width is already accounted for
            var extraWidthPerTabKey = tabWidth - 1;

            selectionWidthInPixels += extraWidthPerTabKey * tabsOnSameLineBeforeCursor * charMeasurements.CharacterWidth;
        }

        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("width: ");
        var fullWidthValue = ViewModel.Virtualization.ScrollWidth;

        if (ViewModel.Virtualization.TextEditorDimensions.Width >
            ViewModel.Virtualization.ScrollWidth)
        {
            // If content does not fill the viewable width of the Text Editor User Interface
            fullWidthValue = ViewModel.Virtualization.TextEditorDimensions.Width;
        }

        var fullWidthValueInPixelsInvariantCulture = fullWidthValue.ToString();

        if (fullWidthOfLineIsSelected)
        {
        	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(fullWidthValueInPixelsInvariantCulture);
        	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
        }
        else if (selectionStartingColumnIndex != 0 &&
                 position_UpperExclusiveIndex > line.Position_EndExclusiveIndex - 1)
        {
        	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("calc(");
        	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(fullWidthValueInPixelsInvariantCulture);
        	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px - ");
        	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(selectionStartInPixelsInvariantCulture);
        	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px);");
        }
        else
        {
        	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(selectionWidthInPixels.ToString(System.Globalization.CultureInfo.InvariantCulture));
        	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
        }

        return ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
    }
    
    private void GetPresentationLayer(
    	List<Key<TextEditorPresentationModel>> presentationLayerKeysList,
    	List<(string CssClassString, int StartInclusiveIndex, int EndExclusiveIndex)> presentationLayerGroupList,
    	List<(string PresentationCssClass, string PresentationCssStyle)> presentationLayerTextSpanList)
    {
    	presentationLayerGroupList.Clear();
    	presentationLayerTextSpanList.Clear();
    
    	for (int presentationKeyIndex = 0; presentationKeyIndex < presentationLayerKeysList.Count; presentationKeyIndex++)
	    {
	    	var presentationKey = presentationLayerKeysList[presentationKeyIndex];
	    	
	    	var presentationLayer = Model.PresentationModelList.FirstOrDefault(
	    		x => x.TextEditorPresentationKey == presentationKey);
	        if (presentationLayer is null)
	        	continue;
	    	
	        var completedCalculation = presentationLayer.CompletedCalculation;
	        if (completedCalculation is null)
	        	continue;
	
			IReadOnlyList<TextEditorTextSpan> textSpansList = completedCalculation.TextSpanList
	            ?? Array.Empty<TextEditorTextSpan>();
	
	        IReadOnlyList<TextEditorTextModification> textModificationList = ((IReadOnlyList<TextEditorTextModification>?)completedCalculation.TextModificationsSinceRequestList)
	            ?? Array.Empty<TextEditorTextModification>();
	
        	// Should be using 'textSpansList' not 'completedCalculation.TextSpanList'?
            textSpansList = PresentationVirtualizeAndShiftTextSpans(textModificationList, completedCalculation.TextSpanList);

			var indexInclusiveStart = presentationLayerTextSpanList.Count;
			
			var hiddenLineCount = 0;
			var checkHiddenLineIndex = 0;

            for (int textSpanIndex = 0; textSpanIndex < textSpansList.Count; textSpanIndex++)
            {
            	var textSpan = textSpansList[textSpanIndex];
            	
                var boundsInPositionIndexUnits = (textSpan.StartInclusiveIndex, textSpan.EndExclusiveIndex);

                var boundsInLineIndexUnits = PresentationGetBoundsInLineIndexUnits(boundsInPositionIndexUnits);
                
                for (; checkHiddenLineIndex < boundsInLineIndexUnits.FirstLineToSelectDataInclusive; checkHiddenLineIndex++)
                {
                	if (ViewModel.PersistentState.HiddenLineIndexHashSet.Contains(checkHiddenLineIndex))
                		hiddenLineCount++;
                }

                for (var i = boundsInLineIndexUnits.FirstLineToSelectDataInclusive;
                     i < boundsInLineIndexUnits.LastLineToSelectDataExclusive;
                     i++)
                {
                	checkHiddenLineIndex++;
                	
                	if (ViewModel.PersistentState.HiddenLineIndexHashSet.Contains(i))
                	{
                		hiddenLineCount++;
                		continue;
                	}
                		
                	presentationLayerTextSpanList.Add((
                		PresentationGetCssClass(presentationLayer, textSpan.DecorationByte),
                		PresentationGetCssStyleString(
                            boundsInPositionIndexUnits.StartInclusiveIndex,
                            boundsInPositionIndexUnits.EndExclusiveIndex,
                            lineIndex: i)));
                }
            }
            
            presentationLayerGroupList.Add(
            	(
            		presentationLayer.CssClassString,
            	    indexInclusiveStart,
            	    indexExclusiveEnd: presentationLayerTextSpanList.Count)
            	);
	    }
    }
    
    public IReadOnlyList<TextEditorTextSpan> PresentationVirtualizeAndShiftTextSpans(
        IReadOnlyList<TextEditorTextModification> textModifications,
        IReadOnlyList<TextEditorTextSpan> inTextSpanList)
    {
    	// TODO: Why virtualize then shift? Isn't it shift then virtualize? (2025-05-01)
    	
    	VirtualizedTextSpanList.Clear();
    	OutTextSpansList.Clear();
    
        // Virtualize the text spans
        if (ViewModel.Virtualization.Count > 0)
        {
            var lowerLineIndexInclusive = ViewModel.Virtualization.EntryList[0].LineIndex;
            var upperLineIndexInclusive = ViewModel.Virtualization.EntryList[ViewModel.Virtualization.Count - 1].LineIndex;

            var lowerLine = Model.GetLineInformation(lowerLineIndexInclusive);
            var upperLine = Model.GetLineInformation(upperLineIndexInclusive);

			// Awkward enumeration was modified 'for loop' (2025-01-22)
			// Also, this shouldn't be done here, it should be done during the editContext.
			var count = inTextSpanList.Count;
            for (int i = 0; i < count; i++)
            {
            	var textSpan = inTextSpanList[i];
            	
                if (lowerLine.Position_StartInclusiveIndex <= textSpan.StartInclusiveIndex &&
                    upperLine.Position_EndExclusiveIndex >= textSpan.StartInclusiveIndex)
                {
                	VirtualizedTextSpanList.Add(textSpan);
                }
            }
        }
        else
        {
            // No 'Virtualization', so don't render any text spans.
            return Array.Empty<TextEditorTextSpan>();
        }

        // Shift the text spans
        {
            for (int textSpanIndex = 0; textSpanIndex < VirtualizedTextSpanList.Count; textSpanIndex++)
            {
            	var textSpan = VirtualizedTextSpanList[textSpanIndex];
            	
                var startingIndexInclusive = textSpan.StartInclusiveIndex;
                var endingIndexExclusive = textSpan.EndExclusiveIndex;

				// Awkward enumeration was modified 'for loop' (2025-01-22)
				// Also, this shouldn't be done here, it should be done during the editContext.
				var count = textModifications.Count;
                for (int i = 0; i < count; i++)
                {
                	var textModification = textModifications[i];
                
                    if (textModification.WasInsertion)
                    {
                        if (startingIndexInclusive >= textModification.TextEditorTextSpan.StartInclusiveIndex)
                        {
                            startingIndexInclusive += textModification.TextEditorTextSpan.Length;
                            endingIndexExclusive += textModification.TextEditorTextSpan.Length;
                        }
                    }
                    else // was deletion
                    {
                        if (startingIndexInclusive >= textModification.TextEditorTextSpan.StartInclusiveIndex)
                        {
                            startingIndexInclusive -= textModification.TextEditorTextSpan.Length;
                            endingIndexExclusive -= textModification.TextEditorTextSpan.Length;
                        }
                    }
                }

                OutTextSpansList.Add(textSpan with
                {
                    StartInclusiveIndex = startingIndexInclusive,
                    EndExclusiveIndex = endingIndexExclusive
                });
            }
        }

        return OutTextSpansList;
    }
    
    public (int FirstLineToSelectDataInclusive, int LastLineToSelectDataExclusive) PresentationGetBoundsInLineIndexUnits(
    	(int StartInclusiveIndex, int EndExclusiveIndex) boundsInPositionIndexUnits)
    {
        var firstLineToSelectDataInclusive = Model
            .GetLineInformationFromPositionIndex(boundsInPositionIndexUnits.StartInclusiveIndex)
            .Index;

        var lastLineToSelectDataExclusive = Model
            .GetLineInformationFromPositionIndex(boundsInPositionIndexUnits.EndExclusiveIndex)
            .Index +
            1;

        return (firstLineToSelectDataInclusive, lastLineToSelectDataExclusive);
    }
    
    public string PresentationGetCssClass(TextEditorPresentationModel presentationModel, byte decorationByte)
    {
        return presentationModel.DecorationMapper.Map(decorationByte);
    }
    
    public string PresentationGetCssStyleString(
        int position_LowerInclusiveIndex,
        int position_UpperExclusiveIndex,
        int lineIndex)
    {
        if (lineIndex >= Model.LineEndList.Count)
            return string.Empty;

        var line = Model.GetLineInformation(lineIndex);
        var tabWidth = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.OptionsApi.GetOptions().TabWidth;

        var startingColumnIndex = 0;
        var endingColumnIndex = line.Position_EndExclusiveIndex - 1;

        var fullWidthOfLineIsSelected = true;

        if (position_LowerInclusiveIndex > line.Position_StartInclusiveIndex)
        {
            startingColumnIndex = position_LowerInclusiveIndex - line.Position_StartInclusiveIndex;
            fullWidthOfLineIsSelected = false;
        }

        if (position_UpperExclusiveIndex < line.Position_EndExclusiveIndex)
        {
            endingColumnIndex = position_UpperExclusiveIndex - line.Position_StartInclusiveIndex;
            fullWidthOfLineIsSelected = false;
        }

        var topInPixelsInvariantCulture = ComponentData.LineIndexCache.Map[lineIndex].TopCssValue;
        
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("position: absolute; ");

		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("top: ");
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(topInPixelsInvariantCulture);
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("height: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(ViewModel.Virtualization.CharAndLineMeasurements.LineHeight.ToString());
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
        
        // This only happens when the 'EOF' position index is "inclusive"
        // as something to be drawn for the presentation.
        if (startingColumnIndex > line.LastValidColumnIndex)
        	startingColumnIndex = line.LastValidColumnIndex;

        var startInPixels = ViewModel.Virtualization.GutterWidth + startingColumnIndex * ViewModel.Virtualization.CharAndLineMeasurements.CharacterWidth;

        // startInPixels offset from Tab keys a width of many characters
        {
            var tabsOnSameLineBeforeCursor = Model.GetTabCountOnSameLineBeforeCursor(
                lineIndex,
                startingColumnIndex);

            // 1 of the character width is already accounted for
            var extraWidthPerTabKey = tabWidth - 1;

            startInPixels += extraWidthPerTabKey * tabsOnSameLineBeforeCursor * ViewModel.Virtualization.CharAndLineMeasurements.CharacterWidth;
        }

        var startInPixelsInvariantCulture = startInPixels.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("left: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(startInPixelsInvariantCulture);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

        var widthInPixels = endingColumnIndex * ViewModel.Virtualization.CharAndLineMeasurements.CharacterWidth - startInPixels + ViewModel.Virtualization.GutterWidth;

        // Tab keys a width of many characters
        {
            var tabsOnSameLineBeforeCursor = Model.GetTabCountOnSameLineBeforeCursor(
                lineIndex,
                line.LastValidColumnIndex);

            // 1 of the character width is already accounted for
            var extraWidthPerTabKey = tabWidth - 1;

            widthInPixels += extraWidthPerTabKey * tabsOnSameLineBeforeCursor * ViewModel.Virtualization.CharAndLineMeasurements.CharacterWidth;
        }

        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("width: ");

        var fullWidthValue = ViewModel.Virtualization.ScrollWidth;

        if (ViewModel.Virtualization.TextEditorDimensions.Width > ViewModel.Virtualization.ScrollWidth)
            fullWidthValue = ViewModel.Virtualization.TextEditorDimensions.Width; // If content does not fill the viewable width of the Text Editor User Interface

        var fullWidthValueInPixelsInvariantCulture = fullWidthValue.ToString();

        var widthInPixelsInvariantCulture = widthInPixels.ToString(System.Globalization.CultureInfo.InvariantCulture);

        if (fullWidthOfLineIsSelected)
        {
            ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(fullWidthValueInPixelsInvariantCulture);
            ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
        }
        else if (startingColumnIndex != 0 && position_UpperExclusiveIndex > line.Position_EndExclusiveIndex - 1)
        {
        	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("calc(");
        	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(fullWidthValueInPixelsInvariantCulture);
        	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px - ");
        	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(startInPixelsInvariantCulture);
        	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px);");
        }
        else
        {
        	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(widthInPixelsInvariantCulture);
        	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
        }

        return ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
    }
    
    public void HORIZONTAL_GetScrollbarHorizontalStyleCss()
    {
    	var scrollbarWidthInPixels = ViewModel.Virtualization.TextEditorDimensions.Width -
                                     ScrollbarFacts.SCROLLBAR_SIZE_IN_PIXELS -
                                     ViewModel.Virtualization.GutterWidth;
        
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("width: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(scrollbarWidthInPixels.ToString());
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

        HORIZONTAL_ScrollbarCssStyle = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
    }
    
    public void HORIZONTAL_GetSliderHorizontalStyleCss()
    {
    	// Divide by 0 exception
    	if (ViewModel.Virtualization.ScrollWidth == 0)
    		return;
    
    	var scrollbarWidthInPixels = ViewModel.Virtualization.TextEditorDimensions.Width -
						             ScrollbarFacts.SCROLLBAR_SIZE_IN_PIXELS -
						             ViewModel.Virtualization.GutterWidth;
        
        // Proportional Left
    	var sliderProportionalLeftInPixels = ViewModel.Virtualization.ScrollLeft *
            scrollbarWidthInPixels /
            ViewModel.Virtualization.ScrollWidth;

		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
		
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("bottom: 0; height: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(ComponentData.ScrollbarSizeCssValue);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px; ");
        
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(" left: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(sliderProportionalLeftInPixels.ToString());
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
        
        // Proportional Width
    	var pageWidth = ViewModel.Virtualization.TextEditorDimensions.Width;

        var sliderProportionalWidthInPixels = pageWidth *
            scrollbarWidthInPixels /
            ViewModel.Virtualization.ScrollWidth;
        
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("width: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(sliderProportionalWidthInPixels.ToString());
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
        
        HORIZONTAL_SliderCssStyle = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
    }
    
    private void DiagnoseIssues()
    {
    	if (ViewModel is null)
    		return;
    }
}
