using System.Collections;
using Walk.Common.RazorLib.Exceptions;
using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Common.RazorLib.WatchWindows.Models;

public class TreeViewEnumerable : TreeViewWithType<WatchWindowObject>
{
    public TreeViewEnumerable(
            WatchWindowObject watchWindowObject,
            bool isExpandable,
            bool isExpanded)
        : base(watchWindowObject, isExpandable, isExpanded)
    {
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TreeViewEnumerable treeViewEnumerable)
            return false;

        return treeViewEnumerable.Item == Item;
    }

    public override int GetHashCode()
    {
        return Item.GetHashCode();
    }

    public override string GetDisplayText() => "Enumerable";

    /*public override TreeViewRenderer GetTreeViewRenderer()
    {
    
        using Microsoft.AspNetCore.Components;
        using Walk.Common.RazorLib.WatchWindows.Models;
        using Walk.Common.RazorLib.Options.Models;
        
        namespace Walk.Common.RazorLib.WatchWindows.Displays;
        
        public partial class TreeViewEnumerableDisplay : ComponentBase
        {
            [Inject]
            private IAppOptionsService AppOptionsService { get; set; } = null!;
            
            [Parameter, EditorRequired]
            public TreeViewEnumerable TreeViewEnumerable { get; set; } = null!;
        }
    
        <div title="@TreeViewEnumerable.Key.Guid">

    	@{
    		var appOptionsState = AppOptionsService.GetAppOptionsState();
    	
    		var iconDriver = new IconDriver(
    			appOptionsState.Options.IconSizeInPixels,
    			appOptionsState.Options.IconSizeInPixels);
    	}
    
        @IconListUnorderedFragment.Render(iconDriver)
        Enumerable
    </div>
    
    
    
        return new TreeViewRenderer(
            _commonComponentRenderers.CommonTreeViews.TreeViewEnumerableRenderer,
            new Dictionary<string, object?>
            {
                {
                    nameof(TreeViewEnumerable),
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

            if (Item.Item is IEnumerable enumerable)
            {
                var enumerator = enumerable.GetEnumerator();

                var genericArgument = GetGenericArgument(Item.Item.GetType());

                while (enumerator.MoveNext())
                {
                    var entry = enumerator.Current;

                    var childNode = new WatchWindowObject(
                        entry,
                        genericArgument,
                        genericArgument.Name,
                        Item.IsPubliclyReadable);

                    ChildList.Add(new TreeViewReflection(
                        childNode,
                        true,
                        false));
                }
            }
            else
            {
                throw new WalkCommonException($"Unexpected failed cast to the Type {nameof(IEnumerable)}. {nameof(TreeViewEnumerable)} are to have a {nameof(Item.Item)} which is castable as {nameof(IEnumerable)}");
            }

            if (ChildList.Count == 0)
            {
                ChildList.Add(new TreeViewText(
                    "Enumeration returned no results.",
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

    // https://stackoverflow.com/questions/906499/getting-type-t-from-ienumerablet
    private static Type GetGenericArgument(Type type)
    {
        // Type is Array
        // short-circuit if you expect lots of arrays 
        if (type.IsArray)
            return type.GetElementType()!;

        // type is IEnumerable<T>;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return type.GetGenericArguments()[0];

        // type implements/extends IEnumerable<T>;
        var enumType = type.GetInterfaces()
            .Where(t => t.IsGenericType &&
                        t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .Select(t => t.GenericTypeArguments[0]).FirstOrDefault();
        return enumType ?? type;
    }
}
