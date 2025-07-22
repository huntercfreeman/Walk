namespace Walk.Common.RazorLib;

public partial class CommonService
{
    public event Action? AppDimensionStateChanged;
    public event Action? AppOptionsStateChanged;
    public event Action? ContextStateChanged;
    public event Action? ContextSwitchStateChanged;
    public event Action? DragStateChanged;
    public event Action? KeymapStateChanged;
    public event Action? ThemeStateChanged;
    public event Action? TreeViewStateChanged;
}
