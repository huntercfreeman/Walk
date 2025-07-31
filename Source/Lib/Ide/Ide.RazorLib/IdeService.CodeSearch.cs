using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Ide.RazorLib.CodeSearches.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService
{
    private readonly Throttle CodeSearch_throttle = new(TimeSpan.FromMilliseconds(300));

    // Moving things from 'CodeSearchDisplay.razor.cs'
    private Key<TextEditorViewModel> CodeSearch_previousTextEditorViewModelKey = Key<TextEditorViewModel>.Empty;
    public Throttle CodeSearch_updateContentThrottle { get; } = new Throttle(TimeSpan.FromMilliseconds(333));

    private CodeSearchState _codeSearchState = new();

    public CodeSearchState GetCodeSearchState() => _codeSearchState;

    public void CodeSearch_With(Func<CodeSearchState, CodeSearchState> withFunc)
    {
        lock (_stateModificationLock)
        {
            var outState = withFunc.Invoke(_codeSearchState);

            if (outState.Query.StartsWith("f:"))
            {
                outState = outState with
                {
                    CodeSearchFilterKind = CodeSearchFilterKind.Files
                };
            }
            else if (outState.Query.StartsWith("t:"))
            {
                outState = outState with
                {
                    CodeSearchFilterKind = CodeSearchFilterKind.Types
                };
            }
            else if (outState.Query.StartsWith("m:"))
            {
                outState = outState with
                {
                    CodeSearchFilterKind = CodeSearchFilterKind.Members
                };
            }
            else
            {
                outState = outState with
                {
                    CodeSearchFilterKind = CodeSearchFilterKind.None
                };
            }

            _codeSearchState = outState;
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.CodeSearchStateChanged);
    }

    public void CodeSearch_AddResult(string result)
    {
        lock (_stateModificationLock)
        {
            var outResultList = new List<string>(_codeSearchState.ResultList);
            outResultList.Add(result);

            _codeSearchState = _codeSearchState with
            {
                ResultList = outResultList
            };
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.CodeSearchStateChanged);
    }

    public void CodeSearch_ClearResultList()
    {
        lock (_stateModificationLock)
        {
            _codeSearchState = _codeSearchState with
            {
                ResultList = new List<string>()
            };
        }

        IdeStateChanged?.Invoke(IdeStateChangedKind.CodeSearchStateChanged);
    }

    /// <summary>
    /// TODO: This method makes use of <see cref="IThrottle"/> and yet is accessing...
    ///       ...searchEffect.CancellationToken.
    ///       The issue here is that the search effect parameter to this method
    ///       could be out of date by the time that the throttle delay is completed.
    ///       This should be fixed. (2024-05-02)
    /// </summary>
    /// <param name="searchEffect"></param>
    /// <param name="dispatcher"></param>
    /// <returns></returns>
    public Task CodeSearch_HandleSearchEffect(CancellationToken cancellationToken = default)
    {
        CodeSearch_throttle.Run(async _ =>
        {
            CodeSearch_ClearResultList();

            var codeSearchState = GetCodeSearchState();
            CodeSearch_ConstructTreeView(codeSearchState);

            var startingAbsolutePathForSearch = codeSearchState.StartingAbsolutePathForSearch;

            if (string.IsNullOrWhiteSpace(startingAbsolutePathForSearch) ||
                string.IsNullOrWhiteSpace(codeSearchState.Query))
            {
                return;
            }

            await RecursiveHandleSearchEffect(startingAbsolutePathForSearch).ConfigureAwait(false);

            CodeSearch_ConstructTreeView(GetCodeSearchState());

            async Task RecursiveHandleSearchEffect(string directoryPathParent)
            {
                var directoryPathChildList = await CommonService.FileSystemProvider.Directory.GetDirectoriesAsync(
                        directoryPathParent,
                        cancellationToken)
                    .ConfigureAwait(false);

                var filePathChildList = await CommonService.FileSystemProvider.Directory.GetFilesAsync(
                        directoryPathParent,
                        cancellationToken)
                    .ConfigureAwait(false);

                foreach (var filePathChild in filePathChildList)
                {
                    var absolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(filePathChild, false);

                    if (absolutePath.NameWithExtension.Contains(codeSearchState.Query))
                        CodeSearch_AddResult(filePathChild);
                }

                foreach (var directoryPathChild in directoryPathChildList)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    if (IFileSystemProvider.IsDirectoryIgnored(directoryPathChild))
                        continue;

                    await RecursiveHandleSearchEffect(directoryPathChild).ConfigureAwait(false);
                }
            }
        });

        return Task.CompletedTask;
    }

    private void CodeSearch_ConstructTreeView(CodeSearchState codeSearchState)
    {
        var treeViewList = codeSearchState.ResultList.Select(
            x => (TreeViewNoType)new TreeViewCodeSearchTextSpan(
                new TextEditorTextSpan(
                    0,
                    0,
                    (byte)GenericDecorationKind.None),
                new AbsolutePath(x, false, CommonService.EnvironmentProvider),
                CommonService.EnvironmentProvider,
                CommonService.FileSystemProvider,
                false,
                false))
            .ToArray();

        var adhocRoot = TreeViewAdhoc.ConstructTreeViewAdhoc(treeViewList);
        var firstNode = treeViewList.FirstOrDefault();

        IReadOnlyList<TreeViewNoType> activeNodes = firstNode is null
            ? Array.Empty<TreeViewNoType>()
            : new List<TreeViewNoType> { firstNode };

        if (!CommonService.TryGetTreeViewContainer(CodeSearchState.TreeViewCodeSearchContainerKey, out _))
        {
            CommonService.TreeView_RegisterContainerAction(new TreeViewContainer(
                CodeSearchState.TreeViewCodeSearchContainerKey,
                adhocRoot,
                activeNodes));
        }
        else
        {
            CommonService.TreeView_WithRootNodeAction(CodeSearchState.TreeViewCodeSearchContainerKey, adhocRoot);

            CommonService.TreeView_SetActiveNodeAction(
                CodeSearchState.TreeViewCodeSearchContainerKey,
                firstNode,
                true,
                false);
        }
    }

    public async Task CodeSearch_UpdateContent(ResourceUri providedResourceUri)
    {
        TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
        {
            Console.WriteLine(nameof(CodeSearch_UpdateContent));

            if (!CommonService.TryGetTreeViewContainer(
                    CodeSearchState.TreeViewCodeSearchContainerKey,
                    out var treeViewContainer))
            {
                Console.WriteLine("TryGetTreeViewContainer");
                return;
            }

            if (treeViewContainer.SelectedNodeList.Count > 1)
            {
                Console.WriteLine("treeViewContainer.SelectedNodeList.Count > 1");
                return;
            }

            var activeNode = treeViewContainer.ActiveNode;

            if (activeNode is not TreeViewCodeSearchTextSpan treeViewCodeSearchTextSpan)
            {
                Console.WriteLine("activeNode is not TreeViewCodeSearchTextSpan treeViewCodeSearchTextSpan");
                return;
            }

            var inPreviewViewModelKey = GetCodeSearchState().PreviewViewModelKey;
            var outPreviewViewModelKey = Key<TextEditorViewModel>.NewKey();

            var filePath = treeViewCodeSearchTextSpan.AbsolutePath.Value;
            var resourceUri = new ResourceUri(treeViewCodeSearchTextSpan.AbsolutePath.Value);

            if (TextEditorService.TextEditorConfig.RegisterModelFunc is null)
                return;

            await TextEditorService.TextEditorConfig.RegisterModelFunc.Invoke(
                    new RegisterModelArgs(editContext, resourceUri, CommonService, this))
                .ConfigureAwait(false);

            if (TextEditorService.TextEditorConfig.TryRegisterViewModelFunc is not null)
            {
                var viewModelKey = await TextEditorService.TextEditorConfig.TryRegisterViewModelFunc.Invoke(new TryRegisterViewModelArgs(
                        editContext,
                        outPreviewViewModelKey,
                        resourceUri,
                        new Category(nameof(IdeService)),
                        false,
                        CommonService,
                        this))
                    .ConfigureAwait(false);

                if (viewModelKey != Key<TextEditorViewModel>.Empty &&
                    TextEditorService.TextEditorConfig.TryShowViewModelFunc is not null)
                {
                    CodeSearch_With(inState => inState with
                    {
                        PreviewFilePath = filePath,
                        PreviewViewModelKey = viewModelKey,
                    });

                    if (inPreviewViewModelKey != Key<TextEditorViewModel>.Empty &&
                        inPreviewViewModelKey != viewModelKey)
                    {
                        TextEditorService.ViewModel_Dispose(editContext, inPreviewViewModelKey);
                    }
                }
            }
        });
    }
}
