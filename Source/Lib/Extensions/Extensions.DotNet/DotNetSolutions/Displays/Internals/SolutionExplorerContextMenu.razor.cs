using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Namespaces.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.CompilerServices.DotNetSolution.Models;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.FormsGenerics.Displays;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Extensions.DotNet.CSharpProjects.Displays;
using Walk.Extensions.DotNet.CSharpProjects.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.CommandLines.Models;
using Walk.Extensions.DotNet.Namespaces.Models;

namespace Walk.Extensions.DotNet.DotNetSolutions.Displays.Internals;

public partial class SolutionExplorerContextMenu : ComponentBase
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;

    [Parameter, EditorRequired]
    public TreeViewCommandArgs TreeViewCommandArgs { get; set; }

    private static readonly Key<IDynamicViewModel> _solutionEditorDialogKey = Key<IDynamicViewModel>.NewKey();
    private static readonly Key<IDynamicViewModel> _solutionPropertiesDialogKey = Key<IDynamicViewModel>.NewKey();
    private static readonly Key<IDynamicViewModel> _newCSharpProjectDialogKey = Key<IDynamicViewModel>.NewKey();

    public static readonly Key<DropdownRecord> ContextMenuEventDropdownKey = Key<DropdownRecord>.NewKey();

    /// <summary>
    /// The program is currently running using Photino locally on the user's computer
    /// therefore this static solution works without leaking any information.
    /// </summary>
    public static TreeViewNoType? ParentOfCutFile;

    private (TreeViewCommandArgs treeViewCommandArgs, MenuRecord menuRecord) _previousGetMenuRecordInvocation;

    private MenuRecord GetMenuRecord(TreeViewCommandArgs commandArgs)
    {
        if (_previousGetMenuRecordInvocation.treeViewCommandArgs == commandArgs)
            return _previousGetMenuRecordInvocation.menuRecord;

        if (commandArgs.TreeViewContainer.SelectedNodeList.Count > 1)
            return GetMenuRecordManySelections(commandArgs);

        if (commandArgs.TreeViewContainer.ActiveNode is null)
        {
            var menuRecord = new MenuRecord(MenuRecord.NoMenuOptionsExistList);
            _previousGetMenuRecordInvocation = (commandArgs, menuRecord);
            return menuRecord;
        }

        var menuOptionList = new List<MenuOptionRecord>();
        var treeViewModel = commandArgs.TreeViewContainer.ActiveNode;
        var parentTreeViewModel = treeViewModel.Parent;
        var parentTreeViewNamespacePath = parentTreeViewModel as TreeViewNamespacePath;

        if (treeViewModel is TreeViewNamespacePath treeViewNamespacePath)
        {
            if (treeViewNamespacePath.Item.AbsolutePath.IsDirectory)
            {
                menuOptionList.AddRange(GetFileMenuOptions(treeViewNamespacePath, parentTreeViewNamespacePath)
                    .Union(GetDirectoryMenuOptions(treeViewNamespacePath))
                    .Union(GetDebugMenuOptions(treeViewNamespacePath)));
            }
            else
            {
                switch (treeViewNamespacePath.Item.AbsolutePath.ExtensionNoPeriod)
                {
                    case ExtensionNoPeriodFacts.C_SHARP_PROJECT:
                        menuOptionList.AddRange(GetCSharpProjectMenuOptions(treeViewNamespacePath)
                            .Union(GetDebugMenuOptions(treeViewNamespacePath)));
                        break;
                    default:
                        menuOptionList.AddRange(GetFileMenuOptions(treeViewNamespacePath, parentTreeViewNamespacePath)
                            .Union(GetDebugMenuOptions(treeViewNamespacePath)));
                        break;
                }
            }
        }
        else if (treeViewModel is TreeViewSolution treeViewSolution)
        {
            if (ExtensionNoPeriodFacts.DOT_NET_SOLUTION == treeViewSolution.Item.NamespacePath.AbsolutePath.ExtensionNoPeriod ||
                ExtensionNoPeriodFacts.DOT_NET_SOLUTION_X == treeViewSolution.Item.NamespacePath.AbsolutePath.ExtensionNoPeriod)
            {
                if (treeViewSolution.Parent is null || treeViewSolution.Parent is TreeViewAdhoc)
                    menuOptionList.AddRange(GetDotNetSolutionMenuOptions(treeViewSolution));
            }
        }
        else if (treeViewModel is TreeViewCSharpProjectToProjectReference treeViewCSharpProjectToProjectReference)
        {
            menuOptionList.AddRange(GetCSharpProjectToProjectReferenceMenuOptions(
                treeViewCSharpProjectToProjectReference));
        }
        else if (treeViewModel is TreeViewCSharpProjectNugetPackageReference treeViewCSharpProjectNugetPackageReference)
        {
            menuOptionList.AddRange(GetTreeViewLightWeightNugetPackageRecordMenuOptions(
                treeViewCSharpProjectNugetPackageReference));
        }

        if (!menuOptionList.Any())
        {
            var menuRecord = new MenuRecord(MenuRecord.NoMenuOptionsExistList);
            _previousGetMenuRecordInvocation = (commandArgs, menuRecord);
            return menuRecord;
        }

        // Default case
        {
            var menuRecord = new MenuRecord(menuOptionList);
            _previousGetMenuRecordInvocation = (commandArgs, menuRecord);
            return menuRecord;
        }
    }

    private MenuRecord GetMenuRecordManySelections(TreeViewCommandArgs commandArgs)
    {
        var menuOptionList = new List<MenuOptionRecord>();

        var getFileOptions = true;
        var filenameList = new List<string>();

        foreach (var selectedNode in commandArgs.TreeViewContainer.SelectedNodeList)
        {
            if (selectedNode is TreeViewNamespacePath treeViewNamespacePath)
            {
                if (treeViewNamespacePath.Item.AbsolutePath.ExtensionNoPeriod == ExtensionNoPeriodFacts.C_SHARP_PROJECT)
                    getFileOptions = false;
                else if (getFileOptions)
                    filenameList.Add(treeViewNamespacePath.Item.AbsolutePath.NameWithExtension + " __FROM__ " + (treeViewNamespacePath.Item.AbsolutePath.ParentDirectory ?? "null"));
            }
            else
            {
                getFileOptions = false;
            }
        }

        if (getFileOptions)
        {
            menuOptionList.Add(new MenuOptionRecord(
                "Delete",
                MenuOptionKind.Delete,
                widgetRendererType: typeof(Walk.Ide.RazorLib.FormsGenerics.Displays.BooleanPromptOrCancelDisplay),
                widgetParameterMap: new Dictionary<string, object?>
                {
                    { nameof(BooleanPromptOrCancelDisplay.IncludeCancelOption), false },
                    { nameof(BooleanPromptOrCancelDisplay.Message), $"DELETE:" },
                    { nameof(BooleanPromptOrCancelDisplay.ListOfMessages), filenameList },
                    { nameof(BooleanPromptOrCancelDisplay.AcceptOptionTextOverride), null },
                    { nameof(BooleanPromptOrCancelDisplay.DeclineOptionTextOverride), null },
                    {
                        nameof(BooleanPromptOrCancelDisplay.OnAfterAcceptFunc),
                        async () =>
                        {
                            await commandArgs.RestoreFocusToTreeView
                                .Invoke()
                                .ConfigureAwait(false);

                            DotNetService.Enqueue(new DotNetWorkArgs
                            {
                                WorkKind = DotNetWorkKind.SolutionExplorer_TreeView_MultiSelect_DeleteFiles,
                                TreeViewCommandArgs = commandArgs,
                            });
                        }
                    },
                    { nameof(BooleanPromptOrCancelDisplay.OnAfterDeclineFunc), commandArgs.RestoreFocusToTreeView },
                    { nameof(BooleanPromptOrCancelDisplay.OnAfterCancelFunc), commandArgs.RestoreFocusToTreeView },
                }));
        }

        if (!menuOptionList.Any())
        {
            var menuRecord = new MenuRecord(MenuRecord.NoMenuOptionsExistList);
            _previousGetMenuRecordInvocation = (commandArgs, menuRecord);
            return menuRecord;
        }

        // Default case
        {
            var menuRecord = new MenuRecord(menuOptionList);
            _previousGetMenuRecordInvocation = (commandArgs, menuRecord);
            return menuRecord;
        }
    }

    private MenuOptionRecord[] GetDotNetSolutionMenuOptions(TreeViewSolution treeViewSolution)
    {
        // TODO: Add menu options for non C# projects perhaps a more generic option is good

        var addNewCSharpProject = new MenuOptionRecord(
            "New C# Project",
            MenuOptionKind.Other,
            () => OpenNewCSharpProjectDialog(treeViewSolution.Item));

        var addExistingCSharpProject = new MenuOptionRecord(
            "Existing C# Project",
            MenuOptionKind.Other,
            () =>
            {
                AddExistingProjectToSolution(treeViewSolution.Item);
                return Task.CompletedTask;
            });

        var createOptions = new MenuOptionRecord("Add", MenuOptionKind.Create,
            subMenu: new MenuRecord(new List<MenuOptionRecord>
            {
                addNewCSharpProject,
                addExistingCSharpProject,
            }));

        var openInTextEditor = new MenuOptionRecord(
            "Open in text editor",
            MenuOptionKind.Update,
            () => OpenSolutionInTextEditor(treeViewSolution.Item));
            
        var properties = new MenuOptionRecord(
            "Properties",
            MenuOptionKind.Update,
            () => OpenSolutionProperties(treeViewSolution.Item));

        return new[]
        {
            createOptions,
            openInTextEditor,
            properties,
        };
    }

    private MenuOptionRecord[] GetCSharpProjectMenuOptions(TreeViewNamespacePath treeViewModel)
    {
        var parentDirectory = treeViewModel.Item.AbsolutePath.ParentDirectory;
        var treeViewSolution = treeViewModel.Parent as TreeViewSolution;

        if (treeViewSolution is null)
        {
            var ancestorTreeView = treeViewModel.Parent;

            if (ancestorTreeView?.Parent is null)
                return Array.Empty<MenuOptionRecord>();

            // Parent could be a could be one or many levels of solution folders
            while (ancestorTreeView.Parent is not null)
            {
                ancestorTreeView = ancestorTreeView.Parent;
            }

            treeViewSolution = ancestorTreeView as TreeViewSolution;

            if (treeViewSolution is null)
                return Array.Empty<MenuOptionRecord>();
        }

        var parentDirectoryAbsolutePath = DotNetService.IdeService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(parentDirectory, true);

        return new[]
        {
            DotNetService.IdeService.NewEmptyFile(
                parentDirectoryAbsolutePath,
                async () => await ReloadTreeViewModel(treeViewModel).ConfigureAwait(false)),
            DotNetService.IdeService.NewTemplatedFile(
                new NamespacePath(treeViewModel.Item.Namespace, parentDirectoryAbsolutePath),
                async () => await ReloadTreeViewModel(treeViewModel).ConfigureAwait(false)),
            DotNetService.IdeService.NewDirectory(
                parentDirectoryAbsolutePath,
                async () => await ReloadTreeViewModel(treeViewModel).ConfigureAwait(false)),
            DotNetService.IdeService.PasteClipboard(
                parentDirectoryAbsolutePath,
                async () =>
                {
                    var localParentOfCutFile = ParentOfCutFile;
                    ParentOfCutFile = null;

                    if (localParentOfCutFile is not null)
                        await ReloadTreeViewModel(localParentOfCutFile).ConfigureAwait(false);

                    await ReloadTreeViewModel(treeViewModel).ConfigureAwait(false);
                }),
            DotNetService.AddProjectToProjectReference(
                treeViewModel,
                DotNetService.IdeService.GetTerminalState().TerminalMap[IdeFacts.GENERAL_KEY],
                DotNetService.IdeService,
                () => Task.CompletedTask),
            DotNetService.MoveProjectToSolutionFolder(
                treeViewSolution,
                treeViewModel,
                DotNetService.IdeService.GetTerminalState().TerminalMap[IdeFacts.GENERAL_KEY],
                DotNetService.IdeService.TextEditorService.CommonService,
                () =>
                {
                    DotNetService.Enqueue(new DotNetWorkArgs
                    {
                        WorkKind = DotNetWorkKind.SetDotNetSolution,
                        DotNetSolutionAbsolutePath = treeViewSolution.Item.NamespacePath.AbsolutePath
                    });
                    return Task.CompletedTask;
                }),
            new MenuOptionRecord(
                "Set as Startup Project",
                MenuOptionKind.Other,
                () =>
                {
                    var startupControl = DotNetService.IdeService.GetIdeStartupControlState().StartupControlList.FirstOrDefault(
                        x => x.StartupProjectAbsolutePath.Value == treeViewModel.Item.AbsolutePath.Value);
                        
                    if (startupControl is null)
                        return Task.CompletedTask;
                    
                    DotNetService.IdeService.Ide_SetActiveStartupControlKey(startupControl.Key);
                    return Task.CompletedTask;
                }),
            DotNetService.RemoveCSharpProjectReferenceFromSolution(
                treeViewSolution,
                treeViewModel,
                DotNetService.IdeService.GetTerminalState().TerminalMap[IdeFacts.GENERAL_KEY],
                DotNetService.IdeService.TextEditorService.CommonService,
                () =>
                {
                    DotNetService.Enqueue(new DotNetWorkArgs
                    {
                        WorkKind = DotNetWorkKind.SetDotNetSolution,
                        DotNetSolutionAbsolutePath = treeViewSolution.Item.NamespacePath.AbsolutePath,
                    });
                    return Task.CompletedTask;
                }),
        };
    }

    private MenuOptionRecord[] GetCSharpProjectToProjectReferenceMenuOptions(
        TreeViewCSharpProjectToProjectReference treeViewCSharpProjectToProjectReference)
    {
        return new[]
        {
            DotNetService.RemoveProjectToProjectReference(
                treeViewCSharpProjectToProjectReference,
                DotNetService.IdeService.GetTerminalState().TerminalMap[IdeFacts.GENERAL_KEY],
                DotNetService.IdeService.TextEditorService.CommonService,
                () => Task.CompletedTask),
        };
    }

    private IReadOnlyList<MenuOptionRecord> GetTreeViewLightWeightNugetPackageRecordMenuOptions(
        TreeViewCSharpProjectNugetPackageReference treeViewCSharpProjectNugetPackageReference)
    {
        if (treeViewCSharpProjectNugetPackageReference.Parent
                is not TreeViewCSharpProjectNugetPackageReferences treeViewCSharpProjectNugetPackageReferences)
        {
            return MenuRecord.NoMenuOptionsExistList;
        }

        return new List<MenuOptionRecord>
        {
            DotNetService.RemoveNuGetPackageReferenceFromProject(
                treeViewCSharpProjectNugetPackageReferences.Item.CSharpProjectNamespacePath,
                treeViewCSharpProjectNugetPackageReference,
                DotNetService.IdeService.GetTerminalState().TerminalMap[IdeFacts.GENERAL_KEY],
                DotNetService.IdeService.TextEditorService.CommonService,
                () => Task.CompletedTask),
        };
    }

    private MenuOptionRecord[] GetDirectoryMenuOptions(TreeViewNamespacePath treeViewModel)
    {
        return new[]
        {
            DotNetService.IdeService.NewEmptyFile(
                treeViewModel.Item.AbsolutePath,
                async () => await ReloadTreeViewModel(treeViewModel).ConfigureAwait(false)),
            DotNetService.IdeService.NewTemplatedFile(
                treeViewModel.Item,
                async () => await ReloadTreeViewModel(treeViewModel).ConfigureAwait(false)),
            DotNetService.IdeService.NewDirectory(
                treeViewModel.Item.AbsolutePath,
                async () => await ReloadTreeViewModel(treeViewModel).ConfigureAwait(false)),
            DotNetService.IdeService.PasteClipboard(
                treeViewModel.Item.AbsolutePath,
                async () =>
                {
                    var localParentOfCutFile = ParentOfCutFile;
                    ParentOfCutFile = null;

                    if (localParentOfCutFile is not null)
                        await ReloadTreeViewModel(localParentOfCutFile).ConfigureAwait(false);

                    await ReloadTreeViewModel(treeViewModel).ConfigureAwait(false);
                }),
        };
    }

    private MenuOptionRecord[] GetFileMenuOptions(
        TreeViewNamespacePath treeViewModel,
        TreeViewNamespacePath? parentTreeViewModel)
    {
        return new[]
        {
            DotNetService.IdeService.CopyFile(
                treeViewModel.Item.AbsolutePath,
                (Func<Task>)(() => {
                    NotificationHelper.DispatchInformative("Copy Action", $"Copied: {treeViewModel.Item.AbsolutePath.NameWithExtension}", DotNetService.IdeService.TextEditorService.CommonService, TimeSpan.FromSeconds(7));
                    return Task.CompletedTask;
                })),
            DotNetService.IdeService.CutFile(
                treeViewModel.Item.AbsolutePath,
                (Func<Task>)(() => {
                    ParentOfCutFile = parentTreeViewModel;
                    NotificationHelper.DispatchInformative("Cut Action", $"Cut: {treeViewModel.Item.AbsolutePath.NameWithExtension}", DotNetService.IdeService.TextEditorService.CommonService, TimeSpan.FromSeconds(7));
                    return Task.CompletedTask;
                })),
            DotNetService.IdeService.DeleteFile(
                treeViewModel.Item.AbsolutePath,
                async () => await ReloadTreeViewModel(parentTreeViewModel).ConfigureAwait(false)),
            DotNetService.IdeService.RenameFile(
                treeViewModel.Item.AbsolutePath,
                DotNetService.IdeService.TextEditorService.CommonService,
                async ()  => await ReloadTreeViewModel(parentTreeViewModel).ConfigureAwait(false)),
        };
    }

    private MenuOptionRecord[] GetDebugMenuOptions(TreeViewNamespacePath treeViewModel)
    {
        return new MenuOptionRecord[]
        {
            // new MenuOptionRecord(
            //     $"namespace: {treeViewModel.Item.Namespace}",
            //     MenuOptionKind.Read)
        };
    }

    private Task OpenNewCSharpProjectDialog(DotNetSolutionModel dotNetSolutionModel)
    {
        var dialogRecord = new DialogViewModel(
            _newCSharpProjectDialogKey,
            "New C# Project",
            typeof(CSharpProjectFormDisplay),
            new Dictionary<string, object?>
            {
                {
                    nameof(CSharpProjectFormDisplay.DotNetSolutionModelKey),
                    dotNetSolutionModel.Key
                },
            },
            null,
            true,
            null);

        DotNetService.IdeService.TextEditorService.CommonService.Dialog_ReduceRegisterAction(dialogRecord);
        return Task.CompletedTask;
    }

    private void AddExistingProjectToSolution(DotNetSolutionModel dotNetSolutionModel)
    {
        DotNetService.IdeService.Enqueue(new IdeWorkArgs
        {
            WorkKind = IdeWorkKind.RequestInputFileStateForm,
            StringValue = "Existing C# Project to add to solution",
            OnAfterSubmitFunc = absolutePath =>
            {
                if (absolutePath.ExactInput is null)
                    return Task.CompletedTask;

                var localFormattedAddExistingProjectToSolutionCommand = DotNetCliCommandFormatter.FormatAddExistingProjectToSolution(
                    dotNetSolutionModel.NamespacePath.AbsolutePath.Value,
                    absolutePath.Value);

                var terminalCommandRequest = new TerminalCommandRequest(
                    localFormattedAddExistingProjectToSolutionCommand.Value,
                    null)
                {
                    ContinueWithFunc = parsedCommand =>
                    {
                        DotNetService.Enqueue(new DotNetWorkArgs
                        {
                            WorkKind = DotNetWorkKind.SetDotNetSolution,
                            DotNetSolutionAbsolutePath = dotNetSolutionModel.NamespacePath.AbsolutePath,
                        });
                        return Task.CompletedTask;
                    }
                };
                    
                DotNetService.IdeService.GetTerminalState().TerminalMap[IdeFacts.GENERAL_KEY].EnqueueCommand(terminalCommandRequest);
                return Task.CompletedTask;
            },
            SelectionIsValidFunc = absolutePath =>
            {
                if (absolutePath.ExactInput is null || absolutePath.IsDirectory)
                    return Task.FromResult(false);

                return Task.FromResult(absolutePath.ExtensionNoPeriod.EndsWith(ExtensionNoPeriodFacts.C_SHARP_PROJECT));
            },
            InputFilePatterns = new()
            {
                new InputFilePattern(
                    "C# Project",
                    absolutePath => absolutePath.ExtensionNoPeriod.EndsWith(ExtensionNoPeriodFacts.C_SHARP_PROJECT))
            }
        });
    }

    private Task OpenSolutionInTextEditor(DotNetSolutionModel dotNetSolutionModel)
    {
        DotNetService.IdeService.TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
        {
            await DotNetService.IdeService.TextEditorService.OpenInEditorAsync(
                editContext,
                dotNetSolutionModel.AbsolutePath.Value,
                true,
                null,
                new Category("main"),
                Key<TextEditorViewModel>.NewKey());
        });
        return Task.CompletedTask;
    }
    
    private Task OpenSolutionProperties(DotNetSolutionModel dotNetSolutionModel)
    {
        DotNetService.IdeService.TextEditorService.CommonService.Dialog_ReduceRegisterAction(new DialogViewModel(
            dynamicViewModelKey: _solutionPropertiesDialogKey,
            title: "Solution Properties",
            componentType: typeof(SolutionPropertiesDisplay),
            componentParameterMap: null,
            cssClass: null,
            isResizable: true,
            setFocusOnCloseElementId: null));
        return Task.CompletedTask;
    }

    /// <summary>
    /// This method I believe is causing bugs
    /// <br/><br/>
    /// For example, when removing a C# Project the
    /// solution is reloaded and a new root is made.
    /// <br/><br/>
    /// Then there is a timing issue where the new root is made and set
    /// as the root. But this method erroneously reloads the old root.
    /// </summary>
    /// <param name="treeViewModel"></param>
    private async Task ReloadTreeViewModel(TreeViewNoType? treeViewModel)
    {
        if (treeViewModel is null)
            return;

        await treeViewModel.LoadChildListAsync().ConfigureAwait(false);

        DotNetService.IdeService.TextEditorService.CommonService.TreeView_ReRenderNodeAction(DotNetSolutionState.TreeViewSolutionExplorerStateKey, treeViewModel);

        DotNetService.IdeService.TextEditorService.CommonService.TreeView_MoveUpAction(
            DotNetSolutionState.TreeViewSolutionExplorerStateKey,
            false,
            false);
    }
}
