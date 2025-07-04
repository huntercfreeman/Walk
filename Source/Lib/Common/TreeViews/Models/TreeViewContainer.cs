using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.TreeViews.Models;

/// <summary>
/// TODO: SphagettiCode - some logic was added to multi-select nodes, yet it was never
/// finished, and is buggy.(2023-09-19)
/// </summary>
public record TreeViewContainer
{
    /// <summary>
    /// If <see cref="rootNode"/> is null then <see cref="TreeViewAdhoc.ConstructTreeViewAdhoc()"/>
    /// will be invoked and the return value will be used as the <see cref="RootNode"/>
    /// </summary>
    public TreeViewContainer(
        Key<TreeViewContainer> key,
        TreeViewNoType? rootNode,
        IReadOnlyList<TreeViewNoType> selectedNodeList)
    {    
		rootNode ??= TreeViewAdhoc.ConstructTreeViewAdhoc();

        Key = key;
        RootNode = rootNode;
        SelectedNodeList = selectedNodeList;
        
        ActiveNodeElementId = $"di_node-{Key.Guid}";
    }

    public Key<TreeViewContainer> Key { get; init; }
    public TreeViewNoType RootNode { get; init; }
    /// <summary>
    /// The <see cref="ActiveNode"/> is the last or default entry in <see cref="SelectedNodeList"/>
    /// </summary>
    public TreeViewNoType? ActiveNode => SelectedNodeList.FirstOrDefault();
    public IReadOnlyList<TreeViewNoType> SelectedNodeList { get; init; }
    public Guid StateId { get; init; } = Guid.NewGuid();
    public string ActiveNodeElementId { get; }
}