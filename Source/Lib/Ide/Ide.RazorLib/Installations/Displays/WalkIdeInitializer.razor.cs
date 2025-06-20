using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Ide.RazorLib.JsRuntimes.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;

namespace Walk.Ide.RazorLib.Installations.Displays;

/// <remarks>
/// This class is an exception to the naming convention, "don't use the word 'Walk' in class names".
/// 
/// Reason for this exception: when one first starts interacting with this project,
/// 	this type might be one of the first types they interact with. So, the redundancy of namespace
/// 	and type containing 'Walk' feels reasonable here.
/// </remarks>
public partial class WalkIdeInitializer : ComponentBase
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject]
    private WalkHostingInformation WalkHostingInformation { get; set; } = null!;
    [Inject]
    private IdeBackgroundTaskApi IdeBackgroundTaskApi { get; set; } = null!;

	protected override void OnInitialized()
	{
        IdeBackgroundTaskApi.Enqueue(new IdeBackgroundTaskApiWorkArgs
        {
        	WorkKind = IdeBackgroundTaskApiWorkKind.WalkIdeInitializerOnInit,
        });
        base.OnInitialized();
	}
	
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			if (WalkHostingInformation.WalkHostingKind == WalkHostingKind.Photino)
			{
				await JsRuntime.GetWalkIdeApi()
					.PreventDefaultBrowserKeybindings();
			}
		}
	}
}