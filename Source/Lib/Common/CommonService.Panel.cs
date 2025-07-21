using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Common.RazorLib.Panels.Models;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
	private PanelState _panelState = new();
	
	public PanelState GetPanelState() => _panelState;

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

    public void SetPanelTabAsActiveByContextRecordKey(int contextRecordKey)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();

            var inPanelGroup = inState.PanelGroupList.FirstOrDefault(x => x.TabList
                .Any(y => y.ContextRecordKey == contextRecordKey));
                
            if (inPanelGroup is not null)
            {
                var inPanelTab = inPanelGroup.TabList.FirstOrDefault(
                    x => x.ContextRecordKey == contextRecordKey);
                    
                if (inPanelTab is not null)
                {
                    // TODO: This should be thread safe yes?...
                    // ...Only ever would the same thread access the inner lock from invoking this which is the current lock so no deadlock?
                    SetActivePanelTab(inPanelGroup.Key, inPanelTab.Key);
                    return; // Inner reduce will trigger finalize.
                }
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
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
    
    public void Panel_InitializeResizeHandleDimensionUnit(Key<PanelGroup> panelGroupKey, DimensionUnit dimensionUnit)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();

            var inPanelGroup = inState.PanelGroupList.FirstOrDefault(
                x => x.Key == panelGroupKey);

            if (inPanelGroup is not null)
            {
                if (dimensionUnit.Purpose == CommonFacts.PURPOSE_RESIZABLE_HANDLE_ROW ||
                    dimensionUnit.Purpose == CommonFacts.PURPOSE_RESIZABLE_HANDLE_COLUMN)
                {
                    if (dimensionUnit.Purpose == CommonFacts.PURPOSE_RESIZABLE_HANDLE_ROW)
                    {
                        if (inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList is not null)
                        {
                            var existingDimensionUnit = inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList
                                .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
            
                            if (existingDimensionUnit.Purpose is null)
                                inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                        }
                    }
                    else if (dimensionUnit.Purpose != CommonFacts.PURPOSE_RESIZABLE_HANDLE_COLUMN)
                    {
                        if (inPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList is not null)
                        {
                            var existingDimensionUnit = inPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList
                                .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
            
                            if (existingDimensionUnit.Purpose is null)
                                inPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                        }
                    }
                }
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
    }
}
