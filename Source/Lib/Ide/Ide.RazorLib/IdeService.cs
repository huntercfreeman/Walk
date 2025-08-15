using System.Text;
using System.Collections.Concurrent;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Groups.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Ide.RazorLib.Installations.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.FolderExplorers.Models;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService : IBackgroundTaskGroup
{
    private readonly IServiceProvider _serviceProvider;

    public IdeService(
        WalkIdeConfig ideConfig,
        TextEditorService textEditorService,
        IServiceProvider serviceProvider)
    {
        IdeConfig = ideConfig;
        TextEditorService = textEditorService;
        _serviceProvider = serviceProvider;
        
        AddTerminals();
    }
    
    public Key<IBackgroundTaskGroup> BackgroundTaskKey { get; } = Key<IBackgroundTaskGroup>.NewKey();
    
    public WalkIdeConfig IdeConfig { get; }
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
            case IdeWorkKind.FileContentsWereModifiedOnDisk:
                return Editor_Do_FileContentsWereModifiedOnDisk(
                    workArgs.StringValue, workArgs.TextEditorModel, workArgs.FileLastWriteTime, workArgs.NotificationInformativeKey);
            case IdeWorkKind.SaveFile:
                return Do_SaveFile(workArgs.AbsolutePath, workArgs.StringValue, workArgs.OnAfterSaveCompletedWrittenDateTimeFunc, workArgs.CancellationToken);
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
                    workArgs.NamespacePath,
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

    public async Task Editor_FastParseFunc(FastParseArgs fastParseArgs)
    {
        /*var resourceUri = fastParseArgs.ResourceUri;

        var compilerService = _compilerServiceRegistry.GetCompilerService(fastParseArgs.ExtensionNoPeriod);

        compilerService.RegisterResource(
            fastParseArgs.ResourceUri,
            shouldTriggerResourceWasModified: false);
            
        var uniqueTextEditorWork = new UniqueTextEditorWork(TextEditorService, editContext =>
            compilerService.FastParseAsync(editContext, fastParseArgs.ResourceUri, _fileSystemProvider));
        
        TextEditorService.WorkerArbitrary.EnqueueUniqueTextEditorWork(uniqueTextEditorWork);*/
    }
    
    public async Task Editor_RegisterModelFunc(RegisterModelArgs registerModelArgs)
    {
        var model = TextEditorService.Model_GetOrDefault(registerModelArgs.ResourceUri);
        
        if (model is not null)
        {
            await Editor_CheckIfContentsWereModifiedAsync(
                    registerModelArgs.ResourceUri.Value,
                    model)
                .ConfigureAwait(false);
            return;
        }
            
        var resourceUri = registerModelArgs.ResourceUri;

        var fileLastWriteTime = await CommonService.FileSystemProvider.File
            .GetLastWriteTimeAsync(resourceUri.Value)
            .ConfigureAwait(false);

        var content = await CommonService.FileSystemProvider.File
            .ReadAllTextAsync(resourceUri.Value)
            .ConfigureAwait(false);

        var absolutePath = new AbsolutePath(resourceUri.Value, false, CommonService.EnvironmentProvider, tokenBuilder: new StringBuilder(), formattedBuilder: new StringBuilder(), shouldNameContainsExtension: true, nameShouldBeExtensionNoPeriod: true);
        var decorationMapper = TextEditorService.GetDecorationMapper(absolutePath.Name);
        var compilerService = TextEditorService.GetCompilerService(absolutePath.Name);

        model = new TextEditorModel(
            resourceUri,
            fileLastWriteTime,
            absolutePath.Name,
            content,
            decorationMapper,
            compilerService,
            TextEditorService);
            
        var modelModifier = new TextEditorModel(model);
        modelModifier.PerformRegisterPresentationModelAction(TextEditorFacts.CompilerServiceDiagnosticPresentation_EmptyPresentationModel);
        modelModifier.PerformRegisterPresentationModelAction(TextEditorFacts.FindOverlayPresentation_EmptyPresentationModel);
        
        model = modelModifier;

        TextEditorService.Model_RegisterCustom(registerModelArgs.EditContext, model);
        
        model.PersistentState.CompilerService.RegisterResource(
            model.PersistentState.ResourceUri,
            shouldTriggerResourceWasModified: false);
        
        modelModifier = registerModelArgs.EditContext.GetModelModifier(resourceUri);

        if (modelModifier is null)
            return;

        await compilerService.ParseAsync(registerModelArgs.EditContext, modelModifier, shouldApplySyntaxHighlighting: false);
    }

    public async Task<Key<TextEditorViewModel>> Editor_TryRegisterViewModelFunc(TryRegisterViewModelArgs registerViewModelArgs)
    {
        var viewModelKey = Key<TextEditorViewModel>.NewKey();
        
        var model = TextEditorService.Model_GetOrDefault(registerViewModelArgs.ResourceUri);

        if (model is null)
        {
            NotificationHelper.DispatchDebugMessage(nameof(Editor_TryRegisterViewModelFunc), () => "model is null: " + registerViewModelArgs.ResourceUri.Value, CommonService, TimeSpan.FromSeconds(4));
            return Key<TextEditorViewModel>.Empty;
        }

        var viewModel = TextEditorService.Model_GetViewModelsOrEmpty(registerViewModelArgs.ResourceUri)
            .FirstOrDefault(x => x.PersistentState.Category == registerViewModelArgs.Category);

        if (viewModel is not null)
            return viewModel.PersistentState.ViewModelKey;

        viewModel = new TextEditorViewModel(
            viewModelKey,
            registerViewModelArgs.ResourceUri,
            TextEditorService,
            TextEditorVirtualizationResult.ConstructEmpty(),
            new TextEditorDimensions(0, 0, 0, 0),
            scrollLeft: 0,
            scrollTop: 0,
            scrollWidth: 0,
            scrollHeight: 0,
            marginScrollHeight: 0,
            registerViewModelArgs.Category);

        var firstPresentationLayerKeys = new List<Key<TextEditorPresentationModel>>
        {
            TextEditorFacts.CompilerServiceDiagnosticPresentation_PresentationKey,
            TextEditorFacts.FindOverlayPresentation_PresentationKey,
        };

        var absolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(
            registerViewModelArgs.ResourceUri.Value,
            false,
            tokenBuilder: new StringBuilder(),
            formattedBuilder: new StringBuilder(),
            shouldNameContainsExtension: true);

        viewModel.PersistentState.OnSaveRequested = Editor_HandleOnSaveRequested;
        viewModel.PersistentState.GetTabDisplayNameFunc = _ => absolutePath.Name;
        viewModel.PersistentState.FirstPresentationLayerKeysList = firstPresentationLayerKeys;
        
        TextEditorService.ViewModel_Register(registerViewModelArgs.EditContext, viewModel);
        return viewModelKey;
    }
    
    private void Editor_HandleOnSaveRequested(TextEditorModel innerTextEditor)
    {
        var innerContent = innerTextEditor.GetAllText_WithOriginalLineEndings();
        
        var absolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(
            innerTextEditor.PersistentState.ResourceUri.Value,
            false,
            tokenBuilder: new StringBuilder(),
            formattedBuilder: new StringBuilder(),
            shouldNameContainsExtension: true);

        Enqueue(new IdeWorkArgs
        {
            WorkKind = IdeWorkKind.SaveFile,
            AbsolutePath = absolutePath,
            StringValue = innerContent,
            OnAfterSaveCompletedWrittenDateTimeFunc = writtenDateTime =>
            {
                if (writtenDateTime is not null)
                {
                    TextEditorService.WorkerArbitrary.PostUnique(editContext =>
                    {
                        var modelModifier = editContext.GetModelModifier(innerTextEditor.PersistentState.ResourceUri);
                        if (modelModifier is null)
                            return ValueTask.CompletedTask;
                    
                        TextEditorService.Model_SetResourceData(
                            editContext,
                            modelModifier,
                            writtenDateTime.Value);
                        return ValueTask.CompletedTask;
                    });
                }

                return Task.CompletedTask;
            },
            CancellationToken = CancellationToken.None
        });
    }

    public async Task<bool> Editor_TryShowViewModelFunc(TryShowViewModelArgs showViewModelArgs)
    {
        var viewModel = TextEditorService.ViewModel_GetOrDefault(showViewModelArgs.ViewModelKey);

        if (viewModel is null)
            return false;

        if (viewModel.PersistentState.Category == new Category("main") &&
            showViewModelArgs.GroupKey == Key<TextEditorGroup>.Empty)
        {
            showViewModelArgs = new TryShowViewModelArgs(
                showViewModelArgs.ViewModelKey,
                TextEditorService.EditorTextEditorGroupKey,
                showViewModelArgs.ShouldSetFocusToEditor,
                showViewModelArgs.CommonService,
                showViewModelArgs.IdeBackgroundTaskApi);
        }

        if (showViewModelArgs.ViewModelKey == Key<TextEditorViewModel>.Empty ||
            showViewModelArgs.GroupKey == Key<TextEditorGroup>.Empty)
        {
            return false;
        }

        TextEditorService.Group_AddViewModel(
            showViewModelArgs.GroupKey,
            showViewModelArgs.ViewModelKey);

        TextEditorService.Group_SetActiveViewModel(
            showViewModelArgs.GroupKey,
            showViewModelArgs.ViewModelKey);
            
        if (showViewModelArgs.ShouldSetFocusToEditor)
        {
            TextEditorService.WorkerArbitrary.PostUnique(editContext =>
            {
                var viewModelModifier = editContext.GetViewModelModifier(showViewModelArgs.ViewModelKey);
                return viewModel.FocusAsync();
            });
        }

        return true;
    }

    private async Task Editor_CheckIfContentsWereModifiedAsync(
        string inputFileAbsolutePathString,
        TextEditorModel textEditorModel)
    {
        var fileLastWriteTime = await CommonService.FileSystemProvider.File
            .GetLastWriteTimeAsync(inputFileAbsolutePathString)
            .ConfigureAwait(false);

        if (fileLastWriteTime > textEditorModel.ResourceLastWriteTime)
        {
            var notificationInformativeKey = Key<IDynamicViewModel>.NewKey();

            var notificationInformative = new NotificationViewModel(
                notificationInformativeKey,
                "File contents were modified on disk",
                typeof(Walk.Ide.RazorLib.FormsGenerics.Displays.BooleanPromptOrCancelDisplay),
                new Dictionary<string, object?>
                {
                        {
                            nameof(Walk.Ide.RazorLib.FormsGenerics.Displays.BooleanPromptOrCancelDisplay.Message),
                            "File contents were modified on disk"
                        },
                        {
                            nameof(Walk.Ide.RazorLib.FormsGenerics.Displays.BooleanPromptOrCancelDisplay.AcceptOptionTextOverride),
                            "Reload"
                        },
                        {
                            nameof(Walk.Ide.RazorLib.FormsGenerics.Displays.BooleanPromptOrCancelDisplay.OnAfterAcceptFunc),
                            new Func<Task>(() =>
                            {
                                Enqueue(new IdeWorkArgs
                                {
                                    WorkKind = IdeWorkKind.FileContentsWereModifiedOnDisk,
                                    StringValue = inputFileAbsolutePathString,
                                    TextEditorModel = textEditorModel,
                                    FileLastWriteTime = fileLastWriteTime,
                                    NotificationInformativeKey = notificationInformativeKey,
                                });

                                return Task.CompletedTask;
                            })
                        },
                        {
                            nameof(Walk.Ide.RazorLib.FormsGenerics.Displays.BooleanPromptOrCancelDisplay.OnAfterDeclineFunc),
                            new Func<Task>(() =>
                            {
                                CommonService.Notification_ReduceDisposeAction(notificationInformativeKey);
                                return Task.CompletedTask;
                            })
                        },
                },
                TimeSpan.FromSeconds(20),
                true,
                null);

            CommonService.Notification_ReduceRegisterAction(notificationInformative);
        }
    }

    private async ValueTask Editor_Do_FileContentsWereModifiedOnDisk(string inputFileAbsolutePathString, TextEditorModel textEditorModel, DateTime fileLastWriteTime, Key<IDynamicViewModel> notificationInformativeKey)
    {
        CommonService.Notification_ReduceDisposeAction(notificationInformativeKey);

        var content = await CommonService.FileSystemProvider.File
            .ReadAllTextAsync(inputFileAbsolutePathString)
            .ConfigureAwait(false);

        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(textEditorModel.PersistentState.ResourceUri);
            if (modelModifier is null)
                return ValueTask.CompletedTask;

            TextEditorService.Model_Reload(
                editContext,
                modelModifier,
                content,
                fileLastWriteTime);

            if (modelModifier.PersistentState.CompilerService is not null)    
                modelModifier.PersistentState.CompilerService.ResourceWasModified(modelModifier.PersistentState.ResourceUri, Array.Empty<TextEditorTextSpan>());
            return ValueTask.CompletedTask;
        });
    }
    
    private async ValueTask Do_SaveFile(
        AbsolutePath absolutePath,
        string content,
        Func<DateTime?, Task> onAfterSaveCompletedWrittenDateTimeFunc,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        var absolutePathString = absolutePath.Value;

        if (absolutePathString is not null &&
            await CommonService.FileSystemProvider.File.ExistsAsync(absolutePathString).ConfigureAwait(false))
        {
            await CommonService.FileSystemProvider.File.WriteAllTextAsync(absolutePathString, content).ConfigureAwait(false);
        }
        else
        {
            // TODO: Save As to make new file
            NotificationHelper.DispatchInformative("Save Action", "File not found. TODO: Save As", CommonService, TimeSpan.FromSeconds(7));
        }

        DateTime? fileLastWriteTime = null;

        if (absolutePathString is not null)
        {
            fileLastWriteTime = await CommonService.FileSystemProvider.File.GetLastWriteTimeAsync(
                    absolutePathString,
                    CancellationToken.None)
                .ConfigureAwait(false);
        }

        if (onAfterSaveCompletedWrittenDateTimeFunc is not null)
            await onAfterSaveCompletedWrittenDateTimeFunc.Invoke(fileLastWriteTime);
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
            Walk.Ide.RazorLib.Htmls.Models.HtmlFacts.Classes.DIALOG_PADDING_0,
            true,
            null);

        CommonService.Dialog_ReduceRegisterAction(inputFileDialog);

        return ValueTask.CompletedTask;
    }
}
