using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Extensions.DotNet.BackgroundTasks.Models;

namespace Walk.Extensions.DotNet.Shareds.Displays.Internals;

public partial class IdePromptOpenSolutionDisplay : ComponentBase
{
	[Inject]
	private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;

	[Parameter, EditorRequired]
	public AbsolutePath AbsolutePath { get; set; }

	private void OpenSolutionOnClick()
	{
        DotNetBackgroundTaskApi.Enqueue(new DotNetBackgroundTaskApiWorkArgs
        {
        	WorkKind = DotNetBackgroundTaskApiWorkKind.SetDotNetSolution,
        	DotNetSolutionAbsolutePath = AbsolutePath,
        });
	}
}