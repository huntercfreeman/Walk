using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.Namespaces.Models;

namespace Walk.Extensions.DotNet.Commands;

public class DotNetCommandFactory : IDotNetCommandFactory
{
	private readonly TextEditorService _textEditorService;
	private readonly ICommonUtilityService _commonUtilityService;

	public DotNetCommandFactory(
        TextEditorService textEditorService,
		ICommonUtilityService commonUtilityService)
	{
		_textEditorService = textEditorService;
		_commonUtilityService = commonUtilityService;
    }

	private List<TreeViewNoType> _nodeList = new();
    private TreeViewNamespacePath? _nodeOfViewModel = null;

	public void Initialize()
	{
		// NuGetPackageManagerContext
		{
			_ = ContextFacts.GlobalContext.Keymap.TryRegister(
				new KeymapArgs
				{
					Key = "n",
					Code = "KeyN",
					ShiftKey = false,
					CtrlKey = true,
					AltKey = true,
					MetaKey = false,
					LayerKey = Key<KeymapLayer>.Empty,
				},
				ContextHelper.ConstructFocusContextElementCommand(
					ContextFacts.NuGetPackageManagerContext, "Focus: NuGetPackageManager", "focus-nu-get-package-manager", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
		}
		// CSharpReplContext
		{
			_ = ContextFacts.GlobalContext.Keymap.TryRegister(
				new KeymapArgs
				{
					Key = "r",
					Code = "KeyR",
					ShiftKey = false,
					CtrlKey = true,
					AltKey = true,
					MetaKey = false,
					LayerKey = Key<KeymapLayer>.Empty,
				},
				ContextHelper.ConstructFocusContextElementCommand(
					ContextFacts.SolutionExplorerContext, "Focus: C# REPL", "focus-c-sharp-repl", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService));
		}
		// SolutionExplorerContext
		{
			var focusSolutionExplorerCommand = ContextHelper.ConstructFocusContextElementCommand(
				ContextFacts.SolutionExplorerContext, "Focus: SolutionExplorer", "focus-solution-explorer", _commonUtilityService.JsRuntimeCommonApi, _commonUtilityService);

			_ = ContextFacts.GlobalContext.Keymap.TryRegister(
					new KeymapArgs
					{
						Key = "s",
						Code = "KeyS",
						ShiftKey = false,
						CtrlKey = true,
						AltKey = true,
						MetaKey = false,
						LayerKey = Key<KeymapLayer>.Empty,
					},
					focusSolutionExplorerCommand);

			// Set active solution explorer tree view node to be the
			// active text editor view model and,
			// Set focus to the solution explorer;
			{
				var focusTextEditorCommand = new CommonCommand(
					"Focus: SolutionExplorer (with text editor view model)", "focus-solution-explorer_with-text-editor-view-model", false,
					async commandArgs =>
					{
						await PerformGetFlattenedTree().ConfigureAwait(false);

						var localNodeOfViewModel = _nodeOfViewModel;

						if (localNodeOfViewModel is null)
							return;

						_commonUtilityService.TreeView_SetActiveNodeAction(
							DotNetSolutionState.TreeViewSolutionExplorerStateKey,
							localNodeOfViewModel,
							false,
							false);

						var elementId = _commonUtilityService.TreeView_GetActiveNodeElementId(DotNetSolutionState.TreeViewSolutionExplorerStateKey);

						await focusSolutionExplorerCommand.CommandFunc
							.Invoke(commandArgs)
							.ConfigureAwait(false);
					});

				_ = ContextFacts.GlobalContext.Keymap.TryRegister(
						new KeymapArgs
						{
							Key = "S",
							Code = "KeyS",
							CtrlKey = true,
							ShiftKey = true,
							AltKey = true,
							MetaKey = false,
							LayerKey = Key<KeymapLayer>.Empty,
						},
						focusTextEditorCommand);
			}
		}
	}

    private async Task PerformGetFlattenedTree()
    {
		_nodeList.Clear();

		var group = _textEditorService.GroupApi.GetOrDefault(IdeBackgroundTaskApi.EditorTextEditorGroupKey);

		if (group is not null)
		{
			var textEditorViewModel = _textEditorService.ViewModelApi.GetOrDefault(group.ActiveViewModelKey);

			if (textEditorViewModel is not null)
			{
				if (_commonUtilityService.TryGetTreeViewContainer(
						DotNetSolutionState.TreeViewSolutionExplorerStateKey,
						out var treeViewContainer) &&
                    treeViewContainer is not null)
				{
					await RecursiveGetFlattenedTree(treeViewContainer.RootNode, textEditorViewModel).ConfigureAwait(false);
				}
			}
		}
    }

    private async Task RecursiveGetFlattenedTree(
        TreeViewNoType treeViewNoType,
        TextEditorViewModel textEditorViewModel)
    {
        _nodeList.Add(treeViewNoType);

        if (treeViewNoType is TreeViewNamespacePath treeViewNamespacePath)
        {
            if (textEditorViewModel is not null)
            {
                var viewModelAbsolutePath = _commonUtilityService.EnvironmentProvider.AbsolutePathFactory(
                    textEditorViewModel.PersistentState.ResourceUri.Value,
                    false);

                if (viewModelAbsolutePath.Value ==
                        treeViewNamespacePath.Item.AbsolutePath.Value)
                {
                    _nodeOfViewModel = treeViewNamespacePath;
                }
            }

            switch (treeViewNamespacePath.Item.AbsolutePath.ExtensionNoPeriod)
            {
                case ExtensionNoPeriodFacts.C_SHARP_PROJECT:
                    await treeViewNamespacePath.LoadChildListAsync().ConfigureAwait(false);
                    break;
            }
        }

        await treeViewNoType.LoadChildListAsync().ConfigureAwait(false);

        foreach (var node in treeViewNoType.ChildList)
        {
            await RecursiveGetFlattenedTree(node, textEditorViewModel).ConfigureAwait(false);
        }
    }
}
