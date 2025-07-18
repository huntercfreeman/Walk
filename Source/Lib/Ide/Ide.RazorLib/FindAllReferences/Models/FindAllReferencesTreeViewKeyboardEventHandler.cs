/*
// FindAllReferences
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Keyboards.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Installations.Models;

namespace Walk.Ide.RazorLib.FindAllReferences.Models;

public class FindAllReferencesTreeViewKeyboardEventHandler : TreeViewKeyboardEventHandler
{
	private readonly ITextEditorService _textEditorService;
	private readonly IServiceProvider _serviceProvider;

	public FindAllReferencesTreeViewKeyboardEventHandler(
			ITextEditorService textEditorService,
			IServiceProvider serviceProvider,
			ITreeViewService treeViewService,
			IBackgroundTaskService backgroundTaskService)
		: base(treeViewService, backgroundTaskService)
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

		if (activeNode is not TreeViewFindAllReferences treeViewFindAllReferences)
			return Task.CompletedTask;
			
		return FindAllReferencesTextSpanHelper.OpenInEditorOnClick(
			treeViewFindAllReferences,
			shouldSetFocusToEditor,
			_textEditorService);
	}
}
*/