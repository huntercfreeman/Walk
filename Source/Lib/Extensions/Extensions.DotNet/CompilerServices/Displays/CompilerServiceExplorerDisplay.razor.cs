using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Extensions.DotNet.CompilerServices.Displays;

public partial class CompilerServiceExplorerDisplay : ComponentBase
{
	[Inject]
	private CommonUtilityService CommonUtilityService { get; set; } = null!;
}
