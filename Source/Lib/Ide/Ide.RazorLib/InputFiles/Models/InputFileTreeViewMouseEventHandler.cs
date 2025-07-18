using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;

namespace Walk.Ide.RazorLib.InputFiles.Models;

public class InputFileTreeViewMouseEventHandler : TreeViewMouseEventHandler
{
    private readonly IdeBackgroundTaskApi _ideBackgroundTaskApi;
    private readonly Func<AbsolutePath, Task> _setInputFileContentTreeViewRootFunc;

    public InputFileTreeViewMouseEventHandler(
        CommonUtilityService commonUtilityService,
        IdeBackgroundTaskApi ideBackgroundTaskApi,
        Func<AbsolutePath, Task> setInputFileContentTreeViewRootFunc)
        : base(commonUtilityService)
    {
        _ideBackgroundTaskApi = ideBackgroundTaskApi;
        _setInputFileContentTreeViewRootFunc = setInputFileContentTreeViewRootFunc;
    }

    protected override void OnClick(TreeViewCommandArgs commandArgs)
    {
        base.OnClick(commandArgs);

        if (commandArgs.NodeThatReceivedMouseEvent is not TreeViewAbsolutePath treeViewAbsolutePath)
            return;

        _ideBackgroundTaskApi.InputFile_SetSelectedTreeViewModel(treeViewAbsolutePath);
    }

    public override Task OnDoubleClickAsync(TreeViewCommandArgs commandArgs)
    {
        base.OnDoubleClickAsync(commandArgs);

        if (commandArgs.NodeThatReceivedMouseEvent is not TreeViewAbsolutePath treeViewAbsolutePath)
            return Task.CompletedTask;

        _setInputFileContentTreeViewRootFunc.Invoke(treeViewAbsolutePath.Item);
        return Task.CompletedTask;
    }
}