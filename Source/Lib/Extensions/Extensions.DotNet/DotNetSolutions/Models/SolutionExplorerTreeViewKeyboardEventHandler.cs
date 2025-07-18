using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Keyboards.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.Menus.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Displays.Internals;
using Walk.Extensions.DotNet.Namespaces.Models;

namespace Walk.Extensions.DotNet.DotNetSolutions.Models;

public class SolutionExplorerTreeViewKeyboardEventHandler : TreeViewKeyboardEventHandler
{
	private readonly IdeBackgroundTaskApi _ideBackgroundTaskApi;
	private readonly IMenuOptionsFactory _menuOptionsFactory;
	private readonly TextEditorService _textEditorService;

	public SolutionExplorerTreeViewKeyboardEventHandler(
			IdeBackgroundTaskApi ideBackgroundTaskApi,
			IMenuOptionsFactory menuOptionsFactory,
			TextEditorService textEditorService)
		: base(textEditorService.CommonUtilityService)
	{
		_ideBackgroundTaskApi = ideBackgroundTaskApi;
		_menuOptionsFactory = menuOptionsFactory;
		_textEditorService = textEditorService;
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

		if (commandArgs.KeyboardEventArgs.CtrlKey)
		{
			CtrlModifiedKeymap(commandArgs);
			return Task.CompletedTask;
		}
		else if (commandArgs.KeyboardEventArgs.AltKey)
		{
			AltModifiedKeymap(commandArgs);
			return Task.CompletedTask;
		}

		return Task.CompletedTask;
	}

	private void CtrlModifiedKeymap(TreeViewCommandArgs commandArgs)
	{
		if (commandArgs.KeyboardEventArgs is null)
			return;

		if (commandArgs.KeyboardEventArgs.AltKey)
		{
			CtrlAltModifiedKeymap(commandArgs);
			return;
		}

		switch (commandArgs.KeyboardEventArgs.Key)
		{
			case "c":
				InvokeCopyFile(commandArgs);
				return;
			case "x":
				InvokeCutFile(commandArgs);
				return;
			case "v":
				InvokePasteClipboard(commandArgs);
				return;
		}
	}

	private void AltModifiedKeymap(TreeViewCommandArgs commandArgs)
	{
		return;
	}

	private void CtrlAltModifiedKeymap(TreeViewCommandArgs commandArgs)
	{
		return;
	}

	private Task InvokeCopyFile(TreeViewCommandArgs commandArgs)
	{
		var activeNode = commandArgs.TreeViewContainer.ActiveNode;

		if (activeNode is not TreeViewNamespacePath treeViewNamespacePath)
			return Task.CompletedTask;

		var copyFileMenuOption = _menuOptionsFactory.CopyFile(
			treeViewNamespacePath.Item.AbsolutePath,
			() =>
			{
				NotificationHelper.DispatchInformative("Copy Action", $"Copied: {treeViewNamespacePath.Item.AbsolutePath.NameWithExtension}", _textEditorService.CommonUtilityService, TimeSpan.FromSeconds(7));
				return Task.CompletedTask;
			});

		if (copyFileMenuOption.OnClickFunc is null)
			return Task.CompletedTask;

		return copyFileMenuOption.OnClickFunc.Invoke();
	}

	private Task InvokePasteClipboard(TreeViewCommandArgs commandArgs)
	{
		var activeNode = commandArgs.TreeViewContainer.ActiveNode;

		if (activeNode is not TreeViewNamespacePath treeViewNamespacePath)
			return Task.CompletedTask;

		MenuOptionRecord pasteMenuOptionRecord;

		if (treeViewNamespacePath.Item.AbsolutePath.IsDirectory)
		{
			pasteMenuOptionRecord = _menuOptionsFactory.PasteClipboard(
				treeViewNamespacePath.Item.AbsolutePath,
				async () =>
				{
					var localParentOfCutFile = SolutionExplorerContextMenu.ParentOfCutFile;
					SolutionExplorerContextMenu.ParentOfCutFile = null;

					if (localParentOfCutFile is not null)
						await ReloadTreeViewModel(localParentOfCutFile).ConfigureAwait(false);

					await ReloadTreeViewModel(treeViewNamespacePath).ConfigureAwait(false);
				});
		}
		else
		{
			var parentDirectory = treeViewNamespacePath.Item.AbsolutePath.ParentDirectory;
			var parentDirectoryAbsolutePath = _textEditorService.CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(parentDirectory, true);

			pasteMenuOptionRecord = _menuOptionsFactory.PasteClipboard(
				parentDirectoryAbsolutePath,
				async () =>
				{
					var localParentOfCutFile = SolutionExplorerContextMenu.ParentOfCutFile;
					SolutionExplorerContextMenu.ParentOfCutFile = null;

					if (localParentOfCutFile is not null)
						await ReloadTreeViewModel(localParentOfCutFile).ConfigureAwait(false);

					await ReloadTreeViewModel(treeViewNamespacePath).ConfigureAwait(false);
				});
		}

		if (pasteMenuOptionRecord.OnClickFunc is null)
			return Task.CompletedTask;

		return pasteMenuOptionRecord.OnClickFunc.Invoke();
	}

	private Task InvokeCutFile(TreeViewCommandArgs commandArgs)
	{
		var activeNode = commandArgs.TreeViewContainer.ActiveNode;

		if (activeNode is not TreeViewNamespacePath treeViewNamespacePath)
			return Task.CompletedTask;

		var parent = treeViewNamespacePath.Parent as TreeViewNamespacePath;

		MenuOptionRecord cutFileOptionRecord = _menuOptionsFactory.CutFile(
			treeViewNamespacePath.Item.AbsolutePath,
			() =>
			{
				NotificationHelper.DispatchInformative("Cut Action", $"Cut: {treeViewNamespacePath.Item.AbsolutePath.NameWithExtension}", _textEditorService.CommonUtilityService, TimeSpan.FromSeconds(7));
				SolutionExplorerContextMenu.ParentOfCutFile = parent;
				return Task.CompletedTask;
			});

		if (cutFileOptionRecord.OnClickFunc is null)
			return Task.CompletedTask;

		return cutFileOptionRecord.OnClickFunc.Invoke();
	}

	private Task InvokeOpenInEditor(TreeViewCommandArgs commandArgs, bool shouldSetFocusToEditor)
	{
		if (commandArgs.TreeViewContainer.ActiveNode is not TreeViewNamespacePath treeViewNamespacePath)
			return Task.CompletedTask;
			
		_textEditorService.WorkerArbitrary.PostUnique(async editContext =>
		{
			await _textEditorService.OpenInEditorAsync(
				editContext,
				treeViewNamespacePath.Item.AbsolutePath.Value,
				shouldSetFocusToEditor,
				null,
				new Category("main"),
				Key<TextEditorViewModel>.NewKey());
		});
	    return Task.CompletedTask;
	}

	private async Task ReloadTreeViewModel(TreeViewNoType? treeViewModel)
	{
		if (treeViewModel is null)
			return;

		await treeViewModel.LoadChildListAsync().ConfigureAwait(false);

		_textEditorService.CommonUtilityService.TreeView_ReRenderNodeAction(
			DotNetSolutionState.TreeViewSolutionExplorerStateKey,
			treeViewModel);

		_textEditorService.CommonUtilityService.TreeView_MoveUpAction(
			DotNetSolutionState.TreeViewSolutionExplorerStateKey,
			false,
			false);
	}
}