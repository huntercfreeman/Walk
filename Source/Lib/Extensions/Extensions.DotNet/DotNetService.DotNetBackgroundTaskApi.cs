﻿using CliWrap.EventStream;
using System.Runtime.InteropServices;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Namespaces.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.CompilerServices.DotNetSolution.CompilerServiceCase;
using Walk.CompilerServices.DotNetSolution.Models;
using Walk.CompilerServices.DotNetSolution.Models.Project;
using Walk.CompilerServices.DotNetSolution.SyntaxActors;
using Walk.CompilerServices.Xml.Html.SyntaxActors;
using Walk.CompilerServices.Xml.Html.SyntaxEnums;
using Walk.CompilerServices.Xml.Html.SyntaxObjects;
using Walk.Extensions.DotNet.AppDatas.Models;
using Walk.Extensions.DotNet.CommandLines.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Displays;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.Namespaces.Models;
using Walk.Extensions.DotNet.Nugets.Displays;
using Walk.Extensions.DotNet.Nugets.Models;
using Walk.Extensions.DotNet.Outputs.Displays;
using Walk.Extensions.DotNet.TestExplorers.Displays;
using Walk.Extensions.DotNet.TestExplorers.Models;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.BackgroundTasks.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.Extensions.DotNet;

public partial class DotNetService
{
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
}
