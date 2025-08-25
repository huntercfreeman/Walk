using Walk.Common.RazorLib;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Extensions.DotNet.CommandLines.Models;
using Walk.Extensions.DotNet.CSharpProjects.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.Namespaces.Models;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
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
        return new MenuOptionRecord("Remove (no files are deleted)", MenuOptionKind.Delete/*,
            simpleWidgetKind: Walk.Common.RazorLib.Widgets.Models.SimpleWidgetKind.RemoveCSharpProjectFromSolution,
            widgetParameterMap: new Dictionary<string, object?>
            {
                {
                    nameof(Walk.Common.RazorLib.FileSystems.Displays.RemoveCSharpProjectFromSolutionDisplay.AbsolutePath),
                    projectNode.Item
                },
                {
                    nameof(Walk.Common.RazorLib.FileSystems.Displays.RemoveCSharpProjectFromSolutionDisplay.OnAfterSubmitFunc),
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
            }*/);
    }

    public MenuOptionRecord AddProjectToProjectReference(
        TreeViewNamespacePath projectReceivingReference,
        ITerminal terminal,
        IdeService ideService,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("Add Project Reference", MenuOptionKind.Other,
            onClickFunc:
            _ =>
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
                _ =>
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
        return new MenuOptionRecord("Move to Solution Folder", MenuOptionKind.Other/*,
            simpleWidgetKind: Walk.Common.RazorLib.Widgets.Models.SimpleWidgetKind.FileForm,
            widgetParameterMap: new Dictionary<string, object?>
            {
                { nameof(Walk.Common.RazorLib.FileSystems.Displays.FileFormDisplay.FileName), string.Empty },
                { nameof(Walk.Common.RazorLib.FileSystems.Displays.FileFormDisplay.IsDirectory), false },
                {
                    nameof(Walk.Common.RazorLib.FileSystems.Displays.FileFormDisplay.OnAfterSubmitFunc),
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
            }*/);
    }

    public MenuOptionRecord RemoveNuGetPackageReferenceFromProject(
        AbsolutePath modifyProjectAbsolutePath,
        string namespaceString,
        TreeViewCSharpProjectNugetPackageReference treeViewCSharpProjectNugetPackageReference,
        ITerminal terminal,
        CommonService commonService,
        Func<Task> onAfterCompletion)
    {
        return new MenuOptionRecord("Remove NuGet Package Reference", MenuOptionKind.Other,
            onClickFunc: _ =>
            {
                Enqueue_PerformRemoveNuGetPackageReferenceFromProject(
                    modifyProjectAbsolutePath,
                    namespaceString,
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
        var workingDirectory = treeViewSolution.Item.AbsolutePath.CreateSubstringParentDirectory();
        if (workingDirectory is null)
            return ValueTask.CompletedTask;

        var formattedCommandValue = DotNetCliCommandFormatter.FormatRemoveCSharpProjectReferenceFromSolutionAction(
            treeViewSolution.Item.AbsolutePath.Value,
            projectNode.Item.Value);

        var terminalCommandRequest = new TerminalCommandRequest(
            formattedCommandValue,
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
            StringValue = $"Add Project reference to {projectReceivingReference.Item.Name}",
            OnAfterSubmitFunc = referencedProject =>
            {
                if (referencedProject.Value is null)
                    return Task.CompletedTask;

                var formattedCommandValue = DotNetCliCommandFormatter.FormatAddProjectToProjectReference(
                    projectReceivingReference.Item.Value,
                    referencedProject.Value);

                var terminalCommandRequest = new TerminalCommandRequest(
                    formattedCommandValue,
                    null)
                {
                    ContinueWithFunc = parsedCommand =>
                    {
                        CommonFacts.DispatchInformative("Add Project Reference", $"Modified {projectReceivingReference.Item.Name} to have a reference to {referencedProject.Name}", ideService.CommonService, TimeSpan.FromSeconds(7));
                        return onAfterCompletion.Invoke();
                    }
                };

                terminal.EnqueueCommand(terminalCommandRequest);
                return Task.CompletedTask;
            },
            SelectionIsValidFunc = absolutePath =>
            {
                if (absolutePath.Value is null || absolutePath.IsDirectory)
                    return Task.FromResult(false);

                return Task.FromResult(
                    absolutePath.Name.EndsWith(CommonFacts.C_SHARP_PROJECT));
            },
            InputFilePatterns = new()
            {
                new InputFilePattern(
                    "C# Project",
                    absolutePath => absolutePath.Name.EndsWith(CommonFacts.C_SHARP_PROJECT))
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
        var formattedCommandValue = DotNetCliCommandFormatter.FormatRemoveProjectToProjectReference(
            treeViewCSharpProjectToProjectReference.Item.ModifyProjectAbsolutePath.Value,
            treeViewCSharpProjectToProjectReference.Item.ReferenceProjectAbsolutePath.Value);

        var terminalCommandRequest = new TerminalCommandRequest(
            formattedCommandValue,
            null)
        {
            ContinueWithFunc = parsedCommand =>
            {
                CommonFacts.DispatchInformative("Remove Project Reference", $"Modified {treeViewCSharpProjectToProjectReference.Item.ModifyProjectAbsolutePath.Name} to have a reference to {treeViewCSharpProjectToProjectReference.Item.ReferenceProjectAbsolutePath.Name}", commonService, TimeSpan.FromSeconds(7));
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
        var formattedCommandValue = DotNetCliCommandFormatter.FormatMoveProjectToSolutionFolder(
            treeViewSolution.Item.AbsolutePath.Value,
            treeViewProjectToMove.Item.Value,
            solutionFolderPath);

        var terminalCommandRequest = new TerminalCommandRequest(
            formattedCommandValue,
            null)
        {
            ContinueWithFunc = parsedCommand =>
            {
                CommonFacts.DispatchInformative("Move Project To Solution Folder", $"Moved {treeViewProjectToMove.Item.Name} to the Solution Folder path: {solutionFolderPath}", commonService, TimeSpan.FromSeconds(7));
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
        AbsolutePath modifyProjectAbsolutePath,
        string namespaceString,
        TreeViewCSharpProjectNugetPackageReference treeViewCSharpProjectNugetPackageReference,
        ITerminal terminal,
        CommonService commonService,
        Func<Task> onAfterCompletion)
    {
        Enqueue(new DotNetWorkArgs
        {
            WorkKind = DotNetWorkKind.PerformRemoveNuGetPackageReferenceFromProject,
            ModifyProjectAbsolutePath = modifyProjectAbsolutePath,
            ModifyProjectNamespaceString = namespaceString,
            TreeViewCSharpProjectNugetPackageReference = treeViewCSharpProjectNugetPackageReference,
            Terminal = terminal,
            OnAfterCompletion = onAfterCompletion
        });
    }

    public ValueTask Do_PerformRemoveNuGetPackageReferenceFromProject(
        AbsolutePath modifyProjectAbsolutePath,
        string namespaceString,
        TreeViewCSharpProjectNugetPackageReference treeViewCSharpProjectNugetPackageReference,
        ITerminal terminal,
        CommonService commonService,
        Func<Task> onAfterCompletion)
    {
        var formattedCommandValue = DotNetCliCommandFormatter.FormatRemoveNugetPackageReferenceFromProject(
            modifyProjectAbsolutePath.Value,
            treeViewCSharpProjectNugetPackageReference.Item.LightWeightNugetPackageRecord.Id);

        var terminalCommandRequest = new TerminalCommandRequest(
            formattedCommandValue,
            null)
        {
            ContinueWithFunc = parsedCommand =>
            {
                CommonFacts.DispatchInformative("Remove Project Reference", $"Modified {modifyProjectAbsolutePath.Name} to NOT have a reference to {treeViewCSharpProjectNugetPackageReference.Item.LightWeightNugetPackageRecord.Id}", commonService, TimeSpan.FromSeconds(7));
                return onAfterCompletion.Invoke();
            }
        };

        terminal.EnqueueCommand(terminalCommandRequest);
        return ValueTask.CompletedTask;
    }
}
