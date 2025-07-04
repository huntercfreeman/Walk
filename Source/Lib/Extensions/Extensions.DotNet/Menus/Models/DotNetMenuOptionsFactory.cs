using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Namespaces.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Extensions.DotNet.CSharpProjects.Models;
using Walk.Extensions.DotNet.ComponentRenderers.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.CommandLines.Models;
using Walk.Extensions.DotNet.Namespaces.Models;

namespace Walk.Extensions.DotNet.Menus.Models;

public class DotNetMenuOptionsFactory : IDotNetMenuOptionsFactory, IBackgroundTaskGroup
{
	private readonly CommonUtilityService _commonUtilityService;
	private readonly IDotNetComponentRenderers _dotNetComponentRenderers;
	private readonly IIdeComponentRenderers _ideComponentRenderers;

	public DotNetMenuOptionsFactory(
	    CommonUtilityService commonUtilityService,
		IDotNetComponentRenderers dotNetComponentRenderers,
		IIdeComponentRenderers ideComponentRenderers)
	{
	    _commonUtilityService = commonUtilityService;
		_dotNetComponentRenderers = dotNetComponentRenderers;
		_ideComponentRenderers = ideComponentRenderers;
	}

    public Key<IBackgroundTaskGroup> BackgroundTaskKey { get; } = Key<IBackgroundTaskGroup>.NewKey();

    public bool __TaskCompletionSourceWasCreated { get; set; }

    private readonly Queue<DotNetMenuOptionsFactoryWorkKind> _workKindQueue = new();

    private readonly object _workLock = new();

    public MenuOptionRecord RemoveCSharpProjectReferenceFromSolution(
		TreeViewSolution treeViewSolution,
		TreeViewNamespacePath projectNode,
		ITerminal terminal,
		CommonUtilityService commonUtilityService,
		Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("Remove (no files are deleted)", MenuOptionKind.Delete,
			widgetRendererType: _dotNetComponentRenderers.RemoveCSharpProjectFromSolutionRendererType,
			widgetParameterMap: new Dictionary<string, object?>
			{
				{
					nameof(IRemoveCSharpProjectFromSolutionRendererType.AbsolutePath),
					projectNode.Item.AbsolutePath
				},
				{
					nameof(IDeleteFileFormRendererType.OnAfterSubmitFunc),
					new Func<AbsolutePath, Task>(
						_ =>
						{
							Enqueue_PerformRemoveCSharpProjectReferenceFromSolution(
								treeViewSolution,
								projectNode,
								terminal,
								commonUtilityService,
								onAfterCompletion);

							return Task.CompletedTask;
						})
				},
			});
	}

	public MenuOptionRecord AddProjectToProjectReference(
		TreeViewNamespacePath projectReceivingReference,
		ITerminal terminal,
		CommonUtilityService commonUtilityService,
		IdeBackgroundTaskApi ideBackgroundTaskApi,
		Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("Add Project Reference", MenuOptionKind.Other,
			onClickFunc:
			() =>
			{
				PerformAddProjectToProjectReference(
					projectReceivingReference,
					terminal,
					commonUtilityService,
					ideBackgroundTaskApi,
					onAfterCompletion);

				return Task.CompletedTask;
			});
	}

	public MenuOptionRecord RemoveProjectToProjectReference(
		TreeViewCSharpProjectToProjectReference treeViewCSharpProjectToProjectReference,
		ITerminal terminal,
		CommonUtilityService commonUtilityService,
		Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("Remove Project Reference", MenuOptionKind.Other,
			onClickFunc:
				() =>
				{
					Enqueue_PerformRemoveProjectToProjectReference(
						treeViewCSharpProjectToProjectReference,
						terminal,
						commonUtilityService,
						onAfterCompletion);

					return Task.CompletedTask;
				});
	}

	public MenuOptionRecord MoveProjectToSolutionFolder(
		TreeViewSolution treeViewSolution,
		TreeViewNamespacePath treeViewProjectToMove,
		ITerminal terminal,
		CommonUtilityService commonUtilityService,
		Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("Move to Solution Folder", MenuOptionKind.Other,
			widgetRendererType: _ideComponentRenderers.FileFormRendererType,
			widgetParameterMap: new Dictionary<string, object?>
			{
				{ nameof(IFileFormRendererType.FileName), string.Empty },
				{ nameof(IFileFormRendererType.IsDirectory), false },
				{
					nameof(IFileFormRendererType.OnAfterSubmitFunc),
					new Func<string, IFileTemplate?, List<IFileTemplate>, Task>((nextName, _, _) =>
					{
						Enqueue_PerformMoveProjectToSolutionFolder(
							treeViewSolution,
							treeViewProjectToMove,
							nextName,
							terminal,
							commonUtilityService,
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
		CommonUtilityService commonUtilityService,
		Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("Remove NuGet Package Reference", MenuOptionKind.Other,
			onClickFunc: () =>
			{
				Enqueue_PerformRemoveNuGetPackageReferenceFromProject(
					modifyProjectNamespacePath,
					treeViewCSharpProjectNugetPackageReference,
					terminal,
					commonUtilityService,
					onAfterCompletion);

				return Task.CompletedTask;
			});
	}

	private readonly
		Queue<(TreeViewSolution treeViewSolution, TreeViewNamespacePath projectNode, ITerminal terminal, CommonUtilityService commonUtilityService, Func<Task> onAfterCompletion)>
		_queue_PerformRemoveCSharpProjectReferenceFromSolution = new();


    private void Enqueue_PerformRemoveCSharpProjectReferenceFromSolution(
		TreeViewSolution treeViewSolution,
		TreeViewNamespacePath projectNode,
		ITerminal terminal,
		CommonUtilityService commonUtilityService,
		Func<Task> onAfterCompletion)
	{
        lock (_workLock)
        {
            _workKindQueue.Enqueue(DotNetMenuOptionsFactoryWorkKind.PerformRemoveCSharpProjectReferenceFromSolution);

            _queue_PerformRemoveCSharpProjectReferenceFromSolution.Enqueue(
				(treeViewSolution, projectNode, terminal, commonUtilityService, onAfterCompletion));

            _commonUtilityService.Continuous_EnqueueGroup(this);
        }
	}
	
	private ValueTask Do_PerformRemoveCSharpProjectReferenceFromSolution(
		TreeViewSolution treeViewSolution,
		TreeViewNamespacePath projectNode,
		ITerminal terminal,
		CommonUtilityService commonUtilityService,
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
		CommonUtilityService commonUtilityService,
		IdeBackgroundTaskApi ideBackgroundTaskApi,
		Func<Task> onAfterCompletion)
	{
		ideBackgroundTaskApi.Enqueue(new IdeBackgroundTaskApiWorkArgs
		{
			WorkKind = IdeBackgroundTaskApiWorkKind.RequestInputFileStateForm,
			Message = $"Add Project reference to {projectReceivingReference.Item.AbsolutePath.NameWithExtension}",
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
						NotificationHelper.DispatchInformative("Add Project Reference", $"Modified {projectReceivingReference.Item.AbsolutePath.NameWithExtension} to have a reference to {referencedProject.NameWithExtension}", commonUtilityService, TimeSpan.FromSeconds(7));
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

	private readonly
		Queue<(TreeViewCSharpProjectToProjectReference treeViewCSharpProjectToProjectReference, ITerminal terminal, CommonUtilityService commonUtilityService, Func<Task> onAfterCompletion)>
		_queue_PerformRemoveProjectToProjectReference = new();

    public void Enqueue_PerformRemoveProjectToProjectReference(
		TreeViewCSharpProjectToProjectReference treeViewCSharpProjectToProjectReference,
		ITerminal terminal,
		CommonUtilityService commonUtilityService,
		Func<Task> onAfterCompletion)
	{
        lock (_workLock)
        {
            _workKindQueue.Enqueue(DotNetMenuOptionsFactoryWorkKind.PerformRemoveProjectToProjectReference);

            _queue_PerformRemoveProjectToProjectReference.Enqueue(
				(treeViewCSharpProjectToProjectReference, terminal, commonUtilityService, onAfterCompletion));

            _commonUtilityService.Continuous_EnqueueGroup(this);
        }
	}
	
	public ValueTask Do_PerformRemoveProjectToProjectReference(
		TreeViewCSharpProjectToProjectReference treeViewCSharpProjectToProjectReference,
		ITerminal terminal,
		CommonUtilityService commonUtilityService,
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
                NotificationHelper.DispatchInformative("Remove Project Reference", $"Modified {treeViewCSharpProjectToProjectReference.Item.ModifyProjectNamespacePath.AbsolutePath.NameWithExtension} to have a reference to {treeViewCSharpProjectToProjectReference.Item.ReferenceProjectAbsolutePath.NameWithExtension}", commonUtilityService, TimeSpan.FromSeconds(7));
                return onAfterCompletion.Invoke();
            }
        };

        terminal.EnqueueCommand(terminalCommandRequest);
        return ValueTask.CompletedTask;
    }

	private readonly
		Queue<(TreeViewSolution treeViewSolution, TreeViewNamespacePath treeViewProjectToMove, string solutionFolderPath, ITerminal terminal, CommonUtilityService commonUtilityService, Func<Task> onAfterCompletion)>
		_queue_PerformMoveProjectToSolutionFolder = new();

    public void Enqueue_PerformMoveProjectToSolutionFolder(
		TreeViewSolution treeViewSolution,
		TreeViewNamespacePath treeViewProjectToMove,
		string solutionFolderPath,
		ITerminal terminal,
		CommonUtilityService commonUtilityService,
		Func<Task> onAfterCompletion)
	{
        lock (_workLock)
        {
            _workKindQueue.Enqueue(DotNetMenuOptionsFactoryWorkKind.PerformMoveProjectToSolutionFolder);

            _queue_PerformMoveProjectToSolutionFolder.Enqueue(
				(treeViewSolution, treeViewProjectToMove, solutionFolderPath, terminal, commonUtilityService, onAfterCompletion));

            _commonUtilityService.Continuous_EnqueueGroup(this);
        }
	}
	
	public ValueTask Do_PerformMoveProjectToSolutionFolder(
		TreeViewSolution treeViewSolution,
		TreeViewNamespacePath treeViewProjectToMove,
		string solutionFolderPath,
		ITerminal terminal,
		CommonUtilityService commonUtilityService,
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
                NotificationHelper.DispatchInformative("Move Project To Solution Folder", $"Moved {treeViewProjectToMove.Item.AbsolutePath.NameWithExtension} to the Solution Folder path: {solutionFolderPath}", commonUtilityService, TimeSpan.FromSeconds(7));
                return onAfterCompletion.Invoke();
            }
        };

        Enqueue_PerformRemoveCSharpProjectReferenceFromSolution(
            treeViewSolution,
            treeViewProjectToMove,
            terminal,
            commonUtilityService,
            () =>
            {
                terminal.EnqueueCommand(terminalCommandRequest);
                return Task.CompletedTask;
            });

        return ValueTask.CompletedTask;
    }

	private readonly
		Queue<(NamespacePath modifyProjectNamespacePath, TreeViewCSharpProjectNugetPackageReference treeViewCSharpProjectNugetPackageReference, ITerminal terminal, CommonUtilityService commonUtilityService, Func<Task> onAfterCompletion)>
		_queue_PerformRemoveNuGetPackageReferenceFromProject = new();

    public void Enqueue_PerformRemoveNuGetPackageReferenceFromProject(
		NamespacePath modifyProjectNamespacePath,
		TreeViewCSharpProjectNugetPackageReference treeViewCSharpProjectNugetPackageReference,
		ITerminal terminal,
		CommonUtilityService commonUtilityService,
		Func<Task> onAfterCompletion)
	{
        lock (_workLock)
        {
            _workKindQueue.Enqueue(DotNetMenuOptionsFactoryWorkKind.PerformRemoveNuGetPackageReferenceFromProject);

            _queue_PerformRemoveNuGetPackageReferenceFromProject.Enqueue(
				(modifyProjectNamespacePath, treeViewCSharpProjectNugetPackageReference, terminal, commonUtilityService, onAfterCompletion));

            _commonUtilityService.Continuous_EnqueueGroup(this);
        }
	}
	
	public ValueTask Do_PerformRemoveNuGetPackageReferenceFromProject(
		NamespacePath modifyProjectNamespacePath,
		TreeViewCSharpProjectNugetPackageReference treeViewCSharpProjectNugetPackageReference,
		ITerminal terminal,
		CommonUtilityService commonUtilityService,
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
                NotificationHelper.DispatchInformative("Remove Project Reference", $"Modified {modifyProjectNamespacePath.AbsolutePath.NameWithExtension} to NOT have a reference to {treeViewCSharpProjectNugetPackageReference.Item.LightWeightNugetPackageRecord.Id}", commonUtilityService, TimeSpan.FromSeconds(7));
                return onAfterCompletion.Invoke();
            }
        };

        terminal.EnqueueCommand(terminalCommandRequest);
        return ValueTask.CompletedTask;
    }

    public ValueTask HandleEvent()
    {
        DotNetMenuOptionsFactoryWorkKind workKind;

        lock (_workLock)
        {
            if (!_workKindQueue.TryDequeue(out workKind))
                return ValueTask.CompletedTask;
        }

        switch (workKind)
        {
            case DotNetMenuOptionsFactoryWorkKind.PerformRemoveCSharpProjectReferenceFromSolution:
            {
                var args = _queue_PerformRemoveCSharpProjectReferenceFromSolution.Dequeue();
                return Do_PerformRemoveCSharpProjectReferenceFromSolution(
					args.treeViewSolution, args.projectNode, args.terminal, args.commonUtilityService, args.onAfterCompletion);
            }
			case DotNetMenuOptionsFactoryWorkKind.PerformRemoveProjectToProjectReference:
            {
                var args = _queue_PerformRemoveProjectToProjectReference.Dequeue();
                return Do_PerformRemoveProjectToProjectReference(
                    args.treeViewCSharpProjectToProjectReference,
					args.terminal,
					args.commonUtilityService,
                    args.onAfterCompletion);
            }
			case DotNetMenuOptionsFactoryWorkKind.PerformMoveProjectToSolutionFolder:
            {
                var args = _queue_PerformMoveProjectToSolutionFolder.Dequeue();
                return Do_PerformMoveProjectToSolutionFolder(
                    args.treeViewSolution,
                    args.treeViewProjectToMove,
					args.solutionFolderPath,
					args.terminal,
					args.commonUtilityService,
                    args.onAfterCompletion);
            }
			case DotNetMenuOptionsFactoryWorkKind.PerformRemoveNuGetPackageReferenceFromProject:
            {
                var args = _queue_PerformRemoveNuGetPackageReferenceFromProject.Dequeue();
                return Do_PerformRemoveNuGetPackageReferenceFromProject(
                    args.modifyProjectNamespacePath,
                    args.treeViewCSharpProjectNugetPackageReference,
                    args.terminal,
                    args.commonUtilityService,
                    args.onAfterCompletion);
            }
            default:
            {
                Console.WriteLine($"{nameof(DotNetMenuOptionsFactory)} {nameof(HandleEvent)} default case");
				return ValueTask.CompletedTask;
            }
        }
    }
}
