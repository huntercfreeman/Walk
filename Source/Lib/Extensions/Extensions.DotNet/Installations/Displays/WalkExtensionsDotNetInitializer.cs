using Microsoft.AspNetCore.Components;
using Walk.Extensions.DotNet.BackgroundTasks.Models;

namespace Walk.Extensions.DotNet.Installations.Displays;

public partial class WalkExtensionsDotNetInitializer : ComponentBase
{
    [Inject]
	private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;

    protected override void OnInitialized()
	{
		DotNetBackgroundTaskApi.Enqueue(new DotNetBackgroundTaskApiWorkArgs
		{
			WorkKind = DotNetBackgroundTaskApiWorkKind.WalkExtensionsDotNetInitializerOnInit,
		});
	}
	
	protected override void OnAfterRender(bool firstRender)
	{
		if (firstRender)
		{
            DotNetBackgroundTaskApi.Enqueue(new DotNetBackgroundTaskApiWorkArgs
            {
            	WorkKind = DotNetBackgroundTaskApiWorkKind.WalkExtensionsDotNetInitializerOnAfterRender
            });
		}
	}
}