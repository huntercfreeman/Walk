using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.FileSystems.Models;

namespace Walk.Ide.RazorLib.InputFiles.Displays;

public partial class InputFileDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IdeService IdeService { get; set; } = null!;
    
    [CascadingParameter]
    public IDialog? DialogRecord { get; set; }

    private InputFileTreeViewMouseEventHandler _inputFileTreeViewMouseEventHandler = null!;
    private InputFileTreeViewKeyboardEventHandler _inputFileTreeViewKeyboardEventHandler = null!;

    public static readonly Key<TreeViewContainer> InputFileSidebar_TreeViewContainerKey = Key<TreeViewContainer>.NewKey();
    private TreeViewContainerParameter InputFileSidebar_treeViewContainerParameter;

    protected override void OnInitialized()
    {
        _inputFileTreeViewMouseEventHandler = new InputFileTreeViewMouseEventHandler(IdeService);
        _inputFileTreeViewKeyboardEventHandler = new InputFileTreeViewKeyboardEventHandler(IdeService);

        InputFileSidebar_treeViewContainerParameter = new(
            InputFileSidebar_TreeViewContainerKey,
            _inputFileTreeViewKeyboardEventHandler,
            _inputFileTreeViewMouseEventHandler,
            OnTreeViewContextMenuFunc);
        
        IdeService.IdeStateChanged += OnInputFileStateChanged;
    }
    
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var directoryHomeNode = new TreeViewAbsolutePath(
                IdeService.CommonService.EnvironmentProvider.HomeDirectoryAbsolutePath,
                IdeService.CommonService,
                true,
                false);

            var directoryRootNode = new TreeViewAbsolutePath(
                IdeService.CommonService.EnvironmentProvider.RootDirectoryAbsolutePath,
                IdeService.CommonService,
                true,
                false);

            var adhocRootNode = TreeViewAdhoc.ConstructTreeViewAdhoc(directoryHomeNode, directoryRootNode);

            if (!IdeService.CommonService.TryGetTreeViewContainer(InputFileSidebar_TreeViewContainerKey, out var treeViewContainer))
            {
                IdeService.CommonService.TreeView_RegisterContainerAction(new TreeViewContainer(
                    InputFileSidebar_TreeViewContainerKey,
                    adhocRootNode,
                    directoryHomeNode is null
                        ? Array.Empty<TreeViewNoType>()
                        : new List<TreeViewNoType> { directoryHomeNode }));
            }
        }

        return Task.CompletedTask;
    }

    public async void OnInputFileStateChanged(IdeStateChangedKind ideStateChangedKind)
    {
        if (ideStateChangedKind == IdeStateChangedKind.InputFileStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    /* Start InputFileSideBar */
    private Task OnTreeViewContextMenuFunc(TreeViewCommandArgs treeViewCommandArgs)
    {
        var dropdownRecord = new DropdownRecord(
            InputFileContextMenu.ContextMenuKey,
            treeViewCommandArgs.ContextMenuFixedPosition.LeftPositionInPixels,
            treeViewCommandArgs.ContextMenuFixedPosition.TopPositionInPixels,
            typeof(InputFileContextMenu),
            new Dictionary<string, object?>
            {
                {
                    nameof(InputFileContextMenu.TreeViewCommandArgs),
                    treeViewCommandArgs
                }
            },
            restoreFocusOnClose: null);

        IdeService.CommonService.Dropdown_ReduceRegisterAction(dropdownRecord);
        return Task.CompletedTask;
    }
    /* End InputFileSideBar */

    /* Start InputFileBottomControls */
    private void SelectInputFilePatternOnChange(ChangeEventArgs changeEventArgs)
    {
        var patternName = (string)(changeEventArgs.Value ?? string.Empty);

        var pattern = IdeService.GetInputFileState().InputFilePatternsList
            .FirstOrDefault(x => x.PatternName == patternName);

        if (pattern.ConstructorWasInvoked)
            IdeService.InputFile_SetSelectedInputFilePattern(pattern);
    }

    private string GetSelectedTreeViewModelAbsolutePathString(InputFileState inputFileState)
    {
        var selectedAbsolutePath = inputFileState.SelectedTreeViewModel?.Item;

        if (selectedAbsolutePath is null)
            return "Selection is null";

        return selectedAbsolutePath.Value.Value;
    }

    private async Task FireOnAfterSubmit()
    {
        var valid = await IdeService.GetInputFileState().SelectionIsValidFunc
            .Invoke(IdeService.GetInputFileState().SelectedTreeViewModel?.Item ?? default)
            .ConfigureAwait(false);

        if (valid)
        {
            if (DialogRecord is not null)
                IdeService.CommonService.Dialog_ReduceDisposeAction(DialogRecord.DynamicViewModelKey);

            await IdeService.GetInputFileState().OnAfterSubmitFunc
                .Invoke(IdeService.GetInputFileState().SelectedTreeViewModel?.Item ?? default)
                .ConfigureAwait(false);
        }
    }

    private bool OnAfterSubmitIsDisabled()
    {
        return !IdeService.GetInputFileState().SelectionIsValidFunc.Invoke(IdeService.GetInputFileState().SelectedTreeViewModel?.Item ?? default)
            .Result;
    }

    private Task CancelOnClick()
    {
        if (DialogRecord is not null)
            IdeService.CommonService.Dialog_ReduceDisposeAction(DialogRecord.DynamicViewModelKey);

        return Task.CompletedTask;
    }
    
    private bool GetInputFilePatternIsSelected(InputFilePattern inputFilePattern, InputFileState localInputFileState)
    {
        return (localInputFileState.SelectedInputFilePattern?.PatternName ?? string.Empty) ==
            inputFilePattern.PatternName;
    }
    /* End InputFileBottomControls */
    
    public void Dispose()
    {
        IdeService.IdeStateChanged -= OnInputFileStateChanged;
        IdeService.CommonService.TreeView_DisposeContainerAction(InputFileSidebar_TreeViewContainerKey);
    }
}
