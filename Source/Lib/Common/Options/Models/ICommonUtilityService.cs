using System.Text;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.JsRuntimes.Models;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Tooltips.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;

using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Drags.Models;

namespace Walk.Common.RazorLib.Options.Models;

public interface ICommonUtilityService : IBackgroundTaskGroup
{
    public ICommonComponentRenderers CommonComponentRenderers { get; }
    public WalkHostingInformation WalkHostingInformation { get; }

	/* Start IAppDimensionService */
	public event Action? AppDimensionStateChanged;
	
	public AppDimensionState GetAppDimensionState();
	
	public void SetAppDimensions(Func<AppDimensionState, AppDimensionState> withFunc);

	/// <summary>
	/// This action is for resizing that is done to an HTML element that is rendered.
	/// Ex: <see cref="Walk.Common.RazorLib.Resizes.Displays.ResizableColumn"/>
	///
	/// Since these resizes won't affect the application's dimensions as a whole,
	/// nothing needs to be used as a parameter, its just a way to notify.
	/// </summary>
	public void AppDimension_NotifyIntraAppResize(bool useExtraEvent = true);

	/// <summary>
	/// This action is for resizing that is done to the "user agent" / "window" / "document".
	/// </summary>
	public void AppDimension_NotifyUserAgentResize(bool useExtraEvent = true);
	/* End IAppDimensionService */
	
    /* Start IKeymapService */
	public event Action? KeymapStateChanged;
    
    public KeymapState GetKeymapState();
    
    public void RegisterKeymapLayer(KeymapLayer keymapLayer);
    public void DisposeKeymapLayer(Key<KeymapLayer> keymapLayerKey);
    /* End IKeymapService */
    
    /* Start IAppOptionsService */
    /// <summary>
    /// This is used when interacting with the <see cref="IStorageService"/> to set and get data.
    /// </summary>
    public string Options_StorageKey { get; }
    public string Options_ThemeCssClassString { get; }
    public string? Options_FontFamilyCssStyleString { get; }
    public string Options_FontSizeCssStyleString { get; }
    public string Options_ColorSchemeCssStyleString { get; }
    public bool Options_ShowPanelTitles { get; }
    public string Options_ShowPanelTitlesCssClass { get; }
    
    public string Options_ResizeHandleCssWidth { get; set; }
    public string Options_ResizeHandleCssHeight { get; set; }
    
    public event Action? AppOptionsStateChanged;
	
	public AppOptionsState GetAppOptionsState();

    public void Options_SetActiveThemeRecordKey(Key<ThemeRecord> themeKey, bool updateStorage = true);
    public void Options_SetTheme(ThemeRecord theme, bool updateStorage = true);
    public void Options_SetFontFamily(string? fontFamily, bool updateStorage = true);
    public void Options_SetFontSize(int fontSizeInPixels, bool updateStorage = true);
    public void Options_SetResizeHandleWidth(int resizeHandleWidthInPixels, bool updateStorage = true);
    public void Options_SetResizeHandleHeight(int resizeHandleHeightInPixels, bool updateStorage = true);
    public void Options_SetIconSize(int iconSizeInPixels, bool updateStorage = true);
    public Task Options_SetFromLocalStorageAsync();
    public void Options_WriteToStorage();
    /* End IAppOptionsService */
    
    /* Start IThemeService */
	public event Action? ThemeStateChanged;
	
	public ThemeState GetThemeState();

    public void Theme_RegisterAction(ThemeRecord theme);
    public void Theme_RegisterRangeAction(IReadOnlyList<ThemeRecord> theme);
    public void Theme_DisposeAction(Key<ThemeRecord> themeKey);
    /* End IThemeService */
    
    /* Start IClipboardService, JavaScriptInteropClipboardService */
    public Task<string> ReadClipboard();
    public Task SetClipboard(string value);
    /* End IClipboardService, JavaScriptInteropClipboardService */
    
    /* Start IStorageService, LocalStorageService */
    public ValueTask Storage_SetValue(string key, object? value);
    public ValueTask<object?> Storage_GetValue(string key);
    /* End IStorageService, LocalStorageService */
    
    public IEnvironmentProvider EnvironmentProvider { get; }
    public IFileSystemProvider FileSystemProvider { get; }
    
    public WalkCommonJavaScriptInteropApi JsRuntimeCommonApi { get; }
    
    public event Action<CommonUiEventKind>? CommonUiStateChanged;

    /* Start IOutlineService */
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

	public TooltipState GetTooltipState();
	
	public void SetTooltipModel(ITooltipModel tooltipModel);
    /* End ITooltipService */

    /* Start CommonBackgroundTaskApi */
    /// <summary>
    /// A shared StringBuilder, but only use this if you know for certain you are on the "UI thread".
    /// </summary>
    public StringBuilder UiStringBuilder { get; }
    
    public WalkCommonConfig CommonConfig { get; }
    
    public void Enqueue(CommonWorkArgs commonWorkArgs);
    /* End CommonBackgroundTaskApi */

    /* Start IContextService */
	public event Action? ContextStateChanged;
    
    public ContextState GetContextState();
    
    public ContextRecord GetContextRecord(Key<ContextRecord> contextKey);
    
    public ContextSwitchState GetContextSwitchState();
    
    public void SetFocusedContextKey(Key<ContextRecord> contextKey);
    public void SetContextKeymap(Key<ContextRecord> contextKey, IKeymap keymap);
    
    public void RegisterContextSwitchGroup(ContextSwitchGroup contextSwitchGroup);
    /* End IContextService */

    /* Start IDragService */
	public event Action? DragStateChanged;
	
    public DragState GetDragState();
    
    public void Drag_ShouldDisplayAndMouseEventArgsSetAction(
        bool shouldDisplay,
        MouseEventArgs? mouseEventArgs);
    
    public void Drag_ShouldDisplayAndMouseEventArgsAndDragSetAction(
        bool shouldDisplay,
		MouseEventArgs? mouseEventArgs,
		IDrag? drag);
    /* End IDragService */
}
