using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.TextEditor.RazorLib.FindAlls.Models;

public class FindAllService : IFindAllService
{
    private readonly object _stateModificationLock = new();

	private readonly ICommonUtilityService _commonUtilityService;
	private readonly Throttle _throttleSetSearchQuery = new Throttle(TimeSpan.FromMilliseconds(500));
	private readonly Throttle _throttleUiUpdate = new Throttle(ThrottleFacts.TwentyFour_Frames_Per_Second);
	
	private readonly object _flushSearchResultsLock = new();
	
	public FindAllService(ICommonUtilityService commonUtilityService)
	{
		_commonUtilityService = commonUtilityService;
	}
	
    /// <summary>
    /// Each instance of the state will share this cancellation token source because the 'with' keyword
    /// will copy any private members too, and <see cref="CancellationTokenSource"/> is a reference type.
    /// </summary>
    private CancellationTokenSource _searchCancellationTokenSource = new();
	
	private TextEditorFindAllState _findAllState = new();
	
	public event Action? FindAllStateChanged;
	
	public TextEditorFindAllState GetFindAllState() => _findAllState;
    
    public void SetSearchQuery(string searchQuery)
    {
		lock (_stateModificationLock)
		{
			_findAllState = _findAllState with
			{
				SearchQuery = searchQuery
			};
        }

        FindAllStateChanged?.Invoke();
    }

    public void SetStartingDirectoryPath(string startingDirectoryPath)
    {
		lock (_stateModificationLock)
		{
			_findAllState = _findAllState with
			{
				StartingDirectoryPath = startingDirectoryPath
			};
        }

        FindAllStateChanged?.Invoke();
    }

    public void CancelSearch()
    {
		lock (_stateModificationLock)
		{
			_searchCancellationTokenSource.Cancel();
			_searchCancellationTokenSource = new();

			_findAllState = _findAllState with { };
        }

        FindAllStateChanged?.Invoke();
    }

    public void SetProgressBarModel(ProgressBarModel progressBarModel)
    {
		lock (_stateModificationLock)
		{
			_findAllState = _findAllState with
			{
				ProgressBarModel = progressBarModel,
			};
        }

        FindAllStateChanged?.Invoke();
    }

    public void FlushSearchResults(List<(string SourceText, TextEditorTextSpan TextSpan)> searchResultList)
    {
		lock (_stateModificationLock)
		{
			var inState = GetFindAllState();

			List<(string SourceText, TextEditorTextSpan TextSpan)> localSearchResultList;
			lock (_flushSearchResultsLock)
			{
				localSearchResultList = new List<(string SourceText, TextEditorTextSpan TextSpan)>(inState.SearchResultList);
				localSearchResultList.AddRange(searchResultList);
				searchResultList.Clear();
			}

			_findAllState = inState with
			{
				SearchResultList = localSearchResultList
			};
        }

        FindAllStateChanged?.Invoke();
    }

    public void ClearSearch()
    {
		lock (_stateModificationLock)
		{
			var inState = GetFindAllState();

			_findAllState = inState with
			{
				SearchResultList = new()
			};
        }

        FindAllStateChanged?.Invoke();
    }
	
	public Task HandleStartSearchAction()
	{
    	_throttleSetSearchQuery.Run(async _ =>
		{
			CancelSearch();
			ClearSearch();
			
			var textEditorFindAllState = GetFindAllState();

			if (string.IsNullOrWhiteSpace(textEditorFindAllState.StartingDirectoryPath) ||
				string.IsNullOrWhiteSpace(textEditorFindAllState.SearchQuery))
			{
				return;
			}
			
			var cancellationToken = _searchCancellationTokenSource.Token;
			var progressBarModel = new ProgressBarModel();

			ConstructTreeView(textEditorFindAllState);

			SetProgressBarModel(progressBarModel);
			
			try
			{
				await StartSearchTask(
					progressBarModel,
					_commonUtilityService.FileSystemProvider,
					textEditorFindAllState,
					cancellationToken);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		});

		return Task.CompletedTask;
	}
	
	private async Task StartSearchTask(
		ProgressBarModel progressBarModel,
		IFileSystemProvider fileSystemProvider,
		TextEditorFindAllState textEditorFindAllState,
		CancellationToken cancellationToken)
	{
		var filesProcessedCount = 0;
		var textSpanList = new List<(string SourceText, TextEditorTextSpan TextSpan)>();
		var searchException = (Exception?)null;
		
		try
		{
			ShowFilesProcessedCountOnUi(0);
			await RecursiveSearch(textEditorFindAllState.StartingDirectoryPath);
		}
		catch (Exception e)
		{
			searchException = e;
		}
		finally
		{
			FlushSearchResults(textSpanList);
		
			if (searchException is null)
			{
				ShowFilesProcessedCountOnUi(1, true);
			}
			else
			{
				progressBarModel.SetProgress(
					progressBarModel.DecimalPercentProgress,
					searchException.ToString());
					
				progressBarModel.Dispose();
				// The use of '_textEditorFindAllStateWrap.Value' is purposeful.
				ConstructTreeView(GetFindAllState());
				SetProgressBarModel(progressBarModel);
			}
		}
		
		async Task RecursiveSearch(string directoryPath)
		{
			cancellationToken.ThrowIfCancellationRequested();
			
			// Considering the use a breadth first algorithm

			// Search Files
			{
				var childFileList = await fileSystemProvider.Directory
					.GetFilesAsync(directoryPath)
					.ConfigureAwait(false);
	
				foreach (var childFile in childFileList)
				{
					// TODO: Don't hardcode file extensions here to avoid searching through them.
					//       Reason being, hardcoding them isn't going to work well as a long term solution.
					//       How does one detect if a file is not text?
					//       |
					//       I seem to get away with opening some non-text files, but I think a gif I opened
					//       had 1 million characters in it? So this takes 2 million bytes in a 2byte char?
					//       I'm not sure exactly what happened, I opened the gif and the app froze,
					//       I saw the character only at a glance. (2024-07-20)
					if (!childFile.EndsWith(".jpg") &&
						!childFile.EndsWith(".png") &&
						!childFile.EndsWith(".pdf") &&
						!childFile.EndsWith(".gif"))
					{
						await PerformSearchFile(childFile).ConfigureAwait(false);
					}
					
					filesProcessedCount++;
					ShowFilesProcessedCountOnUi(0);
				}
			}
	
			// Recurse into subdirectories
			{
				var subdirectoryList = await fileSystemProvider.Directory
					.GetDirectoriesAsync(directoryPath)
					.ConfigureAwait(false);
	
				foreach (var subdirectory in subdirectoryList)
				{
					if (IFileSystemProvider.IsDirectoryIgnored(subdirectory))
						continue;
	
					await RecursiveSearch(subdirectory).ConfigureAwait(false);
				}
			}
		}
		
		async Task PerformSearchFile(string filePath)
		{
			var text = await fileSystemProvider.File.ReadAllTextAsync(filePath).ConfigureAwait(false);
			var query = textEditorFindAllState.SearchQuery;
				
	        var matchedTextSpanList = new List<(string SourceText, TextEditorTextSpan TextSpan)>();
	
	        for (int outerI = 0; outerI < text.Length; outerI++)
	        {
	            if (outerI + query.Length <= text.Length)
	            {
	                int innerI = 0;
	                for (; innerI < query.Length; innerI++)
	                {
	                    if (text[outerI + innerI] != query[innerI])
	                        break;
	                }
	
	                if (innerI == query.Length)
	                {
	                    // Then the entire query was matched
	                    matchedTextSpanList.Add(
	                    	(
	                    		text,
		                    	new TextEditorTextSpan(
			                        outerI,
			                        outerI + innerI,
			                        (byte)FindOverlayDecorationKind.LongestCommonSubsequence,
			                        new ResourceUri(filePath),
			                        text)
	                        ));
	                }
	            }
	        }
	
	        foreach (var matchedTextSpan in matchedTextSpanList)
	        {
				textSpanList.Add(matchedTextSpan);
	        }
		}
		
		void ShowFilesProcessedCountOnUi(double decimalPercentProgress, bool shouldDisposeProgressBarModel = false)
		{
			_throttleUiUpdate.Run(_ =>
			{
				progressBarModel.SetProgress(
					decimalPercentProgress,
					$"{filesProcessedCount:N0} files processed");
					
				if (shouldDisposeProgressBarModel)
				{
					progressBarModel.Dispose();
					// The use of 'GetFindAllState()' is purposeful.
					ConstructTreeView(GetFindAllState());
					SetProgressBarModel(progressBarModel);
				}
					
				return Task.CompletedTask;
			});
		}
	}
	
	private void ConstructTreeView(TextEditorFindAllState textEditorFindAllState)
	{
		var groupedResults = textEditorFindAllState.SearchResultList.GroupBy(x => x.TextSpan.ResourceUri);
	
	    var treeViewList = groupedResults.Select(group =>
	    {
	    	var absolutePath = _commonUtilityService.EnvironmentProvider.AbsolutePathFactory(
	    		group.Key.Value,
	    		false);
	    		
	    	return (TreeViewNoType)new TreeViewFindAllGroup(
		        group.Select(textSpan => new TreeViewFindAllTextSpan(
		        	textSpan,
		        	absolutePath,
		        	false,
		        	false)).ToList(),
		        absolutePath,
				true,
				false);
	    }).ToArray();
	
	    var adhocRoot = TreeViewAdhoc.ConstructTreeViewAdhoc(treeViewList);
	    var firstNode = treeViewList.FirstOrDefault();
	
	    IReadOnlyList<TreeViewNoType> activeNodes = firstNode is null
	        ? Array.Empty<TreeViewNoType>()
	        : new List<TreeViewNoType> { firstNode };
	
	    if (!_commonUtilityService.TryGetTreeViewContainer(TextEditorFindAllState.TreeViewFindAllContainerKey, out _))
	    {
	        _commonUtilityService.TreeView_RegisterContainerAction(new TreeViewContainer(
	            TextEditorFindAllState.TreeViewFindAllContainerKey,
	            adhocRoot,
	            activeNodes));
	    }
	    else
	    {
	        _commonUtilityService.TreeView_WithRootNodeAction(TextEditorFindAllState.TreeViewFindAllContainerKey, adhocRoot);
	
	        _commonUtilityService.TreeView_SetActiveNodeAction(
	            TextEditorFindAllState.TreeViewFindAllContainerKey,
	            firstNode,
	            true,
	            false);
	    }
	}
	
	public void Dispose()
	{
		CancelSearch();
	}
}
