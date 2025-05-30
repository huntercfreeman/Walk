using Microsoft.AspNetCore.Components;
using Walk.Ide.RazorLib.CodeSearches.Models;

namespace Walk.Ide.RazorLib.CodeSearches.Displays;

public partial class TreeViewCodeSearchTextSpanDisplay : ComponentBase
{
	[Parameter, EditorRequired]
	public TreeViewCodeSearchTextSpan TreeViewCodeSearchTextSpan { get; set; } = null!;
}