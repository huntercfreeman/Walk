using System.Text;
using System.Collections.Concurrent;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.FolderExplorers.Models;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService : IBackgroundTaskGroup
{
    private readonly IServiceProvider _serviceProvider;

    public IdeService(
        TextEditorService textEditorService,
        IServiceProvider serviceProvider)
    {
        TextEditorService = textEditorService;
        _serviceProvider = serviceProvider;
        
        AddTerminals();
    }
    
    public Key<IBackgroundTaskGroup> BackgroundTaskKey { get; } = Key<IBackgroundTaskGroup>.NewKey();
    
    public TextEditorService TextEditorService { get; }
    public CommonService CommonService => TextEditorService.CommonService;

    public bool __TaskCompletionSourceWasCreated { get; set; }

    private readonly ConcurrentQueue<IdeWorkArgs> _workQueue = new();

    public void Enqueue(IdeWorkArgs workArgs)
    {
        _workQueue.Enqueue(workArgs);
        TextEditorService.CommonService.Continuous_Enqueue(this);
    }
    
    public ValueTask HandleEvent()
    {
        if (!_workQueue.TryDequeue(out IdeWorkArgs workArgs))
            return ValueTask.CompletedTask;

        switch (workArgs.WorkKind)
        {
            case IdeWorkKind.SetFolderExplorerState:
                return Do_SetFolderExplorerState(workArgs.AbsolutePath);
            case IdeWorkKind.SetFolderExplorerTreeView:
                return Do_SetFolderExplorerTreeView(workArgs.AbsolutePath);
            case IdeWorkKind.RequestInputFileStateForm:
                return Do_RequestInputFileStateForm(
                    workArgs.StringValue, workArgs.OnAfterSubmitFunc, workArgs.SelectionIsValidFunc, workArgs.InputFilePatterns);
            case IdeWorkKind.OpenParentDirectoryAction:
            {
                return InputFile_Do_OpenParentDirectoryAction(
                    CommonService, workArgs.TreeViewAbsolutePath);
            }
            case IdeWorkKind.RefreshCurrentSelectionAction:
            {
                return InputFile_Do_RefreshCurrentSelectionAction(workArgs.TreeViewAbsolutePath);
            }
            case IdeWorkKind.PerformNewFile:
                return Do_PerformNewFile(
                    workArgs.StringValue,
                    workArgs.ExactMatchFileTemplate,
                    workArgs.RelatedMatchFileTemplatesList,
                    workArgs.AbsolutePath,
                    workArgs.NamespaceString,
                    workArgs.OnAfterCompletion);
            case IdeWorkKind.PerformNewDirectory:
                return Do_PerformNewDirectory(
                    workArgs.StringValue,
                    workArgs.AbsolutePath,
                    workArgs.OnAfterCompletion);
            case IdeWorkKind.PerformDeleteFile:
                return Do_PerformDeleteFile(
                    workArgs.AbsolutePath,
                    workArgs.OnAfterCompletion);
            case IdeWorkKind.PerformCopyFile:
                return Do_PerformCopyFile(
                    workArgs.AbsolutePath,
                    workArgs.OnAfterCompletion);
            case IdeWorkKind.PerformCutFile:
                return Do_PerformCutFile(
                    workArgs.AbsolutePath,
                    workArgs.OnAfterCompletion);
            case IdeWorkKind.PerformPasteFile:
                return Do_PerformPasteFile(
                    workArgs.AbsolutePath,
                    workArgs.OnAfterCompletion);
            default:
                Console.WriteLine($"{nameof(IdeService)} {nameof(HandleEvent)} default case");
                return ValueTask.CompletedTask;
        }
    }

    public void Editor_ShowInputFile()
    {
        Enqueue(new IdeWorkArgs
        {
            WorkKind = IdeWorkKind.RequestInputFileStateForm,
            StringValue = "TextEditor",
            OnAfterSubmitFunc = absolutePath =>
            {
                // TODO: Why does 'isDirectory: false' not work?
                CommonService.EnvironmentProvider.DeletionPermittedRegister(new(absolutePath.Value, isDirectory: true), tokenBuilder: new StringBuilder(), formattedBuilder: new StringBuilder());
            
                TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
                {
                    await TextEditorService.OpenInEditorAsync(
                        editContext,
                        absolutePath.Value,
                        true,
                        null,
                        new Category("main"),
                        Key<TextEditorViewModel>.NewKey());
                });
                return Task.CompletedTask;
            },
            SelectionIsValidFunc = absolutePath =>
            {
                if (absolutePath.Value is null || absolutePath.IsDirectory)
                    return Task.FromResult(false);

                return Task.FromResult(true);
            },
            InputFilePatterns = new()
            {
                new InputFilePattern("File", absolutePath => !absolutePath.IsDirectory)
            }
        });
    }

    private ValueTask Do_SetFolderExplorerState(AbsolutePath folderAbsolutePath)
    {
        FolderExplorer_With(
            inFolderExplorerState => inFolderExplorerState with
            {
                AbsolutePath = folderAbsolutePath
            });

        return Do_SetFolderExplorerTreeView(folderAbsolutePath);
    }

    private async ValueTask Do_SetFolderExplorerTreeView(AbsolutePath folderAbsolutePath)
    {
        FolderExplorer_With(inFolderExplorerState => inFolderExplorerState with
        {
            IsLoadingFolderExplorer = true
        });
        
        CommonService.EnvironmentProvider.DeletionPermittedRegister(
            new(folderAbsolutePath.Value, true),
            tokenBuilder: new StringBuilder(),
            formattedBuilder: new StringBuilder());

        var rootNode = new TreeViewAbsolutePath(
            folderAbsolutePath,
            CommonService,
            true,
            true);

        await rootNode.LoadChildListAsync().ConfigureAwait(false);

        if (!CommonService.TryGetTreeViewContainer(
                FolderExplorerState.TreeViewContentStateKey,
                out var treeViewState))
        {
            CommonService.TreeView_RegisterContainerAction(new TreeViewContainer(
                FolderExplorerState.TreeViewContentStateKey,
                rootNode,
                new List<TreeViewNoType>() { rootNode }));
        }
        else
        {
            CommonService.TreeView_WithRootNodeAction(FolderExplorerState.TreeViewContentStateKey, rootNode);

            CommonService.TreeView_SetActiveNodeAction(
                FolderExplorerState.TreeViewContentStateKey,
                rootNode,
                true,
                false);
        }

        FolderExplorer_With(inFolderExplorerState => inFolderExplorerState with
        {
            IsLoadingFolderExplorer = false
        });
    }

    public void FolderExplorer_ShowInputFile()
    {
        Enqueue(new IdeWorkArgs
        {
            WorkKind = IdeWorkKind.RequestInputFileStateForm,
            StringValue = "Folder Explorer",
            OnAfterSubmitFunc = async absolutePath =>
            {
                if (absolutePath.Value is not null)
                    await Do_SetFolderExplorerState(absolutePath).ConfigureAwait(false);
            },
            SelectionIsValidFunc = absolutePath =>
            {
                if (absolutePath.Value is null || !absolutePath.IsDirectory)
                    return Task.FromResult(false);

                return Task.FromResult(true);
            },
            InputFilePatterns = [
                new InputFilePattern("Directory", absolutePath => absolutePath.IsDirectory)
            ]
        });
    }
    
    private ValueTask Do_RequestInputFileStateForm(
        string message,
        Func<AbsolutePath, Task> onAfterSubmitFunc,
        Func<AbsolutePath, Task<bool>> selectionIsValidFunc,
        List<InputFilePattern> inputFilePatternsList)
    {
        InputFile_StartInputFileStateForm(
            message,
            onAfterSubmitFunc,
            selectionIsValidFunc,
            inputFilePatternsList);

        var inputFileDialog = new DialogViewModel(
            CommonFacts.InputFileDialogKey,
            "Input File",
            typeof(Walk.Ide.RazorLib.InputFiles.Displays.InputFileDisplay),
            null,
            "di_ide_dialog-padding-0",
            true,
            null);

        CommonService.Dialog_ReduceRegisterAction(inputFileDialog);

        return ValueTask.CompletedTask;
    }
}
