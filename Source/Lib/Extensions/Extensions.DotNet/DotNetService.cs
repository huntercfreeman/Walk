using System.Net.Http.Json;
using System.Text;
using System.Web;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using CliWrap.EventStream;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Namespaces.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.TreeViews.Models.Utils;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.BackgroundTasks.Models;
using Walk.Extensions.DotNet.Nugets.Models;
using Walk.Extensions.DotNet.CommandLines.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.Namespaces.Models;
using Walk.Extensions.DotNet.CSharpProjects.Models;
using Walk.Extensions.DotNet.ComponentRenderers.Models;
using Walk.Extensions.DotNet.TestExplorers.Models;
using Walk.Extensions.DotNet.Outputs.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Displays;
using Walk.Extensions.DotNet.TestExplorers.Displays;
using Walk.Extensions.DotNet.Nugets.Displays;
using Walk.Extensions.DotNet.Outputs.Displays;
using Walk.Extensions.DotNet.AppDatas.Models;
using Walk.CompilerServices.DotNetSolution.Models.Project;
using Walk.CompilerServices.DotNetSolution.Models;
using Walk.CompilerServices.DotNetSolution.SyntaxActors;
using Walk.CompilerServices.DotNetSolution.CompilerServiceCase;
using Walk.CompilerServices.Xml.Html.SyntaxActors;
using Walk.CompilerServices.Xml.Html.SyntaxEnums;
using Walk.CompilerServices.Xml.Html.SyntaxObjects;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.AppDatas.Models;
using Walk.Ide.RazorLib.Shareds.Models;

namespace Walk.Extensions.DotNet;

public class DotNetService : IBackgroundTaskGroup, IDisposable
{
    private readonly HttpClient _httpClient;
	
	public DotNetService(
	    IDotNetComponentRenderers dotNetComponentRenderers,
	    IdeService ideService,
	    HttpClient httpClient,
	    IAppDataService appDataService,
        IServiceProvider serviceProvider)
	{
	    IdeService = ideService;
	    AppDataService = appDataService;
	    DotNetComponentRenderers = dotNetComponentRenderers;
		_httpClient = httpClient;
        
        DotNetSolutionStateChanged += OnDotNetSolutionStateChanged;
	}
	
	public Key<IBackgroundTaskGroup> BackgroundTaskKey { get; } = Key<IBackgroundTaskGroup>.NewKey();

    public bool __TaskCompletionSourceWasCreated { get; set; }
    public IdeService IdeService { get; }
    public TextEditorService TextEditorService => IdeService.TextEditorService;
    public CommonService CommonService => IdeService.TextEditorService.CommonService;
	public IDotNetComponentRenderers DotNetComponentRenderers { get; }
	public IAppDataService AppDataService { get; }

    private readonly ConcurrentQueue<DotNetWorkArgs> _workQueue = new();
    
    public void Enqueue(DotNetWorkArgs workArgs)
    {
		_workQueue.Enqueue(workArgs);
        IdeService.TextEditorService.CommonService.Continuous_EnqueueGroup(this);
    }
    
    public ValueTask HandleEvent()
    {
        if (!_workQueue.TryDequeue(out DotNetWorkArgs workArgs))
            return ValueTask.CompletedTask;

        switch (workArgs.WorkKind)
        {
            case DotNetWorkKind.SolutionExplorer_TreeView_MultiSelect_DeleteFiles:
                return Do_SolutionExplorer_TreeView_MultiSelect_DeleteFiles(workArgs.TreeViewCommandArgs);
            case DotNetWorkKind.WalkExtensionsDotNetInitializerOnInit:
                return Do_WalkExtensionsDotNetInitializerOnInit();
            case DotNetWorkKind.WalkExtensionsDotNetInitializerOnAfterRender:
                return Do_WalkExtensionsDotNetInitializerOnAfterRender();
            case DotNetWorkKind.SubmitNuGetQuery:
                return Do_SubmitNuGetQuery(workArgs.NugetPackageManagerQuery);
            case DotNetWorkKind.RunTestByFullyQualifiedName:
                return Do_RunTestByFullyQualifiedName(workArgs.TreeViewStringFragment, workArgs.FullyQualifiedName, workArgs.TreeViewProjectTestModel);
            case DotNetWorkKind.SetDotNetSolution:
	            return Do_SetDotNetSolution(workArgs.DotNetSolutionAbsolutePath);
			case DotNetWorkKind.SetDotNetSolutionTreeView:
	            return Do_SetDotNetSolutionTreeView(workArgs.DotNetSolutionModelKey);
			case DotNetWorkKind.Website_AddExistingProjectToSolution:
	            return Do_Website_AddExistingProjectToSolution(
	                workArgs.DotNetSolutionModelKey,
					workArgs.ProjectTemplateShortName,
					workArgs.CSharpProjectName,
	                workArgs.CSharpProjectAbsolutePath);
            case DotNetWorkKind.PerformRemoveCSharpProjectReferenceFromSolution:
            {
                return Do_PerformRemoveCSharpProjectReferenceFromSolution(
					workArgs.TreeViewSolution, workArgs.ProjectNode, workArgs.Terminal, CommonService, workArgs.OnAfterCompletion);
            }
			case DotNetWorkKind.PerformRemoveProjectToProjectReference:
            {
                return Do_PerformRemoveProjectToProjectReference(
                    workArgs.TreeViewCSharpProjectToProjectReference,
					workArgs.Terminal,
					CommonService,
                    workArgs.OnAfterCompletion);
            }
			case DotNetWorkKind.PerformMoveProjectToSolutionFolder:
            {
                return Do_PerformMoveProjectToSolutionFolder(
                    workArgs.TreeViewSolution,
                    workArgs.TreeViewProjectToMove,
					workArgs.SolutionFolderPath,
					workArgs.Terminal,
					CommonService,
                    workArgs.OnAfterCompletion);
            }
			case DotNetWorkKind.PerformRemoveNuGetPackageReferenceFromProject:
            {
                return Do_PerformRemoveNuGetPackageReferenceFromProject(
                    workArgs.ModifyProjectNamespacePath,
                    workArgs.TreeViewCSharpProjectNugetPackageReference,
                    workArgs.Terminal,
                    CommonService,
                    workArgs.OnAfterCompletion);
            }
            case DotNetWorkKind.ConstructTreeView:
            {
                return TestExplorer_Do_ConstructTreeView();
            }
            case DotNetWorkKind.DiscoverTests:
            {
                return Do_DiscoverTests();
            }
            default:
                Console.WriteLine($"{nameof(DotNetService)} {nameof(HandleEvent)} default case");
                return ValueTask.CompletedTask;
        }
    }

	/* Start INugetPackageManagerProvider */
	public string ProviderWebsiteUrlNoFormatting { get; } = "https://azuresearch-usnc.nuget.org/";

	public async Task<List<NugetPackageRecord>> QueryForNugetPackagesAsync(
		string queryValue,
		bool includePrerelease = false,
		CancellationToken cancellationToken = default)
	{
		return await QueryForNugetPackagesAsync(
				BuildQuery(queryValue, includePrerelease),
				cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task<List<NugetPackageRecord>> QueryForNugetPackagesAsync(
		INugetPackageManagerQuery nugetPackageManagerQuery,
		CancellationToken cancellationToken = default)
	{
		var query = nugetPackageManagerQuery.Query;

		var nugetPackages = await _httpClient
			.GetFromJsonAsync<NugetResponse>(
				query,
				cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (nugetPackages is not null)
			return nugetPackages.Data;

		return new();
	}

	public INugetPackageManagerQuery BuildQuery(string query, bool includePrerelease = false)
	{
		var queryBuilder = new StringBuilder(ProviderWebsiteUrlNoFormatting + "query?");

		queryBuilder.Append($"q={HttpUtility.UrlEncode(query)}");

		queryBuilder.Append('&');

		queryBuilder.Append($"prerelease={includePrerelease}");

		return new NugetPackageManagerQuery(queryBuilder.ToString());
	}

	private record NugetPackageManagerQuery(string Query) : INugetPackageManagerQuery;
	/* End INugetPackageManagerProvider */
	
	/* Start INugetPackageManagerService */
	private NuGetPackageManagerState _nuGetPackageManagerState = new();
	
	public event Action? NuGetPackageManagerStateChanged;
	
	public NuGetPackageManagerState GetNuGetPackageManagerState() => _nuGetPackageManagerState;
	
    public void ReduceSetSelectedProjectToModifyAction(IDotNetProject? selectedProjectToModify)
    {
    	var inState = GetNuGetPackageManagerState();
    
        _nuGetPackageManagerState = inState with
        {
            SelectedProjectToModify = selectedProjectToModify
        };
        
        NuGetPackageManagerStateChanged?.Invoke();
        return;
    }

    public void ReduceSetNugetQueryAction(string nugetQuery)
    {
    	var inState = GetNuGetPackageManagerState();
    
        _nuGetPackageManagerState = inState with { NugetQuery = nugetQuery };
        
        NuGetPackageManagerStateChanged?.Invoke();
        return;
    }

    public void ReduceSetIncludePrereleaseAction(bool includePrerelease)
    {
    	var inState = GetNuGetPackageManagerState();
    
        _nuGetPackageManagerState = inState with { IncludePrerelease = includePrerelease };
        
        NuGetPackageManagerStateChanged?.Invoke();
        return;
    }

    public void ReduceSetMostRecentQueryResultAction(List<NugetPackageRecord> queryResultList)
    {
    	var inState = GetNuGetPackageManagerState();
    
        _nuGetPackageManagerState = inState with { QueryResultList = queryResultList };
        
        NuGetPackageManagerStateChanged?.Invoke();
        return;
    }
	/* End INugetPackageManagerService */
	
	/* Start DotNetCliOutputParser */
	private readonly object _listLock = new();
	
	private DotNetRunParseResult _dotNetRunParseResult = new(
		message: string.Empty,
		allDiagnosticLineList: new(),
		errorList: new(),
		warningList: new(),
		otherList: new());

	public event Action? StateChanged;
	
	/// <summary>
	/// This immutable list is calculated everytime, so if necessary invoke once and then store the result.
	/// </summary>
	public DotNetRunParseResult GetDotNetRunParseResult()
	{
		lock (_listLock)
		{
			return _dotNetRunParseResult;
		}
	}

	public List<ProjectTemplate>? ProjectTemplateList { get; private set; }
	public List<string>? TheFollowingTestsAreAvailableList { get; private set; }
	public NewListModel NewListModelSession { get; private set; }
	
	/// <summary>The results can be retrieved by invoking <see cref="GetDiagnosticLineList"/></summary>
	public void ParseOutputEntireDotNetRun(string output, string message)
	{
		var stringWalker = new StringWalker(
			new ResourceUri("/__DEV_IN__/DotNetRunOutputParser.txt"),
			output);
			
		var diagnosticLineList = new List<DiagnosticLine>();
		
		var diagnosticLine = new DiagnosticLine
		{
			StartInclusiveIndex = stringWalker.PositionIndex
		};
			
		int? startInclusiveIndex = null;
		int? endExclusiveIndex = null;
		
		var badState = false;
			
		while (!stringWalker.IsEof)
		{
			// Once inside this while loop for the first time
			// stringWalker.CurrentCharacter == the first character of the output
			
			if (WhitespaceFacts.LINE_ENDING_CHARACTER_LIST.Contains(stringWalker.CurrentCharacter))
			{
				if (stringWalker.CurrentCharacter == '\r' &&
					stringWalker.NextCharacter == '\n')
				{
					_ = stringWalker.ReadCharacter();
				}

				// Make a decision
				if (diagnosticLine.IsValid)
				{
					diagnosticLine.EndExclusiveIndex = stringWalker.PositionIndex;
					
					diagnosticLine.Text = stringWalker.SourceText.Substring(
						diagnosticLine.StartInclusiveIndex,
						diagnosticLine.EndExclusiveIndex - diagnosticLine.StartInclusiveIndex);
						
					var diagnosticLineKindText = stringWalker.SourceText.Substring(
						diagnosticLine.DiagnosticKindTextSpan.StartInclusiveIndex,
						diagnosticLine.DiagnosticKindTextSpan.EndExclusiveIndex -
							diagnosticLine.DiagnosticKindTextSpan.StartInclusiveIndex);
							
					if (string.Equals(diagnosticLineKindText, nameof(DiagnosticLineKind.Warning), StringComparison.OrdinalIgnoreCase))
						diagnosticLine.DiagnosticLineKind = DiagnosticLineKind.Warning;
					else if (string.Equals(diagnosticLineKindText, nameof(DiagnosticLineKind.Error), StringComparison.OrdinalIgnoreCase))
						diagnosticLine.DiagnosticLineKind = DiagnosticLineKind.Error;
					else
						diagnosticLine.DiagnosticLineKind = DiagnosticLineKind.Other;
				
					diagnosticLineList.Add(diagnosticLine);
				}
				
				diagnosticLine = new DiagnosticLine
				{
					StartInclusiveIndex = stringWalker.PositionIndex
				};
				
				startInclusiveIndex = null;
				endExclusiveIndex = null;
				badState = false;
			}
			else
			{
				if (diagnosticLine.FilePathTextSpan is null)
				{
					if (startInclusiveIndex is null) // Start: Char at index 0
					{
						startInclusiveIndex = stringWalker.PositionIndex;
					}
					else if (endExclusiveIndex is null) // Algorithm: start at position 0 inclusive until '(' exclusive
					{
						if (stringWalker.CurrentCharacter == '(')
						{
							endExclusiveIndex = stringWalker.PositionIndex;
							
							diagnosticLine.FilePathTextSpan = new(
								startInclusiveIndex.Value,
								endExclusiveIndex.Value,
								stringWalker.SourceText);
							
							startInclusiveIndex = null;
							endExclusiveIndex = null;
							
							_ = stringWalker.BacktrackCharacter();
						}
					}
				}
				else if (diagnosticLine.LineAndColumnIndicesTextSpan is null)
				{
					if (startInclusiveIndex is null)
					{
						startInclusiveIndex = stringWalker.PositionIndex;
					}
					else if (endExclusiveIndex is null)
					{
						if (stringWalker.CurrentCharacter == ')')
						{
							endExclusiveIndex = stringWalker.PositionIndex + 1;
							
							diagnosticLine.LineAndColumnIndicesTextSpan = new(
								startInclusiveIndex.Value,
								endExclusiveIndex.Value,
								stringWalker.SourceText);
							
							startInclusiveIndex = null;
							endExclusiveIndex = null;
						}
					}
				}
				else if (diagnosticLine.DiagnosticKindTextSpan is null)
				{
					if (startInclusiveIndex is null)
					{
						if (stringWalker.CurrentCharacter == ':')
						{
							// Skip the ':'
							_ = stringWalker.ReadCharacter();
							// Skip the ' '
							_ = stringWalker.ReadCharacter();
							
							startInclusiveIndex = stringWalker.PositionIndex;
						}
					}
					else if (endExclusiveIndex is null)
					{
						if (stringWalker.CurrentCharacter == ' ')
						{
							endExclusiveIndex = stringWalker.PositionIndex;
							
							diagnosticLine.DiagnosticKindTextSpan = new(
								startInclusiveIndex.Value,
								endExclusiveIndex.Value,
								stringWalker.SourceText);
							
							startInclusiveIndex = null;
							endExclusiveIndex = null;
						}
					}
				}
				else if (diagnosticLine.DiagnosticCodeTextSpan is null)
				{
					if (startInclusiveIndex is null)
					{
						startInclusiveIndex = stringWalker.PositionIndex;
					}
					else if (endExclusiveIndex is null)
					{
						if (stringWalker.CurrentCharacter == ':')
						{
							endExclusiveIndex = stringWalker.PositionIndex;
							
							diagnosticLine.DiagnosticCodeTextSpan = new(
								startInclusiveIndex.Value,
								endExclusiveIndex.Value,
								stringWalker.SourceText);
							
							startInclusiveIndex = null;
							endExclusiveIndex = null;
						}
					}
				}
				else if (diagnosticLine.MessageTextSpan is null)
				{
					if (startInclusiveIndex is null)
					{
						// Skip the ' '
						_ = stringWalker.ReadCharacter();
					
						startInclusiveIndex = stringWalker.PositionIndex;
					}
					else if (endExclusiveIndex is null)
					{
						if (badState)
						{
							_ = stringWalker.ReadCharacter();
							continue;
						}
						
						if (stringWalker.CurrentCharacter == ']' &&
							stringWalker.NextCharacter == '\n' || stringWalker.NextCharacter == '\r')
						{
							while (stringWalker.CurrentCharacter != '[')
							{
								if (stringWalker.BacktrackCharacter() == ParserFacts.END_OF_FILE)
								{
									badState = true;
									break;
								}
							}

							if (!badState)
							{
								_ = stringWalker.BacktrackCharacter();
								endExclusiveIndex = stringWalker.PositionIndex;
								
								diagnosticLine.MessageTextSpan = new(
									startInclusiveIndex.Value,
									endExclusiveIndex.Value,
									stringWalker.SourceText);
						
								startInclusiveIndex = null;
								endExclusiveIndex = null;
							}
						}
					}
				}
				else if (diagnosticLine.ProjectTextSpan is null)
				{
					if (startInclusiveIndex is null)
					{
						// Skip the ' '
						_ = stringWalker.ReadCharacter();
						// Skip the '['
						_ = stringWalker.ReadCharacter();
						
						startInclusiveIndex = stringWalker.PositionIndex;
					}
					else if (endExclusiveIndex is null)
					{
						if (stringWalker.CurrentCharacter == ']')
						{
							endExclusiveIndex = stringWalker.PositionIndex;
							
							diagnosticLine.ProjectTextSpan = new(
								startInclusiveIndex.Value,
								endExclusiveIndex.Value,
								stringWalker.SourceText);
							
							startInclusiveIndex = null;
							endExclusiveIndex = null;
						}
					}
				}
			}
		
			_ = stringWalker.ReadCharacter();
		}
		
		lock (_listLock)
		{
			var allDiagnosticLineList = diagnosticLineList.OrderBy(x => x.DiagnosticLineKind).ToList();
		
			_dotNetRunParseResult = new(
				message: message,
				allDiagnosticLineList: allDiagnosticLineList,
				errorList: allDiagnosticLineList.Where(x => x.DiagnosticLineKind == DiagnosticLineKind.Error).ToList(),
				warningList: allDiagnosticLineList.Where(x => x.DiagnosticLineKind == DiagnosticLineKind.Warning).ToList(),
				otherList: allDiagnosticLineList.Where(x => x.DiagnosticLineKind == DiagnosticLineKind.Other).ToList());
		}
		
		StateChanged?.Invoke();
	}

	/// <summary>
	/// (NOTE: this has been fixed but the note is being left here as its a common issue with this code)
	/// ================================================================================================
	/// The following output breaks because the 'Language' for template name of 'dotnet gitignore file'
	/// is left empty.
	///
	/// Template Name                             Short Name                  Language    Tags                                                                         
	/// ----------------------------------------  --------------------------  ----------  -----------------------------------------------------------------------------
	/// Console App                               console                     [C#],F#,VB  Common/Console                                                               
	/// dotnet gitignore file                     gitignore,.gitignore                    Config      
	/// </summary>
	public List<TextEditorTextSpan> ParseOutputLineDotNetNewList(string outputEntire)
	{
		// TODO: This seems to have provided the desired output...
		//       ...the code is quite nasty but I'm not feeling well,
		//       so I'm going to leave it like this for now.
		//       Some edge case in the future will probably break this.
	
		NewListModelSession = new();
	
		// The columns are titled: { "Template Name", "Short Name", "Language", "Tags" }
		var keywordTags = "Tags";

		var resourceUri = ResourceUri.Empty;
		var stringWalker = new StringWalker(resourceUri, outputEntire);

		var shouldCountSpaceBetweenColumns = true;
		var spaceBetweenColumnsCount = 0;
	
		var isFirstColumn = true;
		
		var firstLocateDashes = true;

		while (!stringWalker.IsEof)
		{
			var whitespaceWasRead = false;

		
			if (NewListModelSession.ShouldLocateKeywordTags)
			{
				switch (stringWalker.CurrentCharacter)
				{
					case 'T':
						if (stringWalker.PeekForSubstring(keywordTags))
						{
							// The '-1' is due to the while loop always reading a character at the end.
							stringWalker.SkipRange(keywordTags.Length - 1);

							NewListModelSession.ShouldLocateKeywordTags = false;
						}
						break;
				}
			}
			else if (NewListModelSession.ShouldCountDashes)
			{
				if (NewListModelSession.ShouldLocateDashes)
				{
					// Find the first dash to being counting
					while (!stringWalker.IsEof)
					{
						if (stringWalker.CurrentCharacter != '-')
						{
							_ = stringWalker.ReadCharacter();
							
							if (!firstLocateDashes && shouldCountSpaceBetweenColumns)
								spaceBetweenColumnsCount++;
						}
						else
						{
							break;
						}
					}

					NewListModelSession.ShouldLocateDashes = false;
				}
				
				shouldCountSpaceBetweenColumns = false;

				// Count the '-' (dashes) to know the character length of each column.
				if (stringWalker.CurrentCharacter != '-')
				{
					if (NewListModelSession.LengthOfTemplateNameColumn is null)
						NewListModelSession.LengthOfTemplateNameColumn = NewListModelSession.DashCounter;
					else if (NewListModelSession.LengthOfShortNameColumn is null)
						NewListModelSession.LengthOfShortNameColumn = NewListModelSession.DashCounter;
					else if (NewListModelSession.LengthOfLanguageColumn is null)
						NewListModelSession.LengthOfLanguageColumn = NewListModelSession.DashCounter;
					else if (NewListModelSession.LengthOfTagsColumn is null)
					{
						NewListModelSession.LengthOfTagsColumn = NewListModelSession.DashCounter;
						NewListModelSession.ShouldCountDashes = false;

						// Prep for the next step
						NewListModelSession.ColumnLength = NewListModelSession.LengthOfTemplateNameColumn;
					}

					NewListModelSession.DashCounter = 0;
					NewListModelSession.ShouldLocateDashes = true;

					// If there were to be only one space character, the end of the while loop would read a dash.
					_ = stringWalker.BacktrackCharacter();
				}

				NewListModelSession.DashCounter++;
			}
			else
			{
				/*
				var startPositionIndex = stringWalker.PositionIndex;
				
				var templateName_StartInclusiveIndex = 0;
				var templateName_EndExclusiveIndex = templateName_StartInclusiveIndex + NewListModelSession.LengthOfTemplateNameColumn;
				
				var shortName_StartInclusiveIndex = templateName_EndExclusiveIndex + spaceBetweenColumnsCount;
				var shortName_EndExclusiveIndex = shortName_StartInclusiveIndex + NewListModelSession.LengthOfShortNameColumn;
				
				var language_StartInclusiveIndex = shortName_EndExclusiveIndex + spaceBetweenColumnsCount;
				var language_EndExclusiveIndex = language_StartInclusiveIndex + NewListModelSession.LengthOfLanguageColumn;
				
				var tags_StartInclusiveIndex = language_EndExclusiveIndex + spaceBetweenColumnsCount;
				var tags_EndExclusiveIndex = tags_StartInclusiveIndex + NewListModelSession.LengthOfTagsColumn;
				
				var columnWasEmpty = false;
				*/
				
				if (isFirstColumn)
					isFirstColumn = false;
				else
					stringWalker.SkipRange(spaceBetweenColumnsCount);

				/*			
				// Skip whitespace
				while (!stringWalker.IsEof)
				{
					// TODO: What if a column starts with a lot of whitespace?
					if (char.IsWhiteSpace(stringWalker.CurrentCharacter))
					{
						_ = stringWalker.ReadCharacter();
						whitespaceWasRead = true;
						
						if (startPositionIndex + NewListModelSession.ColumnLength < stringWalker.PositionIndex)
							columnWasEmpty = true;
					}
					else
					{
						break;
					}
				}
				*/

				for (int i = 0; i < NewListModelSession.ColumnLength; i++)
				{
					NewListModelSession.ColumnBuilder.Append(stringWalker.ReadCharacter());
				}

				if (NewListModelSession.ProjectTemplate.TemplateName is null)
				{
					NewListModelSession.ProjectTemplate = NewListModelSession.ProjectTemplate with
					{
						TemplateName = NewListModelSession.ColumnBuilder.ToString().Trim()
					};

					NewListModelSession.ColumnLength = NewListModelSession.LengthOfShortNameColumn;
				}
				else if (NewListModelSession.ProjectTemplate.ShortName is null)
				{
					NewListModelSession.ProjectTemplate = NewListModelSession.ProjectTemplate with
					{
						ShortName = NewListModelSession.ColumnBuilder.ToString().Trim()
					};

					NewListModelSession.ColumnLength = NewListModelSession.LengthOfLanguageColumn;
				}
				else if (NewListModelSession.ProjectTemplate.Language is null)
				{
					NewListModelSession.ProjectTemplate = NewListModelSession.ProjectTemplate with
					{
						Language = NewListModelSession.ColumnBuilder.ToString().Trim()
					};

					NewListModelSession.ColumnLength = NewListModelSession.LengthOfTagsColumn;
				}
				else if (NewListModelSession.ProjectTemplate.Tags is null)
				{
					NewListModelSession.ProjectTemplate = NewListModelSession.ProjectTemplate with
					{
						Tags = NewListModelSession.ColumnBuilder.ToString().Trim()
					};

					NewListModelSession.ProjectTemplateList.Add(NewListModelSession.ProjectTemplate);

					NewListModelSession.ProjectTemplate = new(null, null, null, null);
					NewListModelSession.ColumnLength = NewListModelSession.LengthOfTemplateNameColumn;
					
					isFirstColumn = true;
				}

				NewListModelSession.ColumnBuilder = new();
			}

			if (!whitespaceWasRead)
				_ = stringWalker.ReadCharacter();
		}

		ProjectTemplateList = NewListModelSession.ProjectTemplateList;

		return new();
	}

	public List<string> ParseOutputLineDotNetTestListTests(string outputEntire)
	{
		var textIndicatorForTheList = "The following Tests are available:";
		var indicatorIndex = outputEntire.IndexOf(textIndicatorForTheList);
	
		if (indicatorIndex != -1)
		{
			var theFollowingTestsAreAvailableList = new List<string>();
			var outputIndex = indicatorIndex;
			var hasFoundFirstLineAfterIndicator = false;
			
			while (outputIndex < outputEntire.Length)
			{
				var character = outputEntire[outputIndex];
				
				if (!hasFoundFirstLineAfterIndicator)
				{
					if (character == '\r')
					{
						// Peek for "\r\n"
						var peekIndex = outputIndex + 1;
						if (peekIndex < outputEntire.Length)
						{
							if (outputEntire[peekIndex] == '\n')
								outputIndex++;
						}
						
						hasFoundFirstLineAfterIndicator = true;
					}
					else if (character == '\n')
					{
						hasFoundFirstLineAfterIndicator = true;
					}
				}
				else
				{
					// Read line by line each test's fully qualified name
					if (character == '\t' || character == ' ')
					{
						if (character == '\t')
						{
							outputIndex++;
						}
						if (character == ' ')
						{
							// This code skips 4 space characters
							while (outputIndex < outputEntire.Length)
							{
								if (outputEntire[outputIndex] == ' ')
									outputIndex++;
								else
									break;
							}
						}
					
						var startInclusiveIndex = outputIndex;
						var endExclusiveIndex = -1; // Exclusive because don't include the line ending
						
						while (outputIndex < outputEntire.Length)
						{
							if (outputEntire[outputIndex] == '\r')
							{
								endExclusiveIndex = outputIndex;
							
								// Peek for "\r\n"
								var peekIndex = outputIndex + 1;
								if (peekIndex < outputEntire.Length)
								{
									if (outputEntire[peekIndex] == '\n')
										outputIndex++;
								}
							}
							else if (outputEntire[outputIndex] == '\n')
							{
								endExclusiveIndex = outputIndex;
							}
							
							if (endExclusiveIndex != -1)
								break;
							
							outputIndex++;
						}
						
						// If final test didn't end with a newline. (this is a presumed possibility, NOT backed up by fact)
						if (endExclusiveIndex == -1 && outputIndex == outputEntire.Length)
							endExclusiveIndex = outputEntire.Length;
					
						theFollowingTestsAreAvailableList.Add(
							outputEntire.Substring(startInclusiveIndex, endExclusiveIndex - startInclusiveIndex));
					}
					else
					{
						// The line did not start with '\t' or etc... therefore skip to the next line
						while (outputIndex < outputEntire.Length)
						{
							if (outputEntire[outputIndex] == '\r')
							{
								// Peek for "\r\n"
								var peekIndex = outputIndex + 1;
								if (peekIndex < outputEntire.Length)
								{
									if (outputEntire[peekIndex] == '\n')
										outputIndex++;
								}
								
								break;
							}
							else if (outputEntire[outputIndex] == '\n')
							{
								break;
							}
							
							outputIndex++;
						}
					}
				}
				
				outputIndex++;
			}
			
			return theFollowingTestsAreAvailableList;
		}
		else
		{
			return null;
		}
	}

	public class NewListModel
	{
		public bool ShouldLocateKeywordTags { get; set; } = true;
		public bool ShouldCountDashes { get; set; } = true;
		public bool ShouldLocateDashes { get; set; } = true;
		public int DashCounter { get; set; } = 0;
		public int? LengthOfTemplateNameColumn { get; set; } = null;
		public int? LengthOfShortNameColumn { get; set; } = null;
		public int? LengthOfLanguageColumn { get; set; } = null;
		public int? LengthOfTagsColumn { get; set; } = null;
		public StringBuilder ColumnBuilder { get; set; } = new StringBuilder();
		public int? ColumnLength { get; set; } = null;
		public ProjectTemplate ProjectTemplate { get; set; } = new ProjectTemplate(null, null, null, null);
		public List<ProjectTemplate> ProjectTemplateList { get; set; } = new List<ProjectTemplate>();
	}
	/* End DotNetCliOutputParser */
	
	/* Start IDotNetCommandFactory */
	private List<TreeViewNoType> _nodeList = new();
    private TreeViewNamespacePath? _nodeOfViewModel = null;

	public void Initialize()
	{
		// NuGetPackageManagerContext
		{
			_ = ContextFacts.GlobalContext.Keymap.TryRegister(
				new KeymapArgs
				{
					Key = "n",
					Code = "KeyN",
					ShiftKey = false,
					CtrlKey = true,
					AltKey = true,
					MetaKey = false,
					LayerKey = Key<KeymapLayer>.Empty,
				},
				ContextHelper.ConstructFocusContextElementCommand(
					ContextFacts.NuGetPackageManagerContext, "Focus: NuGetPackageManager", "focus-nu-get-package-manager", CommonService.JsRuntimeCommonApi, CommonService));
		}
		// CSharpReplContext
		{
			_ = ContextFacts.GlobalContext.Keymap.TryRegister(
				new KeymapArgs
				{
					Key = "r",
					Code = "KeyR",
					ShiftKey = false,
					CtrlKey = true,
					AltKey = true,
					MetaKey = false,
					LayerKey = Key<KeymapLayer>.Empty,
				},
				ContextHelper.ConstructFocusContextElementCommand(
					ContextFacts.SolutionExplorerContext, "Focus: C# REPL", "focus-c-sharp-repl", CommonService.JsRuntimeCommonApi, CommonService));
		}
		// SolutionExplorerContext
		{
			var focusSolutionExplorerCommand = ContextHelper.ConstructFocusContextElementCommand(
				ContextFacts.SolutionExplorerContext, "Focus: SolutionExplorer", "focus-solution-explorer", CommonService.JsRuntimeCommonApi, CommonService);

			_ = ContextFacts.GlobalContext.Keymap.TryRegister(
					new KeymapArgs
					{
						Key = "s",
						Code = "KeyS",
						ShiftKey = false,
						CtrlKey = true,
						AltKey = true,
						MetaKey = false,
						LayerKey = Key<KeymapLayer>.Empty,
					},
					focusSolutionExplorerCommand);

			// Set active solution explorer tree view node to be the
			// active text editor view model and,
			// Set focus to the solution explorer;
			{
				var focusTextEditorCommand = new CommonCommand(
					"Focus: SolutionExplorer (with text editor view model)", "focus-solution-explorer_with-text-editor-view-model", false,
					async commandArgs =>
					{
						await PerformGetFlattenedTree().ConfigureAwait(false);

						var localNodeOfViewModel = _nodeOfViewModel;

						if (localNodeOfViewModel is null)
							return;

						CommonService.TreeView_SetActiveNodeAction(
							DotNetSolutionState.TreeViewSolutionExplorerStateKey,
							localNodeOfViewModel,
							false,
							false);

						var elementId = CommonService.TreeView_GetActiveNodeElementId(DotNetSolutionState.TreeViewSolutionExplorerStateKey);

						await focusSolutionExplorerCommand.CommandFunc
							.Invoke(commandArgs)
							.ConfigureAwait(false);
					});

				_ = ContextFacts.GlobalContext.Keymap.TryRegister(
						new KeymapArgs
						{
							Key = "S",
							Code = "KeyS",
							CtrlKey = true,
							ShiftKey = true,
							AltKey = true,
							MetaKey = false,
							LayerKey = Key<KeymapLayer>.Empty,
						},
						focusTextEditorCommand);
			}
		}
	}

    private async Task PerformGetFlattenedTree()
    {
		_nodeList.Clear();

		var group = TextEditorService.Group_GetOrDefault(IdeService.EditorTextEditorGroupKey);

		if (group is not null)
		{
			var textEditorViewModel = TextEditorService.ViewModel_GetOrDefault(group.ActiveViewModelKey);

			if (textEditorViewModel is not null)
			{
				if (CommonService.TryGetTreeViewContainer(
						DotNetSolutionState.TreeViewSolutionExplorerStateKey,
						out var treeViewContainer) &&
                    treeViewContainer is not null)
				{
					await RecursiveGetFlattenedTree(treeViewContainer.RootNode, textEditorViewModel).ConfigureAwait(false);
				}
			}
		}
    }

    private async Task RecursiveGetFlattenedTree(
        TreeViewNoType treeViewNoType,
        TextEditorViewModel textEditorViewModel)
    {
        _nodeList.Add(treeViewNoType);

        if (treeViewNoType is TreeViewNamespacePath treeViewNamespacePath)
        {
            if (textEditorViewModel is not null)
            {
                var viewModelAbsolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(
                    textEditorViewModel.PersistentState.ResourceUri.Value,
                    false);

                if (viewModelAbsolutePath.Value ==
                        treeViewNamespacePath.Item.AbsolutePath.Value)
                {
                    _nodeOfViewModel = treeViewNamespacePath;
                }
            }

            switch (treeViewNamespacePath.Item.AbsolutePath.ExtensionNoPeriod)
            {
                case ExtensionNoPeriodFacts.C_SHARP_PROJECT:
                    await treeViewNamespacePath.LoadChildListAsync().ConfigureAwait(false);
                    break;
            }
        }

        await treeViewNoType.LoadChildListAsync().ConfigureAwait(false);

        foreach (var node in treeViewNoType.ChildList)
        {
            await RecursiveGetFlattenedTree(node, textEditorViewModel).ConfigureAwait(false);
        }
    }
	/* End IDotNetCommandFactory */
	
	/* Start IDotNetMenuOptionsFactory */
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
					nameof(IDeleteFileFormRendererType.OnAfterSubmitFunc),
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
			widgetRendererType: IdeService.IdeComponentRenderers.FileFormRendererType,
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
	/* End IDotNetMenuOptionsFactory */
	
	/* Start DotNetBackgroundTaskApi */
	#region DotNetSolutionIdeApi
	// private readonly IServiceProvider _serviceProvider;
	
	private readonly Key<TerminalCommandRequest> _newDotNetSolutionTerminalCommandRequestKey = Key<TerminalCommandRequest>.NewKey();
    private readonly CancellationTokenSource _newDotNetSolutionCancellationTokenSource = new();
	#endregion

    private Key<PanelGroup> _leftPanelGroupKey;
    private Key<Panel> _solutionExplorerPanelKey;

    private static readonly Key<IDynamicViewModel> _newDotNetSolutionDialogKey = Key<IDynamicViewModel>.NewKey();

    public async ValueTask Do_SolutionExplorer_TreeView_MultiSelect_DeleteFiles(TreeViewCommandArgs commandArgs)
    {
        foreach (var node in commandArgs.TreeViewContainer.SelectedNodeList)
        {
            var treeViewNamespacePath = (TreeViewNamespacePath)node;

            if (treeViewNamespacePath.Item.AbsolutePath.IsDirectory)
            {
                await IdeService.TextEditorService.CommonService.FileSystemProvider.Directory
                    .DeleteAsync(treeViewNamespacePath.Item.AbsolutePath.Value, true, CancellationToken.None)
                    .ConfigureAwait(false);
            }
            else
            {
                await IdeService.TextEditorService.CommonService.FileSystemProvider.File
                    .DeleteAsync(treeViewNamespacePath.Item.AbsolutePath.Value)
                    .ConfigureAwait(false);
            }

            if (IdeService.TextEditorService.CommonService.TryGetTreeViewContainer(commandArgs.TreeViewContainer.Key, out var mostRecentContainer) &&
                mostRecentContainer is not null)
            {
                var localParent = node.Parent;

                if (localParent is not null)
                {
                    await localParent.LoadChildListAsync().ConfigureAwait(false);
                    IdeService.TextEditorService.CommonService.TreeView_ReRenderNodeAction(mostRecentContainer.Key, localParent);
                }
            }
        }
    }

    public ValueTask Do_WalkExtensionsDotNetInitializerOnInit()
    {
        InitializePanelTabs();
        Initialize();
        return ValueTask.CompletedTask;
    }

    private void InitializePanelTabs()
    {
        InitializeLeftPanelTabs();
        InitializeRightPanelTabs();
        InitializeBottomPanelTabs();
    }

    private void InitializeLeftPanelTabs()
    {
        var leftPanel = PanelFacts.GetTopLeftPanelGroup(IdeService.TextEditorService.CommonService.GetPanelState());
        leftPanel.CommonService = IdeService.TextEditorService.CommonService;

        // solutionExplorerPanel
        var solutionExplorerPanel = new Panel(
            "Solution Explorer",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            ContextFacts.SolutionExplorerContext.ContextKey,
            typeof(SolutionExplorerDisplay),
            null,
            IdeService.TextEditorService.CommonService);
        IdeService.TextEditorService.CommonService.RegisterPanel(solutionExplorerPanel);
        IdeService.TextEditorService.CommonService.RegisterPanelTab(leftPanel.Key, solutionExplorerPanel, false);

        // SetActivePanelTabAction
        //
        // HACK: capture the variables and do it in OnAfterRender so it doesn't get overwritten by the IDE
        // 	  settings the active panel tab to the folder explorer.
        _leftPanelGroupKey = leftPanel.Key;
        _solutionExplorerPanelKey = solutionExplorerPanel.Key;
    }

    private void InitializeRightPanelTabs()
    {
        var rightPanel = PanelFacts.GetTopRightPanelGroup(IdeService.TextEditorService.CommonService.GetPanelState());
        rightPanel.CommonService = IdeService.TextEditorService.CommonService;

        /*
        // compilerServiceExplorerPanel
        var compilerServiceExplorerPanel = new Panel(
            "Compiler Service Explorer",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            ContextFacts.CompilerServiceExplorerContext.ContextKey,
            typeof(CompilerServiceExplorerDisplay),
            null,
            IdeService.TextEditorService.CommonService);
        IdeService.TextEditorService.CommonService.RegisterPanel(compilerServiceExplorerPanel);
        IdeService.TextEditorService.CommonService.RegisterPanelTab(rightPanel.Key, compilerServiceExplorerPanel, false);
        */
        
        /*// compilerServiceEditorPanel
        var compilerServiceEditorPanel = new Panel(
            "Compiler Service Editor",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            ContextFacts.CompilerServiceEditorContext.ContextKey,
            typeof(CompilerServiceEditorDisplay),
            null,
            _panelService,
            _dialogService,
            _commonBackgroundTaskApi);
        _panelService.RegisterPanel(compilerServiceEditorPanel);
        _panelService.RegisterPanelTab(rightPanel.Key, compilerServiceEditorPanel, false);*/
    }

    private void InitializeBottomPanelTabs()
    {
        var bottomPanel = PanelFacts.GetBottomPanelGroup(IdeService.TextEditorService.CommonService.GetPanelState());
        bottomPanel.CommonService = IdeService.TextEditorService.CommonService;

        // outputPanel
        var outputPanel = new Panel(
            "Output",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            ContextFacts.OutputContext.ContextKey,
            typeof(OutputPanelDisplay),
            null,
            IdeService.TextEditorService.CommonService);
        IdeService.TextEditorService.CommonService.RegisterPanel(outputPanel);
        IdeService.TextEditorService.CommonService.RegisterPanelTab(bottomPanel.Key, outputPanel, false);

        // testExplorerPanel
        var testExplorerPanel = new Panel(
            "Test Explorer",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            ContextFacts.TestExplorerContext.ContextKey,
            typeof(TestExplorerDisplay),
            null,
            IdeService.TextEditorService.CommonService);
        IdeService.TextEditorService.CommonService.RegisterPanel(testExplorerPanel);
        IdeService.TextEditorService.CommonService.RegisterPanelTab(bottomPanel.Key, testExplorerPanel, false);
        // This UI has resizable parts that need to be initialized.
        ReduceInitializeResizeHandleDimensionUnitAction(
            new DimensionUnit(
                () => IdeService.TextEditorService.CommonService.GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_COLUMN));

        // nuGetPanel
        var nuGetPanel = new Panel(
            "NuGet",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            ContextFacts.NuGetPackageManagerContext.ContextKey,
            typeof(NuGetPackageManager),
            null,
            IdeService.TextEditorService.CommonService);
        IdeService.TextEditorService.CommonService.RegisterPanel(nuGetPanel);
        IdeService.TextEditorService.CommonService.RegisterPanelTab(bottomPanel.Key, nuGetPanel, false);
        
        // SetActivePanelTabAction
        IdeService.TextEditorService.CommonService.SetActivePanelTab(bottomPanel.Key, outputPanel.Key);
    }

    public ValueTask Do_WalkExtensionsDotNetInitializerOnAfterRender()
    {
        var menuOptionOpenDotNetSolution = new MenuOptionRecord(
            ".NET Solution",
            MenuOptionKind.Other,
            () =>
            {
                DotNetSolutionState.ShowInputFile(IdeService, this);
                return Task.CompletedTask;
            });

        IdeService.Ide_ModifyMenuFile(
            inMenu =>
            {
                var indexMenuOptionOpen = inMenu.MenuOptionList.FindIndex(x => x.DisplayName == "Open");

                if (indexMenuOptionOpen == -1)
                {
                    var copyList = new List<MenuOptionRecord>(inMenu.MenuOptionList);
                    copyList.Add(menuOptionOpenDotNetSolution);
                    return inMenu with
                    {
                        MenuOptionList = copyList
                    };
                }

                var menuOptionOpen = inMenu.MenuOptionList[indexMenuOptionOpen];

                if (menuOptionOpen.SubMenu is null)
                    menuOptionOpen.SubMenu = new MenuRecord(new List<MenuOptionRecord>());

                // UI foreach enumeration was modified nightmare. (2025-02-07)
                var copySubMenuList = new List<MenuOptionRecord>(menuOptionOpen.SubMenu.MenuOptionList);
                copySubMenuList.Add(menuOptionOpenDotNetSolution);

                menuOptionOpen.SubMenu = menuOptionOpen.SubMenu with
                {
                    MenuOptionList = copySubMenuList
                };

                // Menu Option New
                {
                    var menuOptionNewDotNetSolution = new MenuOptionRecord(
                        ".NET Solution",
                        MenuOptionKind.Other,
                        OpenNewDotNetSolutionDialog);

                    var menuOptionNew = new MenuOptionRecord(
                        "New",
                        MenuOptionKind.Other,
                        subMenu: new MenuRecord(new List<MenuOptionRecord> { menuOptionNewDotNetSolution }));

                    var copyMenuOptionList = new List<MenuOptionRecord>(inMenu.MenuOptionList);
                    copyMenuOptionList.Insert(0, menuOptionNew);

                    return inMenu with
                    {
                        MenuOptionList = copyMenuOptionList
                    };
                }
            });

        InitializeMenuRun();

        IdeService.TextEditorService.CommonService.SetActivePanelTab(_leftPanelGroupKey, _solutionExplorerPanelKey);

        var compilerService = IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.C_SHARP_CLASS);

        /*if (compilerService is CSharpCompilerService cSharpCompilerService)
		{
			cSharpCompilerService.SetSymbolRendererType(typeof(Walk.Extensions.DotNet.TextEditors.Displays.CSharpSymbolDisplay));
		}*/

        IdeService.TextEditorService.UpsertHeader("cs", typeof(Walk.Extensions.CompilerServices.Displays.TextEditorCompilerServiceHeaderDisplay));

        return ValueTask.CompletedTask;
    }

    private void InitializeMenuRun()
    {
        var menuOptionsList = new List<MenuOptionRecord>();

        // Menu Option Build Project (startup project)
        menuOptionsList.Add(new MenuOptionRecord(
            "Build Project (startup project)",
            MenuOptionKind.Create,
            () =>
            {
                var startupControlState = IdeService.GetIdeStartupControlState();
                var activeStartupControl = startupControlState.StartupControlList.FirstOrDefault(
    	            x => x.Key == startupControlState.ActiveStartupControlKey);

                if (activeStartupControl?.StartupProjectAbsolutePath is not null)
                    BuildProjectOnClick(activeStartupControl.StartupProjectAbsolutePath.Value);
                else
                    NotificationHelper.DispatchError(nameof(BuildProjectOnClick), "activeStartupControl?.StartupProjectAbsolutePath was null", IdeService.TextEditorService.CommonService, TimeSpan.FromSeconds(6));
                return Task.CompletedTask;
            }));

        // Menu Option Clean (startup project)
        menuOptionsList.Add(new MenuOptionRecord(
            "Clean Project (startup project)",
            MenuOptionKind.Create,
            () =>
            {
                var startupControlState = IdeService.GetIdeStartupControlState();
                var activeStartupControl = startupControlState.StartupControlList.FirstOrDefault(
    	            x => x.Key == startupControlState.ActiveStartupControlKey);

                if (activeStartupControl?.StartupProjectAbsolutePath is not null)
                    CleanProjectOnClick(activeStartupControl.StartupProjectAbsolutePath.Value);
                else
                    NotificationHelper.DispatchError(nameof(CleanProjectOnClick), "activeStartupControl?.StartupProjectAbsolutePath was null", IdeService.TextEditorService.CommonService, TimeSpan.FromSeconds(6));
                return Task.CompletedTask;
            }));

        // Menu Option Build Solution
        menuOptionsList.Add(new MenuOptionRecord(
            "Build Solution",
            MenuOptionKind.Delete,
            () =>
            {
                var dotNetSolutionState = GetDotNetSolutionState();
                var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionModel;

                if (dotNetSolutionModel?.AbsolutePath is not null)
                    BuildSolutionOnClick(dotNetSolutionModel.AbsolutePath.Value);
                else
                    NotificationHelper.DispatchError(nameof(BuildSolutionOnClick), "dotNetSolutionModel?.AbsolutePath was null", IdeService.TextEditorService.CommonService, TimeSpan.FromSeconds(6));
                return Task.CompletedTask;
            }));

        // Menu Option Clean Solution
        menuOptionsList.Add(new MenuOptionRecord(
            "Clean Solution",
            MenuOptionKind.Delete,
            () =>
            {
                var dotNetSolutionState = GetDotNetSolutionState();
                var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionModel;

                if (dotNetSolutionModel?.AbsolutePath is not null)
                    CleanSolutionOnClick(dotNetSolutionModel.AbsolutePath.Value);
                else
                    NotificationHelper.DispatchError(nameof(CleanSolutionOnClick), "dotNetSolutionModel?.AbsolutePath was null", IdeService.TextEditorService.CommonService, TimeSpan.FromSeconds(6));
                return Task.CompletedTask;
            }));

        IdeService.Ide_ModifyMenuRun(inMenu =>
        {
            // UI foreach enumeration was modified nightmare. (2025-02-07)
            var copyMenuOptionList = new List<MenuOptionRecord>(inMenu.MenuOptionList);
            copyMenuOptionList.AddRange(menuOptionsList);
            return inMenu with
            {
                MenuOptionList = copyMenuOptionList
            };
        });
    }

    private void BuildProjectOnClick(string projectAbsolutePathString)
    {
        var formattedCommand = DotNetCliCommandFormatter.FormatDotnetBuildProject(projectAbsolutePathString);
        var solutionAbsolutePath = IdeService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(projectAbsolutePathString, false);

        var localParentDirectory = solutionAbsolutePath.ParentDirectory;
        if (localParentDirectory is null)
            return;

        var terminalCommandRequest = new TerminalCommandRequest(
            formattedCommand.Value,
            localParentDirectory)
        {
            BeginWithFunc = parsedCommand =>
            {
                ParseOutputEntireDotNetRun(
                    string.Empty,
                    "Build-Project_started");
                return Task.CompletedTask;
            },
            ContinueWithFunc = parsedCommand =>
            {
                ParseOutputEntireDotNetRun(
                    parsedCommand.OutputCache.ToString(),
                    "Build-Project_completed");
                return Task.CompletedTask;
            }
        };

        IdeService.GetTerminalState().TerminalMap[TerminalFacts.GENERAL_KEY].EnqueueCommand(terminalCommandRequest);
    }

    private void CleanProjectOnClick(string projectAbsolutePathString)
    {
        var formattedCommand = DotNetCliCommandFormatter.FormatDotnetCleanProject(projectAbsolutePathString);
        var solutionAbsolutePath = IdeService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(projectAbsolutePathString, false);

        var localParentDirectory = solutionAbsolutePath.ParentDirectory;
        if (localParentDirectory is null)
            return;

        var terminalCommandRequest = new TerminalCommandRequest(
            formattedCommand.Value,
            localParentDirectory)
        {
            BeginWithFunc = parsedCommand =>
            {
                ParseOutputEntireDotNetRun(
                    string.Empty,
                    "Clean-Project_started");
                return Task.CompletedTask;
            },
            ContinueWithFunc = parsedCommand =>
            {
                ParseOutputEntireDotNetRun(
                    parsedCommand.OutputCache.ToString(),
                    "Clean-Project_completed");
                return Task.CompletedTask;
            }
        };

        IdeService.GetTerminalState().TerminalMap[TerminalFacts.GENERAL_KEY].EnqueueCommand(terminalCommandRequest);
    }

    private void BuildSolutionOnClick(string solutionAbsolutePathString)
    {
        var formattedCommand = DotNetCliCommandFormatter.FormatDotnetBuildSolution(solutionAbsolutePathString);
        var solutionAbsolutePath = IdeService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(solutionAbsolutePathString, false);

        var localParentDirectory = solutionAbsolutePath.ParentDirectory;
        if (localParentDirectory is null)
            return;

        var terminalCommandRequest = new TerminalCommandRequest(
            formattedCommand.Value,
            localParentDirectory)
        {
            BeginWithFunc = parsedCommand =>
            {
                ParseOutputEntireDotNetRun(
                    string.Empty,
                    "Build-Solution_started");
                return Task.CompletedTask;
            },
            ContinueWithFunc = parsedCommand =>
            {
                ParseOutputEntireDotNetRun(
                    parsedCommand.OutputCache.ToString(),
                    "Build-Solution_completed");
                return Task.CompletedTask;
            }
        };

        IdeService.GetTerminalState().TerminalMap[TerminalFacts.GENERAL_KEY].EnqueueCommand(terminalCommandRequest);
    }

    private void CleanSolutionOnClick(string solutionAbsolutePathString)
    {
        var formattedCommand = DotNetCliCommandFormatter.FormatDotnetCleanSolution(solutionAbsolutePathString);
        var solutionAbsolutePath = IdeService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(solutionAbsolutePathString, false);

        var localParentDirectory = solutionAbsolutePath.ParentDirectory;
        if (localParentDirectory is null)
            return;

        var terminalCommandRequest = new TerminalCommandRequest(
            formattedCommand.Value,
            localParentDirectory)
        {
            BeginWithFunc = parsedCommand =>
            {
                ParseOutputEntireDotNetRun(
                    string.Empty,
                    "Clean-Solution_started");
                return Task.CompletedTask;
            },
            ContinueWithFunc = parsedCommand =>
            {
                ParseOutputEntireDotNetRun(
                    parsedCommand.OutputCache.ToString(),
                    "Clean-Solution_completed");
                return Task.CompletedTask;
            }
        };

        IdeService.GetTerminalState().TerminalMap[TerminalFacts.GENERAL_KEY].EnqueueCommand(terminalCommandRequest);
    }

    private Task OpenNewDotNetSolutionDialog()
    {
        var dialogRecord = new DialogViewModel(
            _newDotNetSolutionDialogKey,
            "New .NET Solution",
            typeof(DotNetSolutionFormDisplay),
            null,
            null,
            true,
            null);

        IdeService.TextEditorService.CommonService.Dialog_ReduceRegisterAction(dialogRecord);
        return Task.CompletedTask;
    }

    public async ValueTask Do_SubmitNuGetQuery(INugetPackageManagerQuery query)
    {
        var localNugetResult = await QueryForNugetPackagesAsync(query)
            .ConfigureAwait(false);

        ReduceSetMostRecentQueryResultAction(localNugetResult);
    }
    
    public ValueTask Do_RunTestByFullyQualifiedName(TreeViewStringFragment treeViewStringFragment, string fullyQualifiedName, TreeViewProjectTestModel treeViewProjectTestModel)
    {
        RunTestByFullyQualifiedName(
            treeViewStringFragment,
            fullyQualifiedName,
            treeViewProjectTestModel.Item.AbsolutePath.ParentDirectory);

        return ValueTask.CompletedTask;
    }

    private void RunTestByFullyQualifiedName(
        TreeViewStringFragment treeViewStringFragment,
        string fullyQualifiedName,
        string? directoryNameForTestDiscovery)
    {
        var dotNetTestByFullyQualifiedNameFormattedCommand = DotNetCliCommandFormatter
            .FormatDotNetTestByFullyQualifiedName(fullyQualifiedName);

        if (string.IsNullOrWhiteSpace(directoryNameForTestDiscovery) ||
            string.IsNullOrWhiteSpace(fullyQualifiedName))
        {
            return;
        }

        var terminalCommandRequest = new TerminalCommandRequest(
            dotNetTestByFullyQualifiedNameFormattedCommand.Value,
            directoryNameForTestDiscovery,
            treeViewStringFragment.Item.DotNetTestByFullyQualifiedNameFormattedTerminalCommandRequestKey)
        {
            BeginWithFunc = parsedCommand =>
            {
                treeViewStringFragment.Item.TerminalCommandParsed = parsedCommand;
                IdeService.TextEditorService.CommonService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, treeViewStringFragment);
                return Task.CompletedTask;
            },
            ContinueWithFunc = parsedCommand =>
            {
                treeViewStringFragment.Item.TerminalCommandParsed = parsedCommand;
                var output = treeViewStringFragment.Item.TerminalCommandParsed?.OutputCache.ToString() ?? null;

                if (output is not null && output.Contains("Duration:"))
                {
                    if (output.Contains("Passed!"))
                    {
                        ReduceWithAction(inState =>
                        {
                            var passedTestHashSet = new HashSet<string>(inState.PassedTestHashSet);
                            passedTestHashSet.Add(fullyQualifiedName);

                            var notRanTestHashSet = new HashSet<string>(inState.NotRanTestHashSet);
                            notRanTestHashSet.Remove(fullyQualifiedName);

                            var failedTestHashSet = new HashSet<string>(inState.FailedTestHashSet);
                            failedTestHashSet.Remove(fullyQualifiedName);

                            return inState with
                            {
                                PassedTestHashSet = passedTestHashSet,
                                NotRanTestHashSet = notRanTestHashSet,
                                FailedTestHashSet = failedTestHashSet,
                            };
                        });
                    }
                    else
                    {
                        ReduceWithAction(inState =>
                        {
							var failedTestHashSet = new HashSet<string>(inState.FailedTestHashSet);
							failedTestHashSet.Add(fullyQualifiedName);

							var notRanTestHashSet = new HashSet<string>(inState.NotRanTestHashSet);
							notRanTestHashSet.Remove(fullyQualifiedName);

							var passedTestHashSet = new HashSet<string>(inState.PassedTestHashSet);
							passedTestHashSet.Remove(fullyQualifiedName);

                            return inState with
                            {
                                FailedTestHashSet = failedTestHashSet,
                                NotRanTestHashSet = notRanTestHashSet,
                                PassedTestHashSet = passedTestHashSet,
                            };
                        });
                    }
                }

                IdeService.TextEditorService.CommonService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, treeViewStringFragment);
                return Task.CompletedTask;
            }
        };

        treeViewStringFragment.Item.TerminalCommandRequest = terminalCommandRequest;
        IdeService.GetTerminalState().TerminalMap[TerminalFacts.EXECUTION_KEY].EnqueueCommand(terminalCommandRequest);
    }
    
    #region DotNetSolutionIdeApi
    private async ValueTask Do_SetDotNetSolution(AbsolutePath inSolutionAbsolutePath)
	{
		var dotNetSolutionAbsolutePathString = inSolutionAbsolutePath.Value;

		var content = IdeService.TextEditorService.CommonService.FileSystemProvider.File.ReadAllText(dotNetSolutionAbsolutePathString);

		var solutionAbsolutePath = IdeService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(
			dotNetSolutionAbsolutePathString,
			false);

		var solutionNamespacePath = new NamespacePath(
			string.Empty,
			solutionAbsolutePath);

		var resourceUri = new ResourceUri(solutionAbsolutePath.Value);

		if (IdeService.TextEditorService.Model_GetOrDefault(resourceUri) is null)
		{
			IdeService.TextEditorService.WorkerArbitrary.PostUnique(editContext =>
			{
				var extension = ExtensionNoPeriodFacts.DOT_NET_SOLUTION;
				
				if (dotNetSolutionAbsolutePathString.EndsWith(ExtensionNoPeriodFacts.DOT_NET_SOLUTION_X))
					extension = ExtensionNoPeriodFacts.DOT_NET_SOLUTION_X;
			
				IdeService.TextEditorService.Model_RegisterTemplated(
					editContext,
					extension,
					resourceUri,
					DateTime.UtcNow,
					content);
	
				IdeService.TextEditorService
					.GetCompilerService(extension)
					.RegisterResource(
						resourceUri,
						shouldTriggerResourceWasModified: true);
			
				return ValueTask.CompletedTask;
			});
		}

		DotNetSolutionModel dotNetSolutionModel;

		if (dotNetSolutionAbsolutePathString.EndsWith(ExtensionNoPeriodFacts.DOT_NET_SOLUTION_X))
			dotNetSolutionModel = ParseSlnx(solutionAbsolutePath, resourceUri, content);
		else
			dotNetSolutionModel = ParseSln(solutionAbsolutePath, resourceUri, content);
		
		dotNetSolutionModel.DotNetProjectList = SortProjectReferences(dotNetSolutionModel);
		
		/*	
		// FindAllReferences
		var pathGroupList = new List<(string Name, string Path)>();
		foreach (var project in sortedByProjectReferenceDependenciesDotNetProjectList)
		{
			if (project.AbsolutePath.ParentDirectory is not null)
			{
				pathGroupList.Add((project.DisplayName, project.AbsolutePath.ParentDirectory));
			}
		}
		_findAllReferencesService.PathGroupList = pathGroupList;
		*/

		// TODO: If somehow model was registered already this won't write the state
		ReduceRegisterAction(dotNetSolutionModel);

		ReduceWithAction(new WithAction(
			inDotNetSolutionState => inDotNetSolutionState with
			{
				DotNetSolutionModelKey = dotNetSolutionModel.Key
			}));

		// TODO: Putting a hack for now to overwrite if somehow model was registered already
		ReduceWithAction(ConstructModelReplacement(
			dotNetSolutionModel.Key,
			dotNetSolutionModel));

		var dotNetSolutionCompilerService = (DotNetSolutionCompilerService)IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.DOT_NET_SOLUTION);

		dotNetSolutionCompilerService.ResourceWasModified(
			new ResourceUri(solutionAbsolutePath.Value),
			Array.Empty<TextEditorTextSpan>());

		var parentDirectory = solutionAbsolutePath.ParentDirectory;

		if (parentDirectory is not null)
		{
			IdeService.TextEditorService.CommonService.EnvironmentProvider.DeletionPermittedRegister(new(parentDirectory, true));

			IdeService.TextEditorService.SetStartingDirectoryPath(parentDirectory);

			IdeService.CodeSearch_With(inState => inState with
			{
				StartingAbsolutePathForSearch = parentDirectory
			});
			
			TerminalCommandRequest terminalCommandRequest;

            var slnFoundString = $"sln found: {solutionAbsolutePath.Value}";
            var prefix = TerminalInteractive.RESERVED_TARGET_FILENAME_PREFIX + nameof(DotNetService);

			// Set 'generalTerminal' working directory
			terminalCommandRequest = new TerminalCommandRequest(
	        	prefix + "_General",
	        	parentDirectory)
	        {
	        	BeginWithFunc = parsedCommand =>
	        	{
	        		IdeService.GetTerminalState().TerminalMap[TerminalFacts.GENERAL_KEY].TerminalOutput.WriteOutput(
						parsedCommand,
						// If newlines are added to this make sure to use '.ReplaceLineEndings("\n")' because the syntax highlighting and text editor are expecting this line ending.
						new StandardOutputCommandEvent(slnFoundString));
	        		return Task.CompletedTask;
	        	}
	        };
	        IdeService.GetTerminalState().TerminalMap[TerminalFacts.GENERAL_KEY].EnqueueCommand(terminalCommandRequest);

			// Set 'executionTerminal' working directory
			terminalCommandRequest = new TerminalCommandRequest(
	        	prefix + "_Execution",
	        	parentDirectory)
	        {
	        	BeginWithFunc = parsedCommand =>
	        	{
	        		IdeService.GetTerminalState().TerminalMap[TerminalFacts.EXECUTION_KEY].TerminalOutput.WriteOutput(
						parsedCommand,
						// If newlines are added to this make sure to use '.ReplaceLineEndings("\n")' because the syntax highlighting and text editor are expecting this line ending.
						new StandardOutputCommandEvent(slnFoundString));
	        		return Task.CompletedTask;
	        	}
	        };
			IdeService.GetTerminalState().TerminalMap[TerminalFacts.EXECUTION_KEY].EnqueueCommand(terminalCommandRequest);
		}
		
		try
		{
			await AppDataService.WriteAppDataAsync(new DotNetAppData
			{
				SolutionMostRecent = solutionAbsolutePath.Value
			});
		}
		catch (Exception e)
		{
			NotificationHelper.DispatchError(
		        $"ERROR: nameof(_appDataService.WriteAppDataAsync)",
		        e.ToString(),
		        IdeService.TextEditorService.CommonService,
		        TimeSpan.FromSeconds(5));
		}
		
		IdeService.TextEditorService.WorkerArbitrary.EnqueueUniqueTextEditorWork(
			new UniqueTextEditorWork(IdeService.TextEditorService, async editContext =>
            {
            	await ParseSolution(editContext, dotNetSolutionModel.Key, CompilationUnitKind.SolutionWide_DefinitionsOnly);
            	await ParseSolution(editContext, dotNetSolutionModel.Key, CompilationUnitKind.SolutionWide_MinimumLocalsData);

				// IdeService.TextEditorService.EditContext_GetText_Clear();
        	}));

		await Do_SetDotNetSolutionTreeView(dotNetSolutionModel.Key).ConfigureAwait(false);
	}
	
	public DotNetSolutionModel ParseSlnx(
		AbsolutePath solutionAbsolutePath,
		ResourceUri resourceUri,
		string content)
	{
    	var htmlSyntaxUnit = HtmlSyntaxTree.ParseText(
    		IdeService.TextEditorService,
    		IdeService.TextEditorService.__StringWalker,
			new(solutionAbsolutePath.Value),
			content);

		var syntaxNodeRoot = htmlSyntaxUnit.RootTagSyntax;

		var cSharpProjectSyntaxWalker = new CSharpProjectSyntaxWalker();

		cSharpProjectSyntaxWalker.Visit(syntaxNodeRoot);

		var dotNetProjectList = new List<IDotNetProject>();
		var solutionFolderList = new List<SolutionFolder>();

		var folderTagList = cSharpProjectSyntaxWalker.TagNodes
			.Where(ts => (ts.OpenTagNameNode?.TextEditorTextSpan.GetText(content, IdeService.TextEditorService) ?? string.Empty) == "Folder")
			.ToList();
    	
    	var projectTagList = cSharpProjectSyntaxWalker.TagNodes
			.Where(ts => (ts.OpenTagNameNode?.TextEditorTextSpan.GetText(content, IdeService.TextEditorService) ?? string.Empty) == "Project")
			.ToList();
		
		var solutionFolderPathHashSet = new HashSet<string>();
		
		var stringNestedProjectEntryList = new List<StringNestedProjectEntry>();
		
		foreach (var folder in folderTagList)
		{
			var attributeNameValueTuples = folder
				.AttributeNodes
				.Select(x => (
					x.AttributeNameSyntax.TextEditorTextSpan
						.GetText(content, IdeService.TextEditorService)
						.Trim(),
					x.AttributeValueSyntax.TextEditorTextSpan
						.GetText(content, IdeService.TextEditorService)
						.Replace("\"", string.Empty)
						.Replace("=", string.Empty)
						.Trim()))
				.ToArray();

			var attribute = attributeNameValueTuples.FirstOrDefault(x => x.Item1 == "Name");
			if (attribute.Item2 is null)
				continue;

			var ancestorDirectoryList = new List<string>();

			var absolutePath = new AbsolutePath(
				attribute.Item2,
				isDirectory: true,
				IdeService.TextEditorService.CommonService.EnvironmentProvider,
				ancestorDirectoryList);

			solutionFolderPathHashSet.Add(absolutePath.Value);
			
			for (int i = 0; i < ancestorDirectoryList.Count; i++)
			{
				if (i == 0)
					continue;
					
				solutionFolderPathHashSet.Add(ancestorDirectoryList[i]);
			}
			
			foreach (var child in folder.ChildContent)
			{
				if (child.HtmlSyntaxKind == HtmlSyntaxKind.TagSelfClosingNode ||
					child.HtmlSyntaxKind == HtmlSyntaxKind.TagClosingNode)
				{
					var tagNode = (TagNode)child;
					
					attributeNameValueTuples = tagNode
						.AttributeNodes
						.Select(x => (
							x.AttributeNameSyntax.TextEditorTextSpan
								.GetText(content, IdeService.TextEditorService)
								.Trim(),
							x.AttributeValueSyntax.TextEditorTextSpan
								.GetText(content, IdeService.TextEditorService)
								.Replace("\"", string.Empty)
								.Replace("=", string.Empty)
								.Trim()))
						.ToArray();
		
					attribute = attributeNameValueTuples.FirstOrDefault(x => x.Item1 == "Path");
					if (attribute.Item2 is null)
						continue;
						
					stringNestedProjectEntryList.Add(new StringNestedProjectEntry(
		    			ChildIsSolutionFolder: false,
					    attribute.Item2,
					    absolutePath.Value));
				}
			}
		}
		
		// I'm too tired to decide if enumerating a HashSet is safe
		var temporarySolutionFolderList = solutionFolderPathHashSet.ToList();
		
		foreach (var solutionFolderPath in temporarySolutionFolderList)
		{
			var absolutePath = new AbsolutePath(
				solutionFolderPath,
				isDirectory: true,
				IdeService.TextEditorService.CommonService.EnvironmentProvider);
			
			solutionFolderList.Add(new SolutionFolder(
		        absolutePath.NameNoExtension,
		        solutionFolderPath));
		}
		
		foreach (var project in projectTagList)
		{
			var attributeNameValueTuples = project
				.AttributeNodes
				.Select(x => (
					x.AttributeNameSyntax.TextEditorTextSpan
						.GetText(content, IdeService.TextEditorService)
						.Trim(),
					x.AttributeValueSyntax.TextEditorTextSpan
						.GetText(content, IdeService.TextEditorService)
						.Replace("\"", string.Empty)
						.Replace("=", string.Empty)
						.Trim()))
				.ToArray();

			var attribute = attributeNameValueTuples.FirstOrDefault(x => x.Item1 == "Path");
			if (attribute.Item2 is null)
				continue;

			var relativePath = new RelativePath(attribute.Item2, isDirectory: false, IdeService.TextEditorService.CommonService.EnvironmentProvider);

			dotNetProjectList.Add(new CSharpProjectModel(
		        relativePath.NameNoExtension,
		        Guid.Empty,
		        attribute.Item2,
		        Guid.Empty,
		        new(),
		        new(),
		        default(AbsolutePath)));
		}

    	var dotNetSolutionHeader = new DotNetSolutionHeader();
    	var dotNetSolutionGlobal = new DotNetSolutionGlobal();
    	
    	// You have to iterate in reverse so ascending will put longest words to shortest (when iterating reverse).
    	var childSolutionFolderList = solutionFolderList.OrderBy(x => x.ActualName).ToList();
    	var parentSolutionFolderList = new List<SolutionFolder>(childSolutionFolderList);
    	
    	for (int parentIndex = parentSolutionFolderList.Count - 1; parentIndex >= 0; parentIndex--)
    	{
    		var parentSolutionFolder = parentSolutionFolderList[parentIndex];
    		
	    	for (int childIndex = childSolutionFolderList.Count - 1; childIndex >= 0; childIndex--)
	    	{
	    		var childSolutionFolder = childSolutionFolderList[childIndex];
	    		
	    		if (childSolutionFolder.ActualName != parentSolutionFolder.ActualName &&
	    			childSolutionFolder.ActualName.StartsWith(parentSolutionFolder.ActualName))
	    		{
	    			stringNestedProjectEntryList.Add(new StringNestedProjectEntry(
		    			ChildIsSolutionFolder: true,
					    childSolutionFolder.ActualName,
					    parentSolutionFolder.ActualName));
					    
				    childSolutionFolderList.RemoveAt(childIndex);
	    		}
	    	}
    	}
	
		return new DotNetSolutionModel(
			solutionAbsolutePath,
			dotNetSolutionHeader,
			dotNetProjectList,
			solutionFolderList,
			guidNestedProjectEntryList: null,
			stringNestedProjectEntryList,
			dotNetSolutionGlobal,
			content);
	}
		
	public DotNetSolutionModel ParseSln(
		AbsolutePath solutionAbsolutePath,
		ResourceUri resourceUri,
		string content)
	{
		var lexer = new DotNetSolutionLexer(
			new StringWalker(),
			resourceUri,
			content);

		lexer.Lex();

		var parser = new DotNetSolutionParser(lexer);

		var compilationUnit = parser.Parse();

		return new DotNetSolutionModel(
			solutionAbsolutePath,
			parser.DotNetSolutionHeader,
			parser.DotNetProjectList,
			parser.SolutionFolderList,
			guidNestedProjectEntryList: parser.NestedProjectEntryList,
			null,
			parser.DotNetSolutionGlobal,
			content);
	}
	
	/// <summary>
	/// This solution is incomplete, the current code for this was just to get a "feel" for things.
	/// </summary>
	private List<IDotNetProject> SortProjectReferences(DotNetSolutionModel dotNetSolutionModel)
	{
		for (int i = dotNetSolutionModel.DotNetProjectList.Count - 1; i >= 0; i--)
		{
			var projectTuple = dotNetSolutionModel.DotNetProjectList[i];
			
			// Debugging Linux-Ubuntu (2024-04-28)
			// -----------------------------------
			// It is believed, that Linux-Ubuntu is not fully working correctly,
			// due to the directory separator character at the os level being '/',
			// meanwhile the .NET solution has as its directory separator character '\'.
			//
			// Will perform a string.Replace("\\", "/") here. And if it solves the issue,
			// then some standard way of doing this needs to be made available in the IEnvironmentProvider.
			//
			// Okay, this single replacement fixes 99% of the solution explorer issue.
			// And I say 99% instead of 100% just because I haven't tested every single part of it yet.
			var relativePathFromSolutionFileString = projectTuple.RelativePathFromSolutionFileString;
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				relativePathFromSolutionFileString = relativePathFromSolutionFileString.Replace("\\", "/");
			var absolutePathString = PathHelper.GetAbsoluteFromAbsoluteAndRelative(
				dotNetSolutionModel.AbsolutePath,
				relativePathFromSolutionFileString,
				IdeService.TextEditorService.CommonService.EnvironmentProvider);
			projectTuple.AbsolutePath = IdeService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(absolutePathString, false);
		
			if (!IdeService.TextEditorService.CommonService.FileSystemProvider.File.Exists(projectTuple.AbsolutePath.Value))
			{
				dotNetSolutionModel.DotNetProjectList.RemoveAt(i);
				continue;
			}
			
			projectTuple.ReferencedAbsolutePathList = new List<AbsolutePath>();
			
			var innerParentDirectory = projectTuple.AbsolutePath.ParentDirectory;
			if (innerParentDirectory is not null)
				IdeService.TextEditorService.CommonService.EnvironmentProvider.DeletionPermittedRegister(new(innerParentDirectory, true));
			
			var content = IdeService.TextEditorService.CommonService.FileSystemProvider.File.ReadAllText(projectTuple.AbsolutePath.Value);
	
			var htmlSyntaxUnit = HtmlSyntaxTree.ParseText(
				IdeService.TextEditorService,
				IdeService.TextEditorService.__StringWalker,
				new(projectTuple.AbsolutePath.Value),
				content);
	
			var syntaxNodeRoot = htmlSyntaxUnit.RootTagSyntax;
	
			var cSharpProjectSyntaxWalker = new CSharpProjectSyntaxWalker();
	
			cSharpProjectSyntaxWalker.Visit(syntaxNodeRoot);
	
			var projectReferences = cSharpProjectSyntaxWalker.TagNodes
				.Where(ts => (ts.OpenTagNameNode?.TextEditorTextSpan.GetText(content, IdeService.TextEditorService) ?? string.Empty) == "ProjectReference")
				.ToList();
	
			foreach (var projectReference in projectReferences)
			{
				var attributeNameValueTuples = projectReference
					.AttributeNodes
					.Select(x => (
						x.AttributeNameSyntax.TextEditorTextSpan
							.GetText(content, IdeService.TextEditorService)
							.Trim(),
						x.AttributeValueSyntax.TextEditorTextSpan
							.GetText(content, IdeService.TextEditorService)
							.Replace("\"", string.Empty)
							.Replace("=", string.Empty)
							.Trim()))
					.ToArray();
	
				var includeAttribute = attributeNameValueTuples.FirstOrDefault(x => x.Item1 == "Include");
	
				var referenceProjectAbsolutePathString = PathHelper.GetAbsoluteFromAbsoluteAndRelative(
					projectTuple.AbsolutePath,
					includeAttribute.Item2,
					IdeService.TextEditorService.CommonService.EnvironmentProvider);
	
				var referenceProjectAbsolutePath = IdeService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(
					referenceProjectAbsolutePathString,
					false);
	
				projectTuple.ReferencedAbsolutePathList.Add(referenceProjectAbsolutePath);
			}
		}
		
		var upperLimit = dotNetSolutionModel.DotNetProjectList.Count > 4 // Extremely arbitrary number being used here.
		    ? 4
		    : dotNetSolutionModel.DotNetProjectList.Count;
		for (int outerIndex = 0; outerIndex < upperLimit; outerIndex++)
		{
			for (int i = 0; i < dotNetSolutionModel.DotNetProjectList.Count; i++)
			{
				var projectTuple = dotNetSolutionModel.DotNetProjectList[i];
				
				foreach (var referenceAbsolutePath in projectTuple.ReferencedAbsolutePathList)
				{
					var referenceIndex = dotNetSolutionModel.DotNetProjectList
						.FindIndex(x => x.AbsolutePath.Value == referenceAbsolutePath.Value);
				
					if (referenceIndex > i)
					{
						var indexDestination = i - 1;
						if (indexDestination == -1)
							indexDestination = 0;
					
						MoveAndShiftList(
							dotNetSolutionModel.DotNetProjectList,
							indexSource: referenceIndex,
							indexDestination);
					}
				}
			}
		}
		
		return dotNetSolutionModel.DotNetProjectList;
	}
	
	private void MoveAndShiftList(
		List<IDotNetProject> enumeratingProjectTupleList,
		int indexSource,
		int indexDestination)
	{
		if (indexSource == 1 && indexDestination == 0)
		{
			var otherTemporary = enumeratingProjectTupleList[indexDestination];
			enumeratingProjectTupleList[indexDestination] = enumeratingProjectTupleList[indexSource];
			enumeratingProjectTupleList[indexSource] = otherTemporary;
			return;
		}
	
		var temporary = enumeratingProjectTupleList[indexDestination];
		enumeratingProjectTupleList[indexDestination] = enumeratingProjectTupleList[indexSource];
		
		for (int i = indexSource; i > indexDestination; i--)
		{
			if (i - 1 == indexDestination)
				enumeratingProjectTupleList[i] = temporary;
			else
				enumeratingProjectTupleList[i] = enumeratingProjectTupleList[i - 1];
		}
	}

	private async ValueTask ParseSolution(
		TextEditorEditContext editContext,
		Key<DotNetSolutionModel> dotNetSolutionModelKey,
		CompilationUnitKind compilationUnitKind)
	{
		var dotNetSolutionState = GetDotNetSolutionState();

		var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionsList.FirstOrDefault(
			x => x.Key == dotNetSolutionModelKey);

		if (dotNetSolutionModel is null)
			return;
		
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
			$"Parse: {dotNetSolutionModel.AbsolutePath.NameWithExtension}",
			progressBarModel,
			IdeService.TextEditorService.CommonService,
			TimeSpan.FromMilliseconds(-1));
			
		try
		{
			foreach (var project in dotNetSolutionModel.DotNetProjectList)
			{
				RegisterStartupControl(project);
			
				var resourceUri = new ResourceUri(project.AbsolutePath.Value);

				if (!await IdeService.TextEditorService.CommonService.FileSystemProvider.File.ExistsAsync(resourceUri.Value))
					continue; // TODO: This can still cause a race condition exception if the file is removed before the next line runs.
			}

			var previousStageProgress = 0.05;
			var dotNetProjectListLength = dotNetSolutionModel.DotNetProjectList.Count;
			var projectsParsedCount = 0;
			foreach (var project in dotNetSolutionModel.DotNetProjectList)
			{
				// foreach project in solution
				// 	foreach C# file in project
				// 		EnqueueBackgroundTask(async () =>
				// 		{
				// 			ParseCSharpFile();
				// 			UpdateProgressBar();
				// 		});
				//
				// Treat every project as an equal weighting with relation to remaining percent to complete
				// on the progress bar.
				//
				// If the project were to be parsed, how much would it move the percent progress completed by?
				//
				// Then, in order to see progress while each C# file in the project gets parsed,
				// multiply the percent progress this project can provide by the proportion
				// of the project's C# files which have been parsed.
				var maximumProgressAvailableToProject = (1 - previousStageProgress) * ((double)1.0 / dotNetProjectListLength);
				var currentProgress = Math.Min(1.0, previousStageProgress + maximumProgressAvailableToProject * projectsParsedCount);

				// This 'SetProgress' is being kept out the throttle, since it sets message 1
				// whereas the per class progress updates set message 2.
				//
				// Otherwise an update to message 2 could result in this message 1 update never being written.
				progressBarModel.SetProgress(
					currentProgress,
					project.AbsolutePath.NameWithExtension);
				
				cancellationToken.ThrowIfCancellationRequested();

				await DiscoverClassesInProject(editContext, project, progressBarModel, currentProgress, maximumProgressAvailableToProject, compilationUnitKind);
				projectsParsedCount++;
			}

			progressBarModel.SetProgress(1, $"Finished parsing: {dotNetSolutionModel.AbsolutePath.NameWithExtension}", string.Empty);
			progressBarModel.Dispose();
		}
		catch (Exception e)
		{
			if (e is OperationCanceledException)
				progressBarModel.IsCancelled = true;
				
			var currentProgress = progressBarModel.GetProgress();
			
			progressBarModel.SetProgress(currentProgress, e.ToString());
			progressBarModel.Dispose();
		}
	}

	private async Task DiscoverClassesInProject(
		TextEditorEditContext editContext, 
		IDotNetProject dotNetProject,
		ProgressBarModel progressBarModel,
		double currentProgress,
		double maximumProgressAvailableToProject,
		CompilationUnitKind compilationUnitKind)
	{
		if (!await IdeService.TextEditorService.CommonService.FileSystemProvider.File.ExistsAsync(dotNetProject.AbsolutePath.Value))
			return; // TODO: This can still cause a race condition exception if the file is removed before the next line runs.

		var parentDirectory = dotNetProject.AbsolutePath.ParentDirectory;
		if (parentDirectory is null)
			return;

		var startingAbsolutePathForSearch = parentDirectory;
		var discoveredFileList = new List<string>();
		
		await DiscoverFilesRecursively(startingAbsolutePathForSearch, discoveredFileList, true).ConfigureAwait(false);

		ParseClassesInProject(
			editContext,
			dotNetProject,
			progressBarModel,
			currentProgress,
			maximumProgressAvailableToProject,
			discoveredFileList,
			compilationUnitKind);

		async Task DiscoverFilesRecursively(string directoryPathParent, List<string> discoveredFileList, bool isFirstInvocation)
		{
			var directoryPathChildList = await IdeService.TextEditorService.CommonService.FileSystemProvider.Directory.GetDirectoriesAsync(
					directoryPathParent,
					CancellationToken.None)
				.ConfigureAwait(false);

			var filePathChildList = await IdeService.TextEditorService.CommonService.FileSystemProvider.Directory.GetFilesAsync(
					directoryPathParent,
					CancellationToken.None)
				.ConfigureAwait(false);

			foreach (var filePathChild in filePathChildList)
			{
				if (filePathChild.EndsWith(".cs"))
					discoveredFileList.Add(filePathChild);
			}

			foreach (var directoryPathChild in directoryPathChildList)
			{
				if (IFileSystemProvider.IsDirectoryIgnored(directoryPathChild))
					continue;

				await DiscoverFilesRecursively(directoryPathChild, discoveredFileList, isFirstInvocation: false).ConfigureAwait(false);
			}
		}
	}

	private void ParseClassesInProject(
		TextEditorEditContext editContext,
		IDotNetProject dotNetProject,
		ProgressBarModel progressBarModel,
		double currentProgress,
		double maximumProgressAvailableToProject,
		List<string> discoveredFileList,
		CompilationUnitKind compilationUnitKind)
	{
		var fileParsedCount = 0;
		
		foreach (var file in discoveredFileList)
		{
			var fileAbsolutePath = IdeService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(file, false);
			var progress = currentProgress + maximumProgressAvailableToProject * (fileParsedCount / (double)discoveredFileList.Count);
			var resourceUri = new ResourceUri(file);
	        var compilerService = IdeService.TextEditorService.GetCompilerService(fileAbsolutePath.ExtensionNoPeriod);
			
			compilerService.RegisterResource(
				resourceUri,
				shouldTriggerResourceWasModified: false);
			
			compilerService.FastParse(editContext, resourceUri, IdeService.TextEditorService.CommonService.FileSystemProvider, compilationUnitKind);
			fileParsedCount++;
		}
	}

	private async ValueTask Do_SetDotNetSolutionTreeView(Key<DotNetSolutionModel> dotNetSolutionModelKey)
	{
		var dotNetSolutionState = GetDotNetSolutionState();

		var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionsList.FirstOrDefault(
			x => x.Key == dotNetSolutionModelKey);

		if (dotNetSolutionModel is null)
			return;

		var rootNode = new TreeViewSolution(
			dotNetSolutionModel,
			DotNetComponentRenderers,
			IdeService.IdeComponentRenderers,
			IdeService.TextEditorService.CommonService,
			true,
			true);

		await rootNode.LoadChildListAsync().ConfigureAwait(false);

		if (!IdeService.TextEditorService.CommonService.TryGetTreeViewContainer(DotNetSolutionState.TreeViewSolutionExplorerStateKey, out _))
		{
			IdeService.TextEditorService.CommonService.TreeView_RegisterContainerAction(new TreeViewContainer(
				DotNetSolutionState.TreeViewSolutionExplorerStateKey,
				rootNode,
				new List<TreeViewNoType> { rootNode }));
		}
		else
		{
			IdeService.TextEditorService.CommonService.TreeView_WithRootNodeAction(DotNetSolutionState.TreeViewSolutionExplorerStateKey, rootNode);

			IdeService.TextEditorService.CommonService.TreeView_SetActiveNodeAction(
				DotNetSolutionState.TreeViewSolutionExplorerStateKey,
				rootNode,
				true,
				false);
		}

		if (dotNetSolutionModel is null)
			return;

		ReduceWithAction(ConstructModelReplacement(
			dotNetSolutionModel.Key,
			dotNetSolutionModel));
	}
	
	private void RegisterStartupControl(IDotNetProject project)
	{
		IdeService.Ide_RegisterStartupControl(
			new StartupControlModel(
				Key<IStartupControlModel>.NewKey(),
				project.DisplayName,
				project.AbsolutePath.Value,
				project.AbsolutePath,
				null,
				null,
				startupControlModel => StartButtonOnClick(startupControlModel, project),
				StopButtonOnClick));
	}
	
	private Task StartButtonOnClick(IStartupControlModel interfaceStartupControlModel, IDotNetProject project)
    {
    	var startupControlModel = (StartupControlModel)interfaceStartupControlModel;
    	
        var ancestorDirectory = project.AbsolutePath.ParentDirectory;

        if (ancestorDirectory is null)
            return Task.CompletedTask;

        var formattedCommand = DotNetCliCommandFormatter.FormatStartProjectWithoutDebugging(
            project.AbsolutePath);
            
        var terminalCommandRequest = new TerminalCommandRequest(
        	formattedCommand.Value,
        	ancestorDirectory,
        	_newDotNetSolutionTerminalCommandRequestKey)
        {
        	BeginWithFunc = parsedCommand =>
        	{
        		ParseOutputEntireDotNetRun(
        			string.Empty,
        			"Run-Project_started");
        			
        		return Task.CompletedTask;
        	},
        	ContinueWithFunc = parsedCommand =>
        	{
        		startupControlModel.ExecutingTerminalCommandRequest = null;
        		IdeService.Ide_TriggerStartupControlStateChanged();
        	
        		ParseOutputEntireDotNetRun(
        			parsedCommand.OutputCache.ToString(),
        			"Run-Project_completed");
        			
        		return Task.CompletedTask;
        	}
        };
        
        startupControlModel.ExecutingTerminalCommandRequest = terminalCommandRequest;
        
		IdeService.GetTerminalState().TerminalMap[TerminalFacts.EXECUTION_KEY].EnqueueCommand(terminalCommandRequest);
    	return Task.CompletedTask;
    }
    
    private Task StopButtonOnClick(IStartupControlModel interfaceStartupControlModel)
    {
    	var startupControlModel = (StartupControlModel)interfaceStartupControlModel;
    	
		IdeService.GetTerminalState().TerminalMap[TerminalFacts.EXECUTION_KEY].KillProcess();
		startupControlModel.ExecutingTerminalCommandRequest = null;
		
        IdeService.Ide_TriggerStartupControlStateChanged();
        return Task.CompletedTask;
    }

	private ValueTask Do_Website_AddExistingProjectToSolution(
		Key<DotNetSolutionModel> dotNetSolutionModelKey,
		string projectTemplateShortName,
		string cSharpProjectName,
		AbsolutePath cSharpProjectAbsolutePath)
	{
		return ValueTask.CompletedTask;
	}

	/// <summary>Don't have the implementation <see cref="WithAction"/> as public scope.</summary>
	public interface IWithAction
	{
	}

	/// <summary>Don't have <see cref="WithAction"/> itself as public scope.</summary>
	public record WithAction(Func<DotNetSolutionState, DotNetSolutionState> WithFunc)
		: IWithAction;

	public static IWithAction ConstructModelReplacement(
			Key<DotNetSolutionModel> dotNetSolutionModelKey,
			DotNetSolutionModel outDotNetSolutionModel)
	{
		return new WithAction(dotNetSolutionState =>
		{
			var indexOfSln = dotNetSolutionState.DotNetSolutionsList.FindIndex(
				sln => sln.Key == dotNetSolutionModelKey);

			if (indexOfSln == -1)
				return dotNetSolutionState;

			var outDotNetSolutions = new List<DotNetSolutionModel>(dotNetSolutionState.DotNetSolutionsList);
			outDotNetSolutions[indexOfSln] = outDotNetSolutionModel;

			return dotNetSolutionState with
			{
				DotNetSolutionsList = outDotNetSolutions
			};
		});
	}
    #endregion
	/* End DotNetBackgroundTaskApi */
	
	/* Start TestExplorerService */
	private readonly object _stateModificationLock = new();

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
		var dotNetSolutionState = GetDotNetSolutionState();
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
		
			Enqueue_ConstructTreeView();
		}
		
		if (_intentToDiscoverTestsInSolutionFilePath != dotNetSolutionModel.AbsolutePath.Value)
		{
			_intentToDiscoverTestsInSolutionFilePath = dotNetSolutionModel.AbsolutePath.Value;
			Enqueue_DiscoverTests();
		}
		
		return Task.CompletedTask;
	}
	
	public Task HandleShouldDiscoverTestsEffect()
	{
		var dotNetSolutionState = GetDotNetSolutionState();
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
		
			Enqueue_ConstructTreeView();
		}
		
		_intentToDiscoverTestsInSolutionFilePath = dotNetSolutionModel.AbsolutePath.Value;
		Enqueue_DiscoverTests();
	
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
		
		IdeService.TextEditorService.CommonService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, ContainsTestsTreeViewGroup);
		
		if (!solutionFilePathWasNull)
		{
			_ = Task.Run(async () =>
			{
				await HandleUserInterfaceWasInitializedEffect()
					.ConfigureAwait(false);
			});
		}
	}

    public void Enqueue_ConstructTreeView()
    {
        Enqueue(new DotNetWorkArgs
        {
            WorkKind = DotNetWorkKind.ConstructTreeView
        });
    }
    
    public void Enqueue_DiscoverTests()
    {
        Enqueue(new DotNetWorkArgs
        {
            WorkKind = DotNetWorkKind.DiscoverTests
        });
    }
    
    private readonly Throttle _throttleDiscoverTests = new(TimeSpan.FromMilliseconds(100));
    
    public TreeViewGroup ContainsTestsTreeViewGroup { get; } = new("Have tests", true, true);
	public TreeViewGroup NoTestsTreeViewGroup { get; } = new("No tests (but still a test-project)", true, true);
	public TreeViewGroup ThrewAnExceptionTreeViewGroup { get; } = new("Projects that threw an exception during discovery", true, true);
	public TreeViewGroup NotValidProjectForUnitTestTreeViewGroup { get; } = new("Not a test-project", true, true);

    public async ValueTask TestExplorer_Do_ConstructTreeView()
    {
        var dotNetSolutionState = GetDotNetSolutionState();
        var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionModel;

        if (dotNetSolutionModel is null)
            return;

        var localDotNetProjectList = dotNetSolutionModel.DotNetProjectList
            .Where(x => x.DotNetProjectKind == DotNetProjectKind.CSharpProject);

        var localProjectTestModelList = localDotNetProjectList.Select(x => new ProjectTestModel(
				x.ProjectIdGuid,
				x.AbsolutePath,
				callback => Task.CompletedTask,
				node => IdeService.TextEditorService.CommonService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, node)))
			.ToList();

        var localFormattedCommand = DotNetCliCommandFormatter.FormatDotNetTestListTests();

        var localTreeViewProjectTestModelList = localProjectTestModelList.Select(x =>
                (TreeViewNoType)new TreeViewProjectTestModel(
                    x,
                    IdeService.TextEditorService.CommonService.CommonComponentRenderers,
                    true,
                    false))
            .ToArray();

        foreach (var entry in localTreeViewProjectTestModelList)
        {
            var treeViewProjectTestModel = (TreeViewProjectTestModel)entry;

            if (string.IsNullOrWhiteSpace(treeViewProjectTestModel.Item.DirectoryNameForTestDiscovery))
                return;
            
            var projectFileText = await IdeService.TextEditorService.CommonService.FileSystemProvider.File.ReadAllTextAsync(treeViewProjectTestModel.Item.AbsolutePath.Value);
            
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
							treeViewProjectTestModel.Item.TestNameFullyQualifiedList = ParseOutputLineDotNetTestListTests(
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
                
				return IdeService.GetTerminalState().TerminalMap[TerminalFacts.EXECUTION_KEY]
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

        if (!IdeService.TextEditorService.CommonService.TryGetTreeViewContainer(TestExplorerState.TreeViewTestExplorerKey, out _))
        {
            IdeService.TextEditorService.CommonService.TreeView_RegisterContainerAction(new TreeViewContainer(
                TestExplorerState.TreeViewTestExplorerKey,
                adhocRoot,
                activeNodes));
        }
        else
        {
            IdeService.TextEditorService.CommonService.TreeView_WithRootNodeAction(TestExplorerState.TreeViewTestExplorerKey, adhocRoot);

            IdeService.TextEditorService.CommonService.TreeView_SetActiveNodeAction(
                TestExplorerState.TreeViewTestExplorerKey,
                firstNode,
                true,
                false);
        }

        ReduceWithAction(inState => inState with
        {
            ProjectTestModelList = localProjectTestModelList,
            SolutionFilePath = dotNetSolutionModel.AbsolutePath.Value,
        });
    }
    
    public ValueTask Do_DiscoverTests()
    {
    	_throttleDiscoverTests.Run(async _ =>
    	{
	    	var dotNetSolutionState = GetDotNetSolutionState();
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
				IdeService.TextEditorService.CommonService,
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
	    		
	    		if (IdeService.TextEditorService.CommonService.TryGetTreeViewContainer(TestExplorerState.TreeViewTestExplorerKey, out var treeViewContainer))
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
		var dotNetSolutionState = GetDotNetSolutionState();
        var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionModel;

        if (dotNetSolutionModel is null)
            return Task.CompletedTask;
    
    	var totalTestCount = 0;
    	var notRanTestHashSet = new HashSet<string>();
    	
    	Console.WriteLine($"NoTestsTreeViewGroup.ChildList.Count: {NoTestsTreeViewGroup.ChildList.Count}");
    	if (IdeService.TextEditorService.CommonService.TryGetTreeViewContainer(TestExplorerState.TreeViewTestExplorerKey, out var treeViewContainer))
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
            
            IdeService.TextEditorService.CommonService.TreeView_WithRootNodeAction(TestExplorerState.TreeViewTestExplorerKey, nextTreeViewAdhoc);
        }
    
    	ReduceWithAction(inState => inState with
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
		
		if (!IdeService.TextEditorService.CommonService.TryGetTreeViewContainer(TestExplorerState.TreeViewTestExplorerKey, out var treeViewContainer))
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
		IdeService.TextEditorService.CommonService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, treeViewProjectTestModel.Parent);
				
		if (treeViewProjectTestModel.Item.TestNameFullyQualifiedList is not null)
    	{
    		if (treeViewProjectTestModel.Item.TestNameFullyQualifiedList.Count > 0)
    		{
    			containsTestsTreeViewGroup.ChildList.Add(treeViewProjectTestModel);
    			containsTestsTreeViewGroup.LinkChildren(
					containsTestsTreeViewGroup.ChildList,
					containsTestsTreeViewGroup.ChildList);
				IdeService.TextEditorService.CommonService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, containsTestsTreeViewGroup);
    		}
    		else
    		{
    			noTestsTreeViewGroup.ChildList.Add(treeViewProjectTestModel);
    			noTestsTreeViewGroup.LinkChildren(
					noTestsTreeViewGroup.ChildList,
					noTestsTreeViewGroup.ChildList);
				IdeService.TextEditorService.CommonService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, noTestsTreeViewGroup);
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
				IdeService.TextEditorService.CommonService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, threwAnExceptionTreeViewGroup);
    		}
    		else
    		{
    			notValidProjectForUnitTestTreeViewGroup.ChildList.Add(treeViewProjectTestModel);
    			notValidProjectForUnitTestTreeViewGroup.LinkChildren(
					notValidProjectForUnitTestTreeViewGroup.ChildList,
					notValidProjectForUnitTestTreeViewGroup.ChildList);
				IdeService.TextEditorService.CommonService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, notValidProjectForUnitTestTreeViewGroup);
    		}
    	}
	}
	/* End TestExplorerService */
	
	/* Start OutputService */
	private readonly Throttle _throttleCreateTreeView = new Throttle(TimeSpan.FromMilliseconds(333));
    
    private OutputState _outputState = new();
    
    public event Action? OutputStateChanged;
    
    public OutputState GetOutputState() => _outputState;

    public void ReduceStateHasChangedAction(Guid dotNetRunParseResultId)
    {
    	var inState = GetOutputState();
    
        _outputState = inState with
        {
        	DotNetRunParseResultId = dotNetRunParseResultId
        };
        
        OutputStateChanged?.Invoke();
        return;
    }
    
	public Task HandleConstructTreeViewEffect()
	{
		_throttleCreateTreeView.Run(async _ => await OutputService_Do_ConstructTreeView());
        return Task.CompletedTask;
	}
	
	public ValueTask OutputService_Do_ConstructTreeView()
    {
    	var dotNetRunParseResult = GetDotNetRunParseResult();
    	
    	var treeViewNodeList = dotNetRunParseResult.AllDiagnosticLineList.Select(x =>
    		(TreeViewNoType)new TreeViewDiagnosticLine(
    			x,
				false,
				false))
			.ToArray();
			
		var filePathGrouping = treeViewNodeList.GroupBy(
			x => ((TreeViewDiagnosticLine)x).Item.FilePathTextSpan.Text);
		
		var projectManualGrouping = new Dictionary<string, TreeViewGroup>();
		var treeViewBadStateGroupList = new List<TreeViewNoType>();

		foreach (var group in filePathGrouping)
		{
			var absolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(group.Key, false);
			var groupEnumerated = group.ToList();
			var groupNameBuilder = new StringBuilder();
			
			var errorCount = groupEnumerated.Count(x =>
				((TreeViewDiagnosticLine)x).Item.DiagnosticLineKind == DiagnosticLineKind.Error);
				
			var warningCount = groupEnumerated.Count(x =>
				((TreeViewDiagnosticLine)x).Item.DiagnosticLineKind == DiagnosticLineKind.Warning);
			
			groupNameBuilder
				.Append(absolutePath.NameWithExtension)
				.Append(" (")
				.Append(errorCount)
				.Append(" errors)")
				.Append(" (")
				.Append(warningCount)
				.Append(" warnings)");
		
			var treeViewGroup = new TreeViewGroup(
				groupNameBuilder.ToString(),
				true,
				groupEnumerated.Any(x => ((TreeViewDiagnosticLine)x).Item.DiagnosticLineKind == DiagnosticLineKind.Error))
			{
				TitleText = absolutePath.ParentDirectory ?? $"{nameof(AbsolutePath.ParentDirectory)} was null"
			};

			treeViewGroup.ChildList = groupEnumerated;
			treeViewGroup.LinkChildren(new(), treeViewGroup.ChildList);
			
			var firstEntry = groupEnumerated.FirstOrDefault();
			
			if (firstEntry is not null)
			{
				var projectText = ((TreeViewDiagnosticLine)firstEntry).Item.ProjectTextSpan.Text;
				var projectAbsolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(projectText, false);
			
				if (!projectManualGrouping.ContainsKey(projectText))
				{
					var treeViewGroupProject = new TreeViewGroup(
						projectAbsolutePath.NameWithExtension,
						true,
						true)
					{
						TitleText = absolutePath.ParentDirectory ?? $"{nameof(AbsolutePath.ParentDirectory)} was null"
					};
				
					projectManualGrouping.Add(projectText, treeViewGroupProject);
				}
				
				projectManualGrouping[projectText].ChildList.Add(treeViewGroup);
			}
			else
			{
				treeViewBadStateGroupList.Add(treeViewGroup);
			}
		}
		
		var treeViewProjectGroupList = projectManualGrouping.Values
			.Select(x => (TreeViewNoType)x)
			.ToList();
			
		// Bad State
		if (treeViewBadStateGroupList.Count != 0)
		{
			var projectText = "Could not find project";
			
			var treeViewGroupProjectBadState = new TreeViewGroup(
				projectText,
				true,
				true)
			{
				TitleText = projectText
			};
			
			treeViewGroupProjectBadState.ChildList = treeViewBadStateGroupList;
		
			treeViewProjectGroupList.Add(treeViewGroupProjectBadState);
		}
		
		foreach (var treeViewProjectGroup in treeViewProjectGroupList)
		{
			treeViewProjectGroup.LinkChildren(new(), treeViewProjectGroup.ChildList);
		}
    
        var adhocRoot = TreeViewAdhoc.ConstructTreeViewAdhoc(treeViewProjectGroupList.ToArray());
        var firstNode = treeViewNodeList.FirstOrDefault();

        var activeNodes = firstNode is null
            ? new List<TreeViewNoType>()
            : new() { firstNode };

        if (!CommonService.TryGetTreeViewContainer(OutputState.TreeViewContainerKey, out _))
        {
            CommonService.TreeView_RegisterContainerAction(new TreeViewContainer(
                OutputState.TreeViewContainerKey,
                adhocRoot,
                activeNodes));
        }
        else
        {
            CommonService.TreeView_WithRootNodeAction(OutputState.TreeViewContainerKey, adhocRoot);

            CommonService.TreeView_SetActiveNodeAction(
                OutputState.TreeViewContainerKey,
                firstNode,
                true,
                false);
        }

        ReduceStateHasChangedAction(dotNetRunParseResult.Id);
        return ValueTask.CompletedTask;
    }
	/* End OutputService */
	
	/* Start DotNetSolutionService */
	private DotNetSolutionState _dotNetSolutionState = new();
	
	public event Action? DotNetSolutionStateChanged;
	
	public DotNetSolutionState GetDotNetSolutionState() => _dotNetSolutionState;

    public void ReduceRegisterAction(DotNetSolutionModel argumentDotNetSolutionModel)
    {
    	var inState = GetDotNetSolutionState();
    
        var dotNetSolutionModel = inState.DotNetSolutionModel;

        if (dotNetSolutionModel is not null)
        {
            DotNetSolutionStateChanged?.Invoke();
            return;
        }

        var nextList = new List<DotNetSolutionModel>(inState.DotNetSolutionsList);
        nextList.Add(argumentDotNetSolutionModel);

        _dotNetSolutionState = inState with
        {
            DotNetSolutionsList = nextList
        };
        
        DotNetSolutionStateChanged?.Invoke();
        return;
    }

    public void ReduceDisposeAction(Key<DotNetSolutionModel> dotNetSolutionModelKey)
    {
    	var inState = GetDotNetSolutionState();
    
        var dotNetSolutionModel = inState.DotNetSolutionModel;

        if (dotNetSolutionModel is null)
        {
            DotNetSolutionStateChanged?.Invoke();
        	return;
        }

        var nextList = new List<DotNetSolutionModel>(inState.DotNetSolutionsList);
        nextList.Remove(dotNetSolutionModel);

        _dotNetSolutionState = inState with
        {
            DotNetSolutionsList = nextList
        };
        
        DotNetSolutionStateChanged?.Invoke();
        return;
    }

    public void ReduceWithAction(IWithAction withActionInterface)
    {
    	var inState = GetDotNetSolutionState();
    
        var withAction = (WithAction)withActionInterface;
        _dotNetSolutionState = withAction.WithFunc.Invoke(inState);
        
        DotNetSolutionStateChanged?.Invoke();
        return;
    }
    
	public Task NotifyDotNetSolutionStateStateHasChanged()
	{
		return Task.CompletedTask;
	}
	/* End DotNetSolutionService */
	
	public void Dispose()
	{
		DotNetSolutionStateChanged -= OnDotNetSolutionStateChanged;
	}
}
