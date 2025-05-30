using Microsoft.AspNetCore.Components;
using Walk.Extensions.DotNet.Outputs.Models;

namespace Walk.Extensions.DotNet.Outputs.Displays.Internals;

public partial class TreeViewDiagnosticLineDisplay : ComponentBase
{
	[Parameter, EditorRequired]
	public TreeViewDiagnosticLine TreeViewDiagnosticLine { get; set; } = null!;
}