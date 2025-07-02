using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Keyboards.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.Extensions.DotNet.TestExplorers.Models;

public class TestExplorerTreeViewKeyboardEventHandler : TreeViewKeyboardEventHandler
{
	private readonly ICompilerServiceRegistry _compilerServiceRegistry;
	private readonly TextEditorService _textEditorService;
	private readonly ICommonUtilityService _commonUtilityService;
	private readonly IServiceProvider _serviceProvider;

	public TestExplorerTreeViewKeyboardEventHandler(
			ICompilerServiceRegistry compilerServiceRegistry,
			TextEditorService textEditorService,
			ICommonUtilityService commonUtilityService,
			IServiceProvider serviceProvider,
			ITreeViewService treeViewService,
			BackgroundTaskService backgroundTaskService)
		: base(treeViewService, backgroundTaskService)
	{
		_compilerServiceRegistry = compilerServiceRegistry;
		_textEditorService = textEditorService;
		_commonUtilityService = commonUtilityService;
		_serviceProvider = serviceProvider;
	}

	public override Task OnKeyDownAsync(TreeViewCommandArgs commandArgs)
	{
		if (commandArgs.KeyboardEventArgs is null)
			return Task.CompletedTask;

		base.OnKeyDownAsync(commandArgs);

		switch (commandArgs.KeyboardEventArgs.Code)
		{
			case KeyboardKeyFacts.WhitespaceCodes.ENTER_CODE:
				return InvokeOpenInEditor(commandArgs, true);
			case KeyboardKeyFacts.WhitespaceCodes.SPACE_CODE:
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
				_commonUtilityService,
				TimeSpan.FromSeconds(5));

			return Task.CompletedTask;
		}

		if (treeViewStringFragment.Parent is not TreeViewStringFragment parentTreeViewStringFragment)
		{
			NotificationHelper.DispatchInformative(
				nameof(TestExplorerTreeViewKeyboardEventHandler),
				$"Could not open in editor because node's parent does not seem to include a class name",
				_commonUtilityService,
				TimeSpan.FromSeconds(5));

			return Task.CompletedTask;
		}

		var className = parentTreeViewStringFragment.Item.Value.Split('.').Last();

		NotificationHelper.DispatchInformative(
			nameof(TestExplorerTreeViewMouseEventHandler),
			className + ".cs",
			_commonUtilityService,
			TimeSpan.FromSeconds(5));

		var methodName = treeViewStringFragment.Item.Value.Trim();

		NotificationHelper.DispatchInformative(
			nameof(TestExplorerTreeViewMouseEventHandler),
			methodName + "()",
			_commonUtilityService,
			TimeSpan.FromSeconds(5));

		_textEditorService.WorkerArbitrary.PostUnique(
			TestExplorerHelper.ShowTestInEditorFactory(
				className,
				methodName,
				_commonUtilityService,
				_compilerServiceRegistry,
				_textEditorService,
				_serviceProvider));

		return Task.CompletedTask;
	}
}