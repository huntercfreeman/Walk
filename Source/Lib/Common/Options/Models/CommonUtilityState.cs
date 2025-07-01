using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Themes.Models;

namespace Walk.Common.RazorLib.Options.Models;

/* Start IAppDimensionService */
/// <summary>
/// The measurements are in pixels (px).
/// This class is in reference to the "browser", "user agent", "desktop application which is rendering a webview", etc...
///
/// When one resizes the application, then <see cref="IDispatcher"/>.<see cref="IDispatcher.Dispatch"/>
/// the <see cref="SetAppDimensionStateAction"/>.
///
/// Any part of the application can subscribe to this state, and be notified
/// when a <see cref="SetAppDimensionStateAction"/> was reduced.
/// </summary>
///
/// <param name="Width">
/// The unit of measurement is Pixels (px).
/// This describes the Width of the application.
/// </param>
///
/// <param name="Height">
/// The unit of measurement is Pixels (px).
/// This describes the Height of the application.
/// </param>
///
/// <param name="Left">
/// The unit of measurement is Pixels (px).
/// This describes the distance the application is from the left side of the "display/monitor".
/// </param>
///
/// <param name="Top">
/// The unit of measurement is Pixels (px).
/// This describes the distance the application is from the top side of the "display/monitor".
/// </param>
public record struct AppDimensionState(int Width, int Height, int Left, int Top)
{
	public AppDimensionState() : this(0, 0, 0, 0)
	{
	}
}
/* End IAppDimensionService */

/* Start IKeymapService */
/// <summary>
/// The list provided should not be modified after passing it as a parameter..
/// Make a shallow copy, and pass the shallow copy, if further modification of your list will be necessary.
///
/// ---
///
/// Use this state to lookup a <see cref="KeymapLayer"> to determine the 'when' clause of the keybind.
/// If a <see cref="KeymapLayer"> is used, but isn't registered in this state, it will still function properly
/// but the 'when' clause cannot be shown when the user inspects the keybind in the keymap.
/// </summary>
public record struct KeymapState(List<KeymapLayer> KeymapLayerList)
{
    public KeymapState() : this(new List<KeymapLayer>())
    {
    }
}
/* End IKeymapService */

/* Start IAppOptionsService */
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
/* End IAppOptionsService */

/* Start IThemeService */
/// <summary>
/// The list provided should not be modified after passing it as a parameter.
/// Make a shallow copy, and pass the shallow copy, if further modification of your list will be necessary.
/// </summary>
public record struct ThemeState(IReadOnlyList<ThemeRecord> ThemeList)
{
    public ThemeState()
        : this(new List<ThemeRecord>()
            {
                ThemeFacts.VisualStudioDarkThemeClone,
                ThemeFacts.VisualStudioLightThemeClone,
            })
    {
        
    }
}
/* End IThemeService */

/* Start IClipboardService, JavaScriptInteropClipboardService */
/* End IClipboardService, JavaScriptInteropClipboardService */

/* Start IStorageService, LocalStorageService */
/* End IStorageService, LocalStorageService */
    