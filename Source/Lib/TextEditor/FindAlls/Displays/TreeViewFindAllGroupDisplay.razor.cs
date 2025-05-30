using Microsoft.AspNetCore.Components;
using Walk.TextEditor.RazorLib.FindAlls.Models;

namespace Walk.TextEditor.RazorLib.FindAlls.Displays;

public partial class TreeViewFindAllGroupDisplay : ComponentBase
{
	[Parameter, EditorRequired]
	public TreeViewFindAllGroup TreeViewFindAllGroup { get; set; } = null!;
}