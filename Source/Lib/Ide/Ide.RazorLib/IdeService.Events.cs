namespace Walk.Ide.RazorLib;

public partial class IdeService
{
    public event Action? CodeSearchStateChanged;
    public event Action? CommandBarStateChanged;
    public event Action? FolderExplorerStateChanged;
    public event Action? Ide_IdeStateChanged;
    public event Action? Ide_StartupControlStateChanged;
    public event Action? InputFileStateChanged;
    public event Action? TerminalStateChanged;
    public event Action? TerminalGroupStateChanged;
}
