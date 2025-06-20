using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Dimensions.Models;



/* Start DragInitializer */
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Drags.Models;
/*namespace*/ using Walk.Common.RazorLib.Drags.Displays;
/* End DragInitializer */



/* Start DialogInitializer */
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Contexts.Displays;
using Walk.Common.RazorLib.Dynamics.Models;
/*namespace*/ using Walk.Common.RazorLib.Dialogs.Displays;
/* End DialogInitializer */



/* Start WidgetInitializer */
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Common.RazorLib.Contexts.Displays;
/*namespace*/ using Walk.Common.RazorLib.Widgets.Displays;
/* End WidgetInitializer */



/* Start NotificationInitializer */
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Contexts.Displays;
using Walk.Common.RazorLib.Dynamics.Models;
/*namespace*/ using Walk.Common.RazorLib.Notifications.Displays;
/* End NotificationInitializer */



/* Start DropdownInitializer */
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Contexts.Displays;
/*namespace*/ using Walk.Common.RazorLib.Dropdowns.Displays;
/* End DropdownInitializer */



/* Start OutlineInitializer */
using System.Text;
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Outlines.Models;
using Walk.Common.RazorLib.Dimensions.Models;
/*namespace*/ using Walk.Common.RazorLib.Outlines.Displays;
/* End OutlineInitializer */



/* Start TooltipInitializer */
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Tooltips.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
/*namespace*/ using Walk.Common.RazorLib.Tooltips.Displays;
/* End TooltipInitializer */



namespace Walk.Common.RazorLib.Installations.Displays;

/// <summary>
/// NOT thread safe.
///
/// Ensure only 1 instance is rendered
/// to avoid race condition with:
/// 'BackgroundTaskService.ContinuousTaskWorker.StartAsyncTask = Task.Run...'
/// </summary>
public partial class WalkCommonInitializer : ComponentBase, IDisposable
{
    [Inject]
    private BackgroundTaskService BackgroundTaskService { get; set; } = null!;
	[Inject]
    private CommonBackgroundTaskApi CommonBackgroundTaskApi { get; set; } = null!;
    [Inject]
    private BrowserResizeInterop BrowserResizeInterop { get; set; } = null!;
    [Inject]
    private WalkHostingInformation WalkHostingInformation { get; set; } = null!;
    
    
    
    /* Start DragInitializer */
    [Inject]
    private IDragService DragService { get; set; } = null!;
    /* End DragInitializer */
    
    
    
    /* Start DialogInitializer */
    [Inject]
    private IDialogService DialogService { get; set; } = null!;
    /* End DialogInitializer */
    
    
    
    /* Start WidgetInitializer */
    [Inject]
    private IWidgetService WidgetService { get; set; } = null!;
    /* End WidgetInitializer */
    
    
    
    /* Start NotificationInitializer */
    [Inject]
    private INotificationService NotificationService { get; set; } = null!;
    /* End NotificationInitializer */
    
    
    
    /* Start DropdownInitializer */
    [Inject]
    private IDropdownService DropdownService { get; set; } = null!;
    /* End DropdownInitializer */
    
    
    
    /* Start OutlineInitializer */
    [Inject]
	public IOutlineService OutlineService { get; set; } = null!;
    /* End OutlineInitializer */
    
    
    
    /* Start TooltipInitializer */
    [Inject]
	private ITooltipService TooltipService { get; set; } = null!;
	[Inject]
	private CommonBackgroundTaskApi CommonBackgroundTaskApi { get; set; } = null!;
    /* End TooltipInitializer */
    
    
    
    public static Key<ContextSwitchGroup> ContextSwitchGroupKey { get; } = Key<ContextSwitchGroup>.NewKey();
    
    private CancellationTokenSource _workerCancellationTokenSource = new();
    
    
    
    /* Start DragInitializer */
    private string StyleCss => DragService.GetDragState().ShouldDisplay
        ? string.Empty
        : "display: none;";

    private ThrottleOptimized<MouseEvent> _throttle;
    
    public struct MouseEvent
    {
    	public MouseEvent(bool isOnMouseMove, MouseEventArgs mouseEventArgs)
    	{
    		IsOnMouseMove = isOnMouseMove;
    		MouseEventArgs = mouseEventArgs;
    	}
    
    	public bool IsOnMouseMove { get; }
    	public MouseEventArgs MouseEventArgs { get; }
    }

    private IDropzone? _onMouseOverDropzone = null;
    /* End DragInitializer */
    
    
    
    /* Start DialogInitializer */
    private ContextBoundary? _dialogContextBoundary;
    /* End DialogInitializer */
    
    
    
    /* Start WidgetInitializer */
    private ContextBoundary? _widgetContextBoundary;
    /* End WidgetInitializer */
    
    
    
    /* Start NotificationInitializer */
    private ContextBoundary? _notificationContextBoundary;
    /* End NotificationInitializer */
    
    
    
    /* Start DropdownInitializer */
    private ContextBoundary? _dropdownContextBoundary;
    /* End DropdownInitializer */
    
    
    
    /* Start OutlineInitializer */
    /// <summary>The unit of measurement is Pixels (px)</summary>
	public const double OUTLINE_THICKNESS = 4;
    /* End OutlineInitializer */
    
    
    
    /* Start TooltipInitializer */
    public double ValueTooltipRelativeX { get; set; }
    public double ValueTooltipRelativeY { get; set; }
	
	public string TooltipRelativeX { get; set; } = string.Empty;
	public string TooltipRelativeY { get; set; } = string.Empty;
    /* End TooltipInitializer */
    
    
    
	protected override void OnInitialized()
	{
        CommonBackgroundTaskApi.Enqueue(new CommonWorkArgs
        {
        	WorkKind = CommonWorkKind.WalkCommonInitializerWork
    	});
        base.OnInitialized();
	}
	
	

    /* Start DragInitializer */
    protected override void OnInitialized()
    {
    	DragService.DragStateChanged += OnDragStateChanged;
    
    	_throttle = new(ThrottleFacts.TwentyFour_Frames_Per_Second, async (args, _) =>
	    {
	    	if (args.IsOnMouseMove)
	    	{
	    		if ((args.MouseEventArgs.Buttons & 1) != 1)
	                DispatchClearDragStateAction();
	            else
	                DragService.ReduceShouldDisplayAndMouseEventArgsSetAction(true, args.MouseEventArgs);
	
	            return;
	    	}
	    	else
	    	{
	    		var dragState = DragService.GetDragState();
				var localOnMouseOverDropzone = _onMouseOverDropzone;
	    	
	    		DispatchClearDragStateAction();
	
	            var draggableViewModel = dragState.Drag;
	            if (draggableViewModel is not null)
	            {
	                await draggableViewModel
	                    .OnDragEndAsync(args.MouseEventArgs, localOnMouseOverDropzone)
	                    .ConfigureAwait(false);
	            }
	    	}
	    });
    	
    	base.OnInitialized();
    }
    /* End DragInitializer */
    
    
    
    /* Start DialogInitializer */
    protected override void OnInitialized()
    {
    	DialogService.DialogStateChanged += OnDialogStateChanged;
    	base.OnInitialized();
    }
    /* End DialogInitializer */
    
    
    
    /* Start WidgetInitializer */
    protected override void OnInitialized()
    {
    	WidgetService.WidgetStateChanged += OnWidgetStateChanged;
    	base.OnInitialized();
    }
    /* End WidgetInitializer */
    
    
    
    /* Start NotificationInitializer */
    protected override void OnInitialized()
    {
    	NotificationService.NotificationStateChanged += OnNotificationStateChanged;
    	base.OnInitialized();
    }
    /* End NotificationInitializer */
    
    
    
    /* Start DropdownInitializer */
    protected override void OnInitialized()
	{
		DropdownService.DropdownStateChanged += OnDropdownStateChanged;
		base.OnInitialized();
	}
    /* End DropdownInitializer */
    
    
    
    /* Start OutlineInitializer */
    protected override void OnInitialized()
	{
		OutlineService.OutlineStateChanged += OnOutlineStateChanged;
		base.OnInitialized();
	}
    /* End OutlineInitializer */
    
    
    
    /* Start TooltipInitializer */
    protected override void OnInitialized()
	{
	    TooltipService.TooltipStateChanged += OnTooltipStateChanged;
	    base.OnInitialized();
	}
    /* End TooltipInitializer */



	protected override void OnAfterRender(bool firstRender)
	{
		if (firstRender)
		{
			var token = _workerCancellationTokenSource.Token;

			if (BackgroundTaskService.ContinuousWorker.StartAsyncTask is null)
			{
				BackgroundTaskService.ContinuousWorker.StartAsyncTask = Task.Run(
					() => BackgroundTaskService.ContinuousWorker.ExecuteAsync(token),
					token);
			}

			if (WalkHostingInformation.WalkPurposeKind == WalkPurposeKind.Ide)
			{
				if (BackgroundTaskService.IndefiniteWorker.StartAsyncTask is null)
				{
					BackgroundTaskService.IndefiniteWorker.StartAsyncTask = Task.Run(
						() => BackgroundTaskService.IndefiniteWorker.ExecuteAsync(token),
						token);
				}
			}

			BrowserResizeInterop.SubscribeWindowSizeChanged(CommonBackgroundTaskApi.JsRuntimeCommonApi);
		}

		base.OnAfterRender(firstRender);
	}
	
	
    
    /* Start DragInitializer */
    /* empty OnAfterRenderAsync(...) */
    /* End DragInitializer */
    
    
    
    /* Start DialogInitializer */
    /* empty OnAfterRenderAsync(...) */
    /* End DialogInitializer */
    
    
    
    /* Start WidgetInitializer */
    /* empty OnAfterRenderAsync(...) */
    /* End WidgetInitializer */
    
    
    
    /* Start NotificationInitializer */
    /* empty OnAfterRenderAsync(...) */
    /* End NotificationInitializer */
    
    
    
    /* Start DropdownInitializer */
    /* empty OnAfterRenderAsync(...) */
    /* End DropdownInitializer */
    
    
    
    /* Start OutlineInitializer */
    /* empty OnAfterRenderAsync(...) */
    /* End OutlineInitializer */
    
    
    
    /* Start TooltipInitializer */
    protected override async Task OnAfterRenderAsync(bool firstRender)
	{
	    TooltipService.HtmlElementDimensions = await CommonBackgroundTaskApi.JsRuntimeCommonApi.MeasureElementById(
	        TooltipService.HtmlElementId);
        TooltipService.GlobalHtmlElementDimensions = await CommonBackgroundTaskApi.JsRuntimeCommonApi.MeasureElementById(
	        ContextFacts.RootHtmlElementId);
	    
	    var tooltipModel = TooltipService.GetTooltipState().TooltipModel;
	    
	    if (tooltipModel is not null && !tooltipModel.WasRepositioned)
	    {
    	    var xLarge = false;
    	    var yLarge = false;
    	    
    	    if (tooltipModel.X + TooltipService.HtmlElementDimensions.WidthInPixels > TooltipService.GlobalHtmlElementDimensions.WidthInPixels)
    	    {
    	        xLarge = true;
    	    }
    	    
    	    if (tooltipModel.Y + TooltipService.HtmlElementDimensions.HeightInPixels > TooltipService.GlobalHtmlElementDimensions.HeightInPixels)
    	    {
    	        yLarge = true;
    	    }
    	    
    	    Console.WriteLine($"xLarge:{xLarge} yLarge:{yLarge}");
    	    
    	    tooltipModel.WasRepositioned = true;
    	    
    	    if (xLarge)
    	    {
        	    tooltipModel.X = TooltipService.GlobalHtmlElementDimensions.WidthInPixels - TooltipService.HtmlElementDimensions.WidthInPixels - 5;
        	    if (tooltipModel.X < 0)
        	        tooltipModel.X = 0;
	        }
    	     
    	    if (yLarge)
    	    {   
    	        tooltipModel.Y = TooltipService.GlobalHtmlElementDimensions.HeightInPixels - TooltipService.HtmlElementDimensions.HeightInPixels - 5;
        	    if (tooltipModel.Y < 0)
        	        tooltipModel.Y = 0;
    	    }
    	    
	        await InvokeAsync(StateHasChanged);
	    }
	    
	    await base.OnAfterRenderAsync(firstRender);
	}
    /* End TooltipInitializer */
    
    
    
    /// <summary>
    /// Presumptions:
	/// - Dispose is invoked from UI thread
	/// - Dispose being ran stops the Blazor lifecycle from being ran in the future
    ///     - i.e.: OnInitialized(), Dispose(), OnAfterRender() <---- bad. Is it possible?
    /// </summary>
    public void Dispose()
    {
    	BrowserResizeInterop.DisposeWindowSizeChanged(CommonBackgroundTaskApi.JsRuntimeCommonApi);
    	
    	_workerCancellationTokenSource.Cancel();
    	_workerCancellationTokenSource.Dispose();
    	
    	BackgroundTaskService.ContinuousWorker.StartAsyncTask = null;
    	BackgroundTaskService.IndefiniteWorker.StartAsyncTask = null;
    }
    
    
    
    /* Start DragInitializer */
    public void Dispose()
	{
		DragService.DragStateChanged -= OnDragStateChanged;
	}
    /* End DragInitializer */
    
    
    
    /* Start DialogInitializer */
    public void Dispose()
    {
    	DialogService.DialogStateChanged -= OnDialogStateChanged;
    }
    /* End DialogInitializer */
    
    
    
    /* Start WidgetInitializer */
    public void Dispose()
    {
    	WidgetService.WidgetStateChanged -= OnWidgetStateChanged;
    }
    /* End WidgetInitializer */
    
    
    
    /* Start NotificationInitializer */
    public void Dispose()
	{
		NotificationService.NotificationStateChanged -= OnNotificationStateChanged;
	
		var notificationState = NotificationService.GetNotificationState();

        foreach (var notification in notificationState.DefaultList)
        {
            NotificationService.ReduceDisposeAction(notification.DynamicViewModelKey);
        }
	}
    /* End NotificationInitializer */
    
    
    
    /* Start DropdownInitializer */
    public void Dispose()
    {
    	DropdownService.DropdownStateChanged -= OnDropdownStateChanged;
    }
    /* End DropdownInitializer */
    
    
    
    /* Start OutlineInitializer */
    public void Dispose()
	{
		OutlineService.OutlineStateChanged -= OnOutlineStateChanged;
	}
    /* End OutlineInitializer */
    
    
    
    /* Start TooltipInitializer */
    public void Dispose()
	{
	    TooltipService.TooltipStateChanged -= OnTooltipStateChanged;
	}
    /* End TooltipInitializer */
    
    
    
    /* Start DragInitializer */
    private async void OnDragStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    /* End DragInitializer */
    
    
    
    /* Start DialogInitializer */
    private Task HandleOnFocusIn(IDialog dialog)
    {
    	var localDialogContextBoundary = _dialogContextBoundary;
    	
    	if (localDialogContextBoundary is not null)
	    	localDialogContextBoundary.HandleOnFocusIn();
    
    	return Task.CompletedTask;
    }
    
    private Task HandleOnFocusOut(IDialog dialog)
    {
    	return Task.CompletedTask;
    }
    
    private async void OnDialogStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    /* End DialogInitializer */
    
    
    
    /* Start WidgetInitializer */
    private Task HandleOnFocusIn(WidgetModel widget)
    {
    	var localWidgetContextBoundary = _widgetContextBoundary;
    	
    	if (localWidgetContextBoundary is not null)
	    	localWidgetContextBoundary.HandleOnFocusIn();
    
    	return Task.CompletedTask;
    }
    
    private Task HandleOnFocusOut(WidgetModel widget)
    {
    	// TODO: neither onfocusout or onblur fit the use case.
    	//       I need to detect when focus leaves either the widget itself
    	//       or leaves its descendents (this part sounds like onfocusout).
    	//       |
    	//       BUT
    	//       |
    	//       I furthermore, ONLY want to have this fire if the newly focused
    	//       HTML element is neither the widget itself or one of its descendents.
    	//       |
    	//       When this event occurs, the widget should no longer render.
    	return Task.CompletedTask;
    }
    
    private async void OnWidgetStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    /* End WidgetInitializer */
    
    
    
    /* Start NotificationInitializer */
    private Task HandleOnFocusIn(INotification notification)
    {
    	var localNotificationContextBoundary = _notificationContextBoundary;
    	
    	if (localNotificationContextBoundary is not null)
	    	localNotificationContextBoundary.HandleOnFocusIn();
	    	
	    return Task.CompletedTask;
    }
    
    private Task HandleOnFocusOut(INotification notification)
    {
    	return Task.CompletedTask;
    }
    
    public async void OnNotificationStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    /* End NotificationInitializer */
    
    
    
    /* Start DropdownInitializer */
    private Task HandleOnFocusIn(DropdownRecord dropdown)
    {
    	var localDropdownContextBoundary = _dropdownContextBoundary;
    	
    	if (localDropdownContextBoundary is not null)
	    	localDropdownContextBoundary.HandleOnFocusIn();
    
    	return Task.CompletedTask;
    }
    
    private Task HandleOnFocusOut(DropdownRecord dropdown)
    {
    	return Task.CompletedTask;
    }
    
    public async void OnDropdownStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    /* End DropdownInitializer */
    
    
    
    /* Start OutlineInitializer */
    private async void OnOutlineStateChanged()
	{
		await InvokeAsync(StateHasChanged);
	}
    /* End OutlineInitializer */
    
    
    
    /* Start TooltipInitializer */
    private async void OnTooltipStateChanged()
	{
	    await InvokeAsync(StateHasChanged);
	}
    /* End TooltipInitializer */
    
    
    
    /* Start DragInitializer */
    public partial class DragInitializer : ComponentBase, IDisposable
    {
        private void DispatchClearDragStateAction()
        {
    		_onMouseOverDropzone = null;
    		
            DragService.ReduceShouldDisplayAndMouseEventArgsAndDragSetAction(
            	false,
                null,
    			null);
        }
    
        private void DispatchSetDragStateActionOnMouseMove(MouseEventArgs mouseEventArgs)
        {
            _throttle.Run(new(isOnMouseMove: true, mouseEventArgs));
        }
    
        private void DispatchSetDragStateActionOnMouseUp(MouseEventArgs mouseEventArgs)
        {
            _throttle.Run(new(isOnMouseMove: false, mouseEventArgs));
        }
    
    	private string GetIsActiveCssClass(IDropzone dropzone)
    	{
    		var onMouseOverDropzoneKey = _onMouseOverDropzone?.DropzoneKey ?? Key<IDropzone>.Empty;
    
    		return onMouseOverDropzoneKey == dropzone.DropzoneKey
                ? "di_active"
    			: string.Empty;
    	}
    }
    /* End DragInitializer */
    
    
    
    /* Start DialogInitializer */
    public partial class DialogInitializer : ComponentBase, IDisposable { }
    /* End DialogInitializer */
    
    
    
    /* Start WidgetInitializer */
    public partial class WidgetInitializer : ComponentBase, IDisposable
    {
    	private Task RemoveWidget()
        {
        	WidgetService.SetWidget(null);
        	return Task.CompletedTask;
        }
    }
    /* End WidgetInitializer */
    
    
    
    /* Start NotificationInitializer */
    public partial class NotificationInitializer : ComponentBase, IDisposable { }
    /* End NotificationInitializer */
    
    
    
    /* Start DropdownInitializer */
    public partial class DropdownInitializer : ComponentBase, IDisposable
    {
    	private async Task ClearActiveKeyList()
        {
        	var firstDropdown = DropdownService.GetDropdownState().DropdownList.FirstOrDefault();
        	
        	if (firstDropdown is not null)
        	{
        		var restoreFocusOnCloseFunc = firstDropdown.RestoreFocusOnClose;
        		
        		if (restoreFocusOnCloseFunc is not null)
        			await restoreFocusOnCloseFunc.Invoke();
        	}
        	
            DropdownService.ReduceClearAction();
        }
    }
    /* End DropdownInitializer */
    
    
    
    /* Start OutlineInitializer */
    public partial class OutlineInitializer : ComponentBase, IDisposable
    {
    	public string GetStyleCssLeft(OutlineState localOutlineState)
    	{
    		var width = OUTLINE_THICKNESS;
    		
    		var height = localOutlineState.MeasuredHtmlElementDimensions.HeightInPixels;
    	
    		var left = localOutlineState.MeasuredHtmlElementDimensions.LeftInPixels;
    		
    		var top = localOutlineState.MeasuredHtmlElementDimensions.TopInPixels;
    		
    		var styleBuilder = new StringBuilder();
    		
    		styleBuilder.Append($"width: {width.ToCssValue()}px; ");
    		styleBuilder.Append($"height: {height.ToCssValue()}px; ");
    		styleBuilder.Append($"left: {left.ToCssValue()}px; ");
    		styleBuilder.Append($"top: {top.ToCssValue()}px; ");
    		
    		return styleBuilder.ToString();
    	}
    	
    	public string GetStyleCssRight(OutlineState localOutlineState)
    	{
    		var width = OUTLINE_THICKNESS;
    		
    		var height = localOutlineState.MeasuredHtmlElementDimensions.HeightInPixels;
    	
    		var left = localOutlineState.MeasuredHtmlElementDimensions.LeftInPixels +
    			localOutlineState.MeasuredHtmlElementDimensions.WidthInPixels -
    			OUTLINE_THICKNESS;
    		
    		var top = localOutlineState.MeasuredHtmlElementDimensions.TopInPixels;
    			
    		var styleBuilder = new StringBuilder();
    		
    		styleBuilder.Append($"width: {width.ToCssValue()}px; ");
    		styleBuilder.Append($"height: {height.ToCssValue()}px; ");
    		styleBuilder.Append($"left: {left.ToCssValue()}px; ");
    		styleBuilder.Append($"top: {top.ToCssValue()}px; ");
    		
    		return styleBuilder.ToString();
    	}
    	
    	public string GetStyleCssTop(OutlineState localOutlineState)
    	{
    		var width = localOutlineState.MeasuredHtmlElementDimensions.WidthInPixels;
    		
    		var height = OUTLINE_THICKNESS;
    	
    		var left = localOutlineState.MeasuredHtmlElementDimensions.LeftInPixels;
    		
    		var top = localOutlineState.MeasuredHtmlElementDimensions.TopInPixels;
    		
    		var styleBuilder = new StringBuilder();
    		
    		styleBuilder.Append($"width: {width.ToCssValue()}px; ");
    		styleBuilder.Append($"height: {height.ToCssValue()}px; ");
    		styleBuilder.Append($"left: {left.ToCssValue()}px; ");
    		styleBuilder.Append($"top: {top.ToCssValue()}px; ");
    		
    		return styleBuilder.ToString();
    	}
    	
    	public string GetStyleCssBottom(OutlineState localOutlineState)
    	{
    		var width = localOutlineState.MeasuredHtmlElementDimensions.WidthInPixels;
    		
    		var height = OUTLINE_THICKNESS;
    	
    		var left = localOutlineState.MeasuredHtmlElementDimensions.LeftInPixels;
    		
    		var top = localOutlineState.MeasuredHtmlElementDimensions.TopInPixels +
    			localOutlineState.MeasuredHtmlElementDimensions.HeightInPixels -
    			OUTLINE_THICKNESS;
    			
    		var styleBuilder = new StringBuilder();
    		
    		styleBuilder.Append($"width: {width.ToCssValue()}px; ");
    		styleBuilder.Append($"height: {height.ToCssValue()}px; ");
    		styleBuilder.Append($"left: {left.ToCssValue()}px; ");
    		styleBuilder.Append($"top: {top.ToCssValue()}px; ");
    		
    		return styleBuilder.ToString();
    	}
    }
    /* End OutlineInitializer */
    
    
    
    /* Start TooltipInitializer */
    public partial class TooltipInitializer : ComponentBase, IDisposable { }
    /* End TooltipInitializer */
    
    

}