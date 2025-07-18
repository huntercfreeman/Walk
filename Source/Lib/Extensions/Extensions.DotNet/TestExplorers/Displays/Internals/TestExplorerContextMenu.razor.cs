using System.Text;
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.TreeViews.Models.Utils;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Extensions.DotNet.TestExplorers.Models;

namespace Walk.Extensions.DotNet.TestExplorers.Displays.Internals;

public partial class TestExplorerContextMenu : ComponentBase
{
	[Inject]
	private DotNetService DotNetService { get; set; } = null!;

	[CascadingParameter]
	public TestExplorerRenderBatchValidated RenderBatch { get; set; } = null!;
	[CascadingParameter]
    public DropdownRecord? Dropdown { get; set; }

	[Parameter, EditorRequired]
	public TreeViewCommandArgs TreeViewCommandArgs { get; set; }

	public static readonly Key<DropdownRecord> ContextMenuEventDropdownKey = Key<DropdownRecord>.NewKey();
	public static readonly Key<TerminalCommandRequest> DotNetTestByFullyQualifiedNameFormattedTerminalCommandRequestKey = Key<TerminalCommandRequest>.NewKey();

	private MenuRecord? _menuRecord = null;
	
	private bool _htmlElementDimensionsChanged = false;

	protected override async Task OnInitializedAsync()
	{
		// Usage of 'OnInitializedAsync' lifecycle method ensure the context menu is only rendered once.
		// Otherwise, one might have the context menu's options change out from under them.
		_menuRecord = await GetMenuRecord(TreeViewCommandArgs).ConfigureAwait(false);
		_htmlElementDimensionsChanged = true;
		await InvokeAsync(StateHasChanged);
	}
	
	protected override void OnAfterRender(bool firstRender)
	{
		var localDropdown = Dropdown;

		if (localDropdown is not null && _htmlElementDimensionsChanged)
		{
			_htmlElementDimensionsChanged = false;
			localDropdown.OnHtmlElementDimensionsChanged();
		}
	}

	private async Task<MenuRecord> GetMenuRecord(TreeViewCommandArgs commandArgs, bool isRecursiveCall = false)
	{
		if (!isRecursiveCall && commandArgs.TreeViewContainer.SelectedNodeList.Count > 1)
		{
			return await GetMultiSelectionMenuRecord(commandArgs).ConfigureAwait(false);
		}

		if (commandArgs.NodeThatReceivedMouseEvent is null)
			return new MenuRecord(MenuRecord.NoMenuOptionsExistList);

		var menuRecordsList = new List<MenuOptionRecord>();

		if (commandArgs.NodeThatReceivedMouseEvent is TreeViewStringFragment treeViewStringFragment)
		{
			var target = treeViewStringFragment;
			var fullyQualifiedNameBuilder = new StringBuilder(target.Item.Value);

			while (target.Parent is TreeViewStringFragment parentNode)
			{
				fullyQualifiedNameBuilder.Insert(0, $"{parentNode.Item.Value}.");
				target = parentNode;
			}

			if (target.Parent is TreeViewProjectTestModel treeViewProjectTestModel &&
				treeViewStringFragment.Item.IsEndpoint)
			{
				var fullyQualifiedName = fullyQualifiedNameBuilder.ToString();

				var menuOptionRecord = GetEndPointMenuOption(
					treeViewStringFragment,
					treeViewProjectTestModel,
					fullyQualifiedName);

				menuRecordsList.Add(menuOptionRecord);
				
				if (commandArgs.TreeViewContainer.SelectedNodeList.Count == 1)
				{
					menuRecordsList.Add(new MenuOptionRecord(
						$"Send to Output panel",
						MenuOptionKind.Other,
						onClickFunc: () =>
						{
							return SendToOutputPanelAsync(treeViewStringFragment.Item.TerminalCommandParsed?.OutputCache.ToString() ?? string.Empty);
						}));
				}
			}
			else
			{
				menuRecordsList.AddRange(await GetNamespaceMenuOption(
					treeViewStringFragment,
					commandArgs,
					isRecursiveCall));
			}
		}
		else if (commandArgs.NodeThatReceivedMouseEvent is TreeViewProjectTestModel treeViewProjectTestModel)
		{
			menuRecordsList.Add(new MenuOptionRecord(
				$"Refresh: {treeViewProjectTestModel.Item.AbsolutePath.NameWithExtension}",
				MenuOptionKind.Other,
				onClickFunc: async () =>
				{
					// TODO: This code is not concurrency safe with 'TestExplorerScheduler.Task_DiscoverTests()'
					DotNetService.ReduceWithAction(inState =>
					{
						if (treeViewProjectTestModel.Item.TestNameFullyQualifiedList is null)
							return inState;

						var mutablePassedTestHashSet = new HashSet<string>(inState.PassedTestHashSet);
						var mutableNotRanTestHashSet = new HashSet<string>(inState.NotRanTestHashSet);
						var mutableFailedTestHashSet = new HashSet<string>(inState.FailedTestHashSet);
						
						foreach (var fullyQualifiedTestName in treeViewProjectTestModel.Item.TestNameFullyQualifiedList)
						{
							mutablePassedTestHashSet.Remove(fullyQualifiedTestName);
							mutableNotRanTestHashSet.Remove(fullyQualifiedTestName);
							mutableFailedTestHashSet.Remove(fullyQualifiedTestName);
						}
						
						return inState with
				        {
				            PassedTestHashSet = mutablePassedTestHashSet,
				            NotRanTestHashSet = mutableNotRanTestHashSet,
				            FailedTestHashSet = mutableFailedTestHashSet,
				        };
				    });
			        
					treeViewProjectTestModel.Item.TestNameFullyQualifiedList = null;
					DotNetService.IdeService.CommonService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, treeViewProjectTestModel);
					
					await treeViewProjectTestModel.LoadChildListAsync();
					DotNetService.IdeService.CommonService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, treeViewProjectTestModel);
					
					DotNetService.MoveNodeToCorrectBranch(treeViewProjectTestModel);
					
					DotNetService.ReduceWithAction(inState =>
					{
						if (treeViewProjectTestModel.Item.TestNameFullyQualifiedList is null)
							return inState;
					
						var mutableNotRanTestHashSet = inState.NotRanTestHashSet.ToHashSet();
						
						foreach (var fullyQualifiedTestName in treeViewProjectTestModel.Item.TestNameFullyQualifiedList)
						{
							mutableNotRanTestHashSet.Add(fullyQualifiedTestName);
						}
						
						return inState with
				        {
				            NotRanTestHashSet = mutableNotRanTestHashSet,
				        };
				    });
				}));
				
			if (treeViewProjectTestModel.Parent is TreeViewGroup tvg &&
				tvg.Item == "Projects that threw an exception during discovery")
			{
				menuRecordsList.Add(new MenuOptionRecord(
					$"Send to Output panel",
					MenuOptionKind.Other,
					onClickFunc: () =>
					{
						return SendToOutputPanelAsync(treeViewProjectTestModel.Item.TerminalCommandParsed?.OutputCache.ToString() ?? string.Empty);
					}));
			}
		}

		if (!menuRecordsList.Any())
			return new MenuRecord(MenuRecord.NoMenuOptionsExistList);

		return new MenuRecord(menuRecordsList);
	}

	private MenuOptionRecord GetEndPointMenuOption(
		TreeViewStringFragment treeViewStringFragment,
		TreeViewProjectTestModel treeViewProjectTestModel,
		string fullyQualifiedName)
	{
		var menuOptionRecord = new MenuOptionRecord(
			$"Run: {treeViewStringFragment.Item.Value}",
			MenuOptionKind.Other,
			onClickFunc: () =>
			{
				Console.WriteLine($"aaa {treeViewProjectTestModel.Item.AbsolutePath.ParentDirectory is null}");
			
				if (treeViewProjectTestModel.Item.AbsolutePath.ParentDirectory is not null)
				{
					DotNetService.Enqueue(new DotNetWorkArgs
					{
						WorkKind = DotNetWorkKind.RunTestByFullyQualifiedName,
                        TreeViewStringFragment = treeViewStringFragment,
                        FullyQualifiedName = fullyQualifiedName,
                        TreeViewProjectTestModel = treeViewProjectTestModel,
                    });
				}

				return Task.CompletedTask;
			});

		return menuOptionRecord;
	}

	private async Task<List<MenuOptionRecord>> GetNamespaceMenuOption(
		TreeViewStringFragment treeViewStringFragment,
		TreeViewCommandArgs commandArgs,
		bool isRecursiveCall = false)
	{
		void RecursiveStep(TreeViewStringFragment treeViewStringFragmentNamespace, List<TreeViewNoType> fabricateSelectedNodeList)
		{
			foreach (var childNode in treeViewStringFragmentNamespace.ChildList)
			{
				if (childNode is TreeViewStringFragment childTreeViewStringFragment)
				{
					if (childTreeViewStringFragment.Item.IsEndpoint)
					{
						fabricateSelectedNodeList.Add(childTreeViewStringFragment);
					}
					else
					{
						RecursiveStep(childTreeViewStringFragment, fabricateSelectedNodeList);
					}
				}
			}
		}

		var fabricateSelectedNodeList = new List<TreeViewNoType>();

		RecursiveStep(treeViewStringFragment, fabricateSelectedNodeList);

		var fabricateTreeViewContainer = commandArgs.TreeViewContainer with
		{
			SelectedNodeList = fabricateSelectedNodeList
		};

		var fabricateCommandArgs = new TreeViewCommandArgs(
			commandArgs.CommonService,
			fabricateTreeViewContainer,
			commandArgs.NodeThatReceivedMouseEvent,
			commandArgs.RestoreFocusToTreeView,
			commandArgs.ContextMenuFixedPosition,
			commandArgs.MouseEventArgs,
			commandArgs.KeyboardEventArgs);

		var multiSelectionMenuRecord = await GetMultiSelectionMenuRecord(fabricateCommandArgs);

		var menuOptionRecord = new MenuOptionRecord(
			$"Namespace: {treeViewStringFragment.Item.Value} | {fabricateSelectedNodeList.Count}",
			MenuOptionKind.Other,
			subMenu: multiSelectionMenuRecord);

		return new() { menuOptionRecord };
	}

	private async Task<MenuRecord> GetMultiSelectionMenuRecord(TreeViewCommandArgs commandArgs)
	{
		var menuOptionRecordList = new List<MenuOptionRecord>();
		Func<Task> runAllOnClicksWithinSelection = () => Task.CompletedTask;
		bool runAllOnClicksWithinSelectionHasEffect = false;

		foreach (var node in commandArgs.TreeViewContainer.SelectedNodeList)
		{
			MenuOptionRecord menuOption;

			if (node is TreeViewStringFragment treeViewStringFragment)
			{
				var innerTreeViewCommandArgs = new TreeViewCommandArgs(
					commandArgs.CommonService,
					commandArgs.TreeViewContainer,
					node,
					commandArgs.RestoreFocusToTreeView,
					commandArgs.ContextMenuFixedPosition,
					commandArgs.MouseEventArgs,
					commandArgs.KeyboardEventArgs);

				menuOption = new(
					treeViewStringFragment.Item.Value,
					MenuOptionKind.Other,
					subMenu: await GetMenuRecord(innerTreeViewCommandArgs, true).ConfigureAwait(false));

				var copyRunAllOnClicksWithinSelection = runAllOnClicksWithinSelection;

				runAllOnClicksWithinSelection = async () =>
				{
					await copyRunAllOnClicksWithinSelection.Invoke().ConfigureAwait(false);

					if (menuOption.SubMenu?.MenuOptionList.Single().OnClickFunc is not null)
					{
						await menuOption.SubMenu.MenuOptionList
							.Single().OnClickFunc!
							.Invoke()
							.ConfigureAwait(false);
					}
				};

				runAllOnClicksWithinSelectionHasEffect = true;
			}
			else
			{
				menuOption = new(
					node.GetType().Name,
					MenuOptionKind.Other,
					subMenu: new MenuRecord(MenuRecord.NoMenuOptionsExistList));
			}

			menuOptionRecordList.Add(menuOption);
		}

		if (runAllOnClicksWithinSelectionHasEffect)
		{
			menuOptionRecordList.Insert(0, new(
				"Run all OnClicks within selection",
				MenuOptionKind.Create,
				onClickFunc: runAllOnClicksWithinSelection));
		}

		if (!menuOptionRecordList.Any())
			return new MenuRecord(MenuRecord.NoMenuOptionsExistList);

		return new MenuRecord(menuOptionRecordList);
	}
	
	private async Task SendToOutputPanelAsync(string output)
	{
		var contextRecord = ContextFacts.OutputContext;
		
		DotNetService.ParseOutputEntireDotNetRun(output, "Unit-Test_results");
		
		DotNetService.IdeService.CommonService.SetPanelTabAsActiveByContextRecordKey(contextRecord.ContextKey);
	
		if (contextRecord != default)
		{
			var command = ContextHelper.ConstructFocusContextElementCommand(
		        contextRecord,
		        nameof(ContextHelper.ConstructFocusContextElementCommand),
		        nameof(ContextHelper.ConstructFocusContextElementCommand),
		        DotNetService.IdeService.CommonService.JsRuntimeCommonApi,
		        DotNetService.IdeService.CommonService);
		        
		    await command.CommandFunc.Invoke(null).ConfigureAwait(false);
		}
	}
}