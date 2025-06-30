namespace Walk.Common.RazorLib.Dynamics.Models;

public interface ICommonUiService
{
    /* Start IOutlineService */
    using Walk.Common.RazorLib.JavaScriptObjects.Models;

    namespace Walk.Common.RazorLib.Outlines.Models;
    
    public interface IOutlineService
    {
    	public event Action? OutlineStateChanged;
    	
    	public OutlineState GetOutlineState();
    
    	public void SetOutline(
    		string? elementId,
    		MeasuredHtmlElementDimensions? measuredHtmlElementDimensions,
    		bool needsMeasured);
    	
    	/// <summary>
    	/// The element which was measured is included in order to "handshake" that
    	/// the element being outlined did not change out from under us.
    	///
    	/// If the element did happen to change out from under us, then this action
    	/// will not do anything.
    	/// </summary>
    	public void SetMeasurements(
    		string? elementId,
    		MeasuredHtmlElementDimensions? measuredHtmlElementDimensions);
    }
    /* End IOutlineService */
    
    /* Start IPanelService */
    using Walk.Common.RazorLib.Contexts.Models;
    using Walk.Common.RazorLib.Keys.Models;
    using Walk.Common.RazorLib.Dimensions.Models;
    
    namespace Walk.Common.RazorLib.Panels.Models;
    
    public interface IPanelService
    {
    	public event Action? PanelStateChanged;
    	
    	public PanelState GetPanelState();
    	
    	public void RegisterPanel(Panel panel);
        public void DisposePanel(Key<Panel> panelKey);
        public void RegisterPanelGroup(PanelGroup panelGroup);
        public void DisposePanelGroup(Key<PanelGroup> panelGroupKey);
    
        public void RegisterPanelTab(
        	Key<PanelGroup> panelGroupKey,
        	IPanelTab panelTab,
        	bool insertAtIndexZero);
    
        public void DisposePanelTab(Key<PanelGroup> panelGroupKey, Key<Panel> panelTabKey);
        public void SetActivePanelTab(Key<PanelGroup> panelGroupKey, Key<Panel> panelTabKey);
        public void SetPanelTabAsActiveByContextRecordKey(Key<ContextRecord> contextRecordKey);
        public void SetDragEventArgs((IPanelTab PanelTab, PanelGroup PanelGroup)? dragEventArgs);
        public void InitializeResizeHandleDimensionUnit(Key<PanelGroup> panelGroupKey, DimensionUnit dimensionUnit);
    }
    /* End IPanelService */
    
    /* Start IWidgetService */
    namespace Walk.Common.RazorLib.Widgets.Models;

    public interface IWidgetService
    {
    	public event Action? WidgetStateChanged;
    	
    	public WidgetState GetWidgetState();
    
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
        public void SetWidget(WidgetModel? widget);
    }
    /* End IWidgetService */
    
    /* Start IDialogService */
    using Walk.Common.RazorLib.Dynamics.Models;
    using Walk.Common.RazorLib.Keys.Models;
    
    namespace Walk.Common.RazorLib.Dialogs.Models;
    
    public interface IDialogService
    {
    	public event Action? DialogStateChanged;
    	public event Action? ActiveDialogKeyChanged;
    	
    	/// <summary>
    	/// Capture the reference and re-use it,
    	/// because the state will change out from under you, if you continually invoke this.
    	/// </summary>
    	public DialogState GetDialogState();
    
        public void ReduceRegisterAction(IDialog dialog);
    
        public void ReduceSetIsMaximizedAction(
            Key<IDynamicViewModel> dynamicViewModelKey,
            bool isMaximized);
        
        public void ReduceSetActiveDialogKeyAction(Key<IDynamicViewModel> dynamicViewModelKey);
        public void ReduceDisposeAction(Key<IDynamicViewModel> dynamicViewModelKey);
    }
    /* End IDialogService */
    
    /* Start INotificationService */
    using Walk.Common.RazorLib.Dynamics.Models;
    using Walk.Common.RazorLib.Keys.Models;
    
    namespace Walk.Common.RazorLib.Notifications.Models;
    
    public interface INotificationService
    {
    	public event Action? NotificationStateChanged;
    	
    	public NotificationState GetNotificationState();
    
        public void ReduceRegisterAction(INotification notification);
        public void ReduceDisposeAction(Key<IDynamicViewModel> key);
        public void ReduceMakeReadAction(Key<IDynamicViewModel> key);
        public void ReduceUndoMakeReadAction(Key<IDynamicViewModel> key);
        public void ReduceMakeDeletedAction(Key<IDynamicViewModel> key);
        public void ReduceUndoMakeDeletedAction(Key<IDynamicViewModel> key);
        public void ReduceMakeArchivedAction(Key<IDynamicViewModel> key);
        public void ReduceUndoMakeArchivedAction(Key<IDynamicViewModel> key);
        public void ReduceClearDefaultAction();
        public void ReduceClearReadAction();
        public void ReduceClearDeletedAction();
        public void ReduceClearArchivedAction();
    }
    /* End INotificationService */
    
    /* Start IDropdownService */
    using Walk.Common.RazorLib.Keys.Models;

    namespace Walk.Common.RazorLib.Dropdowns.Models;
    
    public interface IDropdownService
    {
    	public event Action? DropdownStateChanged;
    	
    	public DropdownState GetDropdownState();
    	
        public void ReduceRegisterAction(DropdownRecord dropdown);
        public void ReduceDisposeAction(Key<DropdownRecord> key);
        public void ReduceClearAction();
        public void ReduceFitOnScreenAction(DropdownRecord dropdown);
    }
    /* End IDropdownService */
    
    /* Start ITooltipService */
    using System.Text;
    using Walk.Common.RazorLib.JavaScriptObjects.Models;
    
    namespace Walk.Common.RazorLib.Tooltips.Models;
    
    public interface ITooltipService
    {
    	public string HtmlElementId { get; }
    	public MeasuredHtmlElementDimensions HtmlElementDimensions { get; set; }
    	public MeasuredHtmlElementDimensions GlobalHtmlElementDimensions { get; set; }
    	public bool IsOffScreenHorizontally { get; }
    	public bool IsOffScreenVertically { get; }
    	public int RenderCount { get; }
    	
    	public StringBuilder StyleBuilder { get; }
    
    	public event Action? TooltipStateChanged;
    	
    	public TooltipState GetTooltipState();
    	
    	public void SetTooltipModel(ITooltipModel tooltipModel);
    }
    /* End ITooltipService */
}
