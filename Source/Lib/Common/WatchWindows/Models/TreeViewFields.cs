using System.Reflection;
using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Common.RazorLib.WatchWindows.Models;

public class TreeViewFields : TreeViewWithType<WatchWindowObject>
{
    public TreeViewFields(
            WatchWindowObject watchWindowObject,
            bool isExpandable,
            bool isExpanded)
        : base(watchWindowObject, isExpandable, isExpanded)
    {
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TreeViewFields treeViewFields)
            return false;

        return treeViewFields.Item == Item;
    }

    public override int GetHashCode()
    {
        return Item.GetHashCode();
    }

    public override string GetDisplayText() => "Fields";

    /*public override TreeViewRenderer GetTreeViewRenderer()
    {
    
        using Microsoft.AspNetCore.Components;
        using Walk.Common.RazorLib.WatchWindows.Models;
        using Walk.Common.RazorLib.Options.Models;
        
        namespace Walk.Common.RazorLib.WatchWindows.Displays;
        
        public partial class TreeViewFieldsDisplay : ComponentBase
        {
            [Inject]
            private IAppOptionsService AppOptionsService { get; set; } = null!;
            
            [Parameter, EditorRequired]
            public TreeViewFields TreeViewFields { get; set; } = null!;
        }
        
        <div title="@TreeViewFields.Key.Guid">
            <span>
            	@{
            		var appOptionsState = AppOptionsService.GetAppOptionsState();
            	
            		var iconDriver = new IconDriver(
        				appOptionsState.Options.IconSizeInPixels,
        				appOptionsState.Options.IconSizeInPixels);
            	}
            
                @IconSymbolFieldFragment.Render(iconDriver)
                Fields
            </span>
        </div>
        
    
        return new TreeViewRenderer(
            _commonComponentRenderers.CommonTreeViews.TreeViewFieldsRenderer,
            new Dictionary<string, object?>
            {
                {
                    nameof(TreeViewFields),
                    this
                },
            });
    }*/

    public override Task LoadChildListAsync()
    {
        var previousChildren = new List<TreeViewNoType>(ChildList);

        try
        {
            ChildList.Clear();

            var fieldInfoList = Item.ItemType.GetFields(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Static);

            foreach (var fieldInfo in fieldInfoList)
            {
                var childValue = Item.Item is null
                    ? null
                    : fieldInfo.GetValue(Item.Item);

                var childType = fieldInfo.FieldType;

                var childNode = new WatchWindowObject(
                    childValue,
                    childType,
                    fieldInfo.Name,
                    fieldInfo.IsPublic);

                ChildList.Add(new TreeViewReflection(
                    childNode,
                    true,
                    false));
            }

            if (ChildList.Count == 0)
            {
                ChildList.Add(new TreeViewText(
                    "No fields exist for this Type",
                    false,
                    false));
            }
        }
        catch (Exception e)
        {
            ChildList.Clear();

            ChildList.Add(new TreeViewException(
                e,
                false,
                false));
        }

        LinkChildren(previousChildren, ChildList);

        return Task.CompletedTask;
    }
}
