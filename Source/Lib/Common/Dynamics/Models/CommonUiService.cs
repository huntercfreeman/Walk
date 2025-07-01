using System.Text;
using Microsoft.JSInterop;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Common.RazorLib.JsRuntimes.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Tooltips.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Dynamics.Models;

public class CommonUiService : ICommonUiService
{
    private readonly object _stateModificationLock = new();
    
    private ICommonUtilityService _commonUtilityService;
    
    public CommonUiService(IJSRuntime jsRuntime)
	{
        JsRuntimeCommonApi = jsRuntime.GetWalkCommonApi();
	}
	
	public WalkCommonJavaScriptInteropApi JsRuntimeCommonApi { get; }
	
	public event Action<CommonUiEventKind>? CommonUiStateChanged;
	
	public void HACK_SetCommonUtilityServiceCircularReferenceTemporaryFix(ICommonUtilityService commonUtilityService)
	{
	    _commonUtilityService = commonUtilityService;
	}
	
    /* Start IOutlineService */
	private OutlineState _outlineState = new();
	
	public OutlineState GetOutlineState() => _outlineState;

	public void SetOutline(
		string? elementId,
		MeasuredHtmlElementDimensions? measuredHtmlElementDimensions,
		bool needsMeasured)
	{
		lock (_stateModificationLock)
		{
			_outlineState = _outlineState with
			{
				ElementId = elementId,
				MeasuredHtmlElementDimensions = measuredHtmlElementDimensions,
				NeedsMeasured = needsMeasured,
			};
		}

        if (needsMeasured && elementId is not null)
        {
            _ = Task.Run(async () =>
            {
                var elementDimensions = await JsRuntimeCommonApi
                    .MeasureElementById(elementId)
                    .ConfigureAwait(false);

                Outline_SetMeasurements(
                    elementId,
                    elementDimensions);
            });

            return; // The state has changed will occur in 'ReduceSetMeasurementsAction'
        }
        else
        {
            goto finalize;
        }

        finalize:
        CommonUiStateChanged?.Invoke(CommonUiEventKind.OutlineStateChanged);
    }
	
	public void Outline_SetMeasurements(
		string? elementId,
		MeasuredHtmlElementDimensions? measuredHtmlElementDimensions)
	{
		lock (_stateModificationLock)
		{
			if (_outlineState.ElementId == elementId)
			{
    			_outlineState = _outlineState with
    			{
    				MeasuredHtmlElementDimensions = measuredHtmlElementDimensions,
    				NeedsMeasured = false,
    			};
    		}
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.OutlineStateChanged);
    }
    /* End IOutlineService */
    
    /* Start IPanelService */
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
            _commonUtilityService.AppDimension_NotifyIntraAppResize();
    }

    public void SetPanelTabAsActiveByContextRecordKey(Key<ContextRecord> contextRecordKey)
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
                if (dimensionUnit.Purpose == DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_ROW ||
                    dimensionUnit.Purpose == DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_COLUMN)
                {
                    if (dimensionUnit.Purpose == DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_ROW)
                    {
                        if (inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList is not null)
                        {
                            var existingDimensionUnit = inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList
                                .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
            
                            if (existingDimensionUnit.Purpose is null)
                                inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                        }
                    }
                    else if (dimensionUnit.Purpose != DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_COLUMN)
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
    /* End IPanelService */
    
    /* Start IWidgetService */
	private WidgetState _widgetState = new();
	
	public WidgetState GetWidgetState() => _widgetState;

	/// <summary>
	/// When this action causes the transition from a widget being rendered,
	/// to NO widget being rendered.
	///
	/// Then the user's focus will go "somewhere" so we want
	/// redirect it to the main layout at least so they can use IDE keybinds
	///
	/// As if the "somewhere" their focus moves to is outside the blazor app components
	/// they IDE keybinds won't fire.
	///
	/// TODO: Where does focus go when you delete the element which the user is focused on.
	///
	/// TODO: Prior to focusing the widget (i.e.: NO widget transitions to a widget being rendered)
	///           we should track where the user's focus is, then restore that focus once the
	///           widget is closed.
	/// </summary>
    public void SetWidget(WidgetModel? widget)
    {
		var sideEffect = false;

		lock (_stateModificationLock)
		{
			var inState = GetWidgetState();

			if (widget != inState.Widget && (widget is null))
				sideEffect = true;

			_widgetState = inState with
			{
				Widget = widget,
			};
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.WidgetStateChanged);

		if (sideEffect)
		{
            _ = Task.Run(async () =>
            {
                await JsRuntimeCommonApi
                    .FocusHtmlElementById(IDynamicViewModel.DefaultSetFocusOnCloseElementId)
                    .ConfigureAwait(false);
            });
        }
    }
    /* End IWidgetService */
    
    /* Start IDialogService */
    /// <summary>
    /// TODO: Some methods just invoke a single method, so remove the redundant middle man.
    /// TODO: Thread safety.
    /// </summary>
    private DialogState _dialogState = new();
	
	public DialogState GetDialogState() => _dialogState;
    
    public void Dialog_ReduceRegisterAction(IDialog dialog)
    {
    	var inState = GetDialogState();
    	
        if (inState.DialogList.Any(x => x.DynamicViewModelKey == dialog.DynamicViewModelKey))
        {
        	_ = Task.Run(async () =>
        		await JsRuntimeCommonApi
	                .FocusHtmlElementById(dialog.DialogFocusPointHtmlElementId)
	                .ConfigureAwait(false));
        	
        	CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
        	return;
        }

		var outDialogList = new List<IDialog>(inState.DialogList);
        outDialogList.Add(dialog);

        _dialogState = inState with 
        {
            DialogList = outDialogList,
            ActiveDialogKey = dialog.DynamicViewModelKey,
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
        return;
    }

    public void Dialog_ReduceSetIsMaximizedAction(
        Key<IDynamicViewModel> dynamicViewModelKey,
        bool isMaximized)
    {
    	var inState = GetDialogState();
    	
        var indexDialog = inState.DialogList.FindIndex(
            x => x.DynamicViewModelKey == dynamicViewModelKey);

        if (indexDialog == -1)
        {
            CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
        	return;
        }
            
        var inDialog = inState.DialogList[indexDialog];

        var outDialogList = new List<IDialog>(inState.DialogList);
        
        outDialogList[indexDialog] = inDialog.SetDialogIsMaximized(isMaximized);

        _dialogState = inState with { DialogList = outDialogList };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
        return;
    }
    
    public void Dialog_ReduceSetActiveDialogKeyAction(Key<IDynamicViewModel> dynamicViewModelKey)
    {
    	var inState = GetDialogState();
    	
        _dialogState = inState with { ActiveDialogKey = dynamicViewModelKey };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.ActiveDialogKeyChanged);
        return;
    }

    public void Dialog_ReduceDisposeAction(Key<IDynamicViewModel> dynamicViewModelKey)
    {
    	var inState = GetDialogState();
    
        var indexDialog = inState.DialogList.FindIndex(
            x => x.DynamicViewModelKey == dynamicViewModelKey);

        if (indexDialog == -1)
        {
        	CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
        	return;
        }

		var inDialog = inState.DialogList[indexDialog];

        var outDialogList = new List<IDialog>(inState.DialogList);
        outDialogList.RemoveAt(indexDialog);

        _dialogState = inState with { DialogList = outDialogList };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
        return;
    }
    /* End IDialogService */
    
    /* Start INotificationService */
	private NotificationState _notificationState = new();
	
	public NotificationState GetNotificationState() => _notificationState;

    public void Notification_ReduceRegisterAction(INotification notification)
    {
    	lock (_stateModificationLock)
    	{
	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
	        outDefaultList.Add(notification);
	        _notificationState = _notificationState with { DefaultList = outDefaultList };
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceDisposeAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var indexNotification = _notificationState.DefaultList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (indexNotification != -1)
	        {
    	        var inNotification = _notificationState.DefaultList[indexNotification];
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.RemoveAt(indexNotification);
    	        _notificationState = _notificationState with { DefaultList = outDefaultList };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceMakeReadAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.DefaultList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.DefaultList[inNotificationIndex];
    	
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.RemoveAt(inNotificationIndex);
    	        
    	        var outReadList = new List<INotification>(_notificationState.ReadList);
    	        outReadList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            ReadList = outReadList
    	        };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }
    
    public void Notification_ReduceUndoMakeReadAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.ReadList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.ReadList[inNotificationIndex];
    	
    	        var outReadList = new List<INotification>(_notificationState.ReadList);
    	        outReadList.RemoveAt(inNotificationIndex);
    	        
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            ReadList = outReadList
    	        };
    	    }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceMakeDeletedAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.DefaultList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.DefaultList[inNotificationIndex];
    	
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.RemoveAt(inNotificationIndex);
    	        
    	        var outDeletedList = new List<INotification>(_notificationState.DeletedList);
    	        outDeletedList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            DeletedList = outDeletedList
    	        };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceUndoMakeDeletedAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.DeletedList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.DeletedList[inNotificationIndex];
    	
    	        var outDeletedList = new List<INotification>(_notificationState.DeletedList);
    	        outDeletedList.RemoveAt(inNotificationIndex);
    	
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            DeletedList = outDeletedList
    	        };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceMakeArchivedAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.DefaultList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.DefaultList[inNotificationIndex];
    	
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.RemoveAt(inNotificationIndex);
    	
    	        var outArchivedList = new List<INotification>(_notificationState.ArchivedList);
    	        outArchivedList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            ArchivedList = outArchivedList
    	        };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }
    
    public void Notification_ReduceUndoMakeArchivedAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.ArchivedList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.ArchivedList[inNotificationIndex];
    	
    	        var outArchivedList = new List<INotification>(_notificationState.ArchivedList);
    	        outArchivedList.RemoveAt(inNotificationIndex);
    	        
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            ArchivedList = outArchivedList
    	        };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceClearDefaultAction()
    {
    	lock (_stateModificationLock)
    	{
	        _notificationState = _notificationState with
	        {
	            DefaultList = new List<INotification>()
	        };
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }
    
    public void Notification_ReduceClearReadAction()
    {
    	lock (_stateModificationLock)
    	{
	        _notificationState = _notificationState with
	        {
	            ReadList = new List<INotification>()
	        };
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }
    
    public void Notification_ReduceClearDeletedAction()
    {
    	lock (_stateModificationLock)
    	{
	        _notificationState = _notificationState with
	        {
	            DeletedList = new List<INotification>()
	        };
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceClearArchivedAction()
    {
    	lock (_stateModificationLock)
    	{
	        _notificationState = _notificationState with
	        {
	            ArchivedList = new List<INotification>()
	        };
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }
    /* End INotificationService */
    
    /* Start IDropdownService */
	private DropdownState _dropdownState = new();
	
	public DropdownState GetDropdownState() => _dropdownState;
	
    public void Dropdown_ReduceRegisterAction(DropdownRecord dropdown)
    {
    	var inState = GetDropdownState();
    
		var indexExistingDropdown = inState.DropdownList.FindIndex(
			x => x.Key == dropdown.Key);

		if (indexExistingDropdown != -1)
		{
			CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
			return;
		}

		var outDropdownList = new List<DropdownRecord>(inState.DropdownList);
		outDropdownList.Add(dropdown);

        _dropdownState = inState with
        {
            DropdownList = outDropdownList
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
		return;
    }

    public void Dropdown_ReduceDisposeAction(Key<DropdownRecord> key)
    {
    	var inState = GetDropdownState();
    
		var indexExistingDropdown = inState.DropdownList.FindIndex(
			x => x.Key == key);

		if (indexExistingDropdown == -1)
		{
			CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
			return;
		}
			
		var outDropdownList = new List<DropdownRecord>(inState.DropdownList);
		outDropdownList.RemoveAt(indexExistingDropdown);

        _dropdownState = inState with
        {
            DropdownList = outDropdownList
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
		return;
    }

    public void Dropdown_ReduceClearAction()
    {
    	var inState = GetDropdownState();
    
    	var outDropdownList = new List<DropdownRecord>();
    
        _dropdownState = inState with
        {
            DropdownList = outDropdownList
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
		return;
    }

    public void Dropdown_ReduceFitOnScreenAction(DropdownRecord dropdown)
    {
    	var inState = GetDropdownState();
    
		var indexExistingDropdown = inState.DropdownList.FindIndex(
			x => x.Key == dropdown.Key);

		if (indexExistingDropdown == -1)
		{
			CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
			return;
		}
		
		var inDropdown = inState.DropdownList[indexExistingDropdown];

		var outDropdown = inDropdown with
		{
			Width = dropdown.Width,
			Height = dropdown.Height,
			Left = dropdown.Left,
			Top = dropdown.Top
		};
		
		var outDropdownList = new List<DropdownRecord>(inState.DropdownList);
		outDropdownList[indexExistingDropdown] = outDropdown;

        _dropdownState = inState with
        {
            DropdownList = outDropdownList
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
		return;
    }
    /* End IDropdownService */
    
    /* Start ITooltipService */
    public TooltipState _tooltipState = new(tooltipModel: null);

	public static readonly Guid Tooltip_htmlElementIdSalt = Guid.NewGuid();
    
	public string Tooltip_HtmlElementId { get; } = $"di_dropdown_{Tooltip_htmlElementIdSalt}";
	public MeasuredHtmlElementDimensions Tooltip_HtmlElementDimensions { get; set; }
	public MeasuredHtmlElementDimensions Tooltip_GlobalHtmlElementDimensions { get; set; }
	public bool Tooltip_IsOffScreenHorizontally { get; }
	public bool Tooltip_IsOffScreenVertically { get; }
	public int Tooltip_RenderCount { get; } = 1;
	
	public StringBuilder Tooltip_StyleBuilder { get; } = new();
	
	public TooltipState GetTooltipState() => _tooltipState;
	
	public void SetTooltipModel(ITooltipModel tooltipModel)
	{
	    _tooltipState = new TooltipState(tooltipModel);
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.TooltipStateChanged);
	}
    /* End ITooltipService */
}
