namespace Walk.Common.RazorLib;

public partial class CommonService
{
    public event Action<CommonUiEventKind>? CommonUiStateChanged;

    public event Action? AppDimensionStateChanged;
    public event Action? AppOptionsStateChanged;
    public event Action? DragStateChanged;
}
