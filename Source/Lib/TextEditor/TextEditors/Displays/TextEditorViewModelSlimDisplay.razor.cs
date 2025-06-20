using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Drags.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Clipboards.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Icons.Displays;
using Walk.Common.RazorLib.CustomEvents.Models;
using Walk.Common.RazorLib.Tooltips.Models;
using Walk.TextEditor.RazorLib.JavaScriptObjects.Models;
using Walk.TextEditor.RazorLib.Edits.Models;
using Walk.TextEditor.RazorLib.Autocompletes.Models;
using Walk.TextEditor.RazorLib.ComponentRenderers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.Commands.Models.Defaults;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.FindAlls.Models;
using Walk.TextEditor.RazorLib.Cursors.Models;
using Walk.TextEditor.RazorLib.BackgroundTasks.Models;

namespace Walk.TextEditor.RazorLib.TextEditors.Displays;

public sealed partial class TextEditorViewModelSlimDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IAppOptionsService AppOptionsService { get; set; } = null!;
    [Inject]
    public IServiceProvider ServiceProvider { get; set; } = null!;
    [Inject]
    public TextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    public IDirtyResourceUriService DirtyResourceUriService { get; set; } = null!;
    [Inject]
    public IAutocompleteIndexer AutocompleteIndexer { get; set; } = null!;
    [Inject]
    public IJSRuntime JsRuntime { get; set; } = null!;
    [Inject]
    public IClipboardService ClipboardService { get; set; } = null!;
    [Inject]
    public BackgroundTaskService BackgroundTaskService { get; set; } = null!;
    [Inject]
    public IWalkTextEditorComponentRenderers TextEditorComponentRenderers { get; set; } = null!;
    [Inject]
    public ICommonComponentRenderers CommonComponentRenderers { get; set; } = null!;
    [Inject]
    public INotificationService NotificationService { get; set; } = null!;
    [Inject]
    public IEnvironmentProvider EnvironmentProvider { get; set; } = null!;
    [Inject]
    public IFileSystemProvider FileSystemProvider { get; set; } = null!;
    [Inject]
    public IDialogService DialogService { get; set; } = null!;
    [Inject]
    public ITooltipService TooltipService { get; set; } = null!;
    [Inject]
    public IDropdownService DropdownService { get; set; } = null!;
    [Inject]
    public WalkTextEditorConfig TextEditorConfig { get; set; } = null!;
    [Inject]
    private IFindAllService FindAllService { get; set; } = null!;
    [Inject]
    private IDragService DragService { get; set; } = null!;

    [Parameter, EditorRequired]
    public Key<TextEditorViewModel> TextEditorViewModelKey { get; set; } = Key<TextEditorViewModel>.Empty;
    
    [Parameter]
    public ViewModelDisplayOptions ViewModelDisplayOptions { get; set; } = new();

    private DotNetObjectReference<TextEditorViewModelSlimDisplay>? _dotNetHelper;

    private Guid _textEditorHtmlElementId;

    private TextEditorComponentData _componentData = null!;
    public TextEditorRenderBatchPersistentState _textEditorRenderBatchPersistentState;
    
    public TextEditorViewModel? _linkedViewModel;
    
    private bool _thinksTouchIsOccurring;
    private DateTime? _touchStartDateTime = null;
    private TouchEventArgs? _previousTouchEventArgs = null;
    private bool _userMouseIsInside;

    private IconDriver _iconDriver = new IconDriver(widthInPixels: 15, heightInPixels: 15);
    
    private string ContentElementId => _componentData.RowSectionElementId;
    
	/// <summary>
	/// Unit of measurement is pixels (px).
	/// </summary>
	private const int DISTANCE_TO_RESET_SCROLL_POSITION = 300;

	private TextEditorEventArgs _mouseDownEventArgsStruct = new TextEditorEventArgs
    {
        Buttons = -1
    };

    private readonly Guid VERTICAL_scrollbarGuid = Guid.NewGuid();
	private readonly Guid HORIZONTAL_scrollbarGuid = Guid.NewGuid();

    private bool VERTICAL_thinksLeftMouseButtonIsDown;

	private double VERTICAL_clientXThresholdToResetScrollTopPosition;
	private double VERTICAL_scrollTopOnMouseDown;

    private string VERTICAL_ScrollbarElementId;
    private string VERTICAL_ScrollbarSliderElementId;

    private bool HORIZONTAL_thinksLeftMouseButtonIsDown;
	private double HORIZONTAL_clientYThresholdToResetScrollLeftPosition;
	private double HORIZONTAL_scrollLeftOnMouseDown;

    private string HORIZONTAL_ScrollbarElementId;
    private string HORIZONTAL_ScrollbarSliderElementId;
    
    private string CONNECTOR_ScrollbarElementId;
	
	private Func<TextEditorEventArgs, MouseEventArgs, Task>? _dragEventHandler = null;
	
	public bool GlobalShowNewlines => TextEditorService.OptionsApi.GetTextEditorOptionsState().Options.ShowNewlines;
	
    private readonly CancellationTokenSource _onMouseMoveCancellationTokenSource = new();
    private TextEditorEventArgs _onMouseMoveMouseEventArgsStruct = new TextEditorEventArgs
    {
        Buttons = -1
    };
    private Task _onMouseMoveTask = Task.CompletedTask;

	public TextEditorComponentData ComponentData => _componentData;
	
	private Key<TextEditorViewModel> _linkedViewModelKey = Key<TextEditorViewModel>.Empty;
	
	private bool _hasRenderedAtLeastOnce = false;
	
	private bool _focusIn;
	
	private string CaretRowCssClass => _focusIn
	    ? "di_te_text-editor-caret-row di_te_focus"
	    : "di_te_text-editor-caret-row";
	
    protected override void OnInitialized()
    {
        // TODO: Does the object used here matter? Should it be a "smaller" object or is this just reference?
        _dotNetHelper = DotNetObjectReference.Create(this);
    
    	if (ViewModelDisplayOptions.TextEditorHtmlElementId != Guid.Empty)
    		_textEditorHtmlElementId = ViewModelDisplayOptions.TextEditorHtmlElementId;
    	else
    		_textEditorHtmlElementId = Guid.NewGuid();
    
    	SetComponentData();
    	_ = TextEditorService.TextEditorState._componentDataMap.TryAdd(_componentData.ComponentDataKey, _componentData);
    	
    	CssOnInitialized();
  	  
        TextEditorService.TextEditorStateChanged += GeneralOnStateChangedEventHandler;
        TextEditorService.OptionsApi.StaticStateChanged += OnOptionStaticStateChanged;
        TextEditorService.OptionsApi.MeasuredStateChanged += OnOptionMeasuredStateChanged;
        TextEditorService.ViewModelApi.CursorShouldBlinkChanged += ViewModel_CursorShouldBlinkChanged;
        DragService.DragStateChanged += DragStateWrapOnStateChanged;

        base.OnInitialized();
    }
    
    protected override void OnParametersSet()
    {
    	if (_hasRenderedAtLeastOnce && _linkedViewModelKey != TextEditorViewModelKey)
            HandleTextEditorViewModelKeyChange();
            
    	base.OnParametersSet();
    }
    
	protected override bool ShouldRender()
    {
		ComponentData.CreateUi();
        return true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
        	_hasRenderedAtLeastOnce = true;
        	HandleTextEditorViewModelKeyChange();
            
            // Do not ConfigureAwait(false) so that the UI doesn't change out from under you
            // before you finish setting up the events?
            // (is this a thing, I'm just presuming this would be true).
            await TextEditorService.JsRuntimeTextEditorApi
                .SetPreventDefaultsAndStopPropagations(
                    _dotNetHelper,
                    ContentElementId,
                    _componentData.RowSectionElementId,
                    HORIZONTAL_ScrollbarElementId,
                    VERTICAL_ScrollbarElementId,
                    CONNECTOR_ScrollbarElementId);
        }

        if (_componentData.RenderBatch.ViewModel is not null)
        {
        	// It is thought that you shouldn't '.ConfigureAwait(false)' for the scrolling JS Interop,
			// because this could provide a "natural throttle for the scrolling"
			// since more ITextEditorService edit contexts might have time to be calculated
			// and thus not every single one of them need be scrolled to.
			// This idea has not been proven yet.
			//
			// (the same is true for rendering the UI, it might avoid some renders
			//  because the most recent should render took time to get executed).
        	
        	// WARNING: It is only thread safe to read, then assign `_componentData.ScrollLeftChanged` or `_componentData.ScrollTopChanged`...
        	// ...if this method is running synchronously, i.e.: there hasn't been an await.
        	// |
        	// `if (firstRender)` is the only current scenario where an await comes prior to this read and assign.
        	//
        	// ScrollLeft is most likely to shortcircuit, thus it is being put first.
        	if (_componentData.Scroll_LeftChanged && _componentData.Scroll_TopChanged)
        	{
        		_componentData.Scroll_LeftChanged = false;
        		_componentData.Scroll_TopChanged = false;
        		
        		await TextEditorService.JsRuntimeTextEditorApi
		            .SetScrollPositionBoth(
		                _componentData.RowSectionElementId,
		                _componentData.RenderBatch.ViewModel.ScrollLeft,
		                _componentData.RenderBatch.ViewModel.ScrollTop)
	                .ConfigureAwait(false);
        	}
        	else if (_componentData.Scroll_TopChanged) // ScrollTop is most likely to come next
        	{
        		_componentData.Scroll_TopChanged = false;
        		
        		await TextEditorService.JsRuntimeTextEditorApi
		            .SetScrollPositionTop(
		                _componentData.RowSectionElementId,
		                _componentData.RenderBatch.ViewModel.ScrollTop)
	                .ConfigureAwait(false);
        	}
        	else if (_componentData.Scroll_LeftChanged)
        	{
        		_componentData.Scroll_LeftChanged = false;
        		
        		await TextEditorService.JsRuntimeTextEditorApi
		            .SetScrollPositionLeft(
		                _componentData.RowSectionElementId,
		                _componentData.RenderBatch.ViewModel.ScrollLeft)
	                .ConfigureAwait(false);
        	}
        }
    }
    
    private void CssOnInitialized()
	{
		_componentData.SetWrapperCssAndStyle();
    	
    	// ContentElementId = $"di_te_text-editor-content_{_textEditorHtmlElementId}";
	    
	    VERTICAL_ScrollbarElementId = $"di_te_{VERTICAL_scrollbarGuid}";
	    VERTICAL_ScrollbarSliderElementId = $"di_te_{VERTICAL_scrollbarGuid}-slider";
	    
	    HORIZONTAL_ScrollbarElementId = $"di_te_{HORIZONTAL_scrollbarGuid}";
	    HORIZONTAL_ScrollbarSliderElementId = $"di_te_{HORIZONTAL_scrollbarGuid}-slider";
	    
	    CONNECTOR_ScrollbarElementId = $"di_te_{Guid.NewGuid()}";
	    
	    var paddingLeftInPixelsInvariantCulture = TextEditorModel.GUTTER_PADDING_LEFT_IN_PIXELS.ToCssValue();
	    var paddingRightInPixelsInvariantCulture = TextEditorModel.GUTTER_PADDING_RIGHT_IN_PIXELS.ToCssValue();
        _componentData.Gutter_PaddingCssStyle = $"padding-left: {paddingLeftInPixelsInvariantCulture}px; padding-right: {paddingRightInPixelsInvariantCulture}px;";
        
        _componentData.ScrollbarSizeCssValue = ScrollbarFacts.SCROLLBAR_SIZE_IN_PIXELS.ToCssValue();
        
		_componentData.CursorCssClassBlinkAnimationOn = $"di_te_text-editor-cursor di_te_blink {TextCursorKindFacts.BeamCssClassString}";
	    _componentData.CursorCssClassBlinkAnimationOff = $"di_te_text-editor-cursor {TextCursorKindFacts.BeamCssClassString}";
	}
	
    private void SetComponentData()
    {
		_componentData = new(
			_textEditorHtmlElementId,
			ViewModelDisplayOptions,
			TextEditorService.OptionsApi.GetTextEditorOptionsState().Options,
			this,
			DropdownService,
			ClipboardService,
			CommonComponentRenderers,
			NotificationService,
			TextEditorService,
			TextEditorComponentRenderers,
			FindAllService,
			EnvironmentProvider,
			FileSystemProvider,
			ServiceProvider);
			
		SetRenderBatchConstants();
    }
    
    public void SetRenderBatchConstants()
    {
    	var textEditorOptions = TextEditorService.OptionsApi.GetTextEditorOptionsState().Options;
			
		string fontFamily;
		if (!string.IsNullOrWhiteSpace(textEditorOptions.CommonOptions?.FontFamily))
			fontFamily = textEditorOptions.CommonOptions!.FontFamily;
		else
			fontFamily = TextEditorRenderBatch.DEFAULT_FONT_FAMILY;
			
		int fontSizeInPixels;
		if (textEditorOptions.CommonOptions?.FontSizeInPixels is not null)
            fontSizeInPixels = textEditorOptions.CommonOptions.FontSizeInPixels;
        else
        	fontSizeInPixels = TextEditorOptionsState.DEFAULT_FONT_SIZE_IN_PIXELS;
		
		_textEditorRenderBatchPersistentState = new TextEditorRenderBatchPersistentState(
			textEditorOptions,
			fontFamily,
			fontSizeInPixels,
			ViewModelDisplayOptions,
			_componentData);
    }
    
    public void HandleTextEditorViewModelKeyChange()
    {
    	// Avoid infinite loop if the viewmodel does not exist.
    	if (TextEditorService.TextEditorState.ViewModelGetOrDefault(TextEditorViewModelKey) is null)
    		return;
    	
    	TextEditorService.WorkerArbitrary.PostUnique(editContext =>
    	{
    		var localTextEditorViewModelKey = TextEditorViewModelKey;

            var nextViewModel = TextEditorService.TextEditorState.ViewModelGetOrDefault(localTextEditorViewModelKey);

            Key<TextEditorViewModel> nextViewModelKey;

            if (nextViewModel is null)
                nextViewModelKey = Key<TextEditorViewModel>.Empty;
            else
                nextViewModelKey = nextViewModel.PersistentState.ViewModelKey;

            var linkedViewModelKey = _linkedViewModel?.PersistentState.ViewModelKey ?? Key<TextEditorViewModel>.Empty;
            var viewKeyChanged = nextViewModelKey != linkedViewModelKey;

            if (viewKeyChanged)
            {
                _linkedViewModel?.PersistentState.DisposeComponentData(editContext, _componentData);
                nextViewModel?.PersistentState.RegisterComponentData(editContext, _componentData);

                _linkedViewModel = nextViewModel;
                _linkedViewModelKey = _linkedViewModel.PersistentState.ViewModelKey;

                _componentData.Virtualized_LineIndexCache_Clear();
                
                if (nextViewModel is not null)
                {
                    nextViewModel.PersistentState.ShouldRevealCursor = true;
                    nextViewModel.ShouldCalculateVirtualizationResult = true;
                    TextEditorService.FinalizePost(editContext);
                }
            }
            
            return ValueTask.CompletedTask;
    	});
    }

    private async void GeneralOnStateChangedEventHandler() => await InvokeAsync(StateHasChanged);
        
    private async void ViewModel_CursorShouldBlinkChanged() => await InvokeAsync(StateHasChanged);
    
    [JSInvokable]
    public async Task FocusTextEditorAsync()
	{
	  var nextViewModel = TextEditorService.TextEditorState.ViewModelGetOrDefault(TextEditorViewModelKey);
	  
	  if (nextViewModel is not null)
	  	await nextViewModel.FocusAsync();
	}
	
	/// <summary>
	/// I checked when this fires.
	/// For keyboard events it does NOT re-fire as you type.
	/// Clicking and dragging does NOT re-fire as you move the cursor.
	///
	/// But, for an onclick it does re-fire back and forth between focusin and focusout about two times.
	/// </summary>
	[JSInvokable]
    public Task HandleFocusIn()
    {
        _focusIn = true;
        return InvokeAsync(StateHasChanged);
    }
    
    /// <summary>
	/// I checked when this fires.
	/// For keyboard events it does NOT re-fire as you type.
	/// Clicking and dragging does NOT re-fire as you move the cursor.
	///
	/// But, for an onclick it does re-fire back and forth between focusin and focusout about two times.
	/// </summary>
    [JSInvokable]
    public Task HandleFocusOut()
    {
        _focusIn = false;
        return InvokeAsync(StateHasChanged);
    }
    
    [JSInvokable]
    public void ReceiveOnKeyDown(TextEditorEventArgs eventArgs)
    {
    	TextEditorService.WorkerUi.Enqueue(
    	    eventArgs,
    	    _componentData,
        	TextEditorViewModelKey,
	        TextEditorWorkUiKind.OnKeyDown);
	}

    [JSInvokable]
    public void ReceiveOnContextMenu()
    {
		var localViewModelKey = TextEditorViewModelKey;

		TextEditorService.WorkerArbitrary.PostUnique(editContext =>
		{
			var viewModelModifier = editContext.GetViewModelModifier(localViewModelKey);
			var modelModifier = editContext.GetModelModifier(viewModelModifier.PersistentState.ResourceUri);

			if (modelModifier is null || viewModelModifier is null)
				return ValueTask.CompletedTask;

			TextEditorCommandDefaultFunctions.ShowContextMenu(
		        editContext,
		        modelModifier,
		        viewModelModifier,
		        DropdownService,
		        ComponentData);
			
			return ValueTask.CompletedTask;
		});
    }

    [JSInvokable]
    public void ReceiveOnDoubleClick(TextEditorEventArgs eventArgs)
    {
        TextEditorService.WorkerUi.Enqueue(
    	    eventArgs,
        	_componentData,
        	TextEditorViewModelKey,
	        TextEditorWorkUiKind.OnDoubleClick);
    }

    [JSInvokable]
    public void ReceiveContentOnMouseDown(TextEditorEventArgs eventArgs)
    {
        _componentData.ThinksLeftMouseButtonIsDown = true;
        _onMouseMoveMouseEventArgsStruct = new TextEditorEventArgs
        {
            Buttons = -1
        };

        TextEditorService.WorkerUi.Enqueue(
            eventArgs,
        	_componentData,
        	TextEditorViewModelKey,
	        TextEditorWorkUiKind.OnMouseDown);
    }

    [JSInvokable]
    public void ReceiveContentOnMouseMove(TextEditorEventArgs eventArgs)
	{
	    _userMouseIsInside = true;
	
	    // Buttons is a bit flag '& 1' gets if left mouse button is held
	    if ((eventArgs.Buttons & 1) == 0)
	        _componentData.ThinksLeftMouseButtonIsDown = false;
	
	    var localThinksLeftMouseButtonIsDown = _componentData.ThinksLeftMouseButtonIsDown;
	
	    // MouseStoppedMovingTask
	    _onMouseMoveMouseEventArgsStruct = eventArgs;
            
        if (_onMouseMoveTask.IsCompleted)
        {
            var cancellationToken = _onMouseMoveCancellationTokenSource.Token;

            _onMouseMoveTask = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                	var mouseMoveMouseEventArgs = _onMouseMoveMouseEventArgsStruct;
                	
                	if (!_userMouseIsInside || _componentData.ThinksLeftMouseButtonIsDown || mouseMoveMouseEventArgs.Buttons == -1)
                	{
                		TextEditorService.WorkerArbitrary.PostUnique(editContext =>
						{
							var viewModelModifier = editContext.GetViewModelModifier(TextEditorViewModelKey);

                            if (viewModelModifier is null)
                                return ValueTask.CompletedTask;

                            viewModelModifier.PersistentState.TooltipModel = null;
                            TooltipService.SetTooltipModel(viewModelModifier.PersistentState.TooltipModel);

							return ValueTask.CompletedTask;
						});
						break;
                	}
                	
                    await Task.Delay(400).ConfigureAwait(false);
                    
                    if (mouseMoveMouseEventArgs.X == _onMouseMoveMouseEventArgsStruct.X && mouseMoveMouseEventArgs.Y == _onMouseMoveMouseEventArgsStruct.Y)
                    {
                        await _componentData.ContinueRenderingTooltipAsync().ConfigureAwait(false);

				        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
			            {
			            	var viewModelModifier = editContext.GetViewModelModifier(TextEditorViewModelKey);
			                var modelModifier = editContext.GetModelModifier(viewModelModifier.PersistentState.ResourceUri);
			                
			                if (modelModifier is null || viewModelModifier is null)
			                    return ValueTask.CompletedTask;
			    
			                return TextEditorCommandDefaultFunctions.HandleMouseStoppedMovingEventAsync(
			                    editContext,
			                    modelModifier,
			                    viewModelModifier,
			                    mouseMoveMouseEventArgs.X,
                        		mouseMoveMouseEventArgs.Y,
                        		mouseMoveMouseEventArgs.ShiftKey,
                                mouseMoveMouseEventArgs.CtrlKey,
                                mouseMoveMouseEventArgs.AltKey,
			                    _componentData,
			                    TextEditorComponentRenderers,
			                    viewModelModifier.PersistentState.ResourceUri);
			            });

                        break;
                    }
                }
            });
        }
	
	    if (!_componentData.ThinksLeftMouseButtonIsDown)
	        return;
	    
	    if (localThinksLeftMouseButtonIsDown)
	    {
	    	TextEditorService.WorkerUi.Enqueue(
	    	    eventArgs,
	        	_componentData,
	        	TextEditorViewModelKey,
	        	TextEditorWorkUiKind.OnMouseMove);
	    }
	}

    [JSInvokable]
    public void ReceiveContentOnMouseOut(MouseEventArgsClass mouseEventArgsClass)
    {
        _userMouseIsInside = false;
    }
    
    [JSInvokable]
    public async Task HORIZONTAL_HandleOnMouseDownAsync(TextEditorEventArgs eventArgs)
    {
    	var renderBatchLocal = ComponentData.RenderBatch;
    	if (!renderBatchLocal.IsValid)
    		return;
    		
    	HORIZONTAL_thinksLeftMouseButtonIsDown = true;
		HORIZONTAL_scrollLeftOnMouseDown = _componentData.RenderBatch.ViewModel.ScrollLeft;

		var scrollbarBoundingClientRect = await TextEditorService.JsRuntimeCommonApi
			.MeasureElementById(HORIZONTAL_ScrollbarElementId)
			.ConfigureAwait(false);

		// Drag far up to reset scroll to original
		var textEditorDimensions = _componentData.RenderBatch.ViewModel.TextEditorDimensions;
		var distanceBetweenTopEditorAndTopScrollbar = scrollbarBoundingClientRect.TopInPixels - textEditorDimensions.BoundingClientRectTop;
		HORIZONTAL_clientYThresholdToResetScrollLeftPosition = scrollbarBoundingClientRect.TopInPixels - DISTANCE_TO_RESET_SCROLL_POSITION;

		// Subscribe to the drag events
		//
		// NOTE: '_mouseDownEventArgs' Buttons != -1 is what indicates that the subscription is active.
		//       So be wary if one intends to move its assignment elsewhere.
		{
			_mouseDownEventArgsStruct = eventArgs;
			_dragEventHandler = HORIZONTAL_DragEventHandlerScrollAsync;
	
			DragService.ReduceShouldDisplayAndMouseEventArgsSetAction(true, null);
		}
    }
    
    [JSInvokable]
    public async Task VERTICAL_HandleOnMouseDownAsync(TextEditorEventArgs eventArgs)
    {
    	var renderBatchLocal = _componentData.RenderBatch;
    	if (!renderBatchLocal.IsValid)
    		return;
    
    	VERTICAL_thinksLeftMouseButtonIsDown = true;
		VERTICAL_scrollTopOnMouseDown = renderBatchLocal.ViewModel.ScrollTop;

		var scrollbarBoundingClientRect = await TextEditorService.JsRuntimeCommonApi
			.MeasureElementById(VERTICAL_ScrollbarElementId)
			.ConfigureAwait(false);

		// Drag far left to reset scroll to original
		var textEditorDimensions = renderBatchLocal.ViewModel.TextEditorDimensions;
		var distanceBetweenLeftEditorAndLeftScrollbar = scrollbarBoundingClientRect.LeftInPixels - textEditorDimensions.BoundingClientRectLeft;
		VERTICAL_clientXThresholdToResetScrollTopPosition = scrollbarBoundingClientRect.LeftInPixels - DISTANCE_TO_RESET_SCROLL_POSITION;

		// Subscribe to the drag events
		//
		// NOTE: '_mouseDownEventArgs' being non-null is what indicates that the subscription is active.
		//       So be wary if one intends to move its assignment elsewhere.
		{
			_mouseDownEventArgsStruct = eventArgs;
			_dragEventHandler = VERTICAL_DragEventHandlerScrollAsync;
	
			DragService.ReduceShouldDisplayAndMouseEventArgsSetAction(true, null);
		}     
    }
    
    [JSInvokable]
    public void ReceiveOnWheel(TextEditorEventArgs eventArgs)
    {
	    TextEditorService.WorkerUi.Enqueue(
	        eventArgs,
        	_componentData,
        	TextEditorViewModelKey,
	        TextEditorWorkUiKind.OnWheel);
    }

    [JSInvokable]
    public void ReceiveOnTouchStart(TouchEventArgs touchEventArgs)
    {
        _touchStartDateTime = DateTime.UtcNow;

        _previousTouchEventArgs = touchEventArgs;
        _thinksTouchIsOccurring = true;
    }

    [JSInvokable]
    public void ReceiveOnTouchMove(TouchEventArgs touchEventArgs)
    {
        var localThinksTouchIsOccurring = _thinksTouchIsOccurring;

        if (!_thinksTouchIsOccurring)
             return;

        var previousTouchPoint = _previousTouchEventArgs?.ChangedTouches.FirstOrDefault(x => x.Identifier == 0);
        var currentTouchPoint = touchEventArgs.ChangedTouches.FirstOrDefault(x => x.Identifier == 0);

        if (previousTouchPoint is null || currentTouchPoint is null)
             return;

        // Natural scrolling for touch devices
        var diffX = previousTouchPoint.ClientX - currentTouchPoint.ClientX;
        var diffY = previousTouchPoint.ClientY - currentTouchPoint.ClientY;

        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
		{
			var viewModelModifier = editContext.GetViewModelModifier(TextEditorViewModelKey);
			
			if (viewModelModifier is null)
				return ValueTask.CompletedTask;
			
            TextEditorService.ViewModelApi.MutateScrollHorizontalPosition(
            	editContext,
		        viewModelModifier,
            	diffX);

            TextEditorService.ViewModelApi.MutateScrollVerticalPosition(
            	editContext,
		        viewModelModifier,
            	diffY);
            	
            return ValueTask.CompletedTask;
		});

        _previousTouchEventArgs = touchEventArgs;
    }
    
    [JSInvokable]
    public void ClearTouch(TouchEventArgs touchEventArgs)
    {
        var rememberStartTouchEventArgs = _previousTouchEventArgs;

        _thinksTouchIsOccurring = false;
        _previousTouchEventArgs = null;

        var clearTouchDateTime = DateTime.UtcNow;
        var touchTimespan = clearTouchDateTime - _touchStartDateTime;

        if (touchTimespan is null)
            return;

        if (touchTimespan.Value.TotalMilliseconds < 200)
        {
            var startTouchPoint = rememberStartTouchEventArgs?.ChangedTouches.FirstOrDefault(x => x.Identifier == 0);

            if (startTouchPoint is null)
                return;

            ReceiveContentOnMouseDown(new TextEditorEventArgs
            {
                Buttons = 1,
                X = startTouchPoint.ClientX,
                Y = startTouchPoint.ClientY,
            });
        }
    }

	private void QueueRemeasureBackgroundTask(
        TextEditorRenderBatch localRefCurrentRenderBatch,
        CancellationToken cancellationToken)
    {
        var viewModel = TextEditorService.TextEditorState.ViewModelGetOrDefault(TextEditorViewModelKey);
        if (viewModel is null)
            return;

        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
        	var viewModelModifier = editContext.GetViewModelModifier(viewModel.PersistentState.ViewModelKey);

			if (viewModelModifier is null)
				return ValueTask.CompletedTask;

        	return TextEditorService.ViewModelApi.RemeasureAsync(
        		editContext,
		        viewModelModifier);
        });
	}

    public void QueueCalculateVirtualizationResultBackgroundTask()
    {
        var viewModel = TextEditorService.TextEditorState.ViewModelGetOrDefault(TextEditorViewModelKey);
        if (viewModel is null)
            return;

        _componentData.InlineUiWidthStyleCssStringIsOutdated = true;

        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
        	var modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
        	var viewModelModifier = editContext.GetViewModelModifier(viewModel.PersistentState.ViewModelKey);

			if (modelModifier is null || viewModelModifier is null)
				return ValueTask.CompletedTask;
        	
        	viewModelModifier.CharAndLineMeasurements = TextEditorService.OptionsApi.GetOptions().CharAndLineMeasurements;
    	    viewModelModifier.ShouldCalculateVirtualizationResult = true;
    	    _componentData.InlineUiWidthStyleCssStringIsOutdated = true;
    	    
    	    var componentData = viewModelModifier.PersistentState.ComponentData;
    	    if (componentData is not null)
    	    {
    	        componentData.Virtualized_LineIndexCache_IsInvalid = true;
    	        
    	        if (!componentData.ViewModelDisplayOptions.IncludeGutterComponent)
    	        {
    	            // TODO: Consider using the font-size for an indication that various CSS needs to be re-calculated?...
    	            // ...at the moment the gutter width in pixels is used. But if you choose not to render a gutter
    	            // then there is no width difference to see.
    	            //
    	            // The initial value cannot be 0 else any text editor without a gutter cannot detect change on the initial render.
                    // Particularly, whatever the double subtraction -- absolute value precision -- check is, it has to be greater a difference than that.
    	            componentData.ViewModelGutterWidth = -2;
    	        }
    	    }
        	
        	TextEditorService.FinalizePost(editContext);
		    return ValueTask.CompletedTask;
		    // viewModelModifier.PersistentState.PostScrollAndRemeasure();
        });
    }

    private async void DragStateWrapOnStateChanged()
    {
        if (!DragService.GetDragState().ShouldDisplay)
        {
            // NOTE: '_mouseDownEventArgs' being non-null is what indicates that the subscription is active.
			//       So be wary if one intends to move its assignment elsewhere.
            _mouseDownEventArgsStruct = new TextEditorEventArgs
            {
                Buttons = -1
            };
        }
        else
        {
            var localMouseDownEventArgs = _mouseDownEventArgsStruct;
            var dragEventArgs = DragService.GetDragState().MouseEventArgs;
			var localDragEventHandler = _dragEventHandler;

            if (localMouseDownEventArgs.Buttons != -1 && dragEventArgs is not null)
                await localDragEventHandler.Invoke(localMouseDownEventArgs, dragEventArgs).ConfigureAwait(false);
        }
    }

    private Task HORIZONTAL_DragEventHandlerScrollAsync(TextEditorEventArgs localMouseDownEventArgsStruct, MouseEventArgs onDragMouseEventArgs)
    {
    	var renderBatchLocal = _componentData.RenderBatch;
    	if (!renderBatchLocal.IsValid)
    		return Task.CompletedTask;
    
    	var localThinksLeftMouseButtonIsDown = HORIZONTAL_thinksLeftMouseButtonIsDown;

        if (!localThinksLeftMouseButtonIsDown)
            return Task.CompletedTask;

        // Buttons is a bit flag '& 1' gets if left mouse button is held
        if (localThinksLeftMouseButtonIsDown && (onDragMouseEventArgs.Buttons & 1) == 1)
        {
			var textEditorDimensions = renderBatchLocal.ViewModel.TextEditorDimensions;
		
			double scrollLeft;

			if (onDragMouseEventArgs.ClientY < HORIZONTAL_clientYThresholdToResetScrollLeftPosition)
			{
				// Drag far left to reset scroll to original
				scrollLeft = HORIZONTAL_scrollLeftOnMouseDown;
			}
			else
			{
				var diffX = onDragMouseEventArgs.ClientX - localMouseDownEventArgsStruct.X;
	
	            var scrollbarWidthInPixels = textEditorDimensions.Width - ScrollbarFacts.SCROLLBAR_SIZE_IN_PIXELS;
	
	            scrollLeft = HORIZONTAL_scrollLeftOnMouseDown +
					diffX *
	                renderBatchLocal.ViewModel.ScrollWidth /
	                scrollbarWidthInPixels;
	
	            if (scrollLeft + textEditorDimensions.Width > renderBatchLocal.ViewModel.ScrollWidth)
	                scrollLeft = renderBatchLocal.ViewModel.ScrollWidth - textEditorDimensions.Width;

				if (scrollLeft < 0)
					scrollLeft = 0;
			}

			TextEditorService.WorkerUi.Enqueue(
			    new TextEditorEventArgs
			    {
			        X = scrollLeft
			    },
	        	_componentData,
	        	TextEditorViewModelKey,
		        TextEditorWorkUiKind.OnScrollHorizontal);
        }
        else
        {
            HORIZONTAL_thinksLeftMouseButtonIsDown = false;
        }

        return Task.CompletedTask;
    }
    
    private Task VERTICAL_DragEventHandlerScrollAsync(TextEditorEventArgs localMouseDownEventArgsStruct, MouseEventArgs onDragMouseEventArgs)
    {
    	var renderBatchLocal = _componentData.RenderBatch;
    	if (!renderBatchLocal.IsValid)
    		return Task.CompletedTask;
    
    	var localThinksLeftMouseButtonIsDown = VERTICAL_thinksLeftMouseButtonIsDown;

        if (!localThinksLeftMouseButtonIsDown)
            return Task.CompletedTask;

        // Buttons is a bit flag '& 1' gets if left mouse button is held
        if (localThinksLeftMouseButtonIsDown && (onDragMouseEventArgs.Buttons & 1) == 1)
        {
			var textEditorDimensions = renderBatchLocal.ViewModel.TextEditorDimensions;

			double scrollTop;

			if (onDragMouseEventArgs.ClientX < VERTICAL_clientXThresholdToResetScrollTopPosition)
			{
				// Drag far left to reset scroll to original
				scrollTop = VERTICAL_scrollTopOnMouseDown;
			}
			else
			{
	    		var diffY = onDragMouseEventArgs.ClientY - localMouseDownEventArgsStruct.Y;
	
	            var scrollbarHeightInPixels = textEditorDimensions.Height - ScrollbarFacts.SCROLLBAR_SIZE_IN_PIXELS;
	
	            scrollTop = VERTICAL_scrollTopOnMouseDown +
					diffY *
	                renderBatchLocal.ViewModel.ScrollHeight /
	                scrollbarHeightInPixels;
	
	            if (scrollTop + textEditorDimensions.Height > renderBatchLocal.ViewModel.ScrollHeight)
	                scrollTop = renderBatchLocal.ViewModel.ScrollHeight - textEditorDimensions.Height;

				if (scrollTop < 0)
					scrollTop = 0;
			}

			TextEditorService.WorkerUi.Enqueue(
			    new TextEditorEventArgs
			    {
			        Y = scrollTop
			    },
	        	_componentData,
	        	TextEditorViewModelKey,
		        TextEditorWorkUiKind.OnScrollVertical);
        }
        else
        {
            VERTICAL_thinksLeftMouseButtonIsDown = false;
        }

        return Task.CompletedTask;
    }
    
    private async void OnOptionMeasuredStateChanged()
    {
    	_componentData.SetWrapperCssAndStyle();
    	QueueCalculateVirtualizationResultBackgroundTask();
    }
    
    private async void OnOptionStaticStateChanged()
    {
    	_componentData.SetWrapperCssAndStyle();
    	await InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
    	// ScrollbarSection.razor.cs
    	DragService.DragStateChanged -= DragStateWrapOnStateChanged;
    
    	// TextEditorViewModelDisplay.razor.cs
        TextEditorService.TextEditorStateChanged -= GeneralOnStateChangedEventHandler;
        TextEditorService.OptionsApi.StaticStateChanged -= OnOptionStaticStateChanged;
        TextEditorService.OptionsApi.MeasuredStateChanged -= OnOptionMeasuredStateChanged;
		TextEditorService.ViewModelApi.CursorShouldBlinkChanged -= ViewModel_CursorShouldBlinkChanged;

		var linkedViewModel = _linkedViewModel;
        if (linkedViewModel is not null)
        {
        	TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
	    	{
	    		linkedViewModel.PersistentState.DisposeComponentData(editContext, ComponentData);
	    		_linkedViewModel = null;
	    	});
        }
        
        TextEditorService.TextEditorState._componentDataMap.Remove(_componentData.ComponentDataKey);

        _onMouseMoveCancellationTokenSource.Cancel();
        _onMouseMoveCancellationTokenSource.Dispose();
        
        _dotNetHelper?.Dispose();
    }
}
