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
	    count: 0);

    /// <summary>
    /// This constructor permits the Empty instance to be made without
    /// an if statement relating to the previousState.
    /// </summary>
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
	    int count)
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
	    
	    IsValid = Model is not null &&
			      ViewModel is not null &&
			      TextEditorRenderBatchPersistentState?.TextEditorOptions is not null;
    }

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
	    
	    if (previousState is not null)
	    {
	        _textEditorDimensions = previousState._textEditorDimensions;
            _gutterWidth = previousState._gutterWidth;
        	_textEditorDimensions = previousState._textEditorDimensions;
            _scrollTop = previousState._scrollTop;
            _scrollWidth = previousState._scrollWidth;
            _scrollHeight = previousState._scrollHeight;
            _marginScrollHeight = previousState._marginScrollHeight;
            _scrollLeft = previousState._scrollLeft;
            _charAndLineMeasurements = previousState._charAndLineMeasurements;
        }
	    
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
    
    private int _gutterWidth;
    public int GutterWidth
    {
        get => _gutterWidth;
        set
        {
            if (_gutterWidth != value)
                Changed_GutterWidth = true;
            _gutterWidth = value;
        }
    }
    
	private TextEditorDimensions _textEditorDimensions;
	public TextEditorDimensions TextEditorDimensions
	{
	    get => _textEditorDimensions;
	    set
	    {
	        if (_textEditorDimensions.Width != value.Width)
	            Changed_TextEditorWidth = true;
            if (_textEditorDimensions.Height != value.Height)
	            Changed_TextEditorHeight = true;
            _textEditorDimensions = value;
	    }
    }
    
    private int _scrollTop;
    public int ScrollTop
    {
        get => _scrollTop;
        set
        {
            if (_scrollTop != value)
                Changed_ScrollTop = true;
            _scrollTop = value;
        }
    }
    
    private int _scrollWidth;
    public int ScrollWidth
    {
        get => _scrollWidth;
        set
        {
            if (_scrollWidth != value)
                Changed_ScrollWidth = true;
            _scrollWidth = value;
        }
    }
    
    private int _scrollHeight;
    public int ScrollHeight
    {
        get => _scrollHeight;
        set
        {
            if (_scrollHeight != value)
                Changed_ScrollHeight = true;
            _scrollHeight = value;
        }
    }
    
    private int _marginScrollHeight;
    public int MarginScrollHeight
    {
        get => _marginScrollHeight;
        set
        {
            if (_marginScrollHeight != value)
                Changed_MarginScrollHeight = true;
            _marginScrollHeight = value;
        }
    }
    
    private int _scrollLeft;
    public int ScrollLeft
    {
        get => _scrollLeft;
        set
        {
            if (_scrollLeft != value)
                Changed_ScrollLeft = true;
            _scrollLeft = value;
        }
    }
    
    private CharAndLineMeasurements _charAndLineMeasurements;
    public CharAndLineMeasurements CharAndLineMeasurements
    {
        get => _charAndLineMeasurements;
        set
        {
            if (_charAndLineMeasurements.CharacterWidth != value.CharacterWidth)
                Changed_CharacterWidth = true;
            if (_charAndLineMeasurements.LineHeight != value.LineHeight)
                Changed_LineHeight = true;
            _charAndLineMeasurements = value;
        }
    }
    
    public bool Changed_GutterWidth { get; set; }
    public bool Changed_LineHeight { get; set; }
    public bool Changed_CharacterWidth { get; set; }
    public bool Changed_TextEditorWidth { get; set; }
    public bool Changed_TextEditorHeight { get; set; }
    public bool Changed_ScrollHeight { get; set; }
    public bool Changed_ScrollTop { get; set; }
    public bool Changed_ScrollWidth { get; set; }
    public bool Changed_ScrollLeft { get; set; }
    public bool Changed_MarginScrollHeight { get; set; }
	
	public bool ShouldCalculateVirtualizationResult { get; set; }
	
	public Key<TextEditorViewModel> _seenViewModelKey = Key<TextEditorViewModel>.Empty;
	
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
    public string? Gutter_HeightWidthPaddingCssStyle { get; set; }
    
    public string? Gutter_WidthCssStyle { get; set; }
    
    public string? LineHeightStyleCssString { get; set; }
    
    /// <summary>
    /// BodyStyle is the odd one out for the nullable strings being used for CSS.
    /// The text editor needs to initially render with full width so that JavaScript can measure it.
    /// </summary>
    public string BodyStyle { get; set; } = $"width: 100%; left: 0;";
    
    public string? GutterCssStyle { get; set; }
    public string? GutterSectionCssStyle { get; set; }
    public string? GutterColumnTopCss { get; set; }
    
    // string RowSection_GetRowStyleCss(int lineIndex)
    public string? CursorCssStyle { get; set; }
    public string? CaretRowCssStyle { get; set; }
    public string? VERTICAL_SliderCssStyle { get; set; }
    public string? HORIZONTAL_SliderCssStyle { get; set; }
    public string? HORIZONTAL_ScrollbarCssStyle { get; set; }
    
    public string? ScrollbarSection_LeftCssStyle { get; set; }
	
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
    public void CreateUi_NotCacheDependent()
    {
        if (!IsValid)
        {
        	DiagnoseIssues();
        	return;
        }
        
        LineHeightStyleCssString = _previousState.LineHeightStyleCssString;
        
    	if (_previousState.Changed_GutterWidth)
    	{
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
    		
    		ViewModel.Changed_Cursor_AnyState = true;
    	}
    	else
    	{
    		Gutter_WidthCssStyle = _previousState.Gutter_WidthCssStyle;
    		Gutter_HeightWidthPaddingCssStyle = _previousState.Gutter_HeightWidthPaddingCssStyle;
    		BodyStyle = _previousState.BodyStyle;
    		HORIZONTAL_ScrollbarCssStyle = _previousState.HORIZONTAL_ScrollbarCssStyle;
    		HORIZONTAL_SliderCssStyle = _previousState.HORIZONTAL_SliderCssStyle;
    		ScrollbarSection_LeftCssStyle = _previousState.ScrollbarSection_LeftCssStyle;
    		CursorCssStyle = _previousState.CursorCssStyle;
    		CaretRowCssStyle = _previousState.CaretRowCssStyle;
    	}
    	
    	if (_previousState.Changed_ScrollLeft)
    	{
    		ComponentData.LineIndexCache.Clear();
    	}
    
        string gutterColumnTopCssValue;
    	
	    if (Count > 0)
        	gutterColumnTopCssValue = ComponentData.LineIndexCache.Map[EntryList[0].LineIndex].TopCssValue;
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
    	
    	if (_previousState.Changed_LineHeight)
    	{
			ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("height: ");
	        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(CharAndLineMeasurements.LineHeight.ToString());
	        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
	        LineHeightStyleCssString = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
	        
	        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
    		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(LineHeightStyleCssString);
	        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(Gutter_WidthCssStyle);
	        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(ComponentData.Gutter_PaddingCssStyle);
    		Gutter_HeightWidthPaddingCssStyle = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
		
		    ViewModel.Changed_Cursor_AnyState = true;
		}
		else
		{
		    LineHeightStyleCssString = _previousState.LineHeightStyleCssString;
		    Gutter_HeightWidthPaddingCssStyle = _previousState.Gutter_HeightWidthPaddingCssStyle;
		}
		
		bool shouldCalculateVerticalSlider = false;
		bool shouldCalculateHorizontalSlider = false;
		bool shouldCalculateHorizontalScrollbar = false;
		
    	if (_previousState.Changed_TextEditorHeight)
    	{
    		shouldCalculateVerticalSlider = true;
	    }
		
    	if (_previousState.Changed_ScrollHeight)
    	{
    		shouldCalculateVerticalSlider = true;
	    }
		
    	if (_previousState.Changed_ScrollTop)
    	{
    		shouldCalculateVerticalSlider = true;
	    }
		
    	if (_previousState.Changed_TextEditorWidth)
    	{
    		shouldCalculateHorizontalSlider = true;
    		shouldCalculateHorizontalScrollbar = true;
	    }
		
    	if (_previousState.Changed_ScrollWidth)
    	{
    		shouldCalculateHorizontalSlider = true;
	    }
		
    	if (_previousState.Changed_ScrollLeft)
    	{
    		shouldCalculateHorizontalSlider = true;
	    }

        if (shouldCalculateVerticalSlider)
			VERTICAL_GetSliderVerticalStyleCss();
		else
		    VERTICAL_SliderCssStyle = _previousState.VERTICAL_SliderCssStyle;
		
		if (shouldCalculateHorizontalSlider)
			HORIZONTAL_GetSliderHorizontalStyleCss();
		else
		    HORIZONTAL_SliderCssStyle = _previousState.HORIZONTAL_SliderCssStyle;
		
		if (shouldCalculateHorizontalScrollbar)
			HORIZONTAL_GetScrollbarHorizontalStyleCss();
		else
		    HORIZONTAL_ScrollbarCssStyle = _previousState.HORIZONTAL_ScrollbarCssStyle;
    
        if (TotalWidth != _previousState.TotalWidth || TotalHeight != _previousState.TotalHeight)
        {
    	    ConstructVirtualizationStyleCssStrings();
	    }
	    else
	    {
	        // This is such a silly thing to type I don't think this code will work since you can't re-use the _previousState value
	        // cause it is stored on the ComponentData?
	        // 
	        ConstructVirtualizationStyleCssStrings();
        }
    }
    
    public void External_GetCursorCss()
    {
        GetCursorAndCaretRowStyleCss();
        GetSelection();
    }
    
    public void CreateUi_IsCacheDependent()
    {
        if (ViewModel.Changed_Cursor_AnyState || (CursorCssStyle == string.Empty))
            GetCursorAndCaretRowStyleCss();
    
        GetSelection();
        
        /* GetPresentationLayer(
        	ViewModel.PersistentState.FirstPresentationLayerKeysList,
        	FirstPresentationLayerGroupList,
        	FirstPresentationLayerTextSpanList); */
        	
        /* GetPresentationLayer(
        	ViewModel.PersistentState.LastPresentationLayerKeysList,
        	LastPresentationLayerGroupList,
        	LastPresentationLayerTextSpanList); */
        
        /* if (VirtualizedCollapsePointListVersion != ViewModel.PersistentState.VirtualizedCollapsePointListVersion ||
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
        } */
    }
    
    public string GetGutterStyleCss(string topCssValue)
    {
    	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
    
    	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("top: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(topCssValue);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
    
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(Gutter_HeightWidthPaddingCssStyle);

        return ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
    }
    
    public string RowSection_GetRowStyleCss(string topCssValue, string leftCssValue)
    {
    	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
    
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("top: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(topCssValue);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(LineHeightStyleCssString);

        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("left: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(leftCssValue);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

        return ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
    }
    
    /// <summary>TODO: Determine if total width changed?</summary>
    public void ConstructVirtualizationStyleCssStrings()
    {
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
		
    	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("width: ");
    	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(TotalWidth.ToString());
    	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px; ");
    	
    	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("height: ");
    	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(TotalHeight.ToString());
    	ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
    	
    	ComponentData.BothVirtualizationBoundaryStyleCssString = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
    }
    
    public void VERTICAL_GetSliderVerticalStyleCss()
    {
    	// Divide by zero exception
    	if (ScrollHeight == 0)
    		return;
    
        var scrollbarHeightInPixels = TextEditorDimensions.Height - ScrollbarFacts.SCROLLBAR_SIZE_IN_PIXELS;

        // Proportional Top
        var sliderProportionalTopInPixels = ScrollTop *
            scrollbarHeightInPixels /
            ScrollHeight;

		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
		
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("left: 0; width: ");
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(ComponentData.ScrollbarSizeCssValue);
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px; ");
		
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("top: ");
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(sliderProportionalTopInPixels.ToString());
		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

        var sliderProportionalHeightInPixels = TextEditorDimensions.Height *
            scrollbarHeightInPixels /
            ScrollHeight;

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
	    	var widthPixels = CharAndLineMeasurements.CharacterWidth * 3;
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
    		
    		var leftInPixels = GutterWidth + lineAndColumnIndices.columnIndex * CharAndLineMeasurements.CharacterWidth;
    		
    		// Tab key column offset
    		{
	    		var tabsOnSameLineBeforeCursor = Model.GetTabCountOnSameLineBeforeCursor(
				    lineAndColumnIndices.lineIndex,
				    lineAndColumnIndices.columnIndex);
				
				// 1 of the character width is already accounted for
				var extraWidthPerTabKey = tabWidth - 1;
				
				leftInPixels += extraWidthPerTabKey *
				    tabsOnSameLineBeforeCursor *
				    CharAndLineMeasurements.CharacterWidth;
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
    	var shouldAppearAfterCollapsePoint = false; // CursorIsOnHiddenLine;
    	var tabWidth = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.OptionsApi.GetOptions().TabWidth;
    	
    	double leftInPixels = GutterWidth;
    	var topInPixelsInvariantCulture = string.Empty;
	
		/* if (CursorIsOnHiddenLine)
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
			                CharAndLineMeasurements.CharacterWidth;
			        }
			        
			        // +3 for the 3 dots: '[...]'
			        /*leftInPixels += CharAndLineMeasurements.CharacterWidth * (appendToLineInformation.LastValidColumnIndex + 3);
			        
			        if (ComponentData.LineIndexCache.Map.ContainsKey(collapsePoint.AppendToLineIndex))
			        {
			        	topInPixelsInvariantCulture = ComponentData.LineIndexCache.Map[collapsePoint.AppendToLineIndex].TopCssValue;
			        }
			        else
			        {
			        	if (Count > 0)
			        	{
			        		var firstEntry = EntryList[0];
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
		} */

		if (!shouldAppearAfterCollapsePoint)
		{
	        // Tab key column offset
            var tabsOnSameLineBeforeCursor = Model.GetTabCountOnSameLineBeforeCursor(
                ViewModel.LineIndex,
                ViewModel.ColumnIndex);
            // 1 of the character width is already accounted for
            var extraWidthPerTabKey = tabWidth - 1;
            leftInPixels += extraWidthPerTabKey *
                tabsOnSameLineBeforeCursor *
                CharAndLineMeasurements.CharacterWidth;
	        
	        leftInPixels += CharAndLineMeasurements.CharacterWidth * ViewModel.ColumnIndex;
	        
	        /*for (int inlineUiTupleIndex = 0; inlineUiTupleIndex < ViewModel.PersistentState.InlineUiList.Count; inlineUiTupleIndex++)
			{
				var inlineUiTuple = ViewModel.PersistentState.InlineUiList[inlineUiTupleIndex];
				
				var lineAndColumnIndices = Model.GetLineAndColumnIndicesFromPositionIndex(inlineUiTuple.InlineUi.PositionIndex);
				
				if (lineAndColumnIndices.lineIndex == ViewModel.LineIndex)
				{
					if (lineAndColumnIndices.columnIndex == ViewModel.ColumnIndex)
					{
						if (ViewModel.PersistentState.VirtualAssociativityKind == VirtualAssociativityKind.Right)
						{
							leftInPixels += CharAndLineMeasurements.CharacterWidth * 3;
						}
					}
					else if (lineAndColumnIndices.columnIndex <= ViewModel.ColumnIndex)
					{
						leftInPixels += CharAndLineMeasurements.CharacterWidth * 3;
					}
				}
			}*/
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
            (Model.MostCharactersOnASingleLineTuple.lineLength * CharAndLineMeasurements.CharacterWidth)
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
    
    	if (TextEditorSelectionHelper.HasSelectedText(ViewModel) && Count > 0)
	    {
	        SelectionBoundsInPositionIndexUnits = TextEditorSelectionHelper.GetSelectionBounds(
	            ViewModel);
	
	        var selectionBoundsInLineIndexUnits = TextEditorSelectionHelper.ConvertSelectionOfPositionIndexUnitsToLineIndexUnits(
                Model,
                SelectionBoundsInPositionIndexUnits);
	
	        var virtualLowerBoundInclusiveLineIndex = EntryList[0].LineIndex;
	        var virtualUpperBoundExclusiveLineIndex = 1 + EntryList[Count - 1].LineIndex;
	
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

        var charMeasurements = CharAndLineMeasurements;

        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
        
        var topInPixelsInvariantCulture = ComponentData.LineIndexCache.Map[lineIndex].TopCssValue;
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("top: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(topInPixelsInvariantCulture);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(LineHeightStyleCssString);

        var selectionStartInPixels = GutterWidth + selectionStartingColumnIndex * charMeasurements.CharacterWidth;

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
            selectionEndingColumnIndex * charMeasurements.CharacterWidth - selectionStartInPixels + GutterWidth;

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
        var fullWidthValue = ScrollWidth;

        if (TextEditorDimensions.Width > ScrollWidth)
        {
            // If content does not fill the viewable width of the Text Editor User Interface
            fullWidthValue = TextEditorDimensions.Width;
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
        if (Count > 0)
        {
            var lowerLineIndexInclusive = EntryList[0].LineIndex;
            var upperLineIndexInclusive = EntryList[Count - 1].LineIndex;

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
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(CharAndLineMeasurements.LineHeight.ToString());
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
        
        // This only happens when the 'EOF' position index is "inclusive"
        // as something to be drawn for the presentation.
        if (startingColumnIndex > line.LastValidColumnIndex)
        	startingColumnIndex = line.LastValidColumnIndex;

        var startInPixels = GutterWidth + startingColumnIndex * CharAndLineMeasurements.CharacterWidth;

        // startInPixels offset from Tab keys a width of many characters
        {
            var tabsOnSameLineBeforeCursor = Model.GetTabCountOnSameLineBeforeCursor(
                lineIndex,
                startingColumnIndex);

            // 1 of the character width is already accounted for
            var extraWidthPerTabKey = tabWidth - 1;

            startInPixels += extraWidthPerTabKey * tabsOnSameLineBeforeCursor * CharAndLineMeasurements.CharacterWidth;
        }

        var startInPixelsInvariantCulture = startInPixels.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("left: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(startInPixelsInvariantCulture);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

        var widthInPixels = endingColumnIndex * CharAndLineMeasurements.CharacterWidth - startInPixels + GutterWidth;

        // Tab keys a width of many characters
        {
            var tabsOnSameLineBeforeCursor = Model.GetTabCountOnSameLineBeforeCursor(
                lineIndex,
                line.LastValidColumnIndex);

            // 1 of the character width is already accounted for
            var extraWidthPerTabKey = tabWidth - 1;

            widthInPixels += extraWidthPerTabKey * tabsOnSameLineBeforeCursor * CharAndLineMeasurements.CharacterWidth;
        }

        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("width: ");

        var fullWidthValue = ScrollWidth;

        if (TextEditorDimensions.Width > ScrollWidth)
            fullWidthValue = TextEditorDimensions.Width; // If content does not fill the viewable width of the Text Editor User Interface

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
    	var scrollbarWidthInPixels = TextEditorDimensions.Width -
                                     ScrollbarFacts.SCROLLBAR_SIZE_IN_PIXELS -
                                     GutterWidth;
        
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("width: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(scrollbarWidthInPixels.ToString());
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");

        HORIZONTAL_ScrollbarCssStyle = ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.ToString();
    }
    
    public void HORIZONTAL_GetSliderHorizontalStyleCss()
    {
    	// Divide by 0 exception
    	if (ScrollWidth == 0)
    		return;
    
    	var scrollbarWidthInPixels = TextEditorDimensions.Width -
						             ScrollbarFacts.SCROLLBAR_SIZE_IN_PIXELS -
						             GutterWidth;
        
        // Proportional Left
    	var sliderProportionalLeftInPixels = ScrollLeft *
            scrollbarWidthInPixels /
            ScrollWidth;

		ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Clear();
		
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("bottom: 0; height: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(ComponentData.ScrollbarSizeCssValue);
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px; ");
        
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(" left: ");
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append(sliderProportionalLeftInPixels.ToString());
        ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.__StringBuilder.Append("px;");
        
        // Proportional Width
    	var pageWidth = TextEditorDimensions.Width;

        var sliderProportionalWidthInPixels = pageWidth *
            scrollbarWidthInPixels /
            ScrollWidth;
        
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
