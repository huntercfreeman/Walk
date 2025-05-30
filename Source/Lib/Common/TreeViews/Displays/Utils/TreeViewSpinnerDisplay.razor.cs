using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.TreeViews.Models.Utils;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.TreeViews.Displays.Utils;

public partial class TreeViewSpinnerDisplay : ComponentBase
{
    [Inject]
    private IAppOptionsService AppOptionsService { get; set; } = null!;
    
	[Parameter, EditorRequired]
	public TreeViewSpinner TreeViewSpinner { get; set; } = null!;
}