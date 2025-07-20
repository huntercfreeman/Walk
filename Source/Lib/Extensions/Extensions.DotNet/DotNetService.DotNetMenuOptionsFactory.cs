using Walk.Common.RazorLib;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Namespaces.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.Extensions.DotNet.CommandLines.Models;
using Walk.Extensions.DotNet.ComponentRenderers.Models;
using Walk.Extensions.DotNet.CSharpProjects.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.Namespaces.Models;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.Terminals.Models;

namespace Walk.Extensions.DotNet;

public partial class DotNetService
{
	public MenuOptionRecord RemoveCSharpProjectReferenceFromSolution(
		TreeViewSolution treeViewSolution,
		TreeViewNamespacePath projectNode,
		ITerminal terminal,
		CommonService commonService,
		Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("Remove (no files are deleted)", MenuOptionKind.Delete,
			widgetRendererType: DotNetComponentRenderers.RemoveCSharpProjectFromSolutionRendererType,
			widgetParameterMap: new Dictionary<string, object?>
			{
				{
					nameof(IRemoveCSharpProjectFromSolutionRendererType.AbsolutePath),
					projectNode.Item.AbsolutePath
				},
				{
					nameof(Walk.Ide.RazorLib.FileSystems.Displays.DeleteFileFormDisplay.OnAfterSubmitFunc),
					new Func<AbsolutePath, Task>(
						_ =>
						{
							Enqueue_PerformRemoveCSharpProjectReferenceFromSolution(
								treeViewSolution,
								projectNode,
								terminal,
								commonService,
								onAfterCompletion);

							return Task.CompletedTask;
						})
				},
			});
	}

	public MenuOptionRecord AddProjectToProjectReference(
		TreeViewNamespacePath projectReceivingReference,
		ITerminal terminal,
		IdeService ideService,
		Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("Add Project Reference", MenuOptionKind.Other,
			onClickFunc:
			() =>
			{
				PerformAddProjectToProjectReference(
					projectReceivingReference,
					terminal,
					ideService,
					onAfterCompletion);

				return Task.CompletedTask;
			});
	}

	public MenuOptionRecord RemoveProjectToProjectReference(
		TreeViewCSharpProjectToProjectReference treeViewCSharpProjectToProjectReference,
		ITerminal terminal,
		CommonService commonService,
		Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("Remove Project Reference", MenuOptionKind.Other,
			onClickFunc:
				() =>
				{
					Enqueue_PerformRemoveProjectToProjectReference(
						treeViewCSharpProjectToProjectReference,
						terminal,
						commonService,
						onAfterCompletion);

					return Task.CompletedTask;
				});
	}

	public MenuOptionRecord MoveProjectToSolutionFolder(
		TreeViewSolution treeViewSolution,
		TreeViewNamespacePath treeViewProjectToMove,
		ITerminal terminal,
		CommonService commonService,
		Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("Move to Solution Folder", MenuOptionKind.Other,
			widgetRendererType: typeof(Walk.Ide.RazorLib.FileSystems.Displays.FileFormDisplay),
			widgetParameterMap: new Dictionary<string, object?>
			{
				{ nameof(Walk.Ide.RazorLib.FileSystems.Displays.FileFormDisplay.FileName), string.Empty },
				{ nameof(Walk.Ide.RazorLib.FileSystems.Displays.FileFormDisplay.IsDirectory), false },
				{
					nameof(Walk.Ide.RazorLib.FileSystems.Displays.FileFormDisplay.OnAfterSubmitFunc),
					new Func<string, IFileTemplate?, List<IFileTemplate>, Task>((nextName, _, _) =>
					{
						Enqueue_PerformMoveProjectToSolutionFolder(
							treeViewSolution,
							treeViewProjectToMove,
							nextName,
							terminal,
							commonService,
							onAfterCompletion);

						return Task.CompletedTask;
					})
				},
			});
	}

	public MenuOptionRecord RemoveNuGetPackageReferenceFromProject(
		NamespacePath modifyProjectNamespacePath,
		TreeViewCSharpProjectNugetPackageReference treeViewCSharpProjectNugetPackageReference,
		ITerminal terminal,
		CommonService commonService,
		Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("Remove NuGet Package Reference", MenuOptionKind.Other,
			onClickFunc: () =>
			{
				Enqueue_PerformRemoveNuGetPackageReferenceFromProject(
					modifyProjectNamespacePath,
					treeViewCSharpProjectNugetPackageReference,
					terminal,
					commonService,
					onAfterCompletion);

				return Task.CompletedTask;
			});
	}

	private void Enqueue_PerformRemoveCSharpProjectReferenceFromSolution(
		TreeViewSolution treeViewSolution,
		TreeViewNamespacePath projectNode,
		ITerminal terminal,
		CommonService commonService,
		Func<Task> onAfterCompletion)
	{
		Enqueue(new DotNetWorkArgs
		{
			WorkKind = DotNetWorkKind.PerformRemoveCSharpProjectReferenceFromSolution,
			TreeViewSolution = treeViewSolution,
			ProjectNode = projectNode,
			Terminal = terminal,
			OnAfterCompletion = onAfterCompletion
		});
	}

	private ValueTask Do_PerformRemoveCSharpProjectReferenceFromSolution(
		TreeViewSolution treeViewSolution,
		TreeViewNamespacePath projectNode,
		ITerminal terminal,
		CommonService commonService,
		Func<Task> onAfterCompletion)
	{
		var workingDirectory = treeViewSolution.Item.NamespacePath.AbsolutePath.ParentDirectory!;

		var formattedCommand = DotNetCliCommandFormatter.FormatRemoveCSharpProjectReferenceFromSolutionAction(
			treeViewSolution.Item.NamespacePath.AbsolutePath.Value,
			projectNode.Item.AbsolutePath.Value);

		var terminalCommandRequest = new TerminalCommandRequest(
			formattedCommand.Value,
			workingDirectory)
		{
			ContinueWithFunc = parsedCommand => onAfterCompletion.Invoke()
		};

		terminal.EnqueueCommand(terminalCommandRequest);
		return ValueTask.CompletedTask;
	}

	public void PerformAddProjectToProjectReference(
		TreeViewNamespacePath projectReceivingReference,
		ITerminal terminal,
		IdeService ideService,
		Func<Task> onAfterCompletion)
	{
		ideService.Enqueue(new IdeWorkArgs
		{
			WorkKind = IdeWorkKind.RequestInputFileStateForm,
			StringValue = $"Add Project reference to {projectReceivingReference.Item.AbsolutePath.NameWithExtension}",
			OnAfterSubmitFunc = referencedProject =>
			{
				if (referencedProject.ExactInput is null)
					return Task.CompletedTask;

				var formattedCommand = DotNetCliCommandFormatter.FormatAddProjectToProjectReference(
					projectReceivingReference.Item.AbsolutePath.Value,
					referencedProject.Value);

				var terminalCommandRequest = new TerminalCommandRequest(
					formattedCommand.Value,
					null)
				{
					ContinueWithFunc = parsedCommand =>
					{
						NotificationHelper.DispatchInformative("Add Project Reference", $"Modified {projectReceivingReference.Item.AbsolutePath.NameWithExtension} to have a reference to {referencedProject.NameWithExtension}", ideService.CommonService, TimeSpan.FromSeconds(7));
						return onAfterCompletion.Invoke();
					}
				};

				terminal.EnqueueCommand(terminalCommandRequest);
				return Task.CompletedTask;
			},
			SelectionIsValidFunc = absolutePath =>
			{
				if (absolutePath.ExactInput is null || absolutePath.IsDirectory)
					return Task.FromResult(false);

				return Task.FromResult(
					absolutePath.ExtensionNoPeriod.EndsWith(ExtensionNoPeriodFacts.C_SHARP_PROJECT));
			},
			InputFilePatterns = new()
			{
				new InputFilePattern(
					"C# Project",
					absolutePath => absolutePath.ExtensionNoPeriod.EndsWith(ExtensionNoPeriodFacts.C_SHARP_PROJECT))
			}
		});
	}

	public void Enqueue_PerformRemoveProjectToProjectReference(
		TreeViewCSharpProjectToProjectReference treeViewCSharpProjectToProjectReference,
		ITerminal terminal,
		CommonService commonService,
		Func<Task> onAfterCompletion)
	{
		Enqueue(new DotNetWorkArgs
		{
			WorkKind = DotNetWorkKind.PerformRemoveProjectToProjectReference,
			TreeViewCSharpProjectToProjectReference = treeViewCSharpProjectToProjectReference,
			Terminal = terminal,
			OnAfterCompletion = onAfterCompletion
		});
	}

	public ValueTask Do_PerformRemoveProjectToProjectReference(
		TreeViewCSharpProjectToProjectReference treeViewCSharpProjectToProjectReference,
		ITerminal terminal,
		CommonService commonService,
		Func<Task> onAfterCompletion)
	{
		var formattedCommand = DotNetCliCommandFormatter.FormatRemoveProjectToProjectReference(
			treeViewCSharpProjectToProjectReference.Item.ModifyProjectNamespacePath.AbsolutePath.Value,
			treeViewCSharpProjectToProjectReference.Item.ReferenceProjectAbsolutePath.Value);

		var terminalCommandRequest = new TerminalCommandRequest(
			formattedCommand.Value,
			null)
		{
			ContinueWithFunc = parsedCommand =>
			{
				NotificationHelper.DispatchInformative("Remove Project Reference", $"Modified {treeViewCSharpProjectToProjectReference.Item.ModifyProjectNamespacePath.AbsolutePath.NameWithExtension} to have a reference to {treeViewCSharpProjectToProjectReference.Item.ReferenceProjectAbsolutePath.NameWithExtension}", commonService, TimeSpan.FromSeconds(7));
				return onAfterCompletion.Invoke();
			}
		};

		terminal.EnqueueCommand(terminalCommandRequest);
		return ValueTask.CompletedTask;
	}

	public void Enqueue_PerformMoveProjectToSolutionFolder(
		TreeViewSolution treeViewSolution,
		TreeViewNamespacePath treeViewProjectToMove,
		string solutionFolderPath,
		ITerminal terminal,
		CommonService commonService,
		Func<Task> onAfterCompletion)
	{
		Enqueue(new DotNetWorkArgs
		{
			WorkKind = DotNetWorkKind.PerformMoveProjectToSolutionFolder,
			TreeViewSolution = treeViewSolution,
			TreeViewProjectToMove = treeViewProjectToMove,
			SolutionFolderPath = solutionFolderPath,
			Terminal = terminal,
			OnAfterCompletion = onAfterCompletion
		});
	}

	public ValueTask Do_PerformMoveProjectToSolutionFolder(
		TreeViewSolution treeViewSolution,
		TreeViewNamespacePath treeViewProjectToMove,
		string solutionFolderPath,
		ITerminal terminal,
		CommonService commonService,
		Func<Task> onAfterCompletion)
	{
		var formattedCommand = DotNetCliCommandFormatter.FormatMoveProjectToSolutionFolder(
			treeViewSolution.Item.NamespacePath.AbsolutePath.Value,
			treeViewProjectToMove.Item.AbsolutePath.Value,
			solutionFolderPath);

		var terminalCommandRequest = new TerminalCommandRequest(
			formattedCommand.Value,
			null)
		{
			ContinueWithFunc = parsedCommand =>
			{
				NotificationHelper.DispatchInformative("Move Project To Solution Folder", $"Moved {treeViewProjectToMove.Item.AbsolutePath.NameWithExtension} to the Solution Folder path: {solutionFolderPath}", commonService, TimeSpan.FromSeconds(7));
				return onAfterCompletion.Invoke();
			}
		};

		Enqueue_PerformRemoveCSharpProjectReferenceFromSolution(
			treeViewSolution,
			treeViewProjectToMove,
			terminal,
			commonService,
			() =>
			{
				terminal.EnqueueCommand(terminalCommandRequest);
				return Task.CompletedTask;
			});

		return ValueTask.CompletedTask;
	}

	public void Enqueue_PerformRemoveNuGetPackageReferenceFromProject(
		NamespacePath modifyProjectNamespacePath,
		TreeViewCSharpProjectNugetPackageReference treeViewCSharpProjectNugetPackageReference,
		ITerminal terminal,
		CommonService commonService,
		Func<Task> onAfterCompletion)
	{
		Enqueue(new DotNetWorkArgs
		{
			WorkKind = DotNetWorkKind.PerformRemoveNuGetPackageReferenceFromProject,
			ModifyProjectNamespacePath = modifyProjectNamespacePath,
			TreeViewCSharpProjectNugetPackageReference = treeViewCSharpProjectNugetPackageReference,
			Terminal = terminal,
			OnAfterCompletion = onAfterCompletion
		});
	}

	public ValueTask Do_PerformRemoveNuGetPackageReferenceFromProject(
		NamespacePath modifyProjectNamespacePath,
		TreeViewCSharpProjectNugetPackageReference treeViewCSharpProjectNugetPackageReference,
		ITerminal terminal,
		CommonService commonService,
		Func<Task> onAfterCompletion)
	{
		var formattedCommand = DotNetCliCommandFormatter.FormatRemoveNugetPackageReferenceFromProject(
			modifyProjectNamespacePath.AbsolutePath.Value,
			treeViewCSharpProjectNugetPackageReference.Item.LightWeightNugetPackageRecord.Id);

		var terminalCommandRequest = new TerminalCommandRequest(
			formattedCommand.Value,
			null)
		{
			ContinueWithFunc = parsedCommand =>
			{
				NotificationHelper.DispatchInformative("Remove Project Reference", $"Modified {modifyProjectNamespacePath.AbsolutePath.NameWithExtension} to NOT have a reference to {treeViewCSharpProjectNugetPackageReference.Item.LightWeightNugetPackageRecord.Id}", commonService, TimeSpan.FromSeconds(7));
				return onAfterCompletion.Invoke();
			}
		};

		terminal.EnqueueCommand(terminalCommandRequest);
		return ValueTask.CompletedTask;
	}
}
