using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.FileSystems.Models;

namespace Walk.Extensions.DotNet.Shareds.Displays.Internals;

public partial class IdePromptOpenSolutionDisplay : ComponentBase
{
	[Inject]
	private DotNetService DotNetService { get; set; } = null!;

	[Parameter, EditorRequired]
	public AbsolutePath AbsolutePath { get; set; }

	private void OpenSolutionOnClick()
	{
        DotNetService.Enqueue(new DotNetWorkArgs
        {
        	WorkKind = DotNetWorkKind.SetDotNetSolution,
        	DotNetSolutionAbsolutePath = AbsolutePath,
        });
	}
}
