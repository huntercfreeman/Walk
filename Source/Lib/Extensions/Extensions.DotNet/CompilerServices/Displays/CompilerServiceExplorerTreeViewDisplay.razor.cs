using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.WatchWindows.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Extensions.DotNet.CompilerServices.Models;
using Walk.Extensions.DotNet.BackgroundTasks.Models;

namespace Walk.Extensions.DotNet.CompilerServices.Displays;

public partial class CompilerServiceExplorerTreeViewDisplay : ComponentBase, IDisposable
{
	[Inject]
	private TextEditorService TextEditorService { get; set; } = null!;
	[Inject]
	private IdeBackgroundTaskApi IdeBackgroundTaskApi { get; set; } = null!;
	[Inject]
	private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;

	private CompilerServiceExplorerTreeViewKeyboardEventHandler _compilerServiceExplorerTreeViewKeymap = null!;
	private CompilerServiceExplorerTreeViewMouseEventHandler _compilerServiceExplorerTreeViewMouseEventHandler = null!;

	private int OffsetPerDepthInPixels => (int)Math.Ceiling(
		TextEditorService.CommonUtilityService.GetAppOptionsState().Options.IconSizeInPixels * (2.0 / 3.0));

	private static bool _hasInitialized;

	protected override void OnInitialized()
	{
		DotNetBackgroundTaskApi.CompilerServiceExplorerService.CompilerServiceExplorerStateChanged += RerenderAfterEvent;
		TextEditorService.TextEditorStateChanged += RerenderAfterEvent;

		_compilerServiceExplorerTreeViewKeymap = new CompilerServiceExplorerTreeViewKeyboardEventHandler(
			IdeBackgroundTaskApi,
			TextEditorService.CommonUtilityService);

		_compilerServiceExplorerTreeViewMouseEventHandler = new CompilerServiceExplorerTreeViewMouseEventHandler(
			IdeBackgroundTaskApi,
			TextEditorService.CommonUtilityService);
	}

	protected override Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			if (!_hasInitialized)
			{
				_hasInitialized = true;
				ReloadOnClick();
			}
		}
        
        return Task.CompletedTask;
	}

	private async void RerenderAfterEvent()
	{
		await InvokeAsync(StateHasChanged);
	}

	private Task OnTreeViewContextMenuFunc(TreeViewCommandArgs treeViewCommandArgs)
	{
		var dropdownRecord = new DropdownRecord(
			CompilerServiceExplorerTreeViewContextMenu.ContextMenuEventDropdownKey,
			treeViewCommandArgs.ContextMenuFixedPosition.LeftPositionInPixels,
			treeViewCommandArgs.ContextMenuFixedPosition.TopPositionInPixels,
			typeof(CompilerServiceExplorerTreeViewContextMenu),
			new Dictionary<string, object?>
			{
				{
					nameof(CompilerServiceExplorerTreeViewContextMenu.TreeViewCommandArgs),
					treeViewCommandArgs
				}
			},
			restoreFocusOnClose: null);

		TextEditorService.CommonUtilityService.Dropdown_ReduceRegisterAction(dropdownRecord);
		return Task.CompletedTask;
	}

	private void ReloadOnClick()
	{
		TextEditorService.CommonUtilityService.Continuous_EnqueueGroup(new BackgroundTask(
			Key<IBackgroundTaskGroup>.Empty,
			Do_SetCompilerServiceExplorerTreeView));
	}
	
	/// <summary>
    /// TODO: Iterate over _compilerServiceExplorerStateWrap.Value.CompilerServiceList instead...
	///       ...of invoking 'GetCompilerService' with hardcoded values.
    /// </summary>
    public async ValueTask Do_SetCompilerServiceExplorerTreeView()
	{
		var compilerServiceExplorerState = DotNetBackgroundTaskApi.CompilerServiceExplorerService.GetCompilerServiceExplorerState();

		var xmlCompilerServiceWatchWindowObject = new WatchWindowObject(
			TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.XML),
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.XML).GetType(),
			"XML",
			true);

		var dotNetSolutionCompilerServiceWatchWindowObject = new WatchWindowObject(
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.DOT_NET_SOLUTION),
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.DOT_NET_SOLUTION).GetType(),
			".NET Solution",
			true);

		var cSharpProjectCompilerServiceWatchWindowObject = new WatchWindowObject(
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.C_SHARP_PROJECT),
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.C_SHARP_PROJECT).GetType(),
			"C# Project",
			true);

		var cSharpCompilerServiceWatchWindowObject = new WatchWindowObject(
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.C_SHARP_CLASS),
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.C_SHARP_CLASS).GetType(),
			"C#",
			true);

		var razorCompilerServiceWatchWindowObject = new WatchWindowObject(
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.RAZOR_MARKUP),
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.RAZOR_MARKUP).GetType(),
			"Razor",
			true);

		var cssCompilerServiceWatchWindowObject = new WatchWindowObject(
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.CSS),
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.CSS).GetType(),
			"Css",
			true);

		var fSharpCompilerServiceWatchWindowObject = new WatchWindowObject(
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.F_SHARP),
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.F_SHARP).GetType(),
			"F#",
			true);

		var javaScriptCompilerServiceWatchWindowObject = new WatchWindowObject(
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.JAVA_SCRIPT),
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.JAVA_SCRIPT).GetType(),
			"JavaScript",
			true);

		var typeScriptCompilerServiceWatchWindowObject = new WatchWindowObject(
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.TYPE_SCRIPT),
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.TYPE_SCRIPT).GetType(),
			"TypeScript",
			true);

		var jsonCompilerServiceWatchWindowObject = new WatchWindowObject(
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.JSON),
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.JSON).GetType(),
			"JSON",
			true);

		var terminalCompilerServiceWatchWindowObject = new WatchWindowObject(
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.TERMINAL),
            TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.TERMINAL).GetType(),
			"Terminal",
			true);

		var rootNode = TreeViewAdhoc.ConstructTreeViewAdhoc(
			new TreeViewReflection(xmlCompilerServiceWatchWindowObject, true, false, TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(dotNetSolutionCompilerServiceWatchWindowObject, true, false, TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(cSharpProjectCompilerServiceWatchWindowObject, true, false, TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(cSharpCompilerServiceWatchWindowObject, true, false, TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(razorCompilerServiceWatchWindowObject, true, false, TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(cssCompilerServiceWatchWindowObject, true, false, TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(fSharpCompilerServiceWatchWindowObject, true, false, TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(javaScriptCompilerServiceWatchWindowObject, true, false, TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(typeScriptCompilerServiceWatchWindowObject, true, false, TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(jsonCompilerServiceWatchWindowObject, true, false, TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(terminalCompilerServiceWatchWindowObject, true, false, TextEditorService.CommonUtilityService.CommonComponentRenderers));

		await rootNode.LoadChildListAsync().ConfigureAwait(false);

		if (!TextEditorService.CommonUtilityService.TryGetTreeViewContainer(
				CompilerServiceExplorerState.TreeViewCompilerServiceExplorerContentStateKey,
				out var treeViewState))
		{
			TextEditorService.CommonUtilityService.TreeView_RegisterContainerAction(new TreeViewContainer(
				CompilerServiceExplorerState.TreeViewCompilerServiceExplorerContentStateKey,
				rootNode,
				new List<TreeViewNoType> { rootNode }));
		}
		else
		{
			TextEditorService.CommonUtilityService.TreeView_WithRootNodeAction(
				CompilerServiceExplorerState.TreeViewCompilerServiceExplorerContentStateKey,
				rootNode);

			TextEditorService.CommonUtilityService.TreeView_SetActiveNodeAction(
				CompilerServiceExplorerState.TreeViewCompilerServiceExplorerContentStateKey,
				rootNode,
				true,
				false);
		}

		DotNetBackgroundTaskApi.CompilerServiceExplorerService.ReduceNewAction(inCompilerServiceExplorerState =>
			new CompilerServiceExplorerState(inCompilerServiceExplorerState.Model));
	}

	public void Dispose()
	{
		DotNetBackgroundTaskApi.CompilerServiceExplorerService.CompilerServiceExplorerStateChanged -= RerenderAfterEvent;
		TextEditorService.TextEditorStateChanged -= RerenderAfterEvent;
	}
}