using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.TextEditor.RazorLib.JavaScriptObjects.Models;
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
        IPanelService panelService,
        IDialogService dialogService,
        CommonBackgroundTaskApi commonBackgroundTaskApi,
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
			panelService,
            dialogService,
            commonBackgroundTaskApi);
    
        Virtualization = virtualizationResult;
		Virtualization.TextEditorDimensions = textEditorDimensions;
		
		Virtualization.ScrollLeft = scrollLeft;
	    Virtualization.ScrollTop = scrollTop;
	    Virtualization.ScrollWidth = scrollWidth;
	    Virtualization.ScrollHeight = scrollHeight;
	    Virtualization.MarginScrollHeight = marginScrollHeight;
		
        Virtualization.CharAndLineMeasurements = textEditorService.OptionsApi.GetOptions().CharAndLineMeasurements;
        
        LineIndex = 0;
	    ColumnIndex = 0;
	    PreferredColumnIndex = 0;
	    SelectionAnchorPositionIndex = -1;
	    SelectionEndingPositionIndex = 0;
	}
	
	public TextEditorViewModel(TextEditorViewModel other)
	{
		PersistentState = other.PersistentState;
		
	    LineIndex = other.LineIndex;
	    ColumnIndex = other.ColumnIndex;
	    PreferredColumnIndex = other.PreferredColumnIndex;
	    SelectionAnchorPositionIndex = other.SelectionAnchorPositionIndex;
	    SelectionEndingPositionIndex = other.SelectionEndingPositionIndex;
	    
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
                Changed_LineIndex = true;
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
                Changed_ColumnIndex = true;
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
                Changed_PreferredColumnIndex = true;
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
                Changed_SelectionAnchorPositionIndex = true;
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
                Changed_SelectionEndingPositionIndex = true;
            _selectionEndingPositionIndex = value;
        }
    }
    
    public bool Changed_LineIndex { get; set; }
    public bool Changed_ColumnIndex { get; set; }
    public bool Changed_PreferredColumnIndex { get; set; }
    public bool Changed_SelectionAnchorPositionIndex { get; set; }
    public bool Changed_SelectionEndingPositionIndex { get; set; }
    
    /// <summary>
    /// Given the dimensions of the rendered text editor, this provides a subset of the file's content, such that "only what is
    /// visible when rendered" is in this. There is some padding of offscreen content so that scrolling is smoother.
    /// </summary>
    public TextEditorVirtualizationResult Virtualization { get; set; }
	
    public bool ScrollWasModified { get; set; }
    
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
		SetScrollLeft(Virtualization.ScrollLeft + pixels, textEditorDimensions);

	public void SetScrollLeft(int pixels, TextEditorDimensions textEditorDimensions)
	{
		var resultScrollLeft = Math.Max(0, pixels);
		var maxScrollLeft = (int)Math.Max(0, Virtualization.ScrollWidth - textEditorDimensions.Width);

		if (resultScrollLeft > maxScrollLeft)
			resultScrollLeft = maxScrollLeft;

		Virtualization.ScrollLeft = resultScrollLeft;
	}

	public void MutateScrollTop(int pixels, TextEditorDimensions textEditorDimensions) =>
		SetScrollTop(Virtualization.ScrollTop + pixels, textEditorDimensions);

	public void SetScrollTop(int pixels, TextEditorDimensions textEditorDimensions)
	{
		var resultScrollTop = Math.Max(0, pixels);
		var maxScrollTop = (int)Math.Max(0, Virtualization.ScrollHeight - textEditorDimensions.Height);

		if (resultScrollTop > maxScrollTop)
			resultScrollTop = maxScrollTop;

		Virtualization.ScrollTop = resultScrollTop;
	}

    public void Dispose()
    {
        PersistentState.Dispose();
    }
}
