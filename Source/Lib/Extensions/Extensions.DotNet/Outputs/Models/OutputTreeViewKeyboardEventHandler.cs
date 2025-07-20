using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.TextEditor.RazorLib;

namespace Walk.Extensions.DotNet.Outputs.Models;

public class OutputTreeViewKeyboardEventHandler : TreeViewKeyboardEventHandler
{
	private readonly TextEditorService _textEditorService;
	private readonly IServiceProvider _serviceProvider;

	public OutputTreeViewKeyboardEventHandler(
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
			case CommonFacts.WhitespaceCodes.ENTER_CODE:
				return InvokeOpenInEditor(commandArgs, true);
			case CommonFacts.WhitespaceCodes.SPACE_CODE:
				return InvokeOpenInEditor(commandArgs, false);
		}
		
		return Task.CompletedTask;
	}

	private Task InvokeOpenInEditor(TreeViewCommandArgs commandArgs, bool shouldSetFocusToEditor)
	{
		var activeNode = commandArgs.TreeViewContainer.ActiveNode;

		if (activeNode is not TreeViewDiagnosticLine treeViewDiagnosticLine)
			return Task.CompletedTask;
			
		return OutputTextSpanHelper.OpenInEditorOnClick(
			treeViewDiagnosticLine,
			shouldSetFocusToEditor,
			_textEditorService);
	}
}
