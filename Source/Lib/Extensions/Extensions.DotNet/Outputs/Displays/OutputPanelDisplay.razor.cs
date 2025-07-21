using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib;

namespace Walk.Extensions.DotNet.Outputs.Displays;

public partial class OutputPanelDisplay : ComponentBase
{
	[Inject]
	private CommonService CommonService { get; set; } = null!;
}
