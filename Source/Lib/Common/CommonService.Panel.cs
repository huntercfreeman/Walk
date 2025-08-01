using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Common.RazorLib.Panels.Models;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    private PanelState _panelState = new();
    
    public PanelState GetPanelState() => _panelState;
    
    public void SetPanelState(PanelState panelState)
    {
        _panelState = panelState;
    }

    public void RegisterPanel(Panel panel)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();
    
            if (!inState.PanelList.Any(x => x.Key == panel.Key))
            {
                var outPanelList = new List<Panel>(inState.PanelList);
                outPanelList.Add(panel);
    
                _panelState = inState with { PanelList = outPanelList };
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
    }

    public void DisposePanel(Key<Panel> panelKey)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();

            var indexPanel = inState.PanelList.FindIndex(x => x.Key == panelKey);
            if (indexPanel != -1)
            {
                var outPanelList = new List<Panel>(inState.PanelList);
                outPanelList.RemoveAt(indexPanel);
    
                _panelState = inState with { PanelList = outPanelList };
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
    }
    
    public void RegisterPanelGroup(PanelGroup panelGroup)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();

            if (!inState.PanelGroupList.Any(x => x.Key == panelGroup.Key))
            {
                var outPanelGroupList = new List<PanelGroup>(inState.PanelGroupList);
                outPanelGroupList.Add(panelGroup);
    
                _panelState = inState with { PanelGroupList = outPanelGroupList };
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
    }

    public void DisposePanelGroup(Key<PanelGroup> panelGroupKey)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();

            var indexPanelGroup = inState.PanelGroupList.FindIndex(x => x.Key == panelGroupKey);
            if (indexPanelGroup != -1)
            {
                var outPanelGroupList = new List<PanelGroup>(inState.PanelGroupList);
                outPanelGroupList.RemoveAt(indexPanelGroup);
    
                _panelState = inState with { PanelGroupList = outPanelGroupList };
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
    }

    public void RegisterPanelTab(
        Key<PanelGroup> panelGroupKey,
        IPanelTab panelTab,
        bool insertAtIndexZero)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();

            var indexPanelGroup = inState.PanelGroupList.FindIndex(x => x.Key == panelGroupKey);
            if (indexPanelGroup != -1)
            {
                var inPanelGroup = inState.PanelGroupList[indexPanelGroup];
                if (!inPanelGroup.TabList.Any(x => x.Key == panelTab.Key))
                {
                    var outTabList = new List<IPanelTab>(inPanelGroup.TabList);
        
                    var insertionPoint = insertAtIndexZero
                        ? 0
                        : outTabList.Count;
        
                    outTabList.Insert(insertionPoint, panelTab);
        
                    var outPanelGroupList = new List<PanelGroup>(inState.PanelGroupList);
        
                    outPanelGroupList[indexPanelGroup] = inPanelGroup with
                    {
                        TabList = outTabList
                    };
        
                    _panelState = inState with
                    {
                        PanelGroupList = outPanelGroupList
                    };
                }
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
    }

    public void DisposePanelTab(Key<PanelGroup> panelGroupKey, Key<Panel> panelTabKey)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();

            var indexPanelGroup = inState.PanelGroupList.FindIndex(x => x.Key == panelGroupKey);
            if (indexPanelGroup != -1)
            {
                var inPanelGroup = inState.PanelGroupList[indexPanelGroup];
                var indexPanelTab = inPanelGroup.TabList.FindIndex(x => x.Key == panelTabKey);
                if (indexPanelTab != -1)
                {
                    inPanelGroup.TabList[indexPanelTab].TabGroup = null;
                
                    var outTabList = new List<IPanelTab>(inPanelGroup.TabList);
                    outTabList.RemoveAt(indexPanelTab);
        
                    var outPanelGroupList = new List<PanelGroup>(inState.PanelGroupList);
                    outPanelGroupList[indexPanelGroup] = inPanelGroup with
                    {
                        TabList = outTabList
                    };

                    _panelState = inState with { PanelGroupList = outPanelGroupList };
                }
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
    }

    public void SetActivePanelTab(Key<PanelGroup> panelGroupKey, Key<Panel> panelTabKey)
    {
        var sideEffect = false;

        lock (_stateModificationLock)
        {
            var inState = GetPanelState();

            var indexPanelGroup = inState.PanelGroupList.FindIndex(x => x.Key == panelGroupKey);
            if (indexPanelGroup != -1)
            {
                var inPanelGroup = inState.PanelGroupList[indexPanelGroup];
                var outPanelGroupList = new List<PanelGroup>(inState.PanelGroupList);
    
                outPanelGroupList[indexPanelGroup] = inPanelGroup with
                {
                    ActiveTabKey = panelTabKey
                };
    
                _panelState = inState with { PanelGroupList = outPanelGroupList };
                sideEffect = true;
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);

        if (sideEffect)
            AppDimension_NotifyIntraAppResize();
    }

    public void Panel_SetDragEventArgs((IPanelTab PanelTab, PanelGroup PanelGroup)? dragEventArgs)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();
    
            _panelState = inState with
            {
                DragEventArgs = dragEventArgs
            };
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
    }
    
    /// <summary>
    /// If the panel tab exists, then its group will have its active-tab set the tab.
    /// Otherwise, add the panel as a tab to the left panel and set left panel's active tab to that tab.
    /// </summary>
    public async Task ShowOrAddPanelTab(Panel panel)
    {
        var panelGroup = panel.TabGroup as PanelGroup;
                    
        if (panelGroup is not null)
        {
            SetActivePanelTab(panelGroup.Key, panel.Key);
        }
        else
        {
            var dialogState = GetDialogState();
            var existingDialog = dialogState.DialogList.FirstOrDefault(
                x => x.DynamicViewModelKey == panel.DynamicViewModelKey);

            if (existingDialog is not null)
            {
                Dialog_ReduceSetActiveDialogKeyAction(existingDialog.DynamicViewModelKey);

                await JsRuntimeCommonApi
                    .FocusHtmlElementById(existingDialog.DialogFocusPointHtmlElementId)
                    .ConfigureAwait(false);
            }
            else
            {
                RegisterPanelTab(CommonFacts.LeftPanelGroupKey, panel, true);
                SetActivePanelTab(CommonFacts.LeftPanelGroupKey, panel.Key);
            }
        }
    }
}
