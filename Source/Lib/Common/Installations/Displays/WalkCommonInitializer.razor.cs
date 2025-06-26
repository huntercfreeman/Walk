using System.Text;
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Outlines.Models;
using Walk.Common.RazorLib.Tooltips.Models;

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
    [Inject]
    private IDialogService DialogService { get; set; } = null!;
    [Inject]
    private IWidgetService WidgetService { get; set; } = null!;
    [Inject]
    private INotificationService NotificationService { get; set; } = null!;
    [Inject]
    private IDropdownService DropdownService { get; set; } = null!;
    [Inject]
	public IOutlineService OutlineService { get; set; } = null!;
    [Inject]
	private ITooltipService TooltipService { get; set; } = null!;
    
    public static Key<ContextSwitchGroup> ContextSwitchGroupKey { get; } = Key<ContextSwitchGroup>.NewKey();
    
    private CancellationTokenSource _workerCancellationTokenSource = new();
    
    /// <summary>
    /// Only use this from the "UI thread".
    /// </summary>
    private readonly StringBuilder _styleBuilder = new();
    
    /// <summary>The unit of measurement is Pixels (px)</summary>
	public const double OUTLINE_THICKNESS = 4;
    
    public double ValueTooltipRelativeX { get; set; }
    public double ValueTooltipRelativeY { get; set; }
	
	public string TooltipRelativeX { get; set; } = string.Empty;
	public string TooltipRelativeY { get; set; } = string.Empty;
	
	private ITooltipModel? _tooltipModelPrevious = null;
    
	protected override void OnInitialized()
	{
    	DialogService.DialogStateChanged += Shared_OnStateChanged;
    	WidgetService.WidgetStateChanged += Shared_OnStateChanged;
    	NotificationService.NotificationStateChanged += Shared_OnStateChanged;
    	DropdownService.DropdownStateChanged += Shared_OnStateChanged;
		OutlineService.OutlineStateChanged += Shared_OnStateChanged;
		TooltipService.TooltipStateChanged += Shared_OnStateChanged;
	
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
    
    private async void Shared_OnStateChanged() => await InvokeAsync(StateHasChanged);
    
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
    	
		DialogService.DialogStateChanged -= Shared_OnStateChanged;
    	WidgetService.WidgetStateChanged -= Shared_OnStateChanged;
    	NotificationService.NotificationStateChanged -= Shared_OnStateChanged;
        DropdownService.DropdownStateChanged -= Shared_OnStateChanged;
    	OutlineService.OutlineStateChanged -= Shared_OnStateChanged;
		TooltipService.TooltipStateChanged -= Shared_OnStateChanged;
		
		var notificationState = NotificationService.GetNotificationState();

        foreach (var notification in notificationState.DefaultList)
        {
            NotificationService.ReduceDisposeAction(notification.DynamicViewModelKey);
        }
    }
}
