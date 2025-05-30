using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.TreeViews.Models.Utils;

namespace Walk.Common.RazorLib.TreeViews.Displays.Utils;

public partial class TreeViewGroupDisplay : ComponentBase
{
	[Parameter, EditorRequired]
	public TreeViewGroup TreeViewGroup { get; set; } = null!;
}