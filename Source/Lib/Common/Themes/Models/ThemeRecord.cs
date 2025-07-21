using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Themes.Models;

public record ThemeRecord(
    int Key,
    string DisplayName,
    string CssClassString,
    ThemeContrastKind ThemeContrastKind,
    ThemeColorKind ThemeColorKind,
    bool IncludeScopeApp,
    bool IncludeScopeTextEditor);