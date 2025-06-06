using Walk.Common.RazorLib.Themes.Models;

namespace Walk.Common.RazorLib.Options.Models;

public record struct AppOptionsState(CommonOptions Options)
{
    public const int DEFAULT_FONT_SIZE_IN_PIXELS = 20;
    public const int MINIMUM_FONT_SIZE_IN_PIXELS = 5;
    
    public const int DEFAULT_ICON_SIZE_IN_PIXELS = 18;
    public const int MINIMUM_ICON_SIZE_IN_PIXELS = 5;
    
    public const int DEFAULT_RESIZE_HANDLE_WIDTH_IN_PIXELS = 4;
    public const int MINIMUM_RESIZE_HANDLE_WIDTH_IN_PIXELS = 4;
    
    public const int DEFAULT_RESIZE_HANDLE_HEIGHT_IN_PIXELS = 4;
    public const int MINIMUM_RESIZE_HANDLE_HEIGHT_IN_PIXELS = 4;
    
    public static readonly CommonOptions DefaultCommonOptions = new(
        FontSizeInPixels: DEFAULT_FONT_SIZE_IN_PIXELS,
        IconSizeInPixels: DEFAULT_ICON_SIZE_IN_PIXELS,
        ResizeHandleWidthInPixels: DEFAULT_RESIZE_HANDLE_WIDTH_IN_PIXELS,
        ResizeHandleHeightInPixels: DEFAULT_RESIZE_HANDLE_HEIGHT_IN_PIXELS,
        ThemeKey: ThemeFacts.VisualStudioDarkThemeClone.Key,
        FontFamily: null,
        ShowPanelTitles: false);

    public AppOptionsState() : this(DefaultCommonOptions)
    {
    }
}