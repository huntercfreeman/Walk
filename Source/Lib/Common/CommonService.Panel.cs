using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    private PanelState _panelState = new();
    
    public PanelState GetPanelState() => _panelState;
    
    public string BodyElementStyle { get; set; } = "height: 78%;";
    public string BottomPanelStyle { get; set; } = "height: 22%";
    
    public string LeftPanelStyle { get; set; } = "width: 33.3333%";
    public string EditorElementStyle { get; set; } = "width: 33.3333%";
    public string RightPanelStyle { get; set; } = "width: 33.3333%";
    
    private bool _hadSuccessfullyMeasuredAtLeastOnce;
    
    public double WidthAppAtTimeOfCalculations { get; set; }
    public double HeightAppAtTimeOfCalculations { get; set; }
    
    // height: 78% - (GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2) - Options_LineHeight;
    public double BodyElementHeight { get; set; }
    // height: 22% - (GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2) - Options_LineHeight;
    public double BottomPanelHeight { get; set; }
    
    // width: 33.3333% - (GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2);
    public double LeftPanelWidth { get; set; }
    // width: 33.3333% - GetAppOptionsState().Options.ResizeHandleWidthInPixels;
    public double EditorElementWidth { get; set; }
    // width: 33.3333% - (GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2);
    public double RightPanelWidth { get; set; }
    
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
        
        if (appDimensionState.Width < 500 || appDimensionState.Height < 500)
        {
            return;
        }
    
        // DON'T FORGET THE LINEHEIGHT!!!
        // TABS ALWAYS EXIST
        //
        // Why is the header in rem it is just annoying and makes everything 100x more complicated.
        //
        // Why are some of the resize handle unaccounted for
        // editor is bounded on both sides so it is not '/ 2'
        //
        // on app resize calculate percent for the current setup given the width/height that was
        // at that time originally.
        //
        // Then apply those percents to the new measurements.
        //
        // very "easy".
        //
        // you just offset with the cursor the sizes directly it doesn't matter then you get the current percents by looking back at the
        // original width/height of app but now you apply those percents to the new size.
        
        double bodyFraction;
        double bottomPanelFraction;
        
        double leftPanelFraction;
        double editorFraction;
        double rightPanelFraction;
        
        if (_hadSuccessfullyMeasuredAtLeastOnce)
        {
            bodyFraction = (BodyElementHeight + (GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2) + Options_LineHeight) / HeightAppAtTimeOfCalculations;
            bottomPanelFraction = (BottomPanelHeight + (GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2) + Options_LineHeight) / HeightAppAtTimeOfCalculations;
            
            leftPanelFraction = (LeftPanelWidth + (GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2)) / WidthAppAtTimeOfCalculations;
            editorFraction = (EditorElementWidth + GetAppOptionsState().Options.ResizeHandleWidthInPixels) / WidthAppAtTimeOfCalculations;
            rightPanelFraction = (RightPanelWidth + (GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2)) / WidthAppAtTimeOfCalculations;
        }
        else
        {
            _hadSuccessfullyMeasuredAtLeastOnce = true;
            
            // Use default percentages
            
            bodyFraction = 0.78;
            bottomPanelFraction = 0.22;
            
            leftPanelFraction = 0.333333;
            editorFraction = 0.333333;
            rightPanelFraction = 0.333333;
        }
        
        WidthAppAtTimeOfCalculations = appDimensionState.Width;
        HeightAppAtTimeOfCalculations = appDimensionState.Height;
        
        // height: 78% - (GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2) - Options_LineHeight;
        BodyElementHeight = HeightAppAtTimeOfCalculations * bodyFraction - (GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2) - Options_LineHeight;
        // height: 22% - (GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2) - Options_LineHeight;
        BottomPanelHeight = HeightAppAtTimeOfCalculations * bottomPanelFraction - (GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2) - Options_LineHeight;
        
        // width: 33.3333% - (GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2);
        LeftPanelWidth = WidthAppAtTimeOfCalculations * leftPanelFraction - (GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2);
        // width: 33.3333% - GetAppOptionsState().Options.ResizeHandleWidthInPixels;
        EditorElementWidth = WidthAppAtTimeOfCalculations * editorFraction - GetAppOptionsState().Options.ResizeHandleWidthInPixels;
        // width: 33.3333% - (GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2);
        RightPanelWidth = WidthAppAtTimeOfCalculations * rightPanelFraction - (GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2);
        
        BodyElementStyle = $"height: {BodyElementHeight}px;";
        BottomPanelStyle = $"height: {BottomPanelHeight}px";
        
        LeftPanelStyle = $"width: {LeftPanelWidth}px";
        EditorElementStyle = $"width: {EditorElementWidth}px";
        RightPanelStyle = $"width: {RightPanelWidth}px";
        
        /*
        Each tabs listing is size "(Options_LineHeight)px".
        When the tabs listing is displayed vertically, then "(Options_LineHeight)px" represents the width, otherwise the height.
        
        The panels when there is an active tab do not deal with the size of the tabs listing.
        
        They are to have a size > the tabs listing. But it is loosely represented as a % of the application size.
        
        "loosely represented as a %" - anytime the app size changes, a percentage calculation is performed
        to determine the size of each panel.
        
        But, by the end this percentage calculation is converted to pixels.
        
        You need to remember the apps dimensions at this point in time that you'd calculated the sizes of each panel.
        
        The user at this point can freely resize the panels provided they do not go below a min-width.
        
        Then, once the app is resized again, you need to determine based on the current
        pixel sizes of each panel, if they were made a percent of the original app size
        what percentage would they be?
        
        You then apply that percentage to the new app size.
        And convert it to pixels.
        
        ========================================================
        
        When a panel does not have an active tab, its size is "(Options_LineHeight)px".
        
        ========================================================
        
        When you stop showing an active tab in a panel, the width of that panel - "(Options_LineHeight)px"
        is given to the editor.
        
        > (or height to body if you interact with the bottom panel)
        
        The pixel size that the panel which no longer has an active tab has remains known to the app
        as if it were showing, but we know that quantity had been given to the editor.
        
        Once you re-show a panel tab again then you take that pixel amount back from the editor
        until you hit the minimum allowed size.
        
        Any remainder you take from the opposite panel since this scenario means
        the user resized the editor and opposite panel after you'd hidden the original panel's tab.
        
        ========================================================
        
        When the app is resized and a panel has no active tab,
        the pixel size is still known and is still re-calculated as a percentage of the new app size
        just the same as if it had an active tab.
        */
    }
}
