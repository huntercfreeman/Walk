using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Extensions.DotNet.Outputs.Displays;

public partial class OutputPanelDisplay : ComponentBase
{
	[Inject]
	private CommonUtilityService CommonUtilityService { get; set; } = null!;
}