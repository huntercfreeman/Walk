using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Tooltips.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;

namespace Walk.Common.RazorLib.Tooltips.Displays;

public partial class TooltipInitializer : ComponentBase, IDisposable
{
	[Inject]
	private ITooltipService TooltipService { get; set; } = null!;
	[Inject]
	private CommonBackgroundTaskApi CommonBackgroundTaskApi { get; set; } = null!;
	
	public double ValueTooltipRelativeX { get; set; }
    public double ValueTooltipRelativeY { get; set; }
	
	public string TooltipRelativeX { get; set; } = string.Empty;
	public string TooltipRelativeY { get; set; } = string.Empty;
	
	protected override void OnInitialized()
	{
	    TooltipService.TooltipStateChanged += OnTooltipStateChanged;
	    base.OnInitialized();
	}
	
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
	
	private async void OnTooltipStateChanged()
	{
	    await InvokeAsync(StateHasChanged);
	}
	
	public void Dispose()
	{
	    TooltipService.TooltipStateChanged -= OnTooltipStateChanged;
	}
}
