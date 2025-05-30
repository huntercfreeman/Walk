﻿using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Microsoft.AspNetCore.Components;

namespace Walk.Ide.RazorLib.FileSystems.Displays;

public partial class TreeViewAbsolutePathDisplay : ComponentBase, ITreeViewAbsolutePathRendererType
{
    [CascadingParameter]
    public TreeViewContainer TreeViewState { get; set; } = null!;
    [CascadingParameter(Name = "SearchQuery")]
    public string SearchQuery { get; set; } = string.Empty;
    [CascadingParameter(Name = "SearchMatchTuples")]
    public List<(Key<TreeViewContainer> treeViewStateKey, TreeViewAbsolutePath treeViewAbsolutePath)>? SearchMatchTuples { get; set; }

    [Parameter, EditorRequired]
    public TreeViewAbsolutePath TreeViewAbsolutePath { get; set; } = null!;
}