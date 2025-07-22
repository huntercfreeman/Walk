namespace Walk.Ide.RazorLib;

public partial class IdeService
{
    public event Action<IdeStateChangedKind>? IdeStateChanged;
}

public enum IdeStateChangedKind
{
    CodeSearchStateChanged,
    CommandBarStateChanged,
    FolderExplorerStateChanged,
    Ide_IdeStateChanged,
    Ide_StartupControlStateChanged,
    InputFileStateChanged,
    TerminalStateChanged,
    TerminalGroupStateChanged,
}
