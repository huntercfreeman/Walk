using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.TreeViews.Models.Utils;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.TextEditor.RazorLib;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Extensions.DotNet.BackgroundTasks.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.CommandLines.Models;
using Walk.CompilerServices.DotNetSolution.Models.Project;

namespace Walk.Extensions.DotNet.TestExplorers.Models;

public class TestExplorerService : ITestExplorerService, IBackgroundTaskGroup, IStateScheduler, IDisposable
{
	private readonly object _stateModificationLock = new();

	private readonly DotNetBackgroundTaskApi _dotNetBackgroundTaskApi;
    private readonly IdeService _ideService;
    private readonly IDotNetSolutionService _dotNetSolutionService;
    private readonly DotNetCliOutputParser _dotNetCliOutputParser;

    public TestExplorerService(
		DotNetBackgroundTaskApi dotNetBackgroundTaskApi,
		IDotNetSolutionService dotNetSolutionService,
        DotNetCliOutputParser dotNetCliOutputParser)
	{
        _dotNetBackgroundTaskApi = dotNetBackgroundTaskApi;
        _dotNetSolutionService = dotNetSolutionService;
        _dotNetCliOutputParser = dotNetCliOutputParser;
        
        _dotNetSolutionService.DotNetSolutionStateChanged += OnDotNetSolutionStateChanged;
    }
    
    /// <summary>
    /// Each time the user opens the 'Test Explorer' panel,
    /// a check is done to see if the data being displayed
    /// is in sync with the user's selected .NET solution.
    ///
    /// If it is not in sync, then it starts discovering tests for each of the
    /// projects in the solution.
    ///
    /// But, if the user cancels this task, if they change panel tabs
    /// from the 'Test Explorer' to something else, when they return
    /// it will once again try to discover tests in all the projects for the solution.
    ///
    /// This is very annoying from a user perspective.
    /// So this field will track whether we've already started
    /// the task to discover tests in all the projects for the solution or not.
    ///
    /// This is fine because there is a button in the top left of the panel that
    /// has a 'refresh' icon and it will start this task if the
    /// user manually clicks it, (even if they cancelled the automatic invocation).
    /// </summary>
    private string _intentToDiscoverTestsInSolutionFilePath = string.Empty;
    
    private string _treeViewOwnerSolutionFilePath = string.Empty;
    
    private TestExplorerState _testExplorerState = new();
    
    public event Action? TestExplorerStateChanged;
    
    public TestExplorerState GetTestExplorerState() => _testExplorerState;

    public void ReduceWithAction(Func<TestExplorerState, TestExplorerState> withFunc)
    {
    	lock (_stateModificationLock)
    	{
	    	var inState = GetTestExplorerState();
	    
	        _testExplorerState = withFunc.Invoke(inState);
	        
	        TestExplorerStateChanged?.Invoke();
	        return;
	    }
    }
    
    public void ReduceInitializeResizeHandleDimensionUnitAction(DimensionUnit dimensionUnit)
    {
    	lock (_stateModificationLock)
    	{
	    	var inState = GetTestExplorerState();
	    
	        if (dimensionUnit.Purpose != DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_COLUMN)
	        {
	        	TestExplorerStateChanged?.Invoke();
	        	return;
	        }
	        
	        // TreeViewElementDimensions
	        {
	        	if (inState.TreeViewElementDimensions.WidthDimensionAttribute.DimensionUnitList is null)
	        	{
	        		TestExplorerStateChanged?.Invoke();
	        		return;
	        	}
	        		
	        	var existingDimensionUnit = inState.TreeViewElementDimensions.WidthDimensionAttribute.DimensionUnitList
	        		.FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
	        		
	            if (existingDimensionUnit.Purpose is not null)
	            {
	            	TestExplorerStateChanged?.Invoke();
	        		return;
	            }
	        		
	        	inState.TreeViewElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
	        }
	        
	        // DetailsElementDimensions
	        {
	        	if (inState.DetailsElementDimensions.WidthDimensionAttribute.DimensionUnitList is null)
	        	{
	        		TestExplorerStateChanged?.Invoke();
	        		return;
	        	}
	        		
	        	var existingDimensionUnit = inState.DetailsElementDimensions.WidthDimensionAttribute.DimensionUnitList
	        		.FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
	        		
	            if (existingDimensionUnit.Purpose is not null)
	            {
	            	TestExplorerStateChanged?.Invoke();
	        		return;
	            }
	        		
	        	inState.DetailsElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
	        }
	        
	        TestExplorerStateChanged?.Invoke();
	        return;
	    }
    }
    
	/// <summary>
    /// When the user interface for the test explorer is rendered,
    /// then dispatch this in order to start a task that will discover unit tests.
    /// </summary>
	public Task HandleUserInterfaceWasInitializedEffect()
	{
		var dotNetSolutionState = _dotNetSolutionService.GetDotNetSolutionState();
		var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionModel;

		if (dotNetSolutionModel is null)
			return Task.CompletedTask;

		var testExplorerState = GetTestExplorerState();
		
		if (dotNetSolutionModel.AbsolutePath.Value != testExplorerState.SolutionFilePath)
		{
			ReduceWithAction(inState => inState with
			{
				SolutionFilePath = dotNetSolutionModel.AbsolutePath.Value
			});
		
			_dotNetBackgroundTaskApi.TestExplorerService.Enqueue_ConstructTreeView();
		}
		
		if (_intentToDiscoverTestsInSolutionFilePath != dotNetSolutionModel.AbsolutePath.Value)
		{
			_intentToDiscoverTestsInSolutionFilePath = dotNetSolutionModel.AbsolutePath.Value;
			_dotNetBackgroundTaskApi.TestExplorerService.Enqueue_DiscoverTests();
		}
		
		return Task.CompletedTask;
	}
	
	public Task HandleShouldDiscoverTestsEffect()
	{
		var dotNetSolutionState = _dotNetSolutionService.GetDotNetSolutionState();
		var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionModel;

		if (dotNetSolutionModel is null)
			return Task.CompletedTask;

		var testExplorerState = GetTestExplorerState();
	
		if (dotNetSolutionModel.AbsolutePath.Value != testExplorerState.SolutionFilePath)
		{
			ReduceWithAction(inState => inState with
			{
				SolutionFilePath = dotNetSolutionModel.AbsolutePath.Value
			});
		
			_dotNetBackgroundTaskApi.TestExplorerService.Enqueue_ConstructTreeView();
		}
		
		_intentToDiscoverTestsInSolutionFilePath = dotNetSolutionModel.AbsolutePath.Value;
		_dotNetBackgroundTaskApi.TestExplorerService.Enqueue_DiscoverTests();
	
        return Task.CompletedTask;
	}
	
	private async void OnDotNetSolutionStateChanged()
	{
		var solutionFilePathWasNull = GetTestExplorerState().SolutionFilePath is null;
		
		ReduceWithAction(inState => inState with
		{
			SolutionFilePath = null
		});
		
		ContainsTestsTreeViewGroup.ChildList = new();
		NoTestsTreeViewGroup.ChildList = new();
		ThrewAnExceptionTreeViewGroup.ChildList = new();
		NotValidProjectForUnitTestTreeViewGroup.ChildList = new();
		
		_textEditorService.CommonUtilityService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, ContainsTestsTreeViewGroup);
		
		if (!solutionFilePathWasNull)
		{
			_ = Task.Run(async () =>
			{
				await HandleUserInterfaceWasInitializedEffect()
					.ConfigureAwait(false);
			});
		}
	}
	
	public void Dispose()
	{
		_dotNetSolutionService.DotNetSolutionStateChanged -= OnDotNetSolutionStateChanged;
	}

    public Key<IBackgroundTaskGroup> BackgroundTaskKey { get; } = Key<IBackgroundTaskGroup>.NewKey();

    public bool __TaskCompletionSourceWasCreated { get; set; }

    private readonly Queue<TestExplorerSchedulerWorkKind> _workKindQueue = new();
    private readonly object _workLock = new();

    public void Enqueue_ConstructTreeView()
    {
        lock (_workLock)
        {
            _workKindQueue.Enqueue(TestExplorerSchedulerWorkKind.ConstructTreeView);
            _textEditorService.CommonUtilityService.Continuous_EnqueueGroup(this);
        }
    }
    
    public void Enqueue_DiscoverTests()
    {
        lock (_workLock)
        {
            _workKindQueue.Enqueue(TestExplorerSchedulerWorkKind.DiscoverTests);
            _textEditorService.CommonUtilityService.Continuous_EnqueueGroup(this);
        }
    }

    public ValueTask HandleEvent()
    {
        TestExplorerSchedulerWorkKind workKind;

        lock (_workLock)
        {
            if (!_workKindQueue.TryDequeue(out workKind))
                return ValueTask.CompletedTask;
        }

        switch (workKind)
        {
            case TestExplorerSchedulerWorkKind.ConstructTreeView:
            {
                return Do_ConstructTreeView();
            }
            case TestExplorerSchedulerWorkKind.DiscoverTests:
            {
                return Do_DiscoverTests();
            }
            default:
            {
                Console.WriteLine($"{nameof(TestExplorerService)} {nameof(HandleEvent)} default case");
				return ValueTask.CompletedTask;
            }
        }
    }
    
    private readonly Throttle _throttleDiscoverTests = new(TimeSpan.FromMilliseconds(100));
    
    public TreeViewGroup ContainsTestsTreeViewGroup { get; } = new("Have tests", true, true);
	public TreeViewGroup NoTestsTreeViewGroup { get; } = new("No tests (but still a test-project)", true, true);
	public TreeViewGroup ThrewAnExceptionTreeViewGroup { get; } = new("Projects that threw an exception during discovery", true, true);
	public TreeViewGroup NotValidProjectForUnitTestTreeViewGroup { get; } = new("Not a test-project", true, true);

    public async ValueTask Do_ConstructTreeView()
    {
        var dotNetSolutionState = _dotNetSolutionService.GetDotNetSolutionState();
        var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionModel;

        if (dotNetSolutionModel is null)
            return;

        var localDotNetProjectList = dotNetSolutionModel.DotNetProjectList
            .Where(x => x.DotNetProjectKind == DotNetProjectKind.CSharpProject);

        var localProjectTestModelList = localDotNetProjectList.Select(x => new ProjectTestModel(
				x.ProjectIdGuid,
				x.AbsolutePath,
				callback => Task.CompletedTask,
				node => _textEditorService.CommonUtilityService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, node)))
			.ToList();

        var localFormattedCommand = DotNetCliCommandFormatter.FormatDotNetTestListTests();

        var localTreeViewProjectTestModelList = localProjectTestModelList.Select(x =>
                (TreeViewNoType)new TreeViewProjectTestModel(
                    x,
                    _textEditorService.CommonUtilityService.CommonComponentRenderers,
                    true,
                    false))
            .ToArray();

        foreach (var entry in localTreeViewProjectTestModelList)
        {
            var treeViewProjectTestModel = (TreeViewProjectTestModel)entry;

            if (string.IsNullOrWhiteSpace(treeViewProjectTestModel.Item.DirectoryNameForTestDiscovery))
                return;
            
            var projectFileText = await _textEditorService.CommonUtilityService.FileSystemProvider.File.ReadAllTextAsync(treeViewProjectTestModel.Item.AbsolutePath.Value);
            
            if (projectFileText.Contains("xunit"))
            {
            	if (!NoTestsTreeViewGroup.ChildList.Any(x =>
            		((TreeViewProjectTestModel)x).Item.AbsolutePath.Value == treeViewProjectTestModel.Item.AbsolutePath.Value))
            	{
            		NoTestsTreeViewGroup.ChildList.Add(treeViewProjectTestModel);
            	}
            }
            else
            {
            	if (!NotValidProjectForUnitTestTreeViewGroup.ChildList.Any(x =>
            		((TreeViewProjectTestModel)x).Item.AbsolutePath.Value == treeViewProjectTestModel.Item.AbsolutePath.Value))
            	{
            		NotValidProjectForUnitTestTreeViewGroup.ChildList.Add(treeViewProjectTestModel);
            	}
            }

            treeViewProjectTestModel.Item.EnqueueDiscoverTestsFunc = callback =>
            {
				var terminalCommandRequest = new TerminalCommandRequest(
		        	localFormattedCommand.Value,
		        	treeViewProjectTestModel.Item.DirectoryNameForTestDiscovery,
		        	treeViewProjectTestModel.Item.DotNetTestListTestsTerminalCommandRequestKey)
		        {
		        	BeginWithFunc = async parsedCommand =>
		        	{
		        		treeViewProjectTestModel.Item.TerminalCommandParsed = parsedCommand;
		        	},
		        	ContinueWithFunc = async parsedCommand =>
		        	{
		        		treeViewProjectTestModel.Item.TerminalCommandParsed = parsedCommand;
		        	
						try
						{
							treeViewProjectTestModel.Item.TestNameFullyQualifiedList = _dotNetCliOutputParser.ParseOutputLineDotNetTestListTests(
		        				treeViewProjectTestModel.Item.TerminalCommandParsed.OutputCache.ToString());

							// THINKING_ABOUT_TREE_VIEW();
							{
								var splitOutputList = (treeViewProjectTestModel.Item.TestNameFullyQualifiedList ?? new())
									.Select(x =>
									{
										// Theory, InlineData
										// ------------------
										// I don't have it in me to fix this at the moment.
										// Need to handle these differently.
										//
										// Going to just take the non-argumented version
										// for now.
										//
										// It will run all the [InlineData] in one go
										// but can't single out yet. (2025-01-25)
										var openParenthesisIndex = x.IndexOf("(");
										if (openParenthesisIndex != -1)
											x = x[..openParenthesisIndex];
										
										return x.Split('.');
									});

								var rootMap = new Dictionary<string, StringFragment>();

								foreach (var splitOutput in splitOutputList)
								{
									var targetMap = rootMap;
									var lastSeenStringFragment = (StringFragment?)null;

									foreach (var fragment in splitOutput)
									{
										if (!targetMap.ContainsKey(fragment))
											targetMap.Add(fragment, new(fragment));

										lastSeenStringFragment = targetMap[fragment];
										targetMap = lastSeenStringFragment.Map;
									}

									if (lastSeenStringFragment is not null)
										lastSeenStringFragment.IsEndpoint = true;
								}

								treeViewProjectTestModel.Item.RootStringFragmentMap = rootMap;
								await callback.Invoke(rootMap).ConfigureAwait(false);
							}
						}
						catch (Exception)
						{
							await callback.Invoke(new()).ConfigureAwait(false);
							throw;
						}
		        	}
		        };

                treeViewProjectTestModel.Item.TerminalCommandRequest = terminalCommandRequest;
                
				return _terminalService.GetTerminalState().TerminalMap[TerminalFacts.EXECUTION_KEY]
					.EnqueueCommandAsync(terminalCommandRequest);
            };
        }

		var adhocRoot = TreeViewAdhoc.ConstructTreeViewAdhoc(new []
		{
			ContainsTestsTreeViewGroup,
        	NoTestsTreeViewGroup,
        	ThrewAnExceptionTreeViewGroup,
        	NotValidProjectForUnitTestTreeViewGroup
		});
		
		adhocRoot.LinkChildren(new(), adhocRoot.ChildList);
		
		ContainsTestsTreeViewGroup.LinkChildren(new(), ContainsTestsTreeViewGroup.ChildList);
    	NoTestsTreeViewGroup.LinkChildren(new(), NoTestsTreeViewGroup.ChildList);
    	ThrewAnExceptionTreeViewGroup.LinkChildren(new(), ThrewAnExceptionTreeViewGroup.ChildList);
    	NotValidProjectForUnitTestTreeViewGroup.LinkChildren(new(), NotValidProjectForUnitTestTreeViewGroup.ChildList);
        
        var firstNode = localTreeViewProjectTestModelList.FirstOrDefault();

        var activeNodes = new List<TreeViewNoType> { ContainsTestsTreeViewGroup };

        if (!_textEditorService.CommonUtilityService.TryGetTreeViewContainer(TestExplorerState.TreeViewTestExplorerKey, out _))
        {
            _textEditorService.CommonUtilityService.TreeView_RegisterContainerAction(new TreeViewContainer(
                TestExplorerState.TreeViewTestExplorerKey,
                adhocRoot,
                activeNodes));
        }
        else
        {
            _textEditorService.CommonUtilityService.TreeView_WithRootNodeAction(TestExplorerState.TreeViewTestExplorerKey, adhocRoot);

            _textEditorService.CommonUtilityService.TreeView_SetActiveNodeAction(
                TestExplorerState.TreeViewTestExplorerKey,
                firstNode,
                true,
                false);
        }

        _dotNetBackgroundTaskApi.TestExplorerService.ReduceWithAction(inState => inState with
        {
            ProjectTestModelList = localProjectTestModelList,
            SolutionFilePath = dotNetSolutionModel.AbsolutePath.Value,
        });
    }
    
    public ValueTask Do_DiscoverTests()
    {
    	_throttleDiscoverTests.Run(async _ =>
    	{
	    	var dotNetSolutionState = _dotNetSolutionService.GetDotNetSolutionState();
	        var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionModel;
	
	        if (dotNetSolutionModel is null)
	            return;
	    	
	    	var localTestExplorerState = GetTestExplorerState();
	    	var cancellationTokenSource = new CancellationTokenSource();
	    	var cancellationToken = cancellationTokenSource.Token;
	    	
	    	var progressBarModel = new ProgressBarModel(0, "parsing...")
	    	{
	    		OnCancelFunc = () =>
	    		{
	    			cancellationTokenSource.Cancel();
	    			cancellationTokenSource.Dispose();
	    			return Task.CompletedTask;
	    		}
	    	};
	
			NotificationHelper.DispatchProgress(
				$"Test Discovery: {dotNetSolutionModel.AbsolutePath.NameWithExtension}",
				progressBarModel,
				_textEditorService.CommonUtilityService,
				TimeSpan.FromMilliseconds(-1));
				
			var progressThrottle = new Throttle(TimeSpan.FromMilliseconds(100));
		    
			try
			{
				progressThrottle.Run(_ => 
				{
					progressBarModel.SetProgress(0, "Discovering tests...");
					return Task.CompletedTask;
				});
			
				var completionPercentPerProject = 1.0 / (double)NoTestsTreeViewGroup.ChildList.Count;
	    		var projectsHandled = 0;
	    		
	    		if (_textEditorService.CommonUtilityService.TryGetTreeViewContainer(TestExplorerState.TreeViewTestExplorerKey, out var treeViewContainer))
		        {
		        	if (treeViewContainer.RootNode is not TreeViewAdhoc treeViewAdhoc)
						return;
						
					var dotNetProjectListLength = NoTestsTreeViewGroup.ChildList.Count;
		        		
		            foreach (var treeViewProject in NoTestsTreeViewGroup.ChildList)
		            {
		            	if (treeViewProject is not TreeViewProjectTestModel treeViewProjectTestModel)
		            		continue;
		            		
		            	var currentProgress = completionPercentPerProject * projectsHandled;
		            	
		            	progressThrottle.Run(_ => 
						{
							progressBarModel.SetProgress(
								currentProgress,
								$"{projectsHandled + 1}/{dotNetProjectListLength}: {treeViewProjectTestModel.Item.AbsolutePath.NameWithExtension}");
							return Task.CompletedTask;
						});
		            
		            	cancellationToken.ThrowIfCancellationRequested();
		            
		            	await treeViewProject.LoadChildListAsync();
			    		projectsHandled++;
		            }
		       
		       	 progressThrottle.Run(_ => 
					{
						progressBarModel.SetProgress(1, $"Finished test discovery: {dotNetSolutionModel.AbsolutePath.NameWithExtension}", string.Empty);
						progressBarModel.Dispose();
						return Task.CompletedTask;
					});     
		            await Task_SumEachProjectTestCount();
		        }
		        else
		        {
		        	progressThrottle.Run(_ => 
					{
						progressBarModel.SetProgress(0, "not found");
						return Task.CompletedTask;
					});
		        }
			}
			catch (Exception e)
			{
				if (e is OperationCanceledException)
					progressBarModel.IsCancelled = true;
			
				var currentProgress = progressBarModel.GetProgress();
				
				progressThrottle.Run(_ => 
				{
					// TODO: Set message 2 as the error instead so we can see the project...
					//       ... that it was discovering tests for when it threw exception?
					progressBarModel.SetProgress(currentProgress, e.ToString());
					progressBarModel.Dispose();
					return Task.CompletedTask;
				});
			}
		});
		
		return ValueTask.CompletedTask;
    }
    
    public Task Task_SumEachProjectTestCount()
    {
		var dotNetSolutionState = _dotNetSolutionService.GetDotNetSolutionState();
        var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionModel;

        if (dotNetSolutionModel is null)
            return Task.CompletedTask;
    
    	var totalTestCount = 0;
    	var notRanTestHashSet = new HashSet<string>();
    	
    	Console.WriteLine($"NoTestsTreeViewGroup.ChildList.Count: {NoTestsTreeViewGroup.ChildList.Count}");
    	if (_textEditorService.CommonUtilityService.TryGetTreeViewContainer(TestExplorerState.TreeViewTestExplorerKey, out var treeViewContainer))
        {
        	if (treeViewContainer.RootNode is not TreeViewAdhoc treeViewAdhoc)
        		return Task.CompletedTask;
        		
            foreach (var treeViewProject in NoTestsTreeViewGroup.ChildList)
            {
            	if (treeViewProject is not TreeViewProjectTestModel treeViewProjectTestModel)
            		return Task.CompletedTask;
            
            	totalTestCount += treeViewProjectTestModel.Item.TestNameFullyQualifiedList?.Count ?? 0;
            	
            	MoveNodeToCorrectBranch(treeViewProjectTestModel);
            	
            	/*if (treeViewProjectTestModel.Item.TestNameFullyQualifiedList is not null)
            	{
            		if (treeViewProjectTestModel.Item.TestNameFullyQualifiedList.Count > 0)
            		{
            			ContainsTestsTreeViewGroup.ChildList.Add(treeViewProjectTestModel);
            		}
            		else
            		{
            			NoTestsTreeViewGroup.ChildList.Add(treeViewProjectTestModel);
            		}
            	}
            	else
            	{
            		if (treeViewProjectTestModel.Item.TerminalCommandParsed is not null &&
            			treeViewProjectTestModel.Item.TerminalCommandParsed.OutputCache.ToString().Contains("threw an exception"))
            		{
            			ThrewAnExceptionTreeViewGroup.ChildList.Add(treeViewProjectTestModel);
            		}
            		else
            		{
            			NotValidProjectForUnitTestTreeViewGroup.ChildList.Add(treeViewProjectTestModel);
            		}
            	}*/
            	
            	if (treeViewProjectTestModel.Item.TestNameFullyQualifiedList is not null)
            	{
            		foreach (var output in treeViewProjectTestModel.Item.TestNameFullyQualifiedList)
	            	{
	            		notRanTestHashSet.Add(output);
	            	}
            	}
            }
            
            ContainsTestsTreeViewGroup.LinkChildren(new(), ContainsTestsTreeViewGroup.ChildList);
            NoTestsTreeViewGroup.LinkChildren(new(), NoTestsTreeViewGroup.ChildList);
            ThrewAnExceptionTreeViewGroup.LinkChildren(new(), ThrewAnExceptionTreeViewGroup.ChildList);
            NotValidProjectForUnitTestTreeViewGroup.LinkChildren(new(), NotValidProjectForUnitTestTreeViewGroup.ChildList);
            
            var nextTreeViewAdhoc = TreeViewAdhoc.ConstructTreeViewAdhoc(
            	ContainsTestsTreeViewGroup,
            	NoTestsTreeViewGroup,
            	ThrewAnExceptionTreeViewGroup,
            	NotValidProjectForUnitTestTreeViewGroup);
            	
            nextTreeViewAdhoc.LinkChildren(new(), nextTreeViewAdhoc.ChildList);
            
            _textEditorService.CommonUtilityService.TreeView_WithRootNodeAction(TestExplorerState.TreeViewTestExplorerKey, nextTreeViewAdhoc);
        }
    
    	_dotNetBackgroundTaskApi.TestExplorerService.ReduceWithAction(inState => inState with
        {
            TotalTestCount = totalTestCount,
            NotRanTestHashSet = notRanTestHashSet,
            SolutionFilePath = dotNetSolutionModel.AbsolutePath.Value
        });
    
        return Task.CompletedTask;
    }
    
    /// <summary>
	/// This is strategic spaghetti code that is part of my master plan.
	/// Don't @ me on teams; I won't respond.
	/// </summary>
	public void MoveNodeToCorrectBranch(TreeViewProjectTestModel treeViewProjectTestModel)
	{
		if (treeViewProjectTestModel.Parent is null)
			return;
		
		if (!_textEditorService.CommonUtilityService.TryGetTreeViewContainer(TestExplorerState.TreeViewTestExplorerKey, out var treeViewContainer))
			return;
		
		// containsTestsTreeViewGroup
		var containsTestsTreeViewGroupList = treeViewContainer.RootNode.ChildList
			.Where(x =>
				x is TreeViewGroup tvg &&
				tvg.Item == "Have tests");
		if (containsTestsTreeViewGroupList.Count() != 1)
			return;
		var containsTestsTreeViewGroup = containsTestsTreeViewGroupList.Single();
		
		// noTestsTreeViewGroup
		var noTestsTreeViewGroupList = treeViewContainer.RootNode.ChildList
			.Where(x =>
				x is TreeViewGroup tvg &&
				tvg.Item == "Have tests");
		if (noTestsTreeViewGroupList.Count() != 1)
			return;
		var noTestsTreeViewGroup = noTestsTreeViewGroupList.Single();

		// threwAnExceptionTreeViewGroup
		var threwAnExceptionTreeViewGroupList = treeViewContainer.RootNode.ChildList
			.Where(x =>
				x is TreeViewGroup tvg &&
				tvg.Item == "Projects that threw an exception during discovery");
		if (threwAnExceptionTreeViewGroupList.Count() != 1)
			return;
		var threwAnExceptionTreeViewGroup = threwAnExceptionTreeViewGroupList.Single();		
		
		// notValidProjectForUnitTestTreeViewGroup
		var notValidProjectForUnitTestTreeViewGroupList = treeViewContainer.RootNode.ChildList
			.Where(x =>
				x is TreeViewGroup tvg &&
				tvg.Item == "Not a test-project");
		if (notValidProjectForUnitTestTreeViewGroupList.Count() != 1)
			return;
		var notValidProjectForUnitTestTreeViewGroup = notValidProjectForUnitTestTreeViewGroupList.Single();
			
		treeViewProjectTestModel.Parent.ChildList.Remove(treeViewProjectTestModel);
		treeViewProjectTestModel.Parent.LinkChildren(
			treeViewProjectTestModel.Parent.ChildList,
			treeViewProjectTestModel.Parent.ChildList);
		_textEditorService.CommonUtilityService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, treeViewProjectTestModel.Parent);
				
		if (treeViewProjectTestModel.Item.TestNameFullyQualifiedList is not null)
    	{
    		if (treeViewProjectTestModel.Item.TestNameFullyQualifiedList.Count > 0)
    		{
    			containsTestsTreeViewGroup.ChildList.Add(treeViewProjectTestModel);
    			containsTestsTreeViewGroup.LinkChildren(
					containsTestsTreeViewGroup.ChildList,
					containsTestsTreeViewGroup.ChildList);
				_textEditorService.CommonUtilityService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, containsTestsTreeViewGroup);
    		}
    		else
    		{
    			noTestsTreeViewGroup.ChildList.Add(treeViewProjectTestModel);
    			noTestsTreeViewGroup.LinkChildren(
					noTestsTreeViewGroup.ChildList,
					noTestsTreeViewGroup.ChildList);
				_textEditorService.CommonUtilityService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, noTestsTreeViewGroup);
    		}
    	}
    	else
    	{
    		if (treeViewProjectTestModel.Item.TerminalCommandParsed is not null &&
    			treeViewProjectTestModel.Item.TerminalCommandParsed.OutputCache.ToString().Contains("threw an exception"))
    		{
    			threwAnExceptionTreeViewGroup.ChildList.Add(treeViewProjectTestModel);
    			threwAnExceptionTreeViewGroup.LinkChildren(
					threwAnExceptionTreeViewGroup.ChildList,
					threwAnExceptionTreeViewGroup.ChildList);
				_textEditorService.CommonUtilityService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, threwAnExceptionTreeViewGroup);
    		}
    		else
    		{
    			notValidProjectForUnitTestTreeViewGroup.ChildList.Add(treeViewProjectTestModel);
    			notValidProjectForUnitTestTreeViewGroup.LinkChildren(
					notValidProjectForUnitTestTreeViewGroup.ChildList,
					notValidProjectForUnitTestTreeViewGroup.ChildList);
				_textEditorService.CommonUtilityService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, notValidProjectForUnitTestTreeViewGroup);
    		}
    	}
	}
}

