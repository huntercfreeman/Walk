using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.TextEditor.RazorLib.TextEditors.Models;

/// <summary>
/// Do not mutate state on this type unless you
/// have a TextEditorEditContext.
///
/// TODO: 2 interfaces, 1 mutable one readonly?
///
/// Stores the state of the user interface. For example, the user's <see cref="TextEditorCursor"/> instances are stored here.<br/><br/>
/// 
/// Each <see cref="TextEditorViewModel"/> has a unique underlying <see cref="TextEditorModel"/>. Therefore, if one has a
/// <see cref="TextEditorModel"/> of a text file named "myHomework.txt", then arbitrary amount of <see cref="TextEditorViewModel"/>(s) can reference that <see cref="TextEditorModel"/>.<br/><br/>
/// 
/// For example, maybe one has a main text editor, but also a peek window open of the same underlying <see cref="TextEditorModel"/>.
/// The main text editor is one <see cref="TextEditorViewModel"/> and the peek window is a separate <see cref="TextEditorViewModel"/>.
/// Both of those <see cref="TextEditorViewModel"/>(s) are referencing the same <see cref="TextEditorModel"/>.
/// Therefore typing into the peek window will also result in the main text editor re-rendering with the updated text and vice versa.
///
/// Do not use object initializers because the cloning of the TextEditorViewModel
/// will redundantly take time to perform the object initialization.
/// Instead, add "object initialization" to the constructor of the original instance.
/// </summary>
public sealed class TextEditorViewModel : IDisposable
{
	public TextEditorViewModel(
        Key<TextEditorViewModel> viewModelKey,
        ResourceUri resourceUri,
        TextEditorService textEditorService,
        CommonUtilityService commonUtilityService,
        TextEditorVirtualizationResult virtualizationResult,
		TextEditorDimensions textEditorDimensions,
		int scrollLeft,
	    int scrollTop,
	    int scrollWidth,
	    int scrollHeight,
	    int marginScrollHeight,
		Category category)
    {
    	PersistentState = new TextEditorViewModelPersistentState(
		    viewModelKey,
		    resourceUri,
		    textEditorService,
		    category,
		    onSaveRequested: null,
		    getTabDisplayNameFunc: null,
		    firstPresentationLayerKeysList: new(),
		    lastPresentationLayerKeysList: new(),
		    showFindOverlay: false,
		    replaceValueInFindOverlay: string.Empty,
		    showReplaceButtonInFindOverlay: false,
		    findOverlayValue: string.Empty,
		    findOverlayValueExternallyChangedMarker: false,
		    menuKind: MenuKind.None,
	    	tooltipModel: null,
		    shouldRevealCursor: false,
			virtualAssociativityKind: VirtualAssociativityKind.None,
			commonUtilityService,
            textEditorDimensions,
    		scrollLeft,
    	    scrollTop,
    	    scrollWidth,
    	    scrollHeight,
    	    marginScrollHeight,
            textEditorService.OptionsApi.GetOptions().CharAndLineMeasurements);
    
        Virtualization = virtualizationResult;
        
        LineIndex = 0;
	    ColumnIndex = 0;
	    PreferredColumnIndex = 0;
	    SelectionAnchorPositionIndex = -1;
	    SelectionEndingPositionIndex = 0;
	}
	
	public TextEditorViewModel(TextEditorViewModel other)
	{
		PersistentState = other.PersistentState;
		
	    _lineIndex = other._lineIndex;
	    _columnIndex = other._columnIndex;
	    _preferredColumnIndex = other._preferredColumnIndex;
	    _selectionAnchorPositionIndex = other._selectionAnchorPositionIndex;
	    _selectionEndingPositionIndex = other._selectionEndingPositionIndex;
	    
	    // The new instance of `Virtualization` is only made when calculating a virtualization result.
	    // Otherwise, just keep re-using the previous.
	    Virtualization = other.Virtualization;
		
	    /*
	    // Don't copy these properties
	    ScrollWasModified { get; set; }
	    */
	}
	
	public TextEditorViewModelPersistentState PersistentState { get; set; }

    private int _lineIndex;
    public int LineIndex
    {
        get => _lineIndex;
        set
        {
            if (_lineIndex != value)
            {
                Changed_LineIndex = true;
                Changed_Cursor_AnyState = true;
            }
            _lineIndex = value;
        }
    }
    
    private int _columnIndex;
    public int ColumnIndex
    {
        get => _columnIndex;
        set
        {
            if (_columnIndex != value)
            {
                Changed_ColumnIndex = true;
                Changed_Cursor_AnyState = true;
            }
            _columnIndex = value;
        }
    }
    
    private int _preferredColumnIndex;
    public int PreferredColumnIndex
    {
        get => _preferredColumnIndex;
        set
        {
            if (_preferredColumnIndex != value)
            {
                Changed_PreferredColumnIndex = true;
                Changed_Cursor_AnyState = true;
            }
            _preferredColumnIndex = value;
        }
    }
    
    private int _selectionAnchorPositionIndex;
    public int SelectionAnchorPositionIndex
    {
        get => _selectionAnchorPositionIndex;
        set
        {
            if (_selectionAnchorPositionIndex != value)
            {
                Changed_SelectionAnchorPositionIndex = true;
                Changed_Cursor_AnyState = true;
            }
            _selectionAnchorPositionIndex = value;
        }
    }
    
    private int _selectionEndingPositionIndex;
    public int SelectionEndingPositionIndex
    {
        get => _selectionEndingPositionIndex;
        set
        {
            if (_selectionEndingPositionIndex != value)
            {
                Changed_SelectionEndingPositionIndex = true;
                Changed_Cursor_AnyState = true;
            }
            _selectionEndingPositionIndex = value;
        }
    }
    
    /// <summary>
    /// Given the dimensions of the rendered text editor, this provides a subset of the file's content, such that "only what is
    /// visible when rendered" is in this. There is some padding of offscreen content so that scrolling is smoother.
    /// </summary>
    public TextEditorVirtualizationResult Virtualization { get; set; }
	
    public bool ScrollWasModified { get; set; }
    
    /// <summary>
    /// When settings `Changed_...` also set this property.
    /// Then to determine if the many `Changed_...` properties that are not this one
    /// have changed, you can first check this singular property so you short circuit
    /// if nothing changed.
    ///
    /// Don't copy any of the `Changed_...` properties when making a copy of a viewmodel.
    /// They're just used as markers during the lifespan of each viewmodel whether the UI needs to be updated.
    ///
    /// If only this bool is set, then it means while calculating the VirtualizationResult, that
    /// some other variable had changed, and the cursor CSS is dependent on that variable.
    /// </summary>
    public bool Changed_Cursor_AnyState { get; set; }
    
    public bool Changed_LineIndex { get; set; }
    public bool Changed_ColumnIndex { get; set; }
    public bool Changed_PreferredColumnIndex { get; set; }
    public bool Changed_SelectionAnchorPositionIndex { get; set; }
    public bool Changed_SelectionEndingPositionIndex { get; set; }
    
    public ValueTask FocusAsync()
    {
    	var componentData = PersistentState.ComponentData;
    	if (componentData is null)
    		return ValueTask.CompletedTask;
    	
        return PersistentState.TextEditorService.ViewModelApi.FocusPrimaryCursorAsync(componentData.PrimaryCursorContentId);
    }
    
    public void ApplyCollapsePointState(TextEditorEditContext editContext)
    {
    	foreach (var collapsePoint in PersistentState.AllCollapsePointList)
		{
			if (!collapsePoint.IsCollapsed)
				continue;
			var firstToHideLineIndex = collapsePoint.AppendToLineIndex + 1;
			for (var lineOffset = 0; lineOffset < collapsePoint.EndExclusiveLineIndex - collapsePoint.AppendToLineIndex - 1; lineOffset++)
			{
				PersistentState.HiddenLineIndexHashSet.Add(firstToHideLineIndex + lineOffset);
			}
		}
		PersistentState.VirtualizedCollapsePointListVersion++;
    }
    
    public void SetColumnIndexAndPreferred(int columnIndex)
    {
        ColumnIndex = columnIndex;
        PreferredColumnIndex = columnIndex;
    }
    
    public void MutateScrollLeft(int pixels, TextEditorDimensions textEditorDimensions) =>
		SetScrollLeft(PersistentState.ScrollLeft + pixels, textEditorDimensions);

	public void SetScrollLeft(int pixels, TextEditorDimensions textEditorDimensions)
	{
		var resultScrollLeft = Math.Max(0, pixels);
		var maxScrollLeft = (int)Math.Max(0, PersistentState.ScrollWidth - textEditorDimensions.Width);

		if (resultScrollLeft > maxScrollLeft)
			resultScrollLeft = maxScrollLeft;

		PersistentState.ScrollLeft = resultScrollLeft;
	}

	public void MutateScrollTop(int pixels, TextEditorDimensions textEditorDimensions) =>
		SetScrollTop(PersistentState.ScrollTop + pixels, textEditorDimensions);

	public void SetScrollTop(int pixels, TextEditorDimensions textEditorDimensions)
	{
		var resultScrollTop = Math.Max(0, pixels);
		var maxScrollTop = (int)Math.Max(0, PersistentState.ScrollHeight - textEditorDimensions.Height);

		if (resultScrollTop > maxScrollTop)
			resultScrollTop = maxScrollTop;

		PersistentState.ScrollTop = resultScrollTop;
	}

    public void Dispose()
    {
        PersistentState.Dispose();
    }
}
