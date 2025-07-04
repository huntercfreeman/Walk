using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.Ide.RazorLib.CodeSearches.Models;

public class CodeSearchService : ICodeSearchService
{
    private readonly object _stateModificationLock = new();

	private readonly Throttle _throttle = new(TimeSpan.FromMilliseconds(300));
    private readonly CommonUtilityService _commonUtilityService;
    private readonly TextEditorService _textEditorService;
    private readonly WalkTextEditorConfig _textEditorConfig;
    private readonly IServiceProvider _serviceProvider;
    
    // Moving things from 'CodeSearchDisplay.razor.cs'
    private Key<TextEditorViewModel> _previousTextEditorViewModelKey = Key<TextEditorViewModel>.Empty;
	public Throttle _updateContentThrottle { get; } = new Throttle(TimeSpan.FromMilliseconds(333));

    public CodeSearchService(
        CommonUtilityService commonUtilityService,
        TextEditorService textEditorService,
        WalkTextEditorConfig textEditorConfig,
        IServiceProvider serviceProvider)
    {
        _commonUtilityService = commonUtilityService;
        _textEditorService = textEditorService;
        _textEditorConfig = textEditorConfig;
        _serviceProvider = serviceProvider;
    }
    
    private CodeSearchState _codeSearchState = new();
    
    public event Action? CodeSearchStateChanged;
    
    public CodeSearchState GetCodeSearchState() => _codeSearchState;
    
    public void With(Func<CodeSearchState, CodeSearchState> withFunc)
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

        CodeSearchStateChanged?.Invoke();
    }

    public void AddResult(string result)
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

        CodeSearchStateChanged?.Invoke();
    }

    public void ClearResultList()
    {
        lock (_stateModificationLock)
        {
            _codeSearchState = _codeSearchState with
            {
                ResultList = new List<string>()
            };
        }

        CodeSearchStateChanged?.Invoke();
    }
    
    public void InitializeResizeHandleDimensionUnit(DimensionUnit dimensionUnit)
    {
        lock (_stateModificationLock)
        {
            if (dimensionUnit.Purpose == DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_ROW)
            {
                if (_codeSearchState.TopContentElementDimensions.HeightDimensionAttribute.DimensionUnitList is not null)
                {
                    var existingDimensionUnit = _codeSearchState.TopContentElementDimensions.HeightDimensionAttribute.DimensionUnitList
                        .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
    
                    if (existingDimensionUnit.Purpose is null)
                        _codeSearchState.TopContentElementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                }
    
                if (_codeSearchState.BottomContentElementDimensions.HeightDimensionAttribute.DimensionUnitList is not null)
                {
                    var existingDimensionUnit = _codeSearchState.BottomContentElementDimensions.HeightDimensionAttribute.DimensionUnitList
                        .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
                    
                    if (existingDimensionUnit.Purpose is null)
                        _codeSearchState.BottomContentElementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                }
            }
        }

        CodeSearchStateChanged?.Invoke();
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
    public Task HandleSearchEffect(CancellationToken cancellationToken = default)
    {
        _throttle.Run(async _ =>
        {
            ClearResultList();

            var codeSearchState = GetCodeSearchState();
            ConstructTreeView(codeSearchState);

            var startingAbsolutePathForSearch = codeSearchState.StartingAbsolutePathForSearch;

            if (string.IsNullOrWhiteSpace(startingAbsolutePathForSearch) ||
            	string.IsNullOrWhiteSpace(codeSearchState.Query))
            {
                return;
            }

            await RecursiveHandleSearchEffect(startingAbsolutePathForSearch).ConfigureAwait(false);
            
            ConstructTreeView(GetCodeSearchState());

            async Task RecursiveHandleSearchEffect(string directoryPathParent)
            {
                var directoryPathChildList = await _commonUtilityService.FileSystemProvider.Directory.GetDirectoriesAsync(
                        directoryPathParent,
                        cancellationToken)
                    .ConfigureAwait(false);

                var filePathChildList = await _commonUtilityService.FileSystemProvider.Directory.GetFilesAsync(
                        directoryPathParent,
                        cancellationToken)
                    .ConfigureAwait(false);

                foreach (var filePathChild in filePathChildList)
                {
                	var absolutePath = _commonUtilityService.EnvironmentProvider.AbsolutePathFactory(filePathChild, false);
                
                    if (absolutePath.NameWithExtension.Contains(codeSearchState.Query))
                        AddResult(filePathChild);
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
    
    private void ConstructTreeView(CodeSearchState codeSearchState)
	{
	    var treeViewList = codeSearchState.ResultList.Select(
	    	x => (TreeViewNoType)new TreeViewCodeSearchTextSpan(
		        new TextEditorTextSpan(
		        	0,
			        0,
			        (byte)GenericDecorationKind.None,
			        new ResourceUri(x),
			        string.Empty),
				_commonUtilityService.EnvironmentProvider,
				_commonUtilityService.FileSystemProvider,
				false,
				false))
			.ToArray();
	
	    var adhocRoot = TreeViewAdhoc.ConstructTreeViewAdhoc(treeViewList);
	    var firstNode = treeViewList.FirstOrDefault();
	
	    IReadOnlyList<TreeViewNoType> activeNodes = firstNode is null
	        ? Array.Empty<TreeViewNoType>()
	        : new List<TreeViewNoType> { firstNode };
	
	    if (!_commonUtilityService.TryGetTreeViewContainer(CodeSearchState.TreeViewCodeSearchContainerKey, out _))
	    {
	        _commonUtilityService.TreeView_RegisterContainerAction(new TreeViewContainer(
	            CodeSearchState.TreeViewCodeSearchContainerKey,
	            adhocRoot,
	            activeNodes));
	    }
	    else
	    {
	        _commonUtilityService.TreeView_WithRootNodeAction(CodeSearchState.TreeViewCodeSearchContainerKey, adhocRoot);
	
	        _commonUtilityService.TreeView_SetActiveNodeAction(
	            CodeSearchState.TreeViewCodeSearchContainerKey,
	            firstNode,
	            true,
	            false);
	    }
	}
	
	public async Task UpdateContent(ResourceUri providedResourceUri)
	{
		_textEditorService.WorkerArbitrary.PostUnique(async editContext =>
		{
			Console.WriteLine(nameof(UpdateContent));
		
			if (!_commonUtilityService.TryGetTreeViewContainer(
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
	
			var filePath = treeViewCodeSearchTextSpan.Item.ResourceUri.Value;
			var resourceUri = treeViewCodeSearchTextSpan.Item.ResourceUri;
			
			if (providedResourceUri != ResourceUri.Empty)
				resourceUri = providedResourceUri;
	
	        if (_textEditorConfig.RegisterModelFunc is null)
	            return;
	
	        await _textEditorConfig.RegisterModelFunc.Invoke(
	                new RegisterModelArgs(editContext, resourceUri, _commonUtilityService, _textEditorService.IdeBackgroundTaskApi))
	            .ConfigureAwait(false);
	
	        if (_textEditorConfig.TryRegisterViewModelFunc is not null)
	        {
	            var viewModelKey = await _textEditorConfig.TryRegisterViewModelFunc.Invoke(new TryRegisterViewModelArgs(
	            		editContext,
	                    outPreviewViewModelKey,
	                    resourceUri,
	                    new Category(nameof(CodeSearchService)),
	                    false,
	                    _commonUtilityService,
	                    _textEditorService.IdeBackgroundTaskApi))
	                .ConfigureAwait(false);
	
	            if (viewModelKey != Key<TextEditorViewModel>.Empty &&
	                _textEditorConfig.TryShowViewModelFunc is not null)
	            {
	                With(inState => inState with
	                {
	                    PreviewFilePath = filePath,
	                    PreviewViewModelKey = viewModelKey,
	                });
	
	                if (inPreviewViewModelKey != Key<TextEditorViewModel>.Empty &&
	                    inPreviewViewModelKey != viewModelKey)
					{
						_textEditorService.ViewModelApi.Dispose(editContext, inPreviewViewModelKey);
					}
	            }
	        }
		});
    }
}
