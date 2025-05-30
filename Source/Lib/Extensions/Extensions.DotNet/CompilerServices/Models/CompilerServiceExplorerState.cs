using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Extensions.DotNet.CompilerServices.Models;

/// <summary>
/// TODO: Investigate making this a value type.
/// </summary>
public partial class CompilerServiceExplorerState
{
    public static readonly Key<TreeViewContainer> TreeViewCompilerServiceExplorerContentStateKey = Key<TreeViewContainer>.NewKey();

    public CompilerServiceExplorerState()
    {
        Model = new CompilerServiceExplorerModel();
    }

    public CompilerServiceExplorerState(
        CompilerServiceExplorerModel model)
    {
        Model = model;
    }

    public CompilerServiceExplorerModel Model { get; }
}