using System.Text;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.Namespaces.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.Extensions.DotNet;

public partial class DotNetService
{
    private List<TreeViewNoType> _nodeList = new();
    private TreeViewNamespacePath? _nodeOfViewModel = null;

    /*public void Initialize()
    {
        // NuGetPackageManagerContext
        {
            _ = CommonFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs
                {
                    Key = "n",
                    Code = "KeyN",
                    ShiftKey = false,
                    CtrlKey = true,
                    AltKey = true,
                    MetaKey = false,
                    LayerKey = -1,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    CommonFacts.NuGetPackageManagerContext, "Focus: NuGetPackageManager", "focus-nu-get-package-manager", CommonService.JsRuntimeCommonApi, CommonService));
        }
        // CSharpReplContext
        {
            _ = CommonFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs
                {
                    Key = "r",
                    Code = "KeyR",
                    ShiftKey = false,
                    CtrlKey = true,
                    AltKey = true,
                    MetaKey = false,
                    LayerKey = -1,
                },
                ContextHelper.ConstructFocusContextElementCommand(
                    CommonFacts.SolutionExplorerContext, "Focus: C# REPL", "focus-c-sharp-repl", CommonService.JsRuntimeCommonApi, CommonService));
        }
        // SolutionExplorerContext
        {
            var focusSolutionExplorerCommand = ContextHelper.ConstructFocusContextElementCommand(
                CommonFacts.SolutionExplorerContext, "Focus: SolutionExplorer", "focus-solution-explorer", CommonService.JsRuntimeCommonApi, CommonService);

            _ = CommonFacts.GlobalContext.Keymap.TryRegister(
                    new KeymapArgs
                    {
                        Key = "s",
                        Code = "KeyS",
                        ShiftKey = false,
                        CtrlKey = true,
                        AltKey = true,
                        MetaKey = false,
                        LayerKey = -1,
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

                        CommonService.TreeView_SetActiveNodeAction(
                            DotNetSolutionState.TreeViewSolutionExplorerStateKey,
                            localNodeOfViewModel,
                            false,
                            false);

                        var elementId = CommonService.TreeView_GetActiveNodeElementId(DotNetSolutionState.TreeViewSolutionExplorerStateKey);

                        await focusSolutionExplorerCommand.CommandFunc
                            .Invoke(commandArgs)
                            .ConfigureAwait(false);
                    });

                _ = CommonFacts.GlobalContext.Keymap.TryRegister(
                        new KeymapArgs
                        {
                            Key = "S",
                            Code = "KeyS",
                            CtrlKey = true,
                            ShiftKey = true,
                            AltKey = true,
                            MetaKey = false,
                            LayerKey = -1,
                        },
                        focusTextEditorCommand);
            }
        }
    }*/

    private async Task PerformGetFlattenedTree()
    {
        _nodeList.Clear();

        var group = TextEditorService.Group_GetTextEditorGroupState().EditorTextEditorGroup;

        if (group is not null)
        {
            var textEditorViewModel = TextEditorService.ViewModel_GetOrDefault(group.ActiveViewModelKey);

            if (textEditorViewModel is not null)
            {
                if (CommonService.TryGetTreeViewContainer(
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
                var viewModelAbsolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(
                    textEditorViewModel.PersistentState.ResourceUri.Value,
                    false,
                    tokenBuilder: new StringBuilder(),
                    formattedBuilder: new StringBuilder(),
                    AbsolutePathNameKind.NameWithExtension);

                if (viewModelAbsolutePath.Value == treeViewNamespacePath.Item.Value)
                {
                    _nodeOfViewModel = treeViewNamespacePath;
                }
            }

            if (treeViewNamespacePath.Item.Name.EndsWith(CommonFacts.C_SHARP_PROJECT))
            {
                await treeViewNamespacePath.LoadChildListAsync().ConfigureAwait(false);
            }
        }

        await treeViewNoType.LoadChildListAsync().ConfigureAwait(false);

        foreach (var node in treeViewNoType.ChildList)
        {
            await RecursiveGetFlattenedTree(node, textEditorViewModel).ConfigureAwait(false);
        }
    }
}
