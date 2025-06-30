namespace Walk.Common.RazorLib.Dynamics.Models;

public class CommonUiService : ICommonUiService
{
    /* Start IOutlineService */
    using Walk.Common.RazorLib.JavaScriptObjects.Models;
    using Walk.Common.RazorLib.BackgroundTasks.Models;
    
    namespace Walk.Common.RazorLib.Outlines.Models;
    
    public class OutlineService : IOutlineService
    {
        private readonly object _stateModificationLock = new();
    
        private readonly CommonBackgroundTaskApi _commonBackgroundTaskApi;
    
    	public OutlineService(CommonBackgroundTaskApi commonBackgroundTaskApi)
    	{
    		_commonBackgroundTaskApi = commonBackgroundTaskApi;
    	}
    	
    	private OutlineState _outlineState = new();
    		
    	public event Action? OutlineStateChanged;
    	
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
                    var elementDimensions = await _commonBackgroundTaskApi.JsRuntimeCommonApi
                        .MeasureElementById(elementId)
                        .ConfigureAwait(false);
    
                    SetMeasurements(
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
            OutlineStateChanged?.Invoke();
        }
    	
    	public void SetMeasurements(
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
    
            OutlineStateChanged?.Invoke();
        }
    }
    /* End IOutlineService */
    
    /* Start IPanelService */
    using Walk.Common.RazorLib.Contexts.Models;
    using Walk.Common.RazorLib.Keys.Models;
    using Walk.Common.RazorLib.Dimensions.Models;
    using Walk.Common.RazorLib.ListExtensions;
    
    namespace Walk.Common.RazorLib.Panels.Models;
    
    public class PanelService : IPanelService
    {
        private readonly object _stateModificationLock = new();
    
        private readonly IAppDimensionService _appDimensionService;
    
    	public PanelService(IAppDimensionService appDimensionService)
    	{
    		_appDimensionService = appDimensionService;
    	}
    
    	private PanelState _panelState = new();
    	
    	public event Action? PanelStateChanged;
    	
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
    
            PanelStateChanged?.Invoke();
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
    
            PanelStateChanged?.Invoke();
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
    
            PanelStateChanged?.Invoke();
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
    
            PanelStateChanged?.Invoke();
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
    
            PanelStateChanged?.Invoke();
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
    
            PanelStateChanged?.Invoke();
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
    
            PanelStateChanged?.Invoke();
    
            if (sideEffect)
                _appDimensionService.NotifyIntraAppResize();
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
    
            PanelStateChanged?.Invoke();
        }
    
        public void SetDragEventArgs((IPanelTab PanelTab, PanelGroup PanelGroup)? dragEventArgs)
        {
            lock (_stateModificationLock)
            {
                var inState = GetPanelState();
        
                _panelState = inState with
                {
                    DragEventArgs = dragEventArgs
                };
            }
    
            PanelStateChanged?.Invoke();
        }
        
        public void InitializeResizeHandleDimensionUnit(Key<PanelGroup> panelGroupKey, DimensionUnit dimensionUnit)
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
    
            PanelStateChanged?.Invoke();
        }
    }
    /* End IPanelService */
    
    /* Start IWidgetService */
    using Walk.Common.RazorLib.Dynamics.Models;
    using Walk.Common.RazorLib.BackgroundTasks.Models;
    
    namespace Walk.Common.RazorLib.Widgets.Models;
    
    public class WidgetService : IWidgetService
    {
        private readonly object _stateModificationLock = new();
    
        private readonly CommonBackgroundTaskApi _commonBackgroundTaskApi;
    
    	public WidgetService(CommonBackgroundTaskApi commonBackgroundTaskApi)
    	{
    		_commonBackgroundTaskApi = commonBackgroundTaskApi;
    	}
    	
    	private WidgetState _widgetState = new();
    	
    	public event Action? WidgetStateChanged;
    	
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
    
            WidgetStateChanged?.Invoke();
    
    		if (sideEffect)
    		{
                _ = Task.Run(async () =>
                {
                    await _commonBackgroundTaskApi.JsRuntimeCommonApi
                        .FocusHtmlElementById(IDynamicViewModel.DefaultSetFocusOnCloseElementId)
                        .ConfigureAwait(false);
                });
            }
        }
    }
    /* End IWidgetService */
    
    /* Start IDialogService */
    using Microsoft.JSInterop;
    using Walk.Common.RazorLib.Dynamics.Models;
    using Walk.Common.RazorLib.Keys.Models;
    using Walk.Common.RazorLib.JsRuntimes.Models;
    using Walk.Common.RazorLib.ListExtensions;
    
    namespace Walk.Common.RazorLib.Dialogs.Models;
    
    /// <summary>
    /// TODO: Some methods just invoke a single method, so remove the redundant middle man.
    /// TODO: Thread safety.
    /// </summary>
    public class DialogService : IDialogService
    {
    	private readonly IJSRuntime _jsRuntime;
    
        public DialogService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }
        
        private WalkCommonJavaScriptInteropApi _jsRuntimeCommonApi;
        private DialogState _dialogState = new();
        
        private WalkCommonJavaScriptInteropApi JsRuntimeCommonApi => _jsRuntimeCommonApi
    		??= _jsRuntime.GetWalkCommonApi();
    		
    	public event Action? DialogStateChanged;
    	
    	public event Action? ActiveDialogKeyChanged;
    	
    	public DialogState GetDialogState() => _dialogState;
        
        public void ReduceRegisterAction(IDialog dialog)
        {
        	var inState = GetDialogState();
        	
            if (inState.DialogList.Any(x => x.DynamicViewModelKey == dialog.DynamicViewModelKey))
            {
            	_ = Task.Run(async () =>
            		await JsRuntimeCommonApi
    	                .FocusHtmlElementById(dialog.DialogFocusPointHtmlElementId)
    	                .ConfigureAwait(false));
            	
            	DialogStateChanged?.Invoke();
            	return;
            }
    
    		var outDialogList = new List<IDialog>(inState.DialogList);
            outDialogList.Add(dialog);
    
            _dialogState = inState with 
            {
                DialogList = outDialogList,
                ActiveDialogKey = dialog.DynamicViewModelKey,
            };
            
            DialogStateChanged?.Invoke();
            return;
        }
    
        public void ReduceSetIsMaximizedAction(
            Key<IDynamicViewModel> dynamicViewModelKey,
            bool isMaximized)
        {
        	var inState = GetDialogState();
        	
            var indexDialog = inState.DialogList.FindIndex(
                x => x.DynamicViewModelKey == dynamicViewModelKey);
    
            if (indexDialog == -1)
            {
                DialogStateChanged?.Invoke();
            	return;
            }
                
            var inDialog = inState.DialogList[indexDialog];
    
            var outDialogList = new List<IDialog>(inState.DialogList);
            
            outDialogList[indexDialog] = inDialog.SetDialogIsMaximized(isMaximized);
    
            _dialogState = inState with { DialogList = outDialogList };
            
            DialogStateChanged?.Invoke();
            return;
        }
        
        public void ReduceSetActiveDialogKeyAction(Key<IDynamicViewModel> dynamicViewModelKey)
        {
        	var inState = GetDialogState();
        	
            _dialogState = inState with { ActiveDialogKey = dynamicViewModelKey };
            
            ActiveDialogKeyChanged?.Invoke();
            return;
        }
    
        public void ReduceDisposeAction(Key<IDynamicViewModel> dynamicViewModelKey)
        {
        	var inState = GetDialogState();
        
            var indexDialog = inState.DialogList.FindIndex(
                x => x.DynamicViewModelKey == dynamicViewModelKey);
    
            if (indexDialog == -1)
            {
            	DialogStateChanged?.Invoke();
            	return;
            }
    
    		var inDialog = inState.DialogList[indexDialog];
    
            var outDialogList = new List<IDialog>(inState.DialogList);
            outDialogList.RemoveAt(indexDialog);
    
            _dialogState = inState with { DialogList = outDialogList };
            
            DialogStateChanged?.Invoke();
            return;
        }
    }
    /* End IDialogService */
    
    /* Start INotificationService */
    using Walk.Common.RazorLib.Dynamics.Models;
    using Walk.Common.RazorLib.Keys.Models;
    using Walk.Common.RazorLib.ListExtensions;
    
    namespace Walk.Common.RazorLib.Notifications.Models;
    
    public class NotificationService : INotificationService
    {
    	private readonly object _stateModificationLock = new();
    
    	private NotificationState _notificationState = new();
    	
    	public event Action? NotificationStateChanged;
    	
    	public NotificationState GetNotificationState() => _notificationState;
    
        public void ReduceRegisterAction(INotification notification)
        {
        	lock (_stateModificationLock)
        	{
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.Add(notification);
    	        _notificationState = _notificationState with { DefaultList = outDefaultList };
    	    }
    	    
    	    NotificationStateChanged?.Invoke();
        }
    
        public void ReduceDisposeAction(Key<IDynamicViewModel> key)
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
    	    
    	    NotificationStateChanged?.Invoke();
        }
    
        public void ReduceMakeReadAction(Key<IDynamicViewModel> key)
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
    	    
    	    NotificationStateChanged?.Invoke();
        }
        
        public void ReduceUndoMakeReadAction(Key<IDynamicViewModel> key)
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
    	    
    	    NotificationStateChanged?.Invoke();
        }
    
        public void ReduceMakeDeletedAction(Key<IDynamicViewModel> key)
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
    	    
    	    NotificationStateChanged?.Invoke();
        }
    
        public void ReduceUndoMakeDeletedAction(Key<IDynamicViewModel> key)
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
    	    
    	    NotificationStateChanged?.Invoke();
        }
    
        public void ReduceMakeArchivedAction(Key<IDynamicViewModel> key)
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
    	    
    	    NotificationStateChanged?.Invoke();
        }
        
        public void ReduceUndoMakeArchivedAction(Key<IDynamicViewModel> key)
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
    	    
    	    NotificationStateChanged?.Invoke();
        }
    
        public void ReduceClearDefaultAction()
        {
        	lock (_stateModificationLock)
        	{
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = new List<INotification>()
    	        };
    	    }
    	    
    	    NotificationStateChanged?.Invoke();
        }
        
        public void ReduceClearReadAction()
        {
        	lock (_stateModificationLock)
        	{
    	        _notificationState = _notificationState with
    	        {
    	            ReadList = new List<INotification>()
    	        };
    	    }
    	    
    	    NotificationStateChanged?.Invoke();
        }
        
        public void ReduceClearDeletedAction()
        {
        	lock (_stateModificationLock)
        	{
    	        _notificationState = _notificationState with
    	        {
    	            DeletedList = new List<INotification>()
    	        };
    	    }
    	    
    	    NotificationStateChanged?.Invoke();
        }
    
        public void ReduceClearArchivedAction()
        {
        	lock (_stateModificationLock)
        	{
    	        _notificationState = _notificationState with
    	        {
    	            ArchivedList = new List<INotification>()
    	        };
    	    }
    	    
    	    NotificationStateChanged?.Invoke();
        }
    }
    /* End INotificationService */
    
    /* Start IDropdownService */
    using Walk.Common.RazorLib.Keys.Models;
    using Walk.Common.RazorLib.ListExtensions;
    
    namespace Walk.Common.RazorLib.Dropdowns.Models;
    
    public class DropdownService : IDropdownService
    {
    	private DropdownState _dropdownState = new();
    	
    	public event Action? DropdownStateChanged;
    	
    	public DropdownState GetDropdownState() => _dropdownState;
    	
        public void ReduceRegisterAction(DropdownRecord dropdown)
        {
        	var inState = GetDropdownState();
        
    		var indexExistingDropdown = inState.DropdownList.FindIndex(
    			x => x.Key == dropdown.Key);
    
    		if (indexExistingDropdown != -1)
    		{
    			DropdownStateChanged?.Invoke();
    			return;
    		}
    
    		var outDropdownList = new List<DropdownRecord>(inState.DropdownList);
    		outDropdownList.Add(dropdown);
    
            _dropdownState = inState with
            {
                DropdownList = outDropdownList
            };
            
            DropdownStateChanged?.Invoke();
    		return;
        }
    
        public void ReduceDisposeAction(Key<DropdownRecord> key)
        {
        	var inState = GetDropdownState();
        
    		var indexExistingDropdown = inState.DropdownList.FindIndex(
    			x => x.Key == key);
    
    		if (indexExistingDropdown == -1)
    		{
    			DropdownStateChanged?.Invoke();
    			return;
    		}
    			
    		var outDropdownList = new List<DropdownRecord>(inState.DropdownList);
    		outDropdownList.RemoveAt(indexExistingDropdown);
    
            _dropdownState = inState with
            {
                DropdownList = outDropdownList
            };
            
            DropdownStateChanged?.Invoke();
    		return;
        }
    
        public void ReduceClearAction()
        {
        	var inState = GetDropdownState();
        
        	var outDropdownList = new List<DropdownRecord>();
        
            _dropdownState = inState with
            {
                DropdownList = outDropdownList
            };
            
            DropdownStateChanged?.Invoke();
    		return;
        }
    
        public void ReduceFitOnScreenAction(DropdownRecord dropdown)
        {
        	var inState = GetDropdownState();
        
    		var indexExistingDropdown = inState.DropdownList.FindIndex(
    			x => x.Key == dropdown.Key);
    
    		if (indexExistingDropdown == -1)
    		{
    			DropdownStateChanged?.Invoke();
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
            
            DropdownStateChanged?.Invoke();
    		return;
        }
    }
    /* End IDropdownService */
    
    /* Start ITooltipService */
    using System.Text;
    using Walk.Common.RazorLib.JavaScriptObjects.Models;
    
    namespace Walk.Common.RazorLib.Tooltips.Models;
    
    public class TooltipService : ITooltipService
    {
        public TooltipState _tooltipState = new(tooltipModel: null);
    
    	public static readonly Guid _htmlElementIdSalt = Guid.NewGuid();
        
    	public string HtmlElementId { get; } = $"di_dropdown_{_htmlElementIdSalt}";
    	public MeasuredHtmlElementDimensions HtmlElementDimensions { get; set; }
    	public MeasuredHtmlElementDimensions GlobalHtmlElementDimensions { get; set; }
    	public bool IsOffScreenHorizontally { get; }
    	public bool IsOffScreenVertically { get; }
    	public int RenderCount { get; } = 1;
    	
    	public StringBuilder StyleBuilder { get; } = new();
    	
    	public event Action? TooltipStateChanged;
    	
    	public TooltipState GetTooltipState() => _tooltipState;
    	
    	public void SetTooltipModel(ITooltipModel tooltipModel)
    	{
    	    _tooltipState = new TooltipState(tooltipModel);
    	    TooltipStateChanged?.Invoke();
    	}
    }
    /* End ITooltipService */
}
