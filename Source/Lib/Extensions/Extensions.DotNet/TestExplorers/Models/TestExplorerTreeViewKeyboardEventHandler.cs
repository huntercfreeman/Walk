using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.TextEditor.RazorLib;

namespace Walk.Extensions.DotNet.TestExplorers.Models;

public class TestExplorerTreeViewKeyboardEventHandler : TreeViewKeyboardEventHandler
{
	private readonly TextEditorService _textEditorService;
	private readonly IServiceProvider _serviceProvider;

	public TestExplorerTreeViewKeyboardEventHandler(
			TextEditorService textEditorService,
			IServiceProvider serviceProvider)
		: base(textEditorService.CommonService)
	{
		_textEditorService = textEditorService;
		_serviceProvider = serviceProvider;
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

		if (activeNode is not TreeViewStringFragment treeViewStringFragment)
		{
			NotificationHelper.DispatchInformative(
				nameof(TestExplorerTreeViewKeyboardEventHandler),
				$"Could not open in editor because node is not type: {nameof(TreeViewStringFragment)}",
				_textEditorService.CommonService,
				TimeSpan.FromSeconds(5));

			return Task.CompletedTask;
		}

		if (treeViewStringFragment.Parent is not TreeViewStringFragment parentTreeViewStringFragment)
		{
			NotificationHelper.DispatchInformative(
				nameof(TestExplorerTreeViewKeyboardEventHandler),
				$"Could not open in editor because node's parent does not seem to include a class name",
				_textEditorService.CommonService,
				TimeSpan.FromSeconds(5));

			return Task.CompletedTask;
		}

		var className = parentTreeViewStringFragment.Item.Value.Split('.').Last();

		NotificationHelper.DispatchInformative(
			nameof(TestExplorerTreeViewMouseEventHandler),
			className + ".cs",
			_textEditorService.CommonService,
			TimeSpan.FromSeconds(5));

		var methodName = treeViewStringFragment.Item.Value.Trim();

		NotificationHelper.DispatchInformative(
			nameof(TestExplorerTreeViewMouseEventHandler),
			methodName + "()",
			_textEditorService.CommonService,
			TimeSpan.FromSeconds(5));

		_textEditorService.WorkerArbitrary.PostUnique(
			TestExplorerHelper.ShowTestInEditorFactory(
				className,
				methodName,
				_textEditorService,
				_serviceProvider));

		return Task.CompletedTask;
	}
}