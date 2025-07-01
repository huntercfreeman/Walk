using System.Text.Json;
using System.Text;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Storages.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Themes.Models;

namespace Walk.Common.RazorLib.Options.Models;

public class CommonUtilityService : ICommonUtilityService
{
    private readonly object _stateModificationLock = new();

    public CommonUtilityService(
        IStorageService storageService,
        BackgroundTaskService backgroundTaskService)
    {
        _storageService = storageService;
        _backgroundTaskService = backgroundTaskService;
    
        _debounceExtraEvent = new(
	    	TimeSpan.FromMilliseconds(250),
	    	CancellationToken.None,
	    	(_, _) =>
	    	{
	    	    AppDimension_NotifyIntraAppResize(useExtraEvent: false);
	    	    return Task.CompletedTask;
		    });
    }

    /* Start IAppDimensionService */
    /// <summary>
    /// To avoid unexpected HTML movements when responding to a AppDimensionStateChanged
    /// this debounce will add 1 extra event after everything has "settled".
    ///
    /// `byte` is just a throwaway generic type, it isn't used.
    /// </summary>
    private readonly Debounce<byte> _debounceExtraEvent;
    
    private AppDimensionState _appDimensionState;
	
	public event Action? AppDimensionStateChanged;
	
	public AppDimensionState GetAppDimensionState() => _appDimensionState;
	
	public void SetAppDimensions(Func<AppDimensionState, AppDimensionState> withFunc)
	{
		lock (_stateModificationLock)
		{
			_appDimensionState = withFunc.Invoke(_appDimensionState);
        }

        AppDimensionStateChanged?.Invoke();
    }

	public void AppDimension_NotifyIntraAppResize(bool useExtraEvent = true)
	{
		AppDimensionStateChanged?.Invoke();
		
		if (useExtraEvent)
		    _debounceExtraEvent.Run(default);
    }

	public void AppDimension_NotifyUserAgentResize(bool useExtraEvent = true)
	{
		AppDimensionStateChanged?.Invoke();
		
		if (useExtraEvent)
		    _debounceExtraEvent.Run(default);
    }
	/* End IAppDimensionService */
	
    /* Start IKeymapService */
    private KeymapState _keymapState = new();
    
    public event Action? KeymapStateChanged;
    
    public KeymapState GetKeymapState() => _keymapState;
    
    public void RegisterKeymapLayer(KeymapLayer keymapLayer)
    {
        lock (_stateModificationLock)
        {
            var inState = GetKeymapState();

            if (!inState.KeymapLayerList.Any(x => x.Key == keymapLayer.Key))
            {
                var outKeymapLayerList = new List<KeymapLayer>(inState.KeymapLayerList);
                outKeymapLayerList.Add(keymapLayer);
    
                _keymapState = inState with
                {
                    KeymapLayerList = outKeymapLayerList
                };
            }
        }

        KeymapStateChanged?.Invoke();
    }
    
    public void DisposeKeymapLayer(Key<KeymapLayer> keymapLayerKey)
    {
        lock (_stateModificationLock)
        {
            var inState = GetKeymapState();

            var indexExisting = inState.KeymapLayerList.FindIndex(x => x.Key == keymapLayerKey);

            if (indexExisting != -1)
            {
                var outKeymapLayerList = new List<KeymapLayer>(inState.KeymapLayerList);
                outKeymapLayerList.RemoveAt(indexExisting);
    
                _keymapState = inState with
                {
                    KeymapLayerList = outKeymapLayerList
                };
            }
        }

        KeymapStateChanged?.Invoke();
    }
    /* End IKeymapService */
    
    /* Start IAppOptionsService */
    private readonly IStorageService _storageService;
    private readonly BackgroundTaskService _backgroundTaskService;
    
    private AppOptionsState _appOptionsState = new();

    public CommonBackgroundTaskApi Options_CommonBackgroundTaskApi { get; set; }

#if DEBUG
    public string Options_StorageKey => "walk-common_theme-storage-key-debug"; 
#else
    public string Options_StorageKey => "walk-common_theme-storage-key";
#endif

	public string Options_ThemeCssClassString { get; set; } = ThemeFacts.VisualStudioDarkThemeClone.CssClassString;

    public string? Options_FontFamilyCssStyleString { get; set; }

    public string Options_FontSizeCssStyleString { get; set; }
    
    public string Options_ResizeHandleCssWidth { get; set; } =
        $"width: {AppOptionsState.DEFAULT_RESIZE_HANDLE_WIDTH_IN_PIXELS.ToCssValue()}px";
        
    public string Options_ResizeHandleCssHeight { get; set; } =
        $"height: {AppOptionsState.DEFAULT_RESIZE_HANDLE_HEIGHT_IN_PIXELS.ToCssValue()}px";
    
    public bool Options_ShowPanelTitles => GetAppOptionsState().Options.ShowPanelTitles;
    
    public string Options_ShowPanelTitlesCssClass => GetAppOptionsState().Options.ShowPanelTitles
    	? string.Empty
    	: "di_ide_section-no-title";

    public string Options_ColorSchemeCssStyleString { get; set; }

	public event Action? AppOptionsStateChanged;
	
	public AppOptionsState GetAppOptionsState() => _appOptionsState;

    public void Options_SetActiveThemeRecordKey(Key<ThemeRecord> themeKey, bool updateStorage = true)
    {
    	var inState = GetAppOptionsState();
    	
        _appOptionsState = inState with
        {
            Options = inState.Options with
            {
                ThemeKey = themeKey
            }
        };
        
        HandleThemeChange();
        
        AppOptionsStateChanged?.Invoke();

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetTheme(ThemeRecord theme, bool updateStorage = true)
    {
        var inState = GetAppOptionsState();
    	
        _appOptionsState = inState with
        {
            Options = inState.Options with
            {
                ThemeKey = theme.Key
            }
        };
        
        HandleThemeChange();
        
        AppOptionsStateChanged?.Invoke();

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetFontFamily(string? fontFamily, bool updateStorage = true)
    {
        var inState = GetAppOptionsState();
    	
        _appOptionsState = inState with
        {
            Options = inState.Options with
            {
                FontFamily = fontFamily
            }
        };
        
        // I'm optimizing all the expression bound properties that construct
        // a string, and specifically the ones that are rendered in the UI many times.
        //
        // Can probably use 'fontFamily' variable here but
        // I don't want to touch that right now -- incase there are unexpected consequences.
        var usingFontFamily = GetAppOptionsState().Options.FontFamily;
        if (usingFontFamily is null)
        	Options_FontFamilyCssStyleString = null;
        else
        	Options_FontFamilyCssStyleString = $"font-family: {usingFontFamily};";

        AppOptionsStateChanged?.Invoke();

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetFontSize(int fontSizeInPixels, bool updateStorage = true)
    {
        var inState = GetAppOptionsState();
    	
        _appOptionsState = inState with
        {
            Options = inState.Options with
            {
                FontSizeInPixels = fontSizeInPixels
            }
        };
        
        // I'm optimizing all the expression bound properties that construct
        // a string, and specifically the ones that are rendered in the UI many times.
        //
        // Can probably use 'fontSizeInPixels' variable here but
        // I don't want to touch that right now -- incase there are unexpected consequences.
    	var usingFontSizeInPixels = GetAppOptionsState().Options.FontSizeInPixels;
        var usingFontSizeInPixelsCssValue = usingFontSizeInPixels.ToCssValue();
    	Options_FontSizeCssStyleString = $"font-size: {usingFontSizeInPixelsCssValue}px;";
        
        AppOptionsStateChanged?.Invoke();

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetResizeHandleWidth(int resizeHandleWidthInPixels, bool updateStorage = true)
    {
        var inState = GetAppOptionsState();
    	
        _appOptionsState = inState with
        {
            Options = inState.Options with
            {
                ResizeHandleWidthInPixels = resizeHandleWidthInPixels
            }
        };
        
        Options_ResizeHandleCssWidth = $"width: {GetAppOptionsState().Options.ResizeHandleWidthInPixels.ToCssValue()}px";
        
        AppOptionsStateChanged?.Invoke();

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetResizeHandleHeight(int resizeHandleHeightInPixels, bool updateStorage = true)
    {
        var inState = GetAppOptionsState();
    	
        _appOptionsState = inState with
        {
            Options = inState.Options with
            {
                ResizeHandleHeightInPixels = resizeHandleHeightInPixels
            }
        };
        
        Options_ResizeHandleCssHeight = $"height: {GetAppOptionsState().Options.ResizeHandleHeightInPixels.ToCssValue()}px";
        
        AppOptionsStateChanged?.Invoke();

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetIconSize(int iconSizeInPixels, bool updateStorage = true)
    {
        var inState = GetAppOptionsState();
    	
        _appOptionsState = inState with
        {
            Options = inState.Options with
            {
                IconSizeInPixels = iconSizeInPixels
            }
        };
        
        AppOptionsStateChanged?.Invoke();

        if (updateStorage)
            Options_WriteToStorage();
    }

    public async Task Options_SetFromLocalStorageAsync()
    {
        var optionsJsonString = await _storageService.GetValue(Options_StorageKey).ConfigureAwait(false) as string;

        if (string.IsNullOrWhiteSpace(optionsJsonString))
            return;

        var optionsJson = JsonSerializer.Deserialize<CommonOptionsJsonDto>(optionsJsonString);

        if (optionsJson is null)
            return;

        if (optionsJson.ThemeKey is not null)
        {
            var matchedTheme = GetThemeState().ThemeList.FirstOrDefault(
                x => x.Key == optionsJson.ThemeKey);

            Options_SetTheme(matchedTheme ?? ThemeFacts.VisualStudioDarkThemeClone, false);
        }

        if (optionsJson.FontFamily is not null)
            Options_SetFontFamily(optionsJson.FontFamily, false);

        if (optionsJson.FontSizeInPixels is not null)
            Options_SetFontSize(optionsJson.FontSizeInPixels.Value, false);
            
        if (optionsJson.ResizeHandleWidthInPixels is not null)
            Options_SetResizeHandleWidth(optionsJson.ResizeHandleWidthInPixels.Value, false);
            
        if (optionsJson.ResizeHandleHeightInPixels is not null)
            Options_SetResizeHandleHeight(optionsJson.ResizeHandleHeightInPixels.Value, false);

        if (optionsJson.IconSizeInPixels is not null)
            Options_SetIconSize(optionsJson.IconSizeInPixels.Value, false);
    }

    public void Options_WriteToStorage()
    {
        Options_CommonBackgroundTaskApi.Enqueue(new CommonWorkArgs
    	{
    		WorkKind = CommonWorkKind.WriteToLocalStorage,
    		WriteToLocalStorage_Key = Options_StorageKey,
    		WriteToLocalStorage_Value = new CommonOptionsJsonDto(GetAppOptionsState().Options)
    	});
    }
    
    private void HandleThemeChange()
    {
        var usingTheme = GetThemeState().ThemeList
        	.FirstOrDefault(x => x.Key == GetAppOptionsState().Options.ThemeKey)
        	?? ThemeFacts.VisualStudioDarkThemeClone;
        
        Options_ThemeCssClassString = usingTheme.CssClassString;
	    
	    var cssStyleStringBuilder = new StringBuilder("color-scheme: ");
	    if (usingTheme.ThemeColorKind == ThemeColorKind.Dark)
	    	cssStyleStringBuilder.Append("dark");
		else if (usingTheme.ThemeColorKind == ThemeColorKind.Light)
	    	cssStyleStringBuilder.Append("light");
		else
	    	cssStyleStringBuilder.Append("dark");
	    cssStyleStringBuilder.Append(';');
        Options_ColorSchemeCssStyleString = cssStyleStringBuilder.ToString();
    }
    /* End IAppOptionsService */
    
    /* Start IThemeService */
	private ThemeState _themeState = new();
	
	public event Action? ThemeStateChanged;
	
	public ThemeState GetThemeState() => _themeState;

    public void Theme_RegisterAction(ThemeRecord theme)
    {
        var inTheme = _themeState.ThemeList.FirstOrDefault(
            x => x.Key == theme.Key);

        if (inTheme is not null)
            return;

        var outThemeList = new List<ThemeRecord>(_themeState.ThemeList);
        outThemeList.Add(theme);

        _themeState = new ThemeState { ThemeList = outThemeList };
        ThemeStateChanged?.Invoke();
    }
    
    public void Theme_RegisterRangeAction(IReadOnlyList<ThemeRecord> themeList)
    {
        var outThemeList = new List<ThemeRecord>(_themeState.ThemeList);
        
        foreach (var theme in themeList)
        {
            var inTheme = _themeState.ThemeList.FirstOrDefault(
                x => x.Key == theme.Key);
    
            if (inTheme is not null)
                return;
    
            outThemeList.Add(theme);
    
            _themeState = new ThemeState { ThemeList = outThemeList };
            ThemeStateChanged?.Invoke();
        }
    }

    public void Theme_DisposeAction(Key<ThemeRecord> themeKey)
    {
        var inTheme = _themeState.ThemeList.FirstOrDefault(
            x => x.Key == themeKey);

        if (inTheme is null)
            return;

        var outThemeList = new List<ThemeRecord>(_themeState.ThemeList);
        outThemeList.Remove(inTheme);

        _themeState = new ThemeState { ThemeList = outThemeList };
        ThemeStateChanged?.Invoke();
    }
    /* End IThemeService */
}
