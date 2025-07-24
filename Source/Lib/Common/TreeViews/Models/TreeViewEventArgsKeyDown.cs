namespace Walk.Common.RazorLib.TreeViews.Models;

public record struct TreeViewEventArgsKeyDown(
    string Key,
    string Code,
    bool CtrlKey,
    bool ShiftKey,
    bool AltKey,
    bool MetaKey,
    double ScrollLeft,
    double ScrollTop);
