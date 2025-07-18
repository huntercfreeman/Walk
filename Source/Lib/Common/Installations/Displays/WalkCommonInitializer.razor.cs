using System.Text;
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Tooltips.Models;
using Walk.Common.RazorLib.Dynamics.Models;

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
    private BrowserResizeInterop BrowserResizeInterop { get; set; } = null!;
    [Inject]
    private CommonService CommonService { get; set; } = null!;
    
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
    	CommonService.CommonUiStateChanged += OnCommonUiStateChanged;
	
        CommonService.Enqueue(new CommonWorkArgs
        {
        	WorkKind = CommonWorkKind.WalkCommonInitializerWork
    	});
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			var token = _workerCancellationTokenSource.Token;

			if (CommonService.ContinuousWorker.StartAsyncTask is null)
			{
				CommonService.ContinuousWorker.StartAsyncTask = Task.Run(
					() => CommonService.ContinuousWorker.ExecuteAsync(token),
					token);
			}

			if (CommonService.WalkHostingInformation.WalkPurposeKind == WalkPurposeKind.Ide)
			{
				if (CommonService.IndefiniteWorker.StartAsyncTask is null)
				{
					CommonService.IndefiniteWorker.StartAsyncTask = Task.Run(
						() => CommonService.IndefiniteWorker.ExecuteAsync(token),
						token);
				}
			}

			BrowserResizeInterop.SubscribeWindowSizeChanged((JsRuntimes.Models.WalkCommonJavaScriptInteropApi)CommonService.JsRuntimeCommonApi);
		}
	    
	    var tooltipModel = CommonService.GetTooltipState().TooltipModel;
	    
	    if (tooltipModel is not null && !tooltipModel.WasRepositioned && _tooltipModelPrevious != tooltipModel)
	    {
	        _tooltipModelPrevious = tooltipModel;
	        
	        CommonService.Tooltip_HtmlElementDimensions = await CommonService.JsRuntimeCommonApi.MeasureElementById(
    	        CommonService.Tooltip_HtmlElementId);
            CommonService.Tooltip_GlobalHtmlElementDimensions = await CommonService.JsRuntimeCommonApi.MeasureElementById(
    	        ContextFacts.RootHtmlElementId);
	    
    	    var xLarge = false;
    	    var yLarge = false;
    	    
    	    if (tooltipModel.X + CommonService.Tooltip_HtmlElementDimensions.WidthInPixels > CommonService.Tooltip_GlobalHtmlElementDimensions.WidthInPixels)
    	    {
    	        xLarge = true;
    	    }
    	    
    	    if (tooltipModel.Y + CommonService.Tooltip_HtmlElementDimensions.HeightInPixels > CommonService.Tooltip_GlobalHtmlElementDimensions.HeightInPixels)
    	    {
    	        yLarge = true;
    	    }
    	    
    	    tooltipModel.WasRepositioned = true;
    	    
    	    if (xLarge)
    	    {
        	    tooltipModel.X = CommonService.Tooltip_GlobalHtmlElementDimensions.WidthInPixels - CommonService.Tooltip_HtmlElementDimensions.WidthInPixels - 5;
        	    if (tooltipModel.X < 0)
        	        tooltipModel.X = 0;
	        }
    	     
    	    if (yLarge)
    	    {   
    	        tooltipModel.Y = CommonService.Tooltip_GlobalHtmlElementDimensions.HeightInPixels - CommonService.Tooltip_HtmlElementDimensions.HeightInPixels - 5;
        	    if (tooltipModel.Y < 0)
        	        tooltipModel.Y = 0;
    	    }
    	    
	        await InvokeAsync(StateHasChanged);
	    }
	}
    
    private async void OnCommonUiStateChanged(CommonUiEventKind commonUiEventKind)
    {
        switch (commonUiEventKind)
        {
            case CommonUiEventKind.DialogStateChanged:
        	case CommonUiEventKind.WidgetStateChanged:
        	case CommonUiEventKind.NotificationStateChanged:
        	case CommonUiEventKind.DropdownStateChanged:
        	case CommonUiEventKind.OutlineStateChanged:
        	case CommonUiEventKind.TooltipStateChanged:
        	    await InvokeAsync(StateHasChanged);
        	    break;
        }
    }
    
    private Task WIDGET_RemoveWidget()
    {
    	CommonService.SetWidget(null);
    	return Task.CompletedTask;
    }
    
    private async Task DROPDOWN_ClearActiveKeyList()
    {
    	var firstDropdown = CommonService.GetDropdownState().DropdownList.FirstOrDefault();
    	
    	if (firstDropdown is not null)
    	{
    		var restoreFocusOnCloseFunc = firstDropdown.RestoreFocusOnClose;
    		
    		if (restoreFocusOnCloseFunc is not null)
    			await restoreFocusOnCloseFunc.Invoke();
    	}
    	
        CommonService.Dropdown_ReduceClearAction();
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
		BrowserResizeInterop.DisposeWindowSizeChanged((JsRuntimes.Models.WalkCommonJavaScriptInteropApi)CommonService.JsRuntimeCommonApi);
    	
    	_workerCancellationTokenSource.Cancel();
    	_workerCancellationTokenSource.Dispose();
    	
    	CommonService.ContinuousWorker.StartAsyncTask = null;
    	CommonService.IndefiniteWorker.StartAsyncTask = null;
    	
		CommonService.CommonUiStateChanged -= OnCommonUiStateChanged;
		
		var notificationState = CommonService.GetNotificationState();

        foreach (var notification in notificationState.DefaultList)
        {
            CommonService.Notification_ReduceDisposeAction(notification.DynamicViewModelKey);
        }
    }
}
