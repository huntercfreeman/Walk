using System.Collections;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.TreeViews.Models;

namespace Walk.Common.RazorLib.WatchWindows.Models;

public class TreeViewReflection : TreeViewWithType<WatchWindowObject>
{
    private readonly ICommonComponentRenderers _commonComponentRenderers;

    public TreeViewReflection(
            WatchWindowObject watchWindowObject,
            bool isExpandable,
            bool isExpanded,
            ICommonComponentRenderers commonComponentRenderers)
        : base(watchWindowObject, isExpandable, isExpanded)
    {
        _commonComponentRenderers = commonComponentRenderers;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TreeViewReflection treeViewReflection)
            return false;

        return treeViewReflection.Item == Item;
    }

    public override int GetHashCode()
    {
        return Item.GetHashCode();
    }

    /*public override TreeViewRenderer GetTreeViewRenderer()
    {
    
        using Walk.Common.RazorLib.WatchWindows.Models;
        using Microsoft.AspNetCore.Components;
        
        namespace Walk.Common.RazorLib.WatchWindows.Displays;
        
        public partial class TreeViewReflectionDisplay : ComponentBase
        {
            [Parameter, EditorRequired]
            public TreeViewReflection TreeViewReflection { get; set; } = null!;
        
            private string GetCssStylingForValue(Type itemType)
            {
                if (itemType == typeof(string))
                    return "di_te_string-literal";
                else if (itemType == typeof(bool))
                    return "di_te_keyword";
                else
                    return string.Empty;
            }
        }
        
    
        
        <div style="display: flex;"
             title="@TreeViewReflection.Key.Guid">
            
             @{ var textEditorDebugObjectWrap = TreeViewReflection.Item; }
            
            <div class="di_te_keyword">
                @(textEditorDebugObjectWrap.IsPubliclyReadable ? "public" : "private")
                &nbsp;
            </div>
            
            <div class="di_te_type">
                @(textEditorDebugObjectWrap.ItemType.Name)
                &nbsp;
            </div>
            
            <div>
                @(textEditorDebugObjectWrap.DisplayName)
                &nbsp;
            </div>
        
            @if (textEditorDebugObjectWrap.Item is null)
            {
                <div class="di_te_keyword">
                    (null)
                </div>
            }
            else
            {
                var toStringResult = textEditorDebugObjectWrap.Item.ToString();
            
                <div class="@GetCssStylingForValue(textEditorDebugObjectWrap.ItemType)"
                      style="width: 200px; overflow: hidden; white-space: nowrap; text-overflow: ellipsis; display: inline-block;"
                      title="@toStringResult">
                    @toStringResult
                </div>
            }
        </div>
        
        
        
        
        return new TreeViewRenderer(
            _commonComponentRenderers.CommonTreeViews.TreeViewReflectionRenderer,
            new Dictionary<string, object?>
            {
                {
                    nameof(TreeViewReflection),
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

            ChildList.Add(new TreeViewFields(
                Item,
                true,
                false,
                _commonComponentRenderers));

            ChildList.Add(new TreeViewProperties(
                Item,
                true,
                false,
                _commonComponentRenderers));

            if (Item.Item is IEnumerable)
            {
                ChildList.Add(new TreeViewEnumerable(
                    Item,
                    true,
                    false,
                    _commonComponentRenderers));
            }

            if (Item.ItemType.IsInterface && Item.Item is not null)
            {
                var interfaceImplementation = new WatchWindowObject(
                    Item.Item,
                    Item.Item.GetType(),
                    "InterfaceImplementation",
                    false);

                ChildList.Add(new TreeViewInterfaceImplementation(
                    interfaceImplementation,
                    true,
                    false,
                    _commonComponentRenderers));
            }
        }
        catch (Exception e)
        {
            ChildList.Clear();

            ChildList.Add(new TreeViewException(
                e,
                false,
                false,
                _commonComponentRenderers));
        }

        LinkChildren(previousChildren, ChildList);

        return Task.CompletedTask;
    }
}