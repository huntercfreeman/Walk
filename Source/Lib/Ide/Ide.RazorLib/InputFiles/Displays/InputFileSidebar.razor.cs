using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.FileSystems.Models;

namespace Walk.Ide.RazorLib.InputFiles.Displays;

public partial class InputFileSidebar : ComponentBase
{
    [Inject]
    private IIdeComponentRenderers IdeComponentRenderers { get; set; } = null!;
    [Inject]
    private ICommonComponentRenderers CommonComponentRenderers { get; set; } = null!;
    [Inject]
    private ITreeViewService TreeViewService { get; set; } = null!;
    [Inject]
    private IDropdownService DropdownService { get; set; } = null!;
    [Inject]
    private IFileSystemProvider FileSystemProvider { get; set; } = null!;
    [Inject]
    private IEnvironmentProvider EnvironmentProvider { get; set; } = null!;
    [Inject]
    private CommonBackgroundTaskApi CommonBackgroundTaskApi { get; set; } = null!;

    [CascadingParameter(Name="SetInputFileContentTreeViewRootFunc")]
    public Func<AbsolutePath, Task> SetInputFileContentTreeViewRootFunc { get; set; } = null!;
    [CascadingParameter]
    public InputFileTreeViewMouseEventHandler InputFileTreeViewMouseEventHandler { get; set; } = null!;
    [CascadingParameter]
    public InputFileTreeViewKeyboardEventHandler InputFileTreeViewKeyboardEventHandler { get; set; } = null!;
    [CascadingParameter]
    public InputFileState InputFileState { get; set; }
    [CascadingParameter]
    public IDialog DialogRecord { get; set; } = null!;

    [Parameter, EditorRequired]
    public ElementDimensions ElementDimensions { get; set; } = null!;
    [Parameter, EditorRequired]
    public Action<AbsolutePath?> SetSelectedAbsolutePath { get; set; } = null!;

    public static readonly Key<TreeViewContainer> TreeViewContainerKey = Key<TreeViewContainer>.NewKey();

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var directoryHomeNode = new TreeViewAbsolutePath(
                EnvironmentProvider.HomeDirectoryAbsolutePath,
                IdeComponentRenderers,
                CommonComponentRenderers,
                FileSystemProvider,
                EnvironmentProvider,
                true,
                false);

            var directoryRootNode = new TreeViewAbsolutePath(
                EnvironmentProvider.RootDirectoryAbsolutePath,
                IdeComponentRenderers,
                CommonComponentRenderers,
                FileSystemProvider,
                EnvironmentProvider,
                true,
                false);

            var adhocRootNode = TreeViewAdhoc.ConstructTreeViewAdhoc(directoryHomeNode, directoryRootNode);

            if (!TreeViewService.TryGetTreeViewContainer(TreeViewContainerKey, out var treeViewContainer))
            {
                TreeViewService.ReduceRegisterContainerAction(new TreeViewContainer(
                    TreeViewContainerKey,
                    adhocRootNode,
                    directoryHomeNode is null
                        ? Array.Empty<TreeViewNoType>()
                        : new List<TreeViewNoType> { directoryHomeNode }));
            }
        }
        
        return Task.CompletedTask;
    }

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

        DropdownService.ReduceRegisterAction(dropdownRecord);
		return Task.CompletedTask;
	}
}