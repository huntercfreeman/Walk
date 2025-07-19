using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Extensions.DotNet.Outputs.Displays;

public partial class OutputPanelDisplay : ComponentBase
{
	[Inject]
	private CommonService CommonService { get; set; } = null!;
}