/* Start IOutlineService */
using Walk.Common.RazorLib.JavaScriptObjects.Models;
/* End IOutlineService */

/* Start IPanelService */
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;
/*namespace*/ using Walk.Common.RazorLib.Panels.Models;
/* End IPanelService */

/* Start IWidgetService */
/*namespace*/ using Walk.Common.RazorLib.Widgets.Models;
/* End IWidgetService */

/* Start IDialogService */
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;
/*namespace*/ using Walk.Common.RazorLib.Dialogs.Models;
/* End IDialogService */

/* Start INotificationService */
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;
/*namespace*/ using Walk.Common.RazorLib.Notifications.Models;
/* End INotificationService */

/* Start IDropdownService */
using Walk.Common.RazorLib.Keys.Models;
/*namespace*/ using Walk.Common.RazorLib.Dropdowns.Models;
/* End IDropdownService */

/* Start ITooltipService */
using System.Text;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
/*namespace*/ using Walk.Common.RazorLib.Tooltips.Models;
/* End ITooltipService */

namespace Walk.Common.RazorLib.Dynamics.Models;

/// <summary>
/// This type contains any features that would otherwise have their own `I...Service`,
/// but are too light of an implementation, and too infrequently invoked,
/// to warrant an entire object being allocated.
/// </summary>
public interface ICommonUiService
{
    /* Start IOutlineService */
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
	public void Outline_SetMeasurements(
		string? elementId,
		MeasuredHtmlElementDimensions? measuredHtmlElementDimensions);
    /* End IOutlineService */
    
    /* Start IPanelService */
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
    public void Panel_SetDragEventArgs((IPanelTab PanelTab, PanelGroup PanelGroup)? dragEventArgs);
    public void Panel_InitializeResizeHandleDimensionUnit(Key<PanelGroup> panelGroupKey, DimensionUnit dimensionUnit);
    /* End IPanelService */

    /* Start IWidgetService */
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
    /* End IWidgetService */
    
    /* Start IDialogService */
	public event Action? DialogStateChanged;
	public event Action? ActiveDialogKeyChanged;
	
	/// <summary>
	/// Capture the reference and re-use it,
	/// because the state will change out from under you, if you continually invoke this.
	/// </summary>
	public DialogState GetDialogState();

    public void Dialog_ReduceRegisterAction(IDialog dialog);

    public void Dialog_ReduceSetIsMaximizedAction(
        Key<IDynamicViewModel> dynamicViewModelKey,
        bool isMaximized);
    
    public void Dialog_ReduceSetActiveDialogKeyAction(Key<IDynamicViewModel> dynamicViewModelKey);
    public void Dialog_ReduceDisposeAction(Key<IDynamicViewModel> dynamicViewModelKey);
    /* End IDialogService */
    
    /* Start INotificationService */
	public event Action? NotificationStateChanged;
	
	public NotificationState GetNotificationState();

    public void Notification_ReduceRegisterAction(INotification notification);
    public void Notification_ReduceDisposeAction(Key<IDynamicViewModel> key);
    public void Notification_ReduceMakeReadAction(Key<IDynamicViewModel> key);
    public void Notification_ReduceUndoMakeReadAction(Key<IDynamicViewModel> key);
    public void Notification_ReduceMakeDeletedAction(Key<IDynamicViewModel> key);
    public void Notification_ReduceUndoMakeDeletedAction(Key<IDynamicViewModel> key);
    public void Notification_ReduceMakeArchivedAction(Key<IDynamicViewModel> key);
    public void Notification_ReduceUndoMakeArchivedAction(Key<IDynamicViewModel> key);
    public void Notification_ReduceClearDefaultAction();
    public void Notification_ReduceClearReadAction();
    public void Notification_ReduceClearDeletedAction();
    public void Notification_ReduceClearArchivedAction();
    /* End INotificationService */
    
    /* Start IDropdownService */
	public event Action? DropdownStateChanged;
	
	public DropdownState GetDropdownState();
	
    public void Dropdown_ReduceRegisterAction(DropdownRecord dropdown);
    public void Dropdown_ReduceDisposeAction(Key<DropdownRecord> key);
    public void Dropdown_ReduceClearAction();
    public void Dropdown_ReduceFitOnScreenAction(DropdownRecord dropdown);
    /* End IDropdownService */
    
    /* Start ITooltipService */
	public string Tooltip_HtmlElementId { get; }
	public MeasuredHtmlElementDimensions Tooltip_HtmlElementDimensions { get; set; }
	public MeasuredHtmlElementDimensions Tooltip_GlobalHtmlElementDimensions { get; set; }
	public bool Tooltip_IsOffScreenHorizontally { get; }
	public bool Tooltip_IsOffScreenVertically { get; }
	public int Tooltip_RenderCount { get; }
	
	public StringBuilder Tooltip_StyleBuilder { get; }

	public event Action? TooltipStateChanged;
	
	public TooltipState GetTooltipState();
	
	public void SetTooltipModel(ITooltipModel tooltipModel);
    /* End ITooltipService */
}
