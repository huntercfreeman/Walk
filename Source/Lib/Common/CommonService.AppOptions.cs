using System.Text.Json;
using System.Text;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    private AppOptionsState _appOptionsState = new();

#if DEBUG
    public string Options_StorageKey => "walk-common_theme-storage-key-debug"; 
#else
    public string Options_StorageKey => "walk-common_theme-storage-key";
#endif

    public string Options_ThemeCssClassString { get; set; } = CommonFacts.VisualStudioDarkThemeClone.CssClassString;

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

    public void Options_SetActiveThemeRecordKey(int themeKey, bool updateStorage = true)
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
        var optionsJsonString = await Storage_GetValue(Options_StorageKey).ConfigureAwait(false) as string;

        if (string.IsNullOrWhiteSpace(optionsJsonString))
            return;

        CommonOptionsJsonDto? optionsJson = null;
        
        try
        {
            optionsJson = JsonSerializer.Deserialize<CommonOptionsJsonDto>(optionsJsonString);
        }
        catch (System.Text.Json.JsonException)
        {
            // TODO: Preserve the values that do parse.
            await Storage_SetValue(Options_StorageKey, null).ConfigureAwait(false);
        }

        if (optionsJson is null)
            return;

        if (optionsJson.ThemeKey is not null)
        {
            var matchedTheme = GetThemeState().ThemeList.FirstOrDefault(
                x => x.Key == optionsJson.ThemeKey);

            Options_SetTheme(matchedTheme ?? CommonFacts.VisualStudioDarkThemeClone, false);
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
        Enqueue(new CommonWorkArgs
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
            ?? CommonFacts.VisualStudioDarkThemeClone;
        
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
}
