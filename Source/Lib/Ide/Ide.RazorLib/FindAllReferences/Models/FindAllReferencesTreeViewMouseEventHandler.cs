/*
// FindAllReferences
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Installations.Models;

namespace Walk.Ide.RazorLib.FindAllReferences.Models;

public class FindAllReferencesTreeViewMouseEventHandler : TreeViewMouseEventHandler
{
    private readonly ITextEditorService _textEditorService;
    private readonly IServiceProvider _serviceProvider;

    public FindAllReferencesTreeViewMouseEventHandler(
            ITextEditorService textEditorService,
            IServiceProvider serviceProvider,
            ITreeViewService treeViewService,
            IBackgroundTaskService backgroundTaskService)
        : base(treeViewService, backgroundTaskService)
    {
        _textEditorService = textEditorService;
        _serviceProvider = serviceProvider;
    }

    public override Task OnDoubleClickAsync(TreeViewCommandArgs commandArgs)
    {
        base.OnDoubleClickAsync(commandArgs);

        if (commandArgs.NodeThatReceivedMouseEvent is not TreeViewFindAllReferences treeViewFindAllReferences)
            return Task.CompletedTask;
            
        return FindAllReferencesTextSpanHelper.OpenInEditorOnClick(
            treeViewFindAllReferences,
            true,
            _textEditorService);
    }
}
*/
