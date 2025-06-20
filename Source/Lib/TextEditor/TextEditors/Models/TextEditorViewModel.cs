using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.TextEditor.RazorLib.Virtualizations.Models;
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
        VirtualizationGrid virtualizationResult,
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
    
        VirtualizationResult = virtualizationResult;
		TextEditorDimensions = textEditorDimensions;
		
		ScrollLeft = scrollLeft;
	    ScrollTop = scrollTop;
	    ScrollWidth = scrollWidth;
	    ScrollHeight = scrollHeight;
	    MarginScrollHeight = marginScrollHeight;
		
        CharAndLineMeasurements = textEditorService.OptionsApi.GetOptions().CharAndLineMeasurements;
        
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
	    
	    VirtualizationResult = other.VirtualizationResult;
		TextEditorDimensions = other.TextEditorDimensions;
		
		ScrollLeft = other.ScrollLeft;
	    ScrollTop = other.ScrollTop;
	    ScrollWidth = other.ScrollWidth;
	    ScrollHeight = other.ScrollHeight;
	    MarginScrollHeight = other.MarginScrollHeight;
	    
	    GutterWidthInPixels = other.GutterWidthInPixels;
		
	    CharAndLineMeasurements = other.CharAndLineMeasurements;
		
		ShouldCalculateVirtualizationResult = other.ShouldCalculateVirtualizationResult;
		
		VirtualizationResultCount = other.VirtualizationResultCount;
	    
	    /*
	    // Don't copy these properties
	    ScrollWasModified { get; set; }
	    */
	}
	
	public TextEditorViewModelPersistentState PersistentState { get; set; }

    public int LineIndex { get; set; }
    public int ColumnIndex { get; set; }
    public int PreferredColumnIndex { get; set; }
    public int SelectionAnchorPositionIndex { get; set; }
    public int SelectionEndingPositionIndex { get; set; }
    /// <summary>
    /// Given the dimensions of the rendered text editor, this provides a subset of the file's content, such that "only what is
    /// visible when rendered" is in this. There is some padding of offscreen content so that scrolling is smoother.
    /// </summary>
    public VirtualizationGrid VirtualizationResult { get; set; }
	public TextEditorDimensions TextEditorDimensions { get; set; }
	
	public int ScrollLeft { get; set; }
    public int ScrollTop { get; set; }
    public int ScrollWidth { get; set; }
    public int ScrollHeight { get; set; }
    public int MarginScrollHeight { get; set; }
    
    public int GutterWidthInPixels { get; set; }
    
    public int VirtualizationResultCount { get; set; }
	
	/// <summary>
	/// TODO: Rename 'CharAndLineMeasurements' to 'CharAndLineDimensions'...
	///       ...as to bring it inline with 'TextEditorDimensions' and 'ScrollbarDimensions'.
	/// </summary>
    public CharAndLineMeasurements CharAndLineMeasurements { get; set; }
    
    /// <summary>
	/// This property decides whether or not to re-calculate the virtualization result that gets displayed on the UI.
	/// </summary>
    public bool ShouldCalculateVirtualizationResult { get; set; }
	
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
		SetScrollLeft(ScrollLeft + pixels, textEditorDimensions);

	public void SetScrollLeft(int pixels, TextEditorDimensions textEditorDimensions)
	{
		var resultScrollLeft = Math.Max(0, pixels);
		var maxScrollLeft = (int)Math.Max(0, ScrollWidth - textEditorDimensions.Width);

		if (resultScrollLeft > maxScrollLeft)
			resultScrollLeft = maxScrollLeft;

		ScrollLeft = resultScrollLeft;
	}

	public void MutateScrollTop(int pixels, TextEditorDimensions textEditorDimensions) =>
		SetScrollTop(ScrollTop + pixels, textEditorDimensions);

	public void SetScrollTop(int pixels, TextEditorDimensions textEditorDimensions)
	{
		var resultScrollTop = Math.Max(0, pixels);
		var maxScrollTop = (int)Math.Max(0, ScrollHeight - textEditorDimensions.Height);

		if (resultScrollTop > maxScrollTop)
			resultScrollTop = maxScrollTop;

		ScrollTop = resultScrollTop;
	}

    public void Dispose()
    {
        PersistentState.Dispose();
    }
}
