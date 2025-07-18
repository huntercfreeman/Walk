using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.WatchWindows.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Extensions.DotNet.CompilerServices.Models;
using Walk.Extensions.DotNet.BackgroundTasks.Models;

namespace Walk.Extensions.DotNet.CompilerServices.Displays;

public partial class CompilerServiceExplorerTreeViewDisplay : ComponentBase, IDisposable
{
	[Inject]
	private IdeService IdeService { get; set; } = null!;
	[Inject]
	private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;

	private CompilerServiceExplorerTreeViewKeyboardEventHandler _compilerServiceExplorerTreeViewKeymap = null!;
	private CompilerServiceExplorerTreeViewMouseEventHandler _compilerServiceExplorerTreeViewMouseEventHandler = null!;

	private int OffsetPerDepthInPixels => (int)Math.Ceiling(
		IdeService.TextEditorService.CommonUtilityService.GetAppOptionsState().Options.IconSizeInPixels * (2.0 / 3.0));

	private static bool _hasInitialized;

	protected override void OnInitialized()
	{
		DotNetBackgroundTaskApi.CompilerServiceExplorerService.CompilerServiceExplorerStateChanged += RerenderAfterEvent;
		IdeService.TextEditorService.TextEditorStateChanged += RerenderAfterEvent;

		_compilerServiceExplorerTreeViewKeymap = new CompilerServiceExplorerTreeViewKeyboardEventHandler(
			IdeService);

		_compilerServiceExplorerTreeViewMouseEventHandler = new CompilerServiceExplorerTreeViewMouseEventHandler(
			IdeService);
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

		IdeService.TextEditorService.CommonUtilityService.Dropdown_ReduceRegisterAction(dropdownRecord);
		return Task.CompletedTask;
	}

	private void ReloadOnClick()
	{
		IdeService.TextEditorService.CommonUtilityService.Continuous_EnqueueGroup(new BackgroundTask(
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
			IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.XML),
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.XML).GetType(),
			"XML",
			true);

		var dotNetSolutionCompilerServiceWatchWindowObject = new WatchWindowObject(
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.DOT_NET_SOLUTION),
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.DOT_NET_SOLUTION).GetType(),
			".NET Solution",
			true);

		var cSharpProjectCompilerServiceWatchWindowObject = new WatchWindowObject(
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.C_SHARP_PROJECT),
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.C_SHARP_PROJECT).GetType(),
			"C# Project",
			true);

		var cSharpCompilerServiceWatchWindowObject = new WatchWindowObject(
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.C_SHARP_CLASS),
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.C_SHARP_CLASS).GetType(),
			"C#",
			true);

		var razorCompilerServiceWatchWindowObject = new WatchWindowObject(
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.RAZOR_MARKUP),
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.RAZOR_MARKUP).GetType(),
			"Razor",
			true);

		var cssCompilerServiceWatchWindowObject = new WatchWindowObject(
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.CSS),
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.CSS).GetType(),
			"Css",
			true);

		var fSharpCompilerServiceWatchWindowObject = new WatchWindowObject(
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.F_SHARP),
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.F_SHARP).GetType(),
			"F#",
			true);

		var javaScriptCompilerServiceWatchWindowObject = new WatchWindowObject(
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.JAVA_SCRIPT),
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.JAVA_SCRIPT).GetType(),
			"JavaScript",
			true);

		var typeScriptCompilerServiceWatchWindowObject = new WatchWindowObject(
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.TYPE_SCRIPT),
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.TYPE_SCRIPT).GetType(),
			"TypeScript",
			true);

		var jsonCompilerServiceWatchWindowObject = new WatchWindowObject(
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.JSON),
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.JSON).GetType(),
			"JSON",
			true);

		var terminalCompilerServiceWatchWindowObject = new WatchWindowObject(
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.TERMINAL),
            IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.TERMINAL).GetType(),
			"Terminal",
			true);

		var rootNode = TreeViewAdhoc.ConstructTreeViewAdhoc(
			new TreeViewReflection(xmlCompilerServiceWatchWindowObject, true, false, IdeService.TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(dotNetSolutionCompilerServiceWatchWindowObject, true, false, IdeService.TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(cSharpProjectCompilerServiceWatchWindowObject, true, false, IdeService.TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(cSharpCompilerServiceWatchWindowObject, true, false, IdeService.TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(razorCompilerServiceWatchWindowObject, true, false, IdeService.TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(cssCompilerServiceWatchWindowObject, true, false, IdeService.TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(fSharpCompilerServiceWatchWindowObject, true, false, IdeService.TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(javaScriptCompilerServiceWatchWindowObject, true, false, IdeService.TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(typeScriptCompilerServiceWatchWindowObject, true, false, IdeService.TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(jsonCompilerServiceWatchWindowObject, true, false, IdeService.TextEditorService.CommonUtilityService.CommonComponentRenderers),
			new TreeViewReflection(terminalCompilerServiceWatchWindowObject, true, false, IdeService.TextEditorService.CommonUtilityService.CommonComponentRenderers));

		await rootNode.LoadChildListAsync().ConfigureAwait(false);

		if (!IdeService.TextEditorService.CommonUtilityService.TryGetTreeViewContainer(
				CompilerServiceExplorerState.TreeViewCompilerServiceExplorerContentStateKey,
				out var treeViewState))
		{
			IdeService.TextEditorService.CommonUtilityService.TreeView_RegisterContainerAction(new TreeViewContainer(
				CompilerServiceExplorerState.TreeViewCompilerServiceExplorerContentStateKey,
				rootNode,
				new List<TreeViewNoType> { rootNode }));
		}
		else
		{
			IdeService.TextEditorService.CommonUtilityService.TreeView_WithRootNodeAction(
				CompilerServiceExplorerState.TreeViewCompilerServiceExplorerContentStateKey,
				rootNode);

			IdeService.TextEditorService.CommonUtilityService.TreeView_SetActiveNodeAction(
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
		IdeService.TextEditorService.TextEditorStateChanged -= RerenderAfterEvent;
	}
}