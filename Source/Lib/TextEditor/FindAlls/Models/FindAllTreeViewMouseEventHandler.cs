using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.TextEditor.RazorLib.FindAlls.Models;

public class FindAllTreeViewMouseEventHandler : TreeViewMouseEventHandler
{
	private readonly TextEditorService _textEditorService;
	private readonly IServiceProvider _serviceProvider;

	public FindAllTreeViewMouseEventHandler(
			TextEditorService textEditorService,
			IServiceProvider serviceProvider,
			CommonUtilityService commonUtilityService)
		: base(commonUtilityService)
	{
		_textEditorService = textEditorService;
		_serviceProvider = serviceProvider;
	}

	public override Task OnDoubleClickAsync(TreeViewCommandArgs commandArgs)
	{
		base.OnDoubleClickAsync(commandArgs);

		if (commandArgs.NodeThatReceivedMouseEvent is not TreeViewFindAllTextSpan treeViewFindAllTextSpan)
			return Task.CompletedTask;

		_textEditorService.WorkerArbitrary.PostUnique(async editContext =>
    	{
    		await _textEditorService.OpenInEditorAsync(
    			editContext,
    			treeViewFindAllTextSpan.AbsolutePath.Value,
				true,
				treeViewFindAllTextSpan.Item.TextSpan.StartInclusiveIndex,
				new Category("main"),
				Key<TextEditorViewModel>.NewKey());
    	});
    	return Task.CompletedTask;
	}
}