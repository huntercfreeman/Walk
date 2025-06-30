/* Start IOutlineService */
using Walk.Common.RazorLib.JavaScriptObjects.Models;
/* End IOutlineService */

/* Start IPanelService */
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;
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
/*namespace*/ using Walk.Common.RazorLib.Notifications.Models;
/* End INotificationService */

/* Start IDropdownService */
/*namespace*/ using Walk.Common.RazorLib.Dropdowns.Models;
/* End IDropdownService */

/* Start ITooltipService */
/*namespace*/ using Walk.Common.RazorLib.Tooltips.Models;
/* End ITooltipService */

namespace Walk.Common.RazorLib.Dynamics.Models;

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
	IReadOnlyList<PanelGroup> PanelGroupList,
	IReadOnlyList<Panel> PanelList,
	(IPanelTab PanelTab, PanelGroup PanelGroup)? DragEventArgs)
{
    public PanelState() : this(Array.Empty<PanelGroup>(), Array.Empty<Panel>(), null)
    {
        var topLeftGroup = ConstructTopLeftGroup();
        var topRightGroup = ConstructTopRightGroup();
        var bottomGroup = ConstructBottomGroup();
        
        var outPanelGroupList = new List<PanelGroup>();
        outPanelGroupList.Add(topLeftGroup);
        outPanelGroupList.Add(topRightGroup);
        outPanelGroupList.Add(bottomGroup);
        
        PanelGroupList = outPanelGroupList;
    }

    private static PanelGroup ConstructTopLeftGroup()
    {
        var leftPanelGroup = new PanelGroup(
            PanelFacts.LeftPanelGroupKey,
            Key<Panel>.Empty,
            new ElementDimensions(),
            Array.Empty<IPanelTab>());

        leftPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList.AddRange(new[]
        {
            new DimensionUnit(33.3333, DimensionUnitKind.Percentage),
            new DimensionUnit(0, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract, DimensionUnitFacts.Purposes.OFFSET)
        });

        return leftPanelGroup;
    }

    private static PanelGroup ConstructTopRightGroup()
    {
        var rightPanelGroup = new PanelGroup(
            PanelFacts.RightPanelGroupKey,
            Key<Panel>.Empty,
            new ElementDimensions(),
            Array.Empty<IPanelTab>());

        rightPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList.AddRange(new[]
        {
            new DimensionUnit(33.3333, DimensionUnitKind.Percentage),
            new DimensionUnit(0, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract, DimensionUnitFacts.Purposes.OFFSET),
        });

        return rightPanelGroup;
    }

    private static PanelGroup ConstructBottomGroup()
    {
        var bottomPanelGroup = new PanelGroup(
            PanelFacts.BottomPanelGroupKey,
            Key<Panel>.Empty,
            new ElementDimensions(),
            Array.Empty<IPanelTab>());

        bottomPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList.AddRange(new[]
        {
            new DimensionUnit(22, DimensionUnitKind.Percentage),
            new DimensionUnit(0, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract, DimensionUnitFacts.Purposes.OFFSET),
            new DimensionUnit(SizeFacts.Ide.Header.Height.Value / 2, SizeFacts.Ide.Header.Height.DimensionUnitKind, DimensionOperatorKind.Subtract)
        });

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
