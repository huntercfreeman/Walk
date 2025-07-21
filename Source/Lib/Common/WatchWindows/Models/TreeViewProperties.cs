using System.Reflection;
using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Common.RazorLib.WatchWindows.Models;

public class TreeViewProperties : TreeViewWithType<WatchWindowObject>
{
    public TreeViewProperties(
            WatchWindowObject watchWindowObject,
            bool isExpandable,
            bool isExpanded)
        : base(watchWindowObject, isExpandable, isExpanded)
    {
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TreeViewProperties treeViewProperties)
            return false;

        return treeViewProperties.Item == Item;
    }

    public override int GetHashCode()
    {
        return Item.GetHashCode();
    }

    public override string GetDisplayText() => "Properties";

    /*public override TreeViewRenderer GetTreeViewRenderer()
    {
        
        using Microsoft.AspNetCore.Components;
        using Walk.Common.RazorLib.WatchWindows.Models;
        using Walk.Common.RazorLib.Options.Models;
        
        namespace Walk.Common.RazorLib.WatchWindows.Displays;
        
        public partial class TreeViewPropertiesDisplay : ComponentBase
        {
            [Inject]
            private IAppOptionsService AppOptionsService { get; set; } = null!;
            
            [Parameter, EditorRequired]
            public TreeViewProperties TreeViewProperties { get; set; } = null!;
        }
        
        
        <div title="@TreeViewProperties.Key.Guid">
            <span>
                @{
                    var appOptionsState = AppOptionsService.GetAppOptionsState();
                
                    var iconDriver = new IconDriver(
                        appOptionsState.Options.IconSizeInPixels,
                        appOptionsState.Options.IconSizeInPixels);
                }
                @IconSymbolPropertyFragment.Render(iconDriver)
                Properties
            </span>
        </div>
    
    
        return new TreeViewRenderer(
            _commonComponentRenderers.CommonTreeViews.TreeViewPropertiesRenderer,
            new Dictionary<string, object?>
            {
                {
                    nameof(TreeViewProperties),
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

            var propertyInfoList = Item.ItemType.GetProperties(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Static);

            foreach (var propertyInfo in propertyInfoList)
            {
                try
                {
                    var childValue = Item.Item is null
                        ? null
                        : propertyInfo.GetValue(Item.Item);

                    var childType = propertyInfo.PropertyType;

                    // https://stackoverflow.com/questions/3762456/how-to-check-if-property-setter-is-public
                    // The getter exists and is public.
                    var hasPublicGetter = propertyInfo.CanRead &&
                        (propertyInfo.GetGetMethod( /*nonPublic*/ true)?.IsPublic ?? false);

                    var childNode = new WatchWindowObject(
                        childValue,
                        childType,
                        propertyInfo.Name,
                        hasPublicGetter);

                    ChildList.Add(new TreeViewReflection(
                        childNode,
                        true,
                        false));
                }
                catch (TargetParameterCountException)
                {
                    // Types: { 'string', 'ImmutableArray<TItem>' } at the minimum
                    // at throwing System.Reflection.TargetParameterCountException
                    // and it appears to be due to a propertyInfo for the generic type argument?
                    //
                    // Given the use case for code I am okay with continuing when this exception
                    // happens as it seems unrelated to the point of this class.
                }
            }

            if (ChildList.Count == 0)
            {
                ChildList.Add(new TreeViewText(
                    "No properties exist for this Type",
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
