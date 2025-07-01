using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Storages.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Themes.Models;

namespace Walk.Common.RazorLib.Options.Models;

public interface ICommonUtilityService
{
	/* Start IAppDimensionService */
	public event Action? AppDimensionStateChanged;
	
	public AppDimensionState GetAppDimensionState();
	
	public void SetAppDimensions(Func<AppDimensionState, AppDimensionState> withFunc);

	/// <summary>
	/// This action is for resizing that is done to an HTML element that is rendered.
	/// Ex: <see cref="Walk.Common.RazorLib.Resizes.Displays.ResizableColumn"/>
	///
	/// Since these resizes won't affect the application's dimensions as a whole,
	/// nothing needs to be used as a parameter, its just a way to notify.
	/// </summary>
	public void AppDimension_NotifyIntraAppResize(bool useExtraEvent = true);

	/// <summary>
	/// This action is for resizing that is done to the "user agent" / "window" / "document".
	/// </summary>
	public void AppDimension_NotifyUserAgentResize(bool useExtraEvent = true);
	/* End IAppDimensionService */
	
    /* Start IKeymapService */
	public event Action? KeymapStateChanged;
    
    public KeymapState GetKeymapState();
    
    public void RegisterKeymapLayer(KeymapLayer keymapLayer);
    public void DisposeKeymapLayer(Key<KeymapLayer> keymapLayerKey);
    /* End IKeymapService */
    
    /* Start IAppOptionsService */
    /// <summary>
    /// This is used when interacting with the <see cref="IStorageService"/> to set and get data.
    /// </summary>
    public string Options_StorageKey { get; }
    public string Options_ThemeCssClassString { get; }
    public string? Options_FontFamilyCssStyleString { get; }
    public string Options_FontSizeCssStyleString { get; }
    public string Options_ColorSchemeCssStyleString { get; }
    public bool Options_ShowPanelTitles { get; }
    public string Options_ShowPanelTitlesCssClass { get; }
    
    public string Options_ResizeHandleCssWidth { get; set; }
    public string Options_ResizeHandleCssHeight { get; set; }
    
    /// <summary>
    /// Very hacky property to avoid circular services while I work out the details of things.
    /// </summary>
    public CommonBackgroundTaskApi Options_CommonBackgroundTaskApi { get; set; }
    
    public event Action? AppOptionsStateChanged;
	
	public AppOptionsState GetAppOptionsState();

    public void Options_SetActiveThemeRecordKey(Key<ThemeRecord> themeKey, bool updateStorage = true);
    public void Options_SetTheme(ThemeRecord theme, bool updateStorage = true);
    public void Options_SetFontFamily(string? fontFamily, bool updateStorage = true);
    public void Options_SetFontSize(int fontSizeInPixels, bool updateStorage = true);
    public void Options_SetResizeHandleWidth(int resizeHandleWidthInPixels, bool updateStorage = true);
    public void Options_SetResizeHandleHeight(int resizeHandleHeightInPixels, bool updateStorage = true);
    public void Options_SetIconSize(int iconSizeInPixels, bool updateStorage = true);
    public Task Options_SetFromLocalStorageAsync();
    public void Options_WriteToStorage();
    /* End IAppOptionsService */
    
    /* Start IThemeService */
	public event Action? ThemeStateChanged;
	
	public ThemeState GetThemeState();

    public void Theme_RegisterAction(ThemeRecord theme);
    public void Theme_RegisterRangeAction(IReadOnlyList<ThemeRecord> theme);
    public void Theme_DisposeAction(Key<ThemeRecord> themeKey);
    /* End IThemeService */
}
