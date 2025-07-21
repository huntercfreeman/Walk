using Microsoft.AspNetCore.Components;

namespace Walk.Extensions.DotNet.DotNetSolutions.Displays.Internals;

public partial class SolutionPropertiesDisplay : ComponentBase, IDisposable
{
	[Inject]
	private DotNetService DotNetService { get; set; } = null!;
	
	protected override void OnInitialized()
	{
		DotNetService.DotNetSolutionStateChanged += OnDotNetSolutionStateChanged;
	}
	
	private async void OnDotNetSolutionStateChanged()
	{
		await InvokeAsync(StateHasChanged);
	}
	
	public void Dispose()
	{
		DotNetService.DotNetSolutionStateChanged -= OnDotNetSolutionStateChanged;
	}
}
