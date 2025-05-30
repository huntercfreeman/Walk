using Microsoft.AspNetCore.Components;
using Walk.Extensions.DotNet.DotNetSolutions.Models.Internals;

namespace Walk.Extensions.DotNet.DotNetSolutions.Displays.Internals;

public partial class SolutionVisualizationSettingsDisplay : ComponentBase
{
	[Parameter, EditorRequired]
	public SolutionVisualizationModel SolutionVisualizationModel { get; set; } = null!;
}