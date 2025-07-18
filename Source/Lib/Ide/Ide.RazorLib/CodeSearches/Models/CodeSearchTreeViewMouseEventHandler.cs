using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.Ide.RazorLib.CodeSearches.Models;

public class CodeSearchTreeViewMouseEventHandler : TreeViewMouseEventHandler
{
	private readonly TextEditorService _textEditorService;
	private readonly IServiceProvider _serviceProvider;

	public CodeSearchTreeViewMouseEventHandler(
			TextEditorService textEditorService,
			IServiceProvider serviceProvider)
		: base(textEditorService.CommonUtilityService)
	{
		_textEditorService = textEditorService;
		_serviceProvider = serviceProvider;
	}

	public override Task OnDoubleClickAsync(TreeViewCommandArgs commandArgs)
	{
		base.OnDoubleClickAsync(commandArgs);

		if (commandArgs.NodeThatReceivedMouseEvent is not TreeViewCodeSearchTextSpan treeViewCodeSearchTextSpan)
			return Task.CompletedTask;

		_textEditorService.WorkerArbitrary.PostUnique(async editContext =>
		{
			await _textEditorService.OpenInEditorAsync(
				editContext,
				treeViewCodeSearchTextSpan.AbsolutePath.Value,
				true,
				treeViewCodeSearchTextSpan.Item.StartInclusiveIndex,
				new Category("main"),
				Key<TextEditorViewModel>.NewKey());
		});
		return Task.CompletedTask;
	}
}
