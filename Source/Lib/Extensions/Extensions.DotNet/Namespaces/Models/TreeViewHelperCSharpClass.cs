using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Extensions.DotNet.Namespaces.Models;

public class TreeViewHelperCSharpClass
{
    public static Task<List<TreeViewNoType>> LoadChildrenAsync(TreeViewNamespacePath cSharpClassTreeView)
    {
        if (cSharpClassTreeView.Item.Namespace is null)
            return Task.FromResult<List<TreeViewNoType>>(new());

        return Task.FromResult(cSharpClassTreeView.ChildList);
    }
}
