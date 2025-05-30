using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Namespaces.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;

namespace Walk.Ide.RazorLib.Namespaces.Displays;

public partial class TreeViewNamespacePathDisplay : ComponentBase, ITreeViewNamespacePathRendererType
{
	[Inject]
    private IAppOptionsService AppOptionsService { get; set; } = null!;

    [CascadingParameter(Name="WalkCommonIconWidthOverride")]
    public int? WalkCommonIconWidthOverride { get; set; }
    [CascadingParameter(Name="WalkCommonIconHeightOverride")]
    public int? WalkCommonIconHeightOverride { get; set; }

	[Parameter, EditorRequired]
    public NamespacePath NamespacePath { get; set; }
    [Parameter]
    public string CssStyleString { get; set; } = string.Empty;
    
    public int WidthInPixels => WalkCommonIconWidthOverride ??
        AppOptionsService.GetAppOptionsState().Options.IconSizeInPixels;

    public int HeightInPixels => WalkCommonIconHeightOverride ??
        AppOptionsService.GetAppOptionsState().Options.IconSizeInPixels;
}