using System.Collections.Concurrent;
using System.Text;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.JsRuntimes.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Installations.Displays;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Tabs.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.BackgroundTasks.Models;

/// <summary>
/// This seems to be working.
/// But this code is very experimental.
/// Just feeling things out at the moment.
/// </summary>
public class CommonBackgroundTaskApi : IBackgroundTaskGroup
{
	private readonly BackgroundTaskService _backgroundTaskService;
    private readonly ICommonUtilityService _commonUtilityService;
    private readonly IContextService _contextService;
    private readonly ITreeViewService _treeViewService;
    private readonly WalkCommonConfig _commonConfig;
    
    private readonly ConcurrentQueue<CommonWorkArgs> _workQueue = new();

    public CommonBackgroundTaskApi(
		BackgroundTaskService backgroundTaskService,
		ICommonUtilityService commonUtilityService,
		IContextService contextService,
        ITreeViewService treeViewService,
        WalkCommonConfig commonConfig)
    {
        _backgroundTaskService = backgroundTaskService;

        _commonUtilityService = commonUtilityService;
		_commonUtilityService.Options_CommonBackgroundTaskApi = this;

        _contextService = contextService;
        
        _treeViewService = treeViewService;
        _commonConfig = commonConfig;
            
        _treeViewService.CommonBackgroundTaskApi = this;
    }

    public WalkCommonJavaScriptInteropApi JsRuntimeCommonApi => _commonUtilityService.JsRuntimeCommonApi;
    
    public Key<IBackgroundTaskGroup> BackgroundTaskKey { get; } = Key<IBackgroundTaskGroup>.NewKey();
    
    public bool __TaskCompletionSourceWasCreated { get; set; }
    
    /// <summary>
    /// A shared StringBuilder, but only use this if you know for certain you are on the "UI thread".
    /// </summary>
    public StringBuilder UiStringBuilder { get; } = new();

    public void Enqueue(CommonWorkArgs commonWorkArgs)
    {
		_workQueue.Enqueue(commonWorkArgs);
        _backgroundTaskService.Continuous_EnqueueGroup(this);
    }

    private async ValueTask Do_WalkCommonInitializer(Key<ContextSwitchGroup> contextSwitchGroupKey)
    {
        _commonUtilityService.Options_SetActiveThemeRecordKey(_commonConfig.InitialThemeKey, false);

        await _commonUtilityService
            .Options_SetFromLocalStorageAsync()
            .ConfigureAwait(false);

        _contextService.GetContextSwitchState().FocusInitiallyContextSwitchGroupKey = contextSwitchGroupKey;
        _contextService.RegisterContextSwitchGroup(
            new ContextSwitchGroup(
                contextSwitchGroupKey,
                "Contexts",
                () =>
                {
                    var contextState = _contextService.GetContextState();
                    var panelState = _commonUtilityService.GetPanelState();
                    var dialogState = _commonUtilityService.GetDialogState();
                    var menuOptionList = new List<MenuOptionRecord>();

                    foreach (var panel in panelState.PanelList)
                    {
                        var menuOptionPanel = new MenuOptionRecord(
                            panel.Title,
                            MenuOptionKind.Delete,
                            async () =>
                            {
                                var panelGroup = panel.TabGroup as PanelGroup;

                                if (panelGroup is not null)
                                {
                                    _commonUtilityService.SetActivePanelTab(panelGroup.Key, panel.Key);

                                    var contextRecord = ContextFacts.AllContextsList.FirstOrDefault(x => x.ContextKey == panel.ContextRecordKey);

                                    if (contextRecord != default)
                                    {
                                        var command = ContextHelper.ConstructFocusContextElementCommand(
                                            contextRecord,
                                            nameof(ContextHelper.ConstructFocusContextElementCommand),
                                            nameof(ContextHelper.ConstructFocusContextElementCommand),
                                            JsRuntimeCommonApi,
                                            _commonUtilityService);

                                        await command.CommandFunc.Invoke(null).ConfigureAwait(false);
                                    }
                                }
                                else
                                {
                                    var existingDialog = dialogState.DialogList.FirstOrDefault(
                                        x => x.DynamicViewModelKey == panel.DynamicViewModelKey);

                                    if (existingDialog is not null)
                                    {
                                        _commonUtilityService.Dialog_ReduceSetActiveDialogKeyAction(existingDialog.DynamicViewModelKey);

                                        await JsRuntimeCommonApi
                                            .FocusHtmlElementById(existingDialog.DialogFocusPointHtmlElementId)
                                            .ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        _commonUtilityService.RegisterPanelTab(PanelFacts.LeftPanelGroupKey, panel, true);
                                        _commonUtilityService.SetActivePanelTab(PanelFacts.LeftPanelGroupKey, panel.Key);

                                        var contextRecord = ContextFacts.AllContextsList.FirstOrDefault(x => x.ContextKey == panel.ContextRecordKey);

                                        if (contextRecord != default)
                                        {
                                            var command = ContextHelper.ConstructFocusContextElementCommand(
                                                contextRecord,
                                                nameof(ContextHelper.ConstructFocusContextElementCommand),
                                                nameof(ContextHelper.ConstructFocusContextElementCommand),
                                                JsRuntimeCommonApi,
                                                _commonUtilityService);

                                            await command.CommandFunc.Invoke(null).ConfigureAwait(false);
                                        }
                                    }
                                }
                            });

                        menuOptionList.Add(menuOptionPanel);
                    }

                    var menu = menuOptionList.Count == 0
                        ? new MenuRecord(MenuRecord.NoMenuOptionsExistList)
                        : new MenuRecord(menuOptionList);

                    return Task.FromResult(menu);
                }));
    }

    public async ValueTask Do_WriteToLocalStorage(string key, object value)
    {
        var valueJson = System.Text.Json.JsonSerializer.Serialize(value);
        await _commonUtilityService.Storage_SetValue(key, valueJson).ConfigureAwait(false);
    }

    public async ValueTask Do_Tab_ManuallyPropagateOnContextMenu(
        Func<TabContextMenuEventArgs, Task> localHandleTabButtonOnContextMenu, TabContextMenuEventArgs tabContextMenuEventArgs)
    {
        await localHandleTabButtonOnContextMenu.Invoke(tabContextMenuEventArgs).ConfigureAwait(false);
    }

    public async ValueTask Do_TreeView_HandleTreeViewOnContextMenu(
        Func<TreeViewCommandArgs, Task>? onContextMenuFunc, TreeViewCommandArgs treeViewContextMenuCommandArgs)
    {
        if (onContextMenuFunc is not null)
        {
            await onContextMenuFunc
                .Invoke(treeViewContextMenuCommandArgs)
                .ConfigureAwait(false);
        }
    }

    public async ValueTask Do_TreeView_HandleExpansionChevronOnMouseDown(TreeViewNoType localTreeViewNoType, TreeViewContainer treeViewContainer)
    {
        await localTreeViewNoType.LoadChildListAsync().ConfigureAwait(false);
        _treeViewService.ReduceReRenderNodeAction(treeViewContainer.Key, localTreeViewNoType);
    }

    public async ValueTask Do_TreeView_ManuallyPropagateOnContextMenu(Func<MouseEventArgs?, Key<TreeViewContainer>, TreeViewNoType?, Task> handleTreeViewOnContextMenu, MouseEventArgs mouseEventArgs, Key<TreeViewContainer> key, TreeViewNoType treeViewNoType)
    {
        await handleTreeViewOnContextMenu.Invoke(
                mouseEventArgs,
                key,
                treeViewNoType)
            .ConfigureAwait(false);
    }

    public async ValueTask Do_TreeViewService_LoadChildList(Key<TreeViewContainer> containerKey, TreeViewNoType treeViewNoType)
    {
        try
        {
            await treeViewNoType.LoadChildListAsync().ConfigureAwait(false);

            _treeViewService.ReduceReRenderNodeAction(
                containerKey,
                treeViewNoType);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public ValueTask HandleEvent()
	{
		if (!_workQueue.TryDequeue(out CommonWorkArgs workArgs))
			return ValueTask.CompletedTask;
			
		switch (workArgs.WorkKind)
		{
			case CommonWorkKind.WalkCommonInitializerWork:
				return Do_WalkCommonInitializer(WalkCommonInitializer.ContextSwitchGroupKey);
			case CommonWorkKind.WriteToLocalStorage:
				return Do_WriteToLocalStorage(workArgs.WriteToLocalStorage_Key, workArgs.WriteToLocalStorage_Value);
			case CommonWorkKind.Tab_ManuallyPropagateOnContextMenu:
				return Do_Tab_ManuallyPropagateOnContextMenu(workArgs.HandleTabButtonOnContextMenu, workArgs.TabContextMenuEventArgs);
			case CommonWorkKind.TreeView_HandleTreeViewOnContextMenu:
				return Do_TreeView_HandleTreeViewOnContextMenu(workArgs.OnContextMenuFunc, workArgs.TreeViewContextMenuCommandArgs);
            case CommonWorkKind.TreeView_HandleExpansionChevronOnMouseDown:
				return Do_TreeView_HandleExpansionChevronOnMouseDown(workArgs.TreeViewNoType, workArgs.TreeViewContainer);
            case CommonWorkKind.TreeView_ManuallyPropagateOnContextMenu:
				return Do_TreeView_ManuallyPropagateOnContextMenu(workArgs.HandleTreeViewOnContextMenu, workArgs.MouseEventArgs, workArgs.ContainerKey, workArgs.TreeViewNoType);
            case CommonWorkKind.TreeViewService_LoadChildList:
				return Do_TreeViewService_LoadChildList(workArgs.ContainerKey, workArgs.TreeViewNoType);
			default:
				Console.WriteLine($"{nameof(CommonBackgroundTaskApi)} {nameof(HandleEvent)} default case");
				return ValueTask.CompletedTask;
		}
	}
}
