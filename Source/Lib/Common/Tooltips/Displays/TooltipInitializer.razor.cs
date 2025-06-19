using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Tooltips.Models;

namespace Walk.Common.RazorLib.Tooltips.Displays;

public partial class TooltipInitializer : ComponentBase, IDisposable
{
	[Inject]
	private ITooltipService TooltipService { get; set; } = null!;
	
	public double ValueTooltipRelativeX { get; set; }
    public double ValueTooltipRelativeY { get; set; }
	
	public string TooltipRelativeX { get; set; } = string.Empty;
	public string TooltipRelativeY { get; set; } = string.Empty;
	
	protected override void OnInitialized()
	{
	    TooltipService.TooltipStateChanged += OnTooltipStateChanged;
	    base.OnInitialized();
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
