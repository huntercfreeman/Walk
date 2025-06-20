using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Dimensions.Models;


/* Start DialogInitializer */
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Contexts.Displays;
using Walk.Common.RazorLib.Dynamics.Models;
/*namespace*/ using Walk.Common.RazorLib.Dialogs.Displays;
/* End DialogInitializer */


/* Start WidgetInitializer */
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Common.RazorLib.Contexts.Displays;
/*namespace*/ using Walk.Common.RazorLib.Widgets.Displays;
/* End WidgetInitializer */


/* Start NotificationInitializer */
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Contexts.Displays;
using Walk.Common.RazorLib.Dynamics.Models;
/*namespace*/ using Walk.Common.RazorLib.Notifications.Displays;
/* End NotificationInitializer */


/* Start DropdownInitializer */
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Contexts.Displays;
/*namespace*/ using Walk.Common.RazorLib.Dropdowns.Displays;
/* End DropdownInitializer */


/* Start OutlineInitializer */
using System.Text;
using Walk.Common.RazorLib.Outlines.Models;
/* End OutlineInitializer */


/* Start TooltipInitializer */
using Walk.Common.RazorLib.Tooltips.Models;
/* End TooltipInitializer */


namespace Walk.Common.RazorLib.Installations.Displays;

/// <summary>
/// NOT thread safe.
///
/// Ensure only 1 instance is rendered
/// to avoid race condition with:
/// 'BackgroundTaskService.ContinuousTaskWorker.StartAsyncTask = Task.Run...'
///
/// <DragInitializer/> is a separate component and must be rendered prior to this one
/// to get the drag functionality.
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
    /* End TooltipInitializer */
    
    
    public static Key<ContextSwitchGroup> ContextSwitchGroupKey { get; } = Key<ContextSwitchGroup>.NewKey();
    
    private CancellationTokenSource _workerCancellationTokenSource = new();
    
    /// <summary>
    /// Only use this from the "UI thread".
    /// </summary>
    private readonly StringBuilder _styleBuilder = new();
    
    
    /* Start DialogInitializer */
    private ContextBoundary? _dialogContextBoundary;
    /* End DialogInitializer */
    
    
    /* Start OutlineInitializer */
    /// <summary>The unit of measurement is Pixels (px)</summary>
	public const double OUTLINE_THICKNESS = 4;
    /* End OutlineInitializer */
    
    
    /* Start TooltipInitializer */
    public double ValueTooltipRelativeX { get; set; }
    public double ValueTooltipRelativeY { get; set; }
	
	public string TooltipRelativeX { get; set; } = string.Empty;
	public string TooltipRelativeY { get; set; } = string.Empty;
	
	private TooltipModel? _tooltipModelPrevious = null;
    /* End TooltipInitializer */
    
    
	protected override void OnInitialized()
	{
    	DialogService.DialogStateChanged += OnDialogStateChanged;
    	WidgetService.WidgetStateChanged += OnWidgetStateChanged;
    	NotificationService.NotificationStateChanged += OnNotificationStateChanged;
    	DropdownService.DropdownStateChanged += OnDropdownStateChanged;
		OutlineService.OutlineStateChanged += OnOutlineStateChanged;
		TooltipService.TooltipStateChanged += OnTooltipStateChanged;
	
        CommonBackgroundTaskApi.Enqueue(new CommonWorkArgs
        {
        	WorkKind = CommonWorkKind.WalkCommonInitializerWork
    	});
	}
	

	protected override async Task OnAfterRenderAsync(bool firstRender)
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
	    
	    var tooltipModel = TooltipService.GetTooltipState().TooltipModel;
	    
	    if (tooltipModel is not null && !tooltipModel.WasRepositioned && _tooltipModelPrevious != tooltipModel)
	    {
	        _tooltipModelPrevious = tooltipModel;
	        
	        TooltipService.HtmlElementDimensions = await CommonBackgroundTaskApi.JsRuntimeCommonApi.MeasureElementById(
    	        TooltipService.HtmlElementId);
            TooltipService.GlobalHtmlElementDimensions = await CommonBackgroundTaskApi.JsRuntimeCommonApi.MeasureElementById(
    	        ContextFacts.RootHtmlElementId);
	    
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
    	    
    	    Console.WriteLine("tooltip was repositioned");
    	    // Console.WriteLine($"xLarge:{xLarge} yLarge:{yLarge}");
    	    
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
	}
    
    
    /* Start DialogInitializer */
    private Task HandleOnFocusIn()
    {
    	var localDialogContextBoundary = _dialogContextBoundary;
    	
    	if (localDialogContextBoundary is not null)
	    	localDialogContextBoundary.HandleOnFocusIn();
    
    	return Task.CompletedTask;
    }
    
    private Task HandleOnFocusOut()
    {
    	return Task.CompletedTask;
    }
    
    private async void OnDialogStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    /* End DialogInitializer */
    
    
    /* Start WidgetInitializer */
    private async void OnWidgetStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    /* End WidgetInitializer */
    
    
    /* Start NotificationInitializer */
    public async void OnNotificationStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    /* End NotificationInitializer */
    
    
    /* Start DropdownInitializer */
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
    
    
    /* Start of misc */
    private Task WIDGET_RemoveWidget()
    {
    	WidgetService.SetWidget(null);
    	return Task.CompletedTask;
    }
    
    private async Task DROPDOWN_ClearActiveKeyList()
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
    
    public string OUTLINE_GetStyleCssLeft(OutlineState localOutlineState)
	{
		var width = OUTLINE_THICKNESS;
		
		var height = localOutlineState.MeasuredHtmlElementDimensions.HeightInPixels;
	
		var left = localOutlineState.MeasuredHtmlElementDimensions.LeftInPixels;
		
		var top = localOutlineState.MeasuredHtmlElementDimensions.TopInPixels;
		
		_styleBuilder.Clear();
		
		_styleBuilder.Append("width: ");
		_styleBuilder.Append(width.ToCssValue());
		_styleBuilder.Append("px; ");
		
		_styleBuilder.Append("height: ");
		_styleBuilder.Append(height.ToCssValue());
		_styleBuilder.Append("px; ");
		
		_styleBuilder.Append($"left: ");
		_styleBuilder.Append(left.ToCssValue());
		_styleBuilder.Append("px; ");
		
		_styleBuilder.Append("top: ");
		_styleBuilder.Append(top.ToCssValue());
		_styleBuilder.Append("px; ");
		
		return _styleBuilder.ToString();
	}
	
	public string OUTLINE_GetStyleCssRight(OutlineState localOutlineState)
	{
		var width = OUTLINE_THICKNESS;
		
		var height = localOutlineState.MeasuredHtmlElementDimensions.HeightInPixels;
	
		var left = localOutlineState.MeasuredHtmlElementDimensions.LeftInPixels +
			localOutlineState.MeasuredHtmlElementDimensions.WidthInPixels -
			OUTLINE_THICKNESS;
		
		var top = localOutlineState.MeasuredHtmlElementDimensions.TopInPixels;
			
		_styleBuilder.Clear();
		
		_styleBuilder.Append("width: ");
		_styleBuilder.Append(width.ToCssValue());
		_styleBuilder.Append("px; ");
		
		_styleBuilder.Append("height: ");
		_styleBuilder.Append(height.ToCssValue());
		_styleBuilder.Append("px; ");
		
		_styleBuilder.Append("left: ");
		_styleBuilder.Append(left.ToCssValue());
		_styleBuilder.Append("px; ");
		
		_styleBuilder.Append("top: ");
		_styleBuilder.Append(top.ToCssValue());
		_styleBuilder.Append("px; ");
		
		return _styleBuilder.ToString();
	}
	
	public string OUTLINE_GetStyleCssTop(OutlineState localOutlineState)
	{
		var width = localOutlineState.MeasuredHtmlElementDimensions.WidthInPixels;
		
		var height = OUTLINE_THICKNESS;
	
		var left = localOutlineState.MeasuredHtmlElementDimensions.LeftInPixels;
		
		var top = localOutlineState.MeasuredHtmlElementDimensions.TopInPixels;
		
		_styleBuilder.Clear();
		
		_styleBuilder.Append("width: ");
		_styleBuilder.Append(width.ToCssValue());
		_styleBuilder.Append("px; ");
		
		_styleBuilder.Append("height: ");
		_styleBuilder.Append(height.ToCssValue());
		_styleBuilder.Append("px; ");
		
		_styleBuilder.Append("left: ");
		_styleBuilder.Append(left.ToCssValue());
		_styleBuilder.Append("px; ");
		
		_styleBuilder.Append("top: ");
		_styleBuilder.Append(top.ToCssValue());
		_styleBuilder.Append("px; ");
		
		return _styleBuilder.ToString();
	}
	
	public string OUTLINE_GetStyleCssBottom(OutlineState localOutlineState)
	{
		var width = localOutlineState.MeasuredHtmlElementDimensions.WidthInPixels;
		
		var height = OUTLINE_THICKNESS;
	
		var left = localOutlineState.MeasuredHtmlElementDimensions.LeftInPixels;
		
		var top = localOutlineState.MeasuredHtmlElementDimensions.TopInPixels +
			localOutlineState.MeasuredHtmlElementDimensions.HeightInPixels -
			OUTLINE_THICKNESS;
			
		_styleBuilder.Clear();
		
		_styleBuilder.Append($"width: ");
		_styleBuilder.Append(width.ToCssValue());
		_styleBuilder.Append("px; ");
		
		_styleBuilder.Append($"height: ");
		_styleBuilder.Append(height.ToCssValue());
		_styleBuilder.Append("px; ");
		
		_styleBuilder.Append($"left: ");
		_styleBuilder.Append(left.ToCssValue());
		_styleBuilder.Append("px; ");
		
		_styleBuilder.Append($"top: ");
		_styleBuilder.Append(top.ToCssValue());
		_styleBuilder.Append("px; ");
		
		return _styleBuilder.ToString();
	}
    /* End of misc */
    
    
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
    	
		DialogService.DialogStateChanged -= OnDialogStateChanged;
    	WidgetService.WidgetStateChanged -= OnWidgetStateChanged;
    	NotificationService.NotificationStateChanged -= OnNotificationStateChanged;
        DropdownService.DropdownStateChanged -= OnDropdownStateChanged;
    	OutlineService.OutlineStateChanged -= OnOutlineStateChanged;
		TooltipService.TooltipStateChanged -= OnTooltipStateChanged;
		
		var notificationState = NotificationService.GetNotificationState();

        foreach (var notification in notificationState.DefaultList)
        {
            NotificationService.ReduceDisposeAction(notification.DynamicViewModelKey);
        }
    }
}
