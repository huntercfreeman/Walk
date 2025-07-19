using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    private TreeViewState _treeViewState = new();
    
    public event Action? TreeViewStateChanged;
    
    public TreeViewState GetTreeViewState() => _treeViewState;
    
    public TreeViewContainer GetTreeViewContainer(Key<TreeViewContainer> containerKey) =>
    	_treeViewState.ContainerList.FirstOrDefault(x => x.Key == containerKey);

    public bool TryGetTreeViewContainer(Key<TreeViewContainer> containerKey, out TreeViewContainer? container)
    {
        container = GetTreeViewState().ContainerList.FirstOrDefault(
            x => x.Key == containerKey);

        return container is not null;
    }

    public void TreeView_MoveRight(
		Key<TreeViewContainer> containerKey,
		bool addSelectedNodes,
		bool selectNodesBetweenCurrentAndNextActiveNode)
    {
        TreeView_MoveRightAction(
            containerKey,
			addSelectedNodes,
			selectNodesBetweenCurrentAndNextActiveNode,
            treeViewNoType =>
            {
                Enqueue(new CommonWorkArgs
                {
    				WorkKind = CommonWorkKind.TreeViewService_LoadChildList,
                	ContainerKey = containerKey,
                	TreeViewNoType = treeViewNoType,
                });
            });
    }

    public string TreeView_GetActiveNodeElementId(Key<TreeViewContainer> containerKey)
    {
        var inState = GetTreeViewState();
        
        var inContainer = inState.ContainerList.FirstOrDefault(
            x => x.Key == containerKey);
            
        if (inContainer is not null)
            return inContainer.ActiveNodeElementId;
        
        return string.Empty;
    }
		
    public void TreeView_RegisterContainerAction(TreeViewContainer container)
    {
    	var inState = GetTreeViewState();
    
        var inContainer = inState.ContainerList.FirstOrDefault(
            x => x.Key == container.Key);

        if (inContainer is not null)
        {
            TreeViewStateChanged?.Invoke();
            return;
        }

        var outContainerList = new List<TreeViewContainer>(inState.ContainerList);
        outContainerList.Add(container);
        
        _treeViewState = inState with { ContainerList = outContainerList };
        TreeViewStateChanged?.Invoke();
        return;
    }

    public void TreeView_DisposeContainerAction(Key<TreeViewContainer> containerKey)
    {
    	var inState = GetTreeViewState();
    
        var indexContainer = inState.ContainerList.FindIndex(
            x => x.Key == containerKey);

        if (indexContainer == -1)
        {
            TreeViewStateChanged?.Invoke();
        	return;
        }
        
        var outContainerList = new List<TreeViewContainer>(inState.ContainerList);
        outContainerList.RemoveAt(indexContainer);
        
        _treeViewState = inState with { ContainerList = outContainerList };
        TreeViewStateChanged?.Invoke();
        return;
    }

    public void TreeView_WithRootNodeAction(Key<TreeViewContainer> containerKey, TreeViewNoType node)
    {
    	var inState = GetTreeViewState();
    
        var indexContainer = inState.ContainerList.FindIndex(
            x => x.Key == containerKey);

        if (indexContainer == -1)
        {
            TreeViewStateChanged?.Invoke();
        	return;
		}
        
        var inContainer = inState.ContainerList[indexContainer];
        
        var outContainer = inContainer with
        {
            RootNode = node,
            SelectedNodeList = new List<TreeViewNoType>() { node }
        };

        var outContainerList = new List<TreeViewContainer>(inState.ContainerList);
        outContainerList[indexContainer] = outContainer;
        
        _treeViewState = inState with { ContainerList = outContainerList };
        TreeViewStateChanged?.Invoke();
        return;
    }

    public void TreeView_AddChildNodeAction(Key<TreeViewContainer> containerKey, TreeViewNoType parentNode, TreeViewNoType childNode)
    {
    	var inState = GetTreeViewState();
    
        var inContainer = inState.ContainerList.FirstOrDefault(
            x => x.Key == containerKey);

        if (inContainer is null)
        {
            TreeViewStateChanged?.Invoke();
        	return;
        }

        var parent = parentNode;
        var child = childNode;

        child.Parent = parent;
        child.IndexAmongSiblings = parent.ChildList.Count;
        child.TreeViewChangedKey = Key<TreeViewChanged>.NewKey();

        parent.ChildList.Add(child);

        TreeView_ReRenderNodeAction(containerKey, parent);
        return;
    }

    public void TreeView_ReRenderNodeAction(Key<TreeViewContainer> containerKey, TreeViewNoType node)
    {
    	var inState = GetTreeViewState();
    
        var indexContainer = inState.ContainerList.FindIndex(
            x => x.Key == containerKey);

        if (indexContainer == -1)
        {
            TreeViewStateChanged?.Invoke();
        	return;
        }

        var inContainer = inState.ContainerList[indexContainer];
        
        var outContainer = PerformReRenderNode(inContainer, containerKey, node);

        var outContainerList = new List<TreeViewContainer>(inState.ContainerList);
        outContainerList[indexContainer] = outContainer;
        
        _treeViewState = inState with { ContainerList = outContainerList };
        TreeViewStateChanged?.Invoke();
        return;
    }

    public void TreeView_SetActiveNodeAction(
    	Key<TreeViewContainer> containerKey,
		TreeViewNoType? nextActiveNode,
		bool addSelectedNodes,
		bool selectNodesBetweenCurrentAndNextActiveNode)
    {
    	var inState = GetTreeViewState();
    
        var indexContainer = inState.ContainerList.FindIndex(
            x => x.Key == containerKey);

        if (indexContainer == -1)
        {
            TreeViewStateChanged?.Invoke();
       	 return;
        }

		var inContainer = inState.ContainerList[indexContainer];
		
		var outContainer = PerformSetActiveNode(
			inContainer, containerKey, nextActiveNode, addSelectedNodes, selectNodesBetweenCurrentAndNextActiveNode);

		var outContainerList = new List<TreeViewContainer>(inState.ContainerList);
		outContainerList[indexContainer] = outContainer;

        _treeViewState = inState with { ContainerList = outContainerList };
        TreeViewStateChanged?.Invoke();
        return;
    }

    public void TreeView_RemoveSelectedNodeAction(
    	Key<TreeViewContainer> containerKey,
        Key<TreeViewNoType> keyOfNodeToRemove)
    {
    	var inState = GetTreeViewState();
    
        var indexContainer = inState.ContainerList.FindIndex(
            x => x.Key == containerKey);

        if (indexContainer == -1)
        {
            TreeViewStateChanged?.Invoke();
        	return;
        }

		var inContainer = inState.ContainerList[indexContainer];
		
		var outContainer = PerformRemoveSelectedNode(inContainer, containerKey, keyOfNodeToRemove);

		var outContainerList = new List<TreeViewContainer>(inState.ContainerList);
			
		outContainerList[indexContainer] = outContainer;

        _treeViewState = inState with { ContainerList = outContainerList };
        TreeViewStateChanged?.Invoke();
        return;
    }

    public void TreeView_MoveLeftAction(
    	Key<TreeViewContainer> containerKey,
		bool addSelectedNodes,
		bool selectNodesBetweenCurrentAndNextActiveNode)
    {
    	var inState = GetTreeViewState();
    
        var indexContainer = inState.ContainerList.FindIndex(
            x => x.Key == containerKey);

		if (indexContainer == -1)
		{
			TreeViewStateChanged?.Invoke();
        	return;
		}
			
		var inContainer = inState.ContainerList[indexContainer];
        if (inContainer?.ActiveNode is null)
        {
            TreeViewStateChanged?.Invoke();
        	return;
        }

        var outContainer = PerformMoveLeft(inContainer, containerKey, addSelectedNodes, selectNodesBetweenCurrentAndNextActiveNode);

        var outContainerList = new List<TreeViewContainer>(inState.ContainerList);
        outContainerList[indexContainer] = outContainer;

        _treeViewState = inState with { ContainerList = outContainerList };
        TreeViewStateChanged?.Invoke();
        return;
    }

    public void TreeView_MoveDownAction(
    	Key<TreeViewContainer> containerKey,
		bool addSelectedNodes,
		bool selectNodesBetweenCurrentAndNextActiveNode)
    {
    	var inState = GetTreeViewState();
    
        var indexContainer = inState.ContainerList.FindIndex(
            x => x.Key == containerKey);

        if (indexContainer == -1)
        {
        	TreeViewStateChanged?.Invoke();
        	return;
        }
        
        var inContainer = inState.ContainerList[indexContainer];
        if (inContainer?.ActiveNode is null)
        {
            TreeViewStateChanged?.Invoke();
        	return;
        }

        var outContainer = PerformMoveDown(inContainer, containerKey, addSelectedNodes, selectNodesBetweenCurrentAndNextActiveNode);

        var outContainerList = new List<TreeViewContainer>(inState.ContainerList);
        outContainerList[indexContainer] = outContainer;
        
        _treeViewState = inState with { ContainerList = outContainerList };
        TreeViewStateChanged?.Invoke();
        return;
    }

    public void TreeView_MoveUpAction(
    	Key<TreeViewContainer> containerKey,
		bool addSelectedNodes,
		bool selectNodesBetweenCurrentAndNextActiveNode)
    {
    	var inState = GetTreeViewState();
    
        var indexContainer = inState.ContainerList.FindIndex(
            x => x.Key == containerKey);

		if (indexContainer == -1)
		{
            TreeViewStateChanged?.Invoke();
        	return;
        }

        var inContainer = inState.ContainerList[indexContainer];
        
        var outContainer = PerformMoveUp(inContainer, containerKey, addSelectedNodes, selectNodesBetweenCurrentAndNextActiveNode);

        var outContainerList = new List<TreeViewContainer>(inState.ContainerList);
        outContainerList[indexContainer] = outContainer;
        
        _treeViewState = inState with { ContainerList = outContainerList };
        TreeViewStateChanged?.Invoke();
        return;
    }

    public void TreeView_MoveRightAction(
        Key<TreeViewContainer> containerKey,
		bool addSelectedNodes,
		bool selectNodesBetweenCurrentAndNextActiveNode,
		Action<TreeViewNoType> loadChildListAction)
    {
    	var inState = GetTreeViewState();
    
    	var indexContainer = inState.ContainerList.FindIndex(
            x => x.Key == containerKey);
		
		if (indexContainer == -1)
		{
            TreeViewStateChanged?.Invoke();
        	return;
        }
		
        var inContainer = inState.ContainerList[indexContainer];
            
        if (inContainer?.ActiveNode is null)
        {
            TreeViewStateChanged?.Invoke();
        	return;
        }

        var outContainer = PerformMoveRight(inContainer, containerKey, addSelectedNodes, selectNodesBetweenCurrentAndNextActiveNode, loadChildListAction);

		var outContainerList = new List<TreeViewContainer>(inState.ContainerList);
        outContainerList[indexContainer] = outContainer;
        
        _treeViewState = inState with { ContainerList = outContainerList };
        TreeViewStateChanged?.Invoke();
        return;
    }

    public void TreeView_MoveHomeAction(
		Key<TreeViewContainer> containerKey,
		bool addSelectedNodes,
		bool selectNodesBetweenCurrentAndNextActiveNode)
    {
    	var inState = GetTreeViewState();
    
    	var indexContainer = inState.ContainerList.FindIndex(
            x => x.Key == containerKey);
            
		if (indexContainer == -1)
		{
            TreeViewStateChanged?.Invoke();
        	return;
        }
            
        var inContainer = inState.ContainerList[indexContainer];
        if (inContainer?.ActiveNode is null)
        {
            TreeViewStateChanged?.Invoke();
        	return;
        }

        var outContainer = PerformMoveHome(inContainer, containerKey, addSelectedNodes, selectNodesBetweenCurrentAndNextActiveNode);

		var outContainerList = new List<TreeViewContainer>(inState.ContainerList);
        outContainerList[indexContainer] = outContainer;
        
        _treeViewState = inState with { ContainerList = outContainerList };
        TreeViewStateChanged?.Invoke();
        return;
    }

    public void TreeView_MoveEndAction(
		Key<TreeViewContainer> containerKey,
		bool addSelectedNodes,
		bool selectNodesBetweenCurrentAndNextActiveNode)
    {
    	var inState = GetTreeViewState();
    
    	var indexContainer = inState.ContainerList.FindIndex(
            x => x.Key == containerKey);
		
		if (indexContainer == -1)
		{
            TreeViewStateChanged?.Invoke();
        	return;
        }
        
        var inContainer = inState.ContainerList[indexContainer];
        if (inContainer?.ActiveNode is null)
        {
            TreeViewStateChanged?.Invoke();
        	return;
        }

        var outContainer = PerformMoveEnd(inContainer, containerKey, addSelectedNodes, selectNodesBetweenCurrentAndNextActiveNode);

		var outContainerList = new List<TreeViewContainer>(inState.ContainerList);
        outContainerList[indexContainer] = outContainer;
        
        _treeViewState = inState with { ContainerList = outContainerList };
        TreeViewStateChanged?.Invoke();
        return;
    }

	private void PerformMarkForRerender(TreeViewNoType node)
    {
        var markForRerenderTarget = node;

        while (markForRerenderTarget is not null)
        {
            markForRerenderTarget.TreeViewChangedKey = Key<TreeViewChanged>.NewKey();
            markForRerenderTarget = markForRerenderTarget.Parent;
        }
    }

	private TreeViewContainer PerformReRenderNode(
		TreeViewContainer inContainer,
		Key<TreeViewContainer> containerKey,
		TreeViewNoType node)
	{
		PerformMarkForRerender(node);
        return inContainer with { StateId = Guid.NewGuid() };
	}

	private TreeViewContainer PerformSetActiveNode(
		TreeViewContainer inContainer,
		Key<TreeViewContainer> containerKey,
		TreeViewNoType? nextActiveNode,
		bool addSelectedNodes,
		bool selectNodesBetweenCurrentAndNextActiveNode)
	{
		if (inContainer.ActiveNode is not null)
            PerformMarkForRerender(inContainer.ActiveNode);

        if (nextActiveNode is not null)
            PerformMarkForRerender(nextActiveNode);

		var inSelectedNodeList = inContainer.SelectedNodeList;
		var selectedNodeListWasCleared = false;

		TreeViewContainer outContainer;

		// TODO: I'm adding multi-select. I'd like to single out the...
		// ...SelectNodesBetweenCurrentAndNextActiveNode case for now...
		// ...and DRY the code after. (2024-01-13) 
		if (selectNodesBetweenCurrentAndNextActiveNode)
		{
			outContainer = inContainer;
			int direction;

			// Step 1: Determine the selection's direction.
			//
			// That is to say, on the UI, which node appears
			// vertically closer to the root node.
			//
			// Process: -Discover the closest common ancestor node.
			//          -Then backtrack one node depth.
			//          -One now has the two nodes at which
			//              they differ.
			//	      -Compare the 'TreeViewNoType.IndexAmongSiblings'
			//          -Next.IndexAmongSiblings - Current.IndexAmongSiblings
			//          	if (difference > 0)
			//              	then: direction is towards end
			//          	if (difference < 0)
			//              	then: direction is towards home (AKA root)
			{
				var currentTarget = inContainer.ActiveNode;
				var nextTarget = nextActiveNode;

				while (currentTarget.Parent != nextTarget.Parent)
				{
					if (currentTarget.Parent is null || nextTarget.Parent is null)
						break;

					currentTarget = currentTarget.Parent;
					nextTarget = nextTarget.Parent;
				}
				
				direction = nextTarget.IndexAmongSiblings - currentTarget.IndexAmongSiblings;
			}

			if (direction > 0)
			{
				// Move down

				var previousNode = outContainer.ActiveNode;

				while (true)
				{
					outContainer = PerformMoveDown(
						outContainer,
						containerKey,
						true,
						false);

					if (previousNode.Key == outContainer.ActiveNode.Key)
					{
						// No change occurred, avoid an infinite loop and break
						break;
					}
					else
					{
						previousNode = outContainer.ActiveNode;
					}

					if (nextActiveNode.Key == outContainer.ActiveNode.Key)
					{
						// Target acquired
						break;
					}
				}
			}
			else if (direction < 0)
			{
				// Move up

				var previousNode = outContainer.ActiveNode;

				while (true)
				{
					outContainer = PerformMoveUp(
						outContainer,
						containerKey,
						true,
						false);

					if (previousNode.Key == outContainer.ActiveNode.Key)
					{
						// No change occurred, avoid an infinite loop and break
						break;
					}
					else
					{
						previousNode = outContainer.ActiveNode;
					}

					if (nextActiveNode.Key == outContainer.ActiveNode.Key)
					{
						// Target acquired
						break;
					}
				}
			}
			else
			{
				// The next target is the same as the current target.
				return outContainer;
			}
		}
		else
		{
			if (nextActiveNode is null)
			{
				selectedNodeListWasCleared = true;

				outContainer = inContainer with
	            {
	                SelectedNodeList = Array.Empty<TreeViewNoType>()
	            };
			}
			else if (!addSelectedNodes)
			{
				selectedNodeListWasCleared = true;

				outContainer = inContainer with
	            {
	                SelectedNodeList = new List<TreeViewNoType>()
					{
						nextActiveNode
					}
	            };
			}
			else
			{
				var alreadyExistingIndex = inContainer.SelectedNodeList.FindIndex(
					x => nextActiveNode.Equals(x));
				
				if (alreadyExistingIndex != -1)
				{
					var outSelectedNodeList = new List<TreeViewNoType>(inContainer.SelectedNodeList);
					outSelectedNodeList.RemoveAt(alreadyExistingIndex);
				
					inContainer = inContainer with
		            {
		                SelectedNodeList = outSelectedNodeList
		            };
				}

				// Variable name collision on 'outSelectedNodeLists'.
				{
					var outSelectedNodeList = new List<TreeViewNoType>(inContainer.SelectedNodeList);
					outSelectedNodeList.Insert(0, nextActiveNode);
					
					outContainer = inContainer with
		            {
		                SelectedNodeList = outSelectedNodeList
		            };
		        }
			}
		}
		
		if (selectedNodeListWasCleared)
		{
			foreach (var node in inSelectedNodeList)
			{
				PerformMarkForRerender(node);
			}
		}

        return outContainer;
	}
    
    private TreeViewContainer PerformRemoveSelectedNode(
		TreeViewContainer inContainer,
        Key<TreeViewContainer> containerKey,
        Key<TreeViewNoType> keyOfNodeToRemove)
    {
        var indexOfNodeToRemove = inContainer.SelectedNodeList.FindIndex(
            x => x.Key == keyOfNodeToRemove);

		var outSelectedNodeList = new List<TreeViewNoType>(inContainer.SelectedNodeList);
		outSelectedNodeList.RemoveAt(indexOfNodeToRemove);

        return inContainer with
        {
            SelectedNodeList = outSelectedNodeList
        };
    }
    
    private TreeViewContainer PerformMoveLeft(
		TreeViewContainer inContainer,
		Key<TreeViewContainer> containerKey,
		bool addSelectedNodes,
		bool selectNodesBetweenCurrentAndNextActiveNode)
	{
		var outContainer = inContainer;

		if (addSelectedNodes)
            return outContainer;

        if (outContainer.ActiveNode is null)
            return outContainer;

        if (outContainer.ActiveNode.IsExpanded &&
            outContainer.ActiveNode.IsExpandable)
        {
            outContainer.ActiveNode.IsExpanded = false;
            return PerformReRenderNode(outContainer, outContainer.Key, outContainer.ActiveNode);
        }

        if (outContainer.ActiveNode.Parent is not null)
        {
            outContainer = PerformSetActiveNode(
                outContainer,
                outContainer.Key,
                outContainer.ActiveNode.Parent,
                false,
				false);
        }

		return outContainer;
	}

	private TreeViewContainer PerformMoveDown(
		TreeViewContainer inContainer,
		Key<TreeViewContainer> containerKey,
		bool addSelectedNodes,
		bool selectNodesBetweenCurrentAndNextActiveNode)
	{
		var outContainer = inContainer;

        if (outContainer.ActiveNode.IsExpanded &&
            outContainer.ActiveNode.ChildList.Any())
        {
            var nextActiveNode = outContainer.ActiveNode.ChildList[0];

            outContainer = PerformSetActiveNode(
                outContainer,
                outContainer.Key,
                nextActiveNode,
				addSelectedNodes,
				selectNodesBetweenCurrentAndNextActiveNode);
        }
        else
        {
            var target = outContainer.ActiveNode;

            while (target.Parent is not null &&
                   target.IndexAmongSiblings == target.Parent.ChildList.Count - 1)
            {
                target = target.Parent;
            }

            if (target.Parent is null ||
                target.IndexAmongSiblings == target.Parent.ChildList.Count - 1)
            {
                return outContainer;
            }

            var nextActiveNode = target.Parent.ChildList[
                target.IndexAmongSiblings +
                1];

            outContainer = PerformSetActiveNode(
                outContainer,
                outContainer.Key,
                nextActiveNode,
				addSelectedNodes,
				selectNodesBetweenCurrentAndNextActiveNode);
        }

		return outContainer;
	}

	private TreeViewContainer PerformMoveUp(
		TreeViewContainer inContainer,
		Key<TreeViewContainer> containerKey,
		bool addSelectedNodes,
		bool selectNodesBetweenCurrentAndNextActiveNode)
	{
		var outContainer = inContainer;

		if (outContainer?.ActiveNode?.Parent is null)
            return outContainer;

        if (outContainer.ActiveNode.IndexAmongSiblings == 0)
        {
            outContainer = PerformSetActiveNode(
                outContainer,
                outContainer.Key,
                outContainer.ActiveNode!.Parent,
				addSelectedNodes,
				selectNodesBetweenCurrentAndNextActiveNode);
        }
        else
        {
            var target = outContainer.ActiveNode.Parent.ChildList[
                outContainer.ActiveNode.IndexAmongSiblings - 1];

            while (true)
            {
                if (target.IsExpanded &&
                    target.ChildList.Any())
                {
                    target = target.ChildList.Last();
                }
                else
                {
                    break;
                }
            }

            outContainer = PerformSetActiveNode(
                outContainer,
                outContainer.Key,
                target,
				addSelectedNodes,
				selectNodesBetweenCurrentAndNextActiveNode);
        }

		return outContainer;
	}

	private TreeViewContainer PerformMoveRight(
		TreeViewContainer inContainer,
		Key<TreeViewContainer> containerKey,
		bool addSelectedNodes,
		bool selectNodesBetweenCurrentAndNextActiveNode,
		Action<TreeViewNoType> loadChildListAction)
	{
		var outContainer = inContainer;

		if (outContainer is null || outContainer.ActiveNode is null)
            return outContainer;

        if (addSelectedNodes)
            return outContainer;

        if (outContainer.ActiveNode is null)
            return outContainer;

        if (outContainer.ActiveNode.IsExpanded)
        {
            if (outContainer.ActiveNode.ChildList.Any())
            {
                outContainer = PerformSetActiveNode(
                    outContainer,
                    outContainer.Key,
                    outContainer.ActiveNode.ChildList[0],
					addSelectedNodes,
					selectNodesBetweenCurrentAndNextActiveNode);
            }
        }
        else if (outContainer.ActiveNode.IsExpandable)
        {
            outContainer.ActiveNode.IsExpanded = true;

            loadChildListAction.Invoke(outContainer.ActiveNode);
        }

		return outContainer;
	}

	private TreeViewContainer PerformMoveHome(
		TreeViewContainer inContainer,
		Key<TreeViewContainer> containerKey,
		bool addSelectedNodes,
		bool selectNodesBetweenCurrentAndNextActiveNode)
	{
		var outContainer = inContainer;

        TreeViewNoType target;

        if (outContainer.RootNode is TreeViewAdhoc)
        {
            if (outContainer.RootNode.ChildList.Any())
                target = outContainer.RootNode.ChildList[0];
            else
                target = outContainer.RootNode;
        }
        else
        {
            target = outContainer.RootNode;
        }

        return PerformSetActiveNode(
            outContainer,
            outContainer.Key,
            target,
			addSelectedNodes,
			selectNodesBetweenCurrentAndNextActiveNode);
	}

	private TreeViewContainer PerformMoveEnd(
		TreeViewContainer inContainer,
		Key<TreeViewContainer> containerKey,
		bool addSelectedNodes,
		bool selectNodesBetweenCurrentAndNextActiveNode)
	{
		var outContainer = inContainer;

        var target = outContainer.RootNode;

        while (target.IsExpanded && target.ChildList.Any())
        {
            target = target.ChildList.Last();
        }

        return PerformSetActiveNode(
            outContainer,
            outContainer.Key,
            target,
			addSelectedNodes,
			selectNodesBetweenCurrentAndNextActiveNode);
	}
	
	private Dictionary<int, string> _intToCssValueCache = new();
	
	/// <summary>This method should only be invoked by the "UI thread"</summary>
	public string TreeView_GetNodeElementStyle(int offsetInPixels)
	{
	    if (!_intToCssValueCache.ContainsKey(offsetInPixels))
	        _intToCssValueCache.Add(offsetInPixels, offsetInPixels.ToCssValue());
        
        UiStringBuilder.Clear();
        UiStringBuilder.Append("padding-left: ");
        UiStringBuilder.Append(_intToCssValueCache[offsetInPixels]);
        UiStringBuilder.Append("px;");
        
        return UiStringBuilder.ToString();
	}
	
	/// <summary>This method should only be invoked by the "UI thread"</summary>
	public string TreeView_GetNodeTextStyle(int walkTreeViewIconWidth)
	{
	    if (!_intToCssValueCache.ContainsKey(walkTreeViewIconWidth))
	        _intToCssValueCache.Add(walkTreeViewIconWidth, walkTreeViewIconWidth.ToCssValue());
	    
	    UiStringBuilder.Clear();
	    UiStringBuilder.Append("width: calc(100% - ");
	    UiStringBuilder.Append(_intToCssValueCache[walkTreeViewIconWidth]);
	    UiStringBuilder.Append("px); height:  100%;");
	    
	    return UiStringBuilder.ToString();
	}
	
	/// <summary>This method should only be invoked by the "UI thread"</summary>
	public string TreeView_GetNodeBorderStyle(int offsetInPixels, int walkTreeViewIconWidth)
	{
	    var result = offsetInPixels + walkTreeViewIconWidth / 2;
	    
	    if (!_intToCssValueCache.ContainsKey(result))
	        _intToCssValueCache.Add(result, result.ToCssValue());
	
	    UiStringBuilder.Clear();
	    UiStringBuilder.Append("margin-left: ");
	    UiStringBuilder.Append(result);
	    UiStringBuilder.Append("px;");
	    
	    return UiStringBuilder.ToString();
	}
}
