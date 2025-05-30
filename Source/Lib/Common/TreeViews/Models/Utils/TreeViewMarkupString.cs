using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.TreeViews.Displays.Utils;

namespace Walk.Common.RazorLib.TreeViews.Models.Utils;

public class TreeViewMarkupString : TreeViewWithType<MarkupString>
{
    public TreeViewMarkupString(
            MarkupString markupString,
            IFileSystemProvider fileSystemProvider,
            IEnvironmentProvider environmentProvider,
            bool isExpandable,
            bool isExpanded)
        : base(
            markupString,
            isExpandable,
            isExpanded)
    {
        FileSystemProvider = fileSystemProvider;
        EnvironmentProvider = environmentProvider;
    }

    public IFileSystemProvider FileSystemProvider { get; }
    public IEnvironmentProvider EnvironmentProvider { get; }

    public override bool Equals(object? obj)
    {
        // TODO: Equals
        return false;
    }

    public override int GetHashCode()
    {
        // TODO: GetHashCode
        return Path.GetRandomFileName().GetHashCode();
    }

    public override TreeViewRenderer GetTreeViewRenderer()
    {
        return new TreeViewRenderer(
            typeof(TreeViewMarkupStringDisplay),
            new Dictionary<string, object?>
            {
                {
                    nameof(TreeViewMarkupStringDisplay.MarkupString),
                    Item
                },
            });
    }

    public override Task LoadChildListAsync()
    {
        return Task.CompletedTask;
    }

    public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
    {
        return;
    }
}