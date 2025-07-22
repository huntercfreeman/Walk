using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.TextEditor.RazorLib.FindAlls.Models;

public class FindAllTreeViewKeyboardEventHandler : TreeViewKeyboardEventHandler
{
    private readonly TextEditorService _textEditorService;

    public FindAllTreeViewKeyboardEventHandler(TextEditorService textEditorService)
        : base(textEditorService.CommonService)
    {
        _textEditorService = textEditorService;
    }

    public override Task OnKeyDownAsync(TreeViewCommandArgs commandArgs)
    {
        if (commandArgs.KeyboardEventArgs is null)
            return Task.CompletedTask;

        base.OnKeyDownAsync(commandArgs);

        switch (commandArgs.KeyboardEventArgs.Code)
        {
            case CommonFacts.ENTER_CODE:
                return InvokeOpenInEditor(commandArgs, true);
            case CommonFacts.SPACE_CODE:
                return InvokeOpenInEditor(commandArgs, false);
        }
        
        return Task.CompletedTask;
    }

    private Task InvokeOpenInEditor(TreeViewCommandArgs commandArgs, bool shouldSetFocusToEditor)
    {
        var activeNode = commandArgs.TreeViewContainer.ActiveNode;

        if (activeNode is not TreeViewFindAllTextSpan treeViewFindAllTextSpan)
            return Task.CompletedTask;

        _textEditorService.WorkerArbitrary.PostUnique(async editContext =>
        {
            await _textEditorService.OpenInEditorAsync(
                editContext,
                treeViewFindAllTextSpan.AbsolutePath.Value,
                shouldSetFocusToEditor,
                treeViewFindAllTextSpan.Item.TextSpan.StartInclusiveIndex,
                new Category("main"),
                Key<TextEditorViewModel>.NewKey());
        });
        return Task.CompletedTask;
    }
}
