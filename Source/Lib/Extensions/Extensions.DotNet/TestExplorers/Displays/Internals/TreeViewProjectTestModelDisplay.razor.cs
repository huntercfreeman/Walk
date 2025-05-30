using Microsoft.AspNetCore.Components;
using Walk.Extensions.DotNet.TestExplorers.Models;

namespace Walk.Extensions.DotNet.TestExplorers.Displays.Internals;

public partial class TreeViewProjectTestModelDisplay : ComponentBase
{
	[Parameter, EditorRequired]
	public TreeViewProjectTestModel TreeViewProjectTestModel { get; set; } = null!;
}