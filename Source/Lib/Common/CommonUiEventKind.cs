namespace Walk.Common.RazorLib;

public enum CommonUiEventKind
{
    DialogStateChanged,
    ActiveDialogKeyChanged,
    WidgetStateChanged,
    NotificationStateChanged,
    PanelStateChanged,
    DropdownStateChanged,
    OutlineStateChanged,
    TooltipStateChanged,
    TreeViewStateChanged,
    DragStateChanged,
    AppDimensionStateChanged,
    AppOptionsStateChanged,
    // Regarding events that relate to 'LineHeightNeedsMeasured'
    //
    // The order of these events is important because the mainlayout will
    // update the CSS of all the descendent elements.
    //
    // The WalkCommonInitializer doesn't actually apply the CSS itself.
    // It is reliant on the IdeMainLayout having re-rendered first.
    //
    LineHeightNeedsMeasured,
}
