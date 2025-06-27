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

namespace Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

/// <summary>
/// Everytime one renders a unique <see cref="Walk.TextEditor.RazorLib.TextEditors.Displays.TextEditorViewModelSlimDisplay"/>,
/// a unique identifier for the HTML elements is created.
///
/// That unique identifier, and other data, is on this class.
///
/// This class is a "ui event broker" in order to avoid unnecessary invocations of
/// <see cref="ITextEditorService.Post"/.
///
/// For example, all <see cref="TextEditorViewModel"/> have a <see cref="TextEditorViewModel.TooltipViewModel"/>.
/// A sort of 'on mouse over' like event might set the current <see cref="TextEditorViewModel.TooltipViewModel"/>.
/// But, the event logic overhead is not handled by the <see cref="TextEditorViewModel"/>, but instead
/// this class, then the valid events are passed along to result in the <see cref="TextEditorViewModel.TooltipViewModel"/>
/// being set.
/// </summary>
public sealed class TextEditorComponentData
{
	private readonly Throttle _throttleApplySyntaxHighlighting = new(TimeSpan.FromMilliseconds(500));

	public static TimeSpan ThrottleDelayDefault { get; } = TimeSpan.FromMilliseconds(60);
    public static TimeSpan OnMouseOutTooltipDelay { get; } = TimeSpan.FromMilliseconds(1_000);
    public static TimeSpan MouseStoppedMovingDelay { get; } = TimeSpan.FromMilliseconds(400);

	public TextEditorComponentData(
		Guid textEditorHtmlElementId,
		ViewModelDisplayOptions viewModelDisplayOptions,
		TextEditorOptions options,
		TextEditorViewModelSlimDisplay textEditorViewModelSlimDisplay)
	{
		TextEditorHtmlElementId = textEditorHtmlElementId;
		ViewModelDisplayOptions = viewModelDisplayOptions;
		Options = options;
		TextEditorViewModelSlimDisplay = textEditorViewModelSlimDisplay;
		
		ComponentDataKey = new Key<TextEditorComponentData>(TextEditorHtmlElementId);
		
		RowSectionElementId = $"di_te_text-editor-body_{TextEditorHtmlElementId}";
		PrimaryCursorContentId = $"di_te_text-editor-content_{TextEditorHtmlElementId}_primary-cursor";
		GutterElementId = $"di_te_text-editor-gutter_{TextEditorHtmlElementId}";
		FindOverlayId = $"di_te_find-overlay_{TextEditorHtmlElementId}";
	}
	
	public TextEditorViewModelSlimDisplay TextEditorViewModelSlimDisplay { get; }
	public Key<TextEditorComponentData> ComponentDataKey { get; }
	
	public string RowSectionElementId { get; }
	public string PrimaryCursorContentId { get; }
	public string GutterElementId { get; }
	public string FindOverlayId { get; }
	
	public Guid TextEditorHtmlElementId { get; }
	
	public ViewModelDisplayOptions ViewModelDisplayOptions { get; }
    
    /// <summary>
	/// This property contains the global options, with an extra step of overriding any specified options
	/// for a specific text editor. The current implementation of this is quite hacky and not obvious
	/// when reading the code.
	/// </summary>
	public TextEditorOptions Options { get; init; }
	
	/// <summary>
	/// The TextEditorComponentData should never interact with its `LineIndexCache`.
	///
	/// This is for use by the `TextEditorViewModel`.
	///
	/// Do not move this property to the `TextEditorViewModel`,
	/// lest the cache overhead be endured foreach instance of `TextEditorViewModel`.
	///
	/// By having the `LineIndexCache` here, you only incur the overhead
	/// when there is a viewmodel actively being rendered.
	/// </summary>
    public TextEditorLineIndexCache LineIndexCache { get; set; }
    
    public Task MouseStoppedMovingTask { get; set; } = Task.CompletedTask;
    public Task MouseNoLongerOverTooltipTask { get; set; } = Task.CompletedTask;
    public CancellationTokenSource MouseNoLongerOverTooltipCancellationTokenSource { get; set; } = new();
    public CancellationTokenSource MouseStoppedMovingCancellationTokenSource { get; set; } = new();
    
    public long MouseMovedTimestamp { get; private set; }

    /// <summary>
	/// This accounts for one who might hold down Left Mouse Button from outside the TextEditorDisplay's content div
	/// then move their mouse over the content div while holding the Left Mouse Button down.
	/// </summary>
    public bool ThinksLeftMouseButtonIsDown { get; set; }
    
    public bool MenuShouldTakeFocus { get; set; }
    
    private void Css_LineIndexCache_Clear()
    {
	   Css_LineIndexCache_EntryMap.Clear();
	   Css_LineIndexCache_UsageHashSet.Clear();
	   Css_LineIndexCache_KeyList.Clear();
    }
    
    public void Virtualized_LineIndexCache_Clear()
    {
	    Virtualized_LineIndexCache_CreatedWithScrollLeft = -1;
	    Virtualized_LineIndexCache_LineMap.Clear();
	    
	    // This points to a TextEditorViewModel('s) VirtualizationGrid('s) list directly.
	    // If you clear it that'll cause a UI race condition exception.
	    Virtualized_LineIndexCache_SpanList = Virtualized_LineIndexCache_SpanList_Empty;
	    
	    Virtualized_LineIndexCache_LineIndexUsageHashSet.Clear();
	    Virtualized_LineIndexCache_LineIndexKeyList.Clear();
	    Virtualized_LineIndexCache_ViewModelKey = Key<TextEditorViewModel>.Empty;
	    CacheIsInvalid = false;
	    LineIndexWithModificationList.Clear();
    }
    
    public void ThrottleApplySyntaxHighlighting(TextEditorModel modelModifier)
    {
        _throttleApplySyntaxHighlighting.Run(_ =>
        {
            modelModifier.PersistentState.CompilerService.ResourceWasModified(modelModifier.PersistentState.ResourceUri, Array.Empty<TextEditorTextSpan>());
			return Task.CompletedTask;
        });
    }

	public Task ContinueRenderingTooltipAsync()
    {
        MouseNoLongerOverTooltipCancellationTokenSource.Cancel();
        MouseNoLongerOverTooltipCancellationTokenSource = new();

        return Task.CompletedTask;
    }
    
    public void OnMouseMoved()
    {
    	MouseMovedTimestamp = Stopwatch.GetTimestamp();
    }
}
