namespace Walk.Common.RazorLib.Themes.Models;

public record struct ThemeRecord(
    int Key,
    string DisplayName,
    string CssClassString,
    ThemeContrastKind ThemeContrastKind,
    ThemeColorKind ThemeColorKind,
    bool IncludeScopeApp,
    bool IncludeScopeTextEditor);
