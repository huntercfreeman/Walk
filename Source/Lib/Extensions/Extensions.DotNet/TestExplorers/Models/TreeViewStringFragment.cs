using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.WatchWindows.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Extensions.DotNet.TestExplorers.Models;

public class TreeViewStringFragment : TreeViewWithType<StringFragment>
{
	public TreeViewStringFragment(
			StringFragment stringFragment,
			ICommonComponentRenderers commonComponentRenderers,
			bool isExpandable,
			bool isExpanded)
		: base(stringFragment, isExpandable, isExpanded)
	{
		CommonComponentRenderers = commonComponentRenderers;
	}

	public ICommonComponentRenderers CommonComponentRenderers { get; }

	public override bool Equals(object? obj)
	{
		if (obj is not TreeViewStringFragment treeViewStringFragment)
			return false;

		return treeViewStringFragment.Item.Value == Item.Value;
	}

	public override int GetHashCode() => Item.Value.GetHashCode();

	public override string GetDisplayText() => Item.Value;

    /*public override TreeViewRenderer GetTreeViewRenderer()
	{
	    using Microsoft.AspNetCore.Components;
        using Walk.Common.RazorLib.Options.Models;
        using Walk.Ide.RazorLib.Terminals.Models;
        using Walk.Extensions.DotNet.TestExplorers.Models;
        
        namespace Walk.Extensions.DotNet.TestExplorers.Displays.Internals;
        
        public partial class TreeViewStringFragmentDisplay : ComponentBase, IDisposable
        {
        	[Inject]
        	private ITerminalService TerminalService { get; set; } = null!;
            [Inject]
            private IAppOptionsService AppOptionsService { get; set; } = null!;
        
        	[Parameter, EditorRequired]
        	public TreeViewStringFragment TreeViewStringFragment { get; set; } = null!;
        
        	protected override void OnInitialized()
        	{
        		TerminalService.TerminalStateChanged += OnTerminalStateChanged;
        	}
        
        	private string? GetTerminalCommandRequestOutput(ITerminal terminal)
        	{
        		return TreeViewStringFragment.Item.TerminalCommandParsed?.OutputCache.ToString() ?? null;
        	}
        	
        	private async void OnTerminalStateChanged()
        	{
        		await InvokeAsync(StateHasChanged);
        	}
        	
        	public void Dispose()
        	{
        		TerminalService.TerminalStateChanged -= OnTerminalStateChanged;
        	}
        }
	
	
	
	
	    @using Walk.Ide.RazorLib.Terminals.Models

        @{ var terminal = TerminalService.GetTerminalState().TerminalMap[TerminalFacts.EXECUTION_KEY]; }
        
        @if (terminal is null)
        {
        	<text>@nameof(terminal) was null</text>
        }
        else
        {
        	var output = GetTerminalCommandRequestOutput(terminal);
        
        	RenderFragment renderFragment = @<text>?</text>;
        
        	if (output is null)
        	{
        		renderFragment = @<text>?</text>;
        	}
        	else if (!output.Contains("Duration:"))
        	{
        		var appOptionsState = AppOptionsService.GetAppOptionsState();
        	
        		var iconDriver = new IconDriver(
        			appOptionsState.Options.IconSizeInPixels,
        			appOptionsState.Options.IconSizeInPixels);
        		
        		renderFragment = IconLoadingFragment.Render(iconDriver);
        	}
        	else
        	{
        		if (output.Contains("Passed!"))
        		{
        			renderFragment = @<em class="di_em">Passed!</em>;
        		}
        		else
        		{
        			renderFragment = @<span class="di_tree-view-exception">Failed!</span>;
        		}
        	}
        
        	<div style="display: flex;">
        		@renderFragment&nbsp;
        		@TreeViewStringFragment.Item.Value
        
        		@if (TreeViewStringFragment.ChildList.Count > 0)
        		{
        			<span title="Count of child nodes">
        				&nbsp;(@(TreeViewStringFragment.ChildList.Count))
        			</span>
        		}
        	</div>
        }
        

	
	
	
	
		return new TreeViewRenderer(
			typeof(TreeViewStringFragmentDisplay),
			new Dictionary<string, object?>
			{
				{
					nameof(TreeViewStringFragmentDisplay.TreeViewStringFragment),
					this
				},
			});
	}*/

	public override async Task LoadChildListAsync()
	{
		try
		{
			var previousChildren = new List<TreeViewNoType>(ChildList);

			var newChildList = Item.Map.Select(kvp => (TreeViewNoType)new TreeViewStringFragment(
				kvp.Value,
				CommonComponentRenderers,
				true,
				false)).ToList();

			for (var i = 0; i < newChildList.Count; i++)
			{
				var child = (TreeViewStringFragment)newChildList[i];
				await child.LoadChildListAsync().ConfigureAwait(false);

				if (child.ChildList.Count == 0)
				{
					child.IsExpandable = false;
					child.IsExpanded = false;
				}
			}

			if (newChildList.Count == 1)
			{
				// Merge parent and child

				var child = (TreeViewStringFragment)newChildList.Single();

				Item.Value = $"{Item.Value}.{child.Item.Value}";
				Item.Map = child.Item.Map;
				Item.IsEndpoint = child.Item.IsEndpoint;

				newChildList = child.ChildList;
			}

			ChildList = newChildList;
			LinkChildren(previousChildren, ChildList);
		}
		catch (Exception exception)
		{
			ChildList = new List<TreeViewNoType>
			{
				new TreeViewException(exception, false, false, CommonComponentRenderers)
				{
					Parent = this,
					IndexAmongSiblings = 0,
				}
			};
		}

		TreeViewChangedKey = Key<TreeViewChanged>.NewKey();
	}

	public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
	{
		return;
	}
}