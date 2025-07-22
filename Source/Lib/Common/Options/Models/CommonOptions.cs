using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Themes.Models;

namespace Walk.Common.RazorLib.Options.Models;

public record CommonOptions(
    int FontSizeInPixels,
    int IconSizeInPixels,
    int ResizeHandleWidthInPixels,
    int ResizeHandleHeightInPixels,
    int ThemeKey,
    string? FontFamily);
