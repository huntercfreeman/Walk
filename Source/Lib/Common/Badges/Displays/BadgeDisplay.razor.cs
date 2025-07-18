using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Badges.Models;

namespace Walk.Common.RazorLib.Badges.Displays;

/// <summary>
/// This component will subscribe to an event via the BadgeModel Parameter,
/// therefore you MUST use @key Blazor attribute and specify the BadgeModel.Key as the value
/// in order to ensure the component will not be "re-used" for BadgeModel's of a differing Key.
/// </summary>
public partial class BadgeDisplay : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

	[Parameter, EditorRequired]
	public IBadgeModel BadgeModel { get; set; } = null!;
	
	private int _seenCount;
	private string _countCssValue;
	
	protected override void OnInitialized()
	{
	    BadgeModel.AddSubscription(() => InvokeAsync(StateHasChanged));
	}
	
	private string GetCountCssValue()
	{
	    var localCount = BadgeModel.Count;
	    
	    if (_seenCount != localCount)
	    {
	        _seenCount = localCount;
	        _countCssValue = localCount.ToString();
	    }
	    
	    return _countCssValue;
	}
	
	public void Dispose()
	{
	    BadgeModel.DisposeSubscription();
	}
}
