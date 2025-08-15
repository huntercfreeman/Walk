using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Extensions.DotNet.Namespaces.Models;

public class TreeViewHelperCSharpClass
{
    public static Task<List<TreeViewNoType>> LoadChildrenAsync(TreeViewNamespacePath cSharpClassTreeView)
    {
        return Task.FromResult<List<TreeViewNoType>>(new());
    }
}
