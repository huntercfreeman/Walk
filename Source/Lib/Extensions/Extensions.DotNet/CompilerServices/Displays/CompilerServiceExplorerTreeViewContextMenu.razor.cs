using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Menus.Models;

namespace Walk.Extensions.DotNet.CompilerServices.Displays;

public partial class CompilerServiceExplorerTreeViewContextMenu : ComponentBase
{
	[Parameter, EditorRequired]
	public TreeViewCommandArgs TreeViewCommandArgs { get; set; }

	public static readonly Key<DropdownRecord> ContextMenuEventDropdownKey = Key<DropdownRecord>.NewKey();

	private MenuRecord GetMenuRecord(TreeViewCommandArgs treeViewCommandArgs)
	{
		// Suppress unused variable
		_ = treeViewCommandArgs;

		// Default case
		{
			return new MenuRecord(MenuRecord.NoMenuOptionsExistList);
		}
	}
}