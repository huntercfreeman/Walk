using Microsoft.AspNetCore.Components;
using Walk.Extensions.DotNet.BackgroundTasks.Models;

namespace Walk.Extensions.DotNet.DotNetSolutions.Displays.Internals;

public partial class SolutionPropertiesDisplay : ComponentBase, IDisposable
{
	[Inject]
	private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;
	
	protected override void OnInitialized()
	{
		DotNetBackgroundTaskApi.DotNetSolutionService.DotNetSolutionStateChanged += OnDotNetSolutionStateChanged;
	}
	
	private async void OnDotNetSolutionStateChanged()
	{
		await InvokeAsync(StateHasChanged);
	}
	
	public void Dispose()
	{
		DotNetBackgroundTaskApi.DotNetSolutionService.DotNetSolutionStateChanged -= OnDotNetSolutionStateChanged;
	}
}