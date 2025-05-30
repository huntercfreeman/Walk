using Walk.Extensions.DotNet.CompilerServices.Models;
using Walk.Extensions.DotNet.ComponentRenderers.Models;
using Microsoft.AspNetCore.Components;

namespace Walk.Extensions.DotNet.CompilerServices.Displays;

public partial class TreeViewCompilerServiceDisplay : ComponentBase, ITreeViewCompilerServiceRendererType
{
	[Parameter, EditorRequired]
	public TreeViewCompilerService TreeViewCompilerService { get; set; } = null!;
}