using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Ide.RazorLib.AppDatas.Models;

namespace Walk.Ide.RazorLib.Settings.Displays;

public partial class IdeSettingsDisplay : ComponentBase
{
	[Inject]
	private IAppDataService AppDataService { get; set; } = null!;
	[Inject]
	private CommonUtilityService CommonUtilityService { get; set; } = null!;
	
	private void WriteWalkDebugSomethingToConsole()
	{
		Console.WriteLine(WalkDebugSomething.CreateText());
		/*
		#if DEBUG
		Console.WriteLine(WalkDebugSomething.CreateText());
		#else
		Console.WriteLine($"Must run in debug mode to see {nameof(WriteWalkDebugSomethingToConsole)}");
		#endif
		*/
	}
}