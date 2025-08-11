using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Tooltips.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib;

/* Start IAppDimensionService */
/// <summary>
/// The measurements are in pixels (px).
/// This class is in reference to the "browser", "user agent", "desktop application which is rendering a webview", etc...
///
/// When one resizes the application, then <see cref="IDispatcher"/>.<see cref="IDispatcher.Dispatch"/>
/// the <see cref="SetAppDimensionStateAction"/>.
///
/// Any part of the application can subscribe to this state, and be notified
/// when a <see cref="SetAppDimensionStateAction"/> was reduced.
/// </summary>
///
/// <param name="Width">
/// The unit of measurement is Pixels (px).
/// This describes the Width of the application.
/// </param>
///
/// <param name="Height">
/// The unit of measurement is Pixels (px).
/// This describes the Height of the application.
/// </param>
///
/// <param name="Left">
/// The unit of measurement is Pixels (px).
/// This describes the distance the application is from the left side of the "display/monitor".
/// </param>
///
/// <param name="Top">
/// The unit of measurement is Pixels (px).
/// This describes the distance the application is from the top side of the "display/monitor".
/// </param>
public record struct AppDimensionState(int Width, int Height, int Left, int Top)
{
    public AppDimensionState() : this(0, 0, 0, 0)
    {
    }
}
/* End IAppDimensionService */

/* Start IKeymapService */
/// <summary>
/// The list provided should not be modified after passing it as a parameter..
/// Make a shallow copy, and pass the shallow copy, if further modification of your list will be necessary.
///
/// ---
///
/// Use this state to lookup a <see cref="KeymapLayer"> to determine the 'when' clause of the keybind.
/// If a <see cref="KeymapLayer"> is used, but isn't registered in this state, it will still function properly
/// but the 'when' clause cannot be shown when the user inspects the keybind in the keymap.
/// </summary>
public record struct KeymapState(List<KeymapLayer> KeymapLayerList)
{
    public KeymapState() : this(new List<KeymapLayer>())
    {
    }
}
/* End IKeymapService */

/* Start IAppOptionsService */
public record struct AppOptionsState(CommonOptions Options)
{
    public const int DEFAULT_FONT_SIZE_IN_PIXELS = 20;
    public const int MINIMUM_FONT_SIZE_IN_PIXELS = 5;
    
    public const int DEFAULT_ICON_SIZE_IN_PIXELS = 18;
    public const int MINIMUM_ICON_SIZE_IN_PIXELS = 5;
    
    public const int DEFAULT_RESIZE_HANDLE_WIDTH_IN_PIXELS = 4;
    public const int MINIMUM_RESIZE_HANDLE_WIDTH_IN_PIXELS = 4;
    
    public const int DEFAULT_RESIZE_HANDLE_HEIGHT_IN_PIXELS = 4;
    public const int MINIMUM_RESIZE_HANDLE_HEIGHT_IN_PIXELS = 4;
    
    public static readonly CommonOptions DefaultCommonOptions = new(
        FontSizeInPixels: DEFAULT_FONT_SIZE_IN_PIXELS,
        IconSizeInPixels: DEFAULT_ICON_SIZE_IN_PIXELS,
        ResizeHandleWidthInPixels: DEFAULT_RESIZE_HANDLE_WIDTH_IN_PIXELS,
        ResizeHandleHeightInPixels: DEFAULT_RESIZE_HANDLE_HEIGHT_IN_PIXELS,
        ThemeKey: CommonFacts.VisualStudioDarkThemeClone.Key,
        FontFamily: null);

    public AppOptionsState() : this(DefaultCommonOptions)
    {
    }
}
/* End IAppOptionsService */

/* Start IThemeService */
/// <summary>
/// The list provided should not be modified after passing it as a parameter.
/// Make a shallow copy, and pass the shallow copy, if further modification of your list will be necessary.
/// </summary>
public record struct ThemeState(IReadOnlyList<ThemeRecord> ThemeList)
{
    public ThemeState()
        : this(new List<ThemeRecord>()
            {
                CommonFacts.VisualStudioDarkThemeClone,
                CommonFacts.VisualStudioLightThemeClone,
                CommonFacts.DarkTheme,
                CommonFacts.LightTheme,
            })
    {
        
    }
}
/* End IThemeService */

/* Start IClipboardService, JavaScriptInteropClipboardService */
/* End IClipboardService, JavaScriptInteropClipboardService */

/* Start IStorageService, LocalStorageService */
/* End IStorageService, LocalStorageService */



/* Start IOutlineService */
public record struct OutlineState(
    string? ElementId,
    MeasuredHtmlElementDimensions? MeasuredHtmlElementDimensions,
    bool NeedsMeasured)
{
    public OutlineState() : this(null, null, false)
    {
    }
}
/* End IOutlineService */

/* Start IPanelService */
/// <summary>
/// Once the 'PanelGroupList'/'PanelList' are exposed publically,
/// they should NOT be modified.
/// Make a shallow copy 'new List<PanelGroup>(panelState.PanelGroupList);'/
/// 'new List<Panel>(panelState.PanelList);'
/// and modify the shallow copy if modification of the list
/// after exposing it publically is necessary.
///
/// ---
///
/// TODO: SphagettiCode - The resizing and hiding/showing is a bit scuffed. (2023-09-19)
/// </summary>
public record struct PanelState(
    IReadOnlyList<Panel> PanelList,
    (IPanelTab PanelTab, PanelGroup PanelGroup)? DragEventArgs)
{
    public PanelState() : this(new List<Panel>(), null)
    {
        TopLeftPanelGroup = ConstructTopLeftGroup();
        TopRightPanelGroup = ConstructTopRightGroup();
        BottomPanelGroup = ConstructBottomGroup();
    }
    
    public PanelGroup TopLeftPanelGroup { get; set; }
    public PanelGroup TopRightPanelGroup { get; set; }
    public PanelGroup BottomPanelGroup { get; set; }

    private static PanelGroup ConstructTopLeftGroup()
    {
        var leftPanelGroup = new PanelGroup(
            CommonFacts.LeftPanelGroupKey,
            Key<Panel>.Empty,
            new ElementDimensions(),
            new List<IPanelTab>());

        leftPanelGroup.ElementDimensions.Width_Base_0 = new DimensionUnit(33.3333, DimensionUnitKind.Percentage);
        leftPanelGroup.ElementDimensions.Width_Offset = new DimensionUnit(0, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract, DimensionUnitPurposeKind.Offset);

        return leftPanelGroup;
    }

    private static PanelGroup ConstructTopRightGroup()
    {
        var rightPanelGroup = new PanelGroup(
            CommonFacts.RightPanelGroupKey,
            Key<Panel>.Empty,
            new ElementDimensions(),
            new List<IPanelTab>());

        rightPanelGroup.ElementDimensions.Width_Base_0 = new DimensionUnit(33.3333, DimensionUnitKind.Percentage);
        rightPanelGroup.ElementDimensions.Width_Offset = new DimensionUnit(0, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract, DimensionUnitPurposeKind.Offset);
        
        return rightPanelGroup;
    }

    private static PanelGroup ConstructBottomGroup()
    {
        var bottomPanelGroup = new PanelGroup(
            CommonFacts.BottomPanelGroupKey,
            Key<Panel>.Empty,
            new ElementDimensions(),
            new List<IPanelTab>());

        bottomPanelGroup.ElementDimensions.Height_Base_0 = new DimensionUnit(22, DimensionUnitKind.Percentage);
        bottomPanelGroup.ElementDimensions.Height_Base_1 = new DimensionUnit(CommonFacts.Ide_Header_Height.Value / 2, CommonFacts.Ide_Header_Height.DimensionUnitKind, DimensionOperatorKind.Subtract);
        bottomPanelGroup.ElementDimensions.Height_Offset = new DimensionUnit(0, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract, DimensionUnitPurposeKind.Offset);

        return bottomPanelGroup;
    }
}
/* End IPanelService */

/* Start IWidgetService */
/// <summary>
/// This UI is similar, but not equivalent, to <see cref="DialogState"/>.<br/>
///
/// This UI:<br/>
/// - Only one can be rendered at any given time<br/>
/// - The <see cref="Walk.Common.RazorLib.OutOfBoundsClicks.Displays.OutOfBoundsClickDisplay"/>
///       will be rendered, so if the user clicks off, the widget will stop being rendered.<br/>
/// - If the user onfocusout events from the widget, the widget will stop being rendered.<br/>
/// </summary>
public record struct WidgetState(WidgetModel? Widget)
{
    public WidgetState() : this((WidgetModel?)null)
    {
        
    }
}
/* End IWidgetService */

/* Start IDialogService */
/// <summary>
/// The list provided should not be modified after passing it as a parameter.
/// Make a shallow copy, and pass the shallow copy, if further modification of your list will be necessary.
/// </summary>
public record struct DialogState
{
    public DialogState()
    {
    }

    public IReadOnlyList<IDialog> DialogList { get; init; } = Array.Empty<IDialog>();
    /// <summary>
    /// The active dialog is either:<br/><br/>
    /// -the one which has focus within it,<br/>
    /// -most recently focused dialog,<br/>
    /// -most recently registered dialog
    /// <br/><br/>
    /// The motivation for this property is when two dialogs are rendered
    /// at the same time, and one overlaps the other. One of those
    /// dialogs is hidden by the other. To be able to 'bring to front'
    /// the dialog one is interested in by setting focus to it, is useful.
    /// </summary>
    public Key<IDynamicViewModel> ActiveDialogKey { get; init; } = Key<IDynamicViewModel>.Empty;
}
/* End IDialogService */

/* Start INotificationService */
/// <summary>
/// The list provided should not be modified after passing it as a parameter.
/// Make a shallow copy, and pass the shallow copy, if further modification of your list will be necessary.
/// 
/// ---
/// 
/// TODO: SphagettiCode - The NotificationState is written such that there are (2023-09-19)
/// 4 lists. One foreach filter option. And the NotificationRecord gets shuffled around.
/// This is odd. Perhaps use one list and filter it?
/// </summary>
public record struct NotificationState(
    IReadOnlyList<INotification> DefaultList,
    IReadOnlyList<INotification> ReadList,
    IReadOnlyList<INotification> ArchivedList,
    IReadOnlyList<INotification> DeletedList)
{
    public NotificationState() : this(
        Array.Empty<INotification>(),
        Array.Empty<INotification>(),
        Array.Empty<INotification>(),
        Array.Empty<INotification>())
    {
        
    }
}
/* End INotificationService */

/* Start IDropdownService */
/// <summary>
/// The list provided should not be modified after passing it as a parameter.
/// Make a shallow copy, and pass the shallow copy, if further modification of your list will be necessary.
/// </summary>
public record struct DropdownState(IReadOnlyList<DropdownRecord> DropdownList)
{
    public DropdownState() : this(Array.Empty<DropdownRecord>())
    {
        
    }
}
/* End IDropdownService */

/* Start ITooltipService */
public record struct TooltipState
{
    public TooltipState(ITooltipModel tooltipModel)
    {
        TooltipModel = tooltipModel;
    }

    public ITooltipModel TooltipModel { get; }
}
/* End ITooltipService */

/* Start IDragService */
public record struct DragState(
    bool ShouldDisplay,
    MouseEventArgs? MouseEventArgs,
    IDrag? Drag,
    ElementDimensions DragElementDimensions)
{
    public DragState() : this (false, null, null, DialogHelper.ConstructDefaultElementDimensions())
    {
        
    }
}
/* End IDragService */

/* Start ITreeViewService, TreeViewService */
/// <summary>
/// The list provided should not be modified after passing it as a parameter.
/// Make a shallow copy, and pass the shallow copy, if further modification of your list will be necessary.
/// 
/// ---
///
/// Experimenting with 'List' instead of 'ImmutableList / ImmutableArray / Array'
/// Presumption being that in the case of 'ImmutableList / ImmutableArray'
/// the data would be stored as a tree structure, where each node is an object allocation.
/// That being said, I wonder if 'ImmutableList' and etc... are
/// trees that are made via a List that contains the nodes side by side and maybe tracks the length
/// of each sub-tree in order to traverse.
/// If it is done this way each node can more easily be a struct?
/// I want the C# parser to have the CompilationUnit be a struct foreach node. I'm not
/// sure how feasible that is I'm also super delirious at the moment.
/// Array.Empty issue.... store new List<T> somewhere and reference it?
/// but what if someone modifies it.
/// have property be IReadOnlyList and private List that is the?
/// </summary>
public record struct TreeViewState(IReadOnlyList<TreeViewContainer> ContainerList)
{
    public TreeViewState() : this(Array.Empty<TreeViewContainer>())
    {
    }
}
/* End ITreeViewService, TreeViewService */
