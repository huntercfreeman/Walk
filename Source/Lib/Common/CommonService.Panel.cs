using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    private PanelState _panelState = new();
    
    public PanelState GetPanelState() => _panelState;
    
    public double WidthAppAtTimeOfCalculations { get; set; }
    public double HeightAppAtTimeOfCalculations { get; set; }
    
    // width: 100%;
    // height: calc(78% - (DotNetService.CommonService.GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2)px - (CommonFacts.Ide_Header_Height.Value / 2)rem);
    public double BodyElementHeight { get; set; }
    // width: 33.3333% - ???;
    // height: 100%;
    public double LeftPanelWidth { get; set; }
    // width: 33.3333% - (DotNetService.CommonService.GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2 ??????????? )px;
    // height: 100%;
    public double EditorElementWidth { get; set; }
    // width: 33.3333% - ???;
    // height: 100%;
    public double RightPanelWidth { get; set; }
    
    // width: 100%;
    // height: calc(22% - ??? - (CommonFacts.Ide_Header_Height.Value / 2)rem);
    public double BottomPanelHeight { get; set; }
    
    public void SetPanelState(PanelState panelState)
    {
        _panelState = panelState;
    }
    
    public void RegisterPanelTab(
        Key<PanelGroup> panelGroupKey,
        IPanelTab panelTab,
        bool insertAtIndexZero)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();
            
            PanelGroup inPanelGroup;
            
            if (panelGroupKey == inState.TopLeftPanelGroup.Key)
            {
                inPanelGroup = inState.TopLeftPanelGroup;
            }
            else if (panelGroupKey == inState.TopRightPanelGroup.Key)
            {
                inPanelGroup = inState.TopRightPanelGroup;
            }
            else if (panelGroupKey == inState.BottomPanelGroup.Key)
            {
                inPanelGroup = inState.BottomPanelGroup;
            }
            else
            {
                return;
            }

            if (!inPanelGroup.TabList.Any(x => x.Key == panelTab.Key))
            {
                var outTabList = new List<IPanelTab>(inPanelGroup.TabList);
    
                var insertionPoint = insertAtIndexZero
                    ? 0
                    : outTabList.Count;
    
                outTabList.Insert(insertionPoint, panelTab);
    
                var outPanelGroup = inPanelGroup with
                {
                    TabList = outTabList
                };
                
                outPanelGroup.ActiveTab = outPanelGroup.TabList.FirstOrDefault(x => x.Key == outPanelGroup.ActiveTabKey);
                
                if (panelGroupKey == inState.TopLeftPanelGroup.Key)
                {
                    _panelState = inState with
                    {
                        TopLeftPanelGroup = outPanelGroup
                    };
                }
                else if (panelGroupKey == inState.TopRightPanelGroup.Key)
                {
                    _panelState = inState with
                    {
                        TopRightPanelGroup = outPanelGroup
                    };
                }
                else if (panelGroupKey == inState.BottomPanelGroup.Key)
                {
                    _panelState = inState with
                    {
                        BottomPanelGroup = outPanelGroup
                    };
                }
                else
                {
                    return;
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
            
            PanelGroup inPanelGroup;
            
            if (panelGroupKey == inState.TopLeftPanelGroup.Key)
            {
                inPanelGroup = inState.TopLeftPanelGroup;
            }
            else if (panelGroupKey == inState.TopRightPanelGroup.Key)
            {
                inPanelGroup = inState.TopRightPanelGroup;
            }
            else if (panelGroupKey == inState.BottomPanelGroup.Key)
            {
                inPanelGroup = inState.BottomPanelGroup;
            }
            else
            {
                return;
            }

            var indexPanelTab = inPanelGroup.TabList.FindIndex(x => x.Key == panelTabKey);
            if (indexPanelTab != -1)
            {
                inPanelGroup.TabList[indexPanelTab].TabGroup = null;
            
                var outTabList = new List<IPanelTab>(inPanelGroup.TabList);
                outTabList.RemoveAt(indexPanelTab);
    
                var outPanelGroup = inPanelGroup with
                {
                    TabList = outTabList
                };
                
                outPanelGroup.ActiveTab = outPanelGroup.TabList.FirstOrDefault(x => x.Key == outPanelGroup.ActiveTabKey);

                if (panelGroupKey == inState.TopLeftPanelGroup.Key)
                {
                    _panelState = inState with
                    {
                        TopLeftPanelGroup = outPanelGroup
                    };
                }
                else if (panelGroupKey == inState.TopRightPanelGroup.Key)
                {
                    _panelState = inState with
                    {
                        TopRightPanelGroup = outPanelGroup
                    };
                }
                else if (panelGroupKey == inState.BottomPanelGroup.Key)
                {
                    _panelState = inState with
                    {
                        BottomPanelGroup = outPanelGroup
                    };
                }
                else
                {
                    return;
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
            
            PanelGroup inPanelGroup;
            
            if (panelGroupKey == inState.TopLeftPanelGroup.Key)
            {
                inPanelGroup = inState.TopLeftPanelGroup;
            }
            else if (panelGroupKey == inState.TopRightPanelGroup.Key)
            {
                inPanelGroup = inState.TopRightPanelGroup;
            }
            else if (panelGroupKey == inState.BottomPanelGroup.Key)
            {
                inPanelGroup = inState.BottomPanelGroup;
            }
            else
            {
                return;
            }

            var outPanelGroup = inPanelGroup with
            {
                ActiveTabKey = panelTabKey
            };
                
            outPanelGroup.ActiveTab = outPanelGroup.TabList.FirstOrDefault(x => x.Key == outPanelGroup.ActiveTabKey);

            if (panelGroupKey == inState.TopLeftPanelGroup.Key)
            {
                _panelState = inState with
                {
                    TopLeftPanelGroup = outPanelGroup
                };
            }
            else if (panelGroupKey == inState.TopRightPanelGroup.Key)
            {
                _panelState = inState with
                {
                    TopRightPanelGroup = outPanelGroup
                };
            }
            else if (panelGroupKey == inState.BottomPanelGroup.Key)
            {
                _panelState = inState with
                {
                    BottomPanelGroup = outPanelGroup
                };
            }
            else
            {
                return;
            }
            
            sideEffect = true;
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
    
    public void Panel_OnUserAgent_AppDimensionStateChanged()
    {
        var appDimensionState = _appDimensionState;
    
        WidthAppAtTimeOfCalculations = appDimensionState.Width;
        HeightAppAtTimeOfCalculations = appDimensionState.Height;
        
        // DON'T FORGET THE LINEHEIGHT!!!
        // TABS ALWAYS EXIST
        //
        // Why is the header in rem it is just annoying and makes everything 100x more complicated.
        
        // width: 100%;
        // height: calc(78% - (DotNetService.CommonService.GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2)px - (CommonFacts.Ide_Header_Height.Value / 2)rem);
        BodyElementHeight = HeightAppAtTimeOfCalculations * 0.78 - (GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2) - ;
        // width: 33.3333% - ???;
        // height: 100%;
        LeftPanelWidth;
        // width: 33.3333% - (DotNetService.CommonService.GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2 ??????????? )px;
        // height: 100%;
        EditorElementWidth;
        // width: 33.3333% - ???;
        // height: 100%;
        RightPanelWidth;
        
        // width: 100%;
        // height: calc(22% - ??? - (CommonFacts.Ide_Header_Height.Value / 2)rem);
        BottomPanelHeight;
    }
}
