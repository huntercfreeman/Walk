using System.Text.Json;
using System.Text;
using Walk.Common.RazorLib.Dimensions.Models;
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
    
    public string Options_ColorSchemeCssStyleString { get; set; }
    
    public int Options_LineHeight { get; set; } = 20;
    public string Options_LineHeight_CssStyle { get; set; } = "height: 20px;";
    
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
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.AppOptionsStateChanged);

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
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.AppOptionsStateChanged);

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetFontFamily(string? fontFamily, bool updateStorage = true)
    {
        if (string.IsNullOrWhiteSpace(fontFamily))
            fontFamily = null;
        else
            fontFamily = fontFamily.Trim();
            
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

        CommonUiStateChanged?.Invoke(CommonUiEventKind.LineHeightNeedsMeasured);

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetFontSize(int fontSizeInPixels, bool updateStorage = true)
    {
        if (fontSizeInPixels < AppOptionsState.MINIMUM_FONT_SIZE_IN_PIXELS)
            fontSizeInPixels = AppOptionsState.MINIMUM_FONT_SIZE_IN_PIXELS;
    
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
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.LineHeightNeedsMeasured);

        if (updateStorage)
            Options_WriteToStorage();
    }
    
    public void Options_SetLineHeight(int lineHeightInPixels)
    {
        Options_LineHeight = lineHeightInPixels;
        Options_LineHeight_CssStyle = $"height: {Options_LineHeight.ToString()}px;";
        CommonUiStateChanged?.Invoke(CommonUiEventKind.AppOptionsStateChanged);
    }

    public void Options_SetResizeHandleWidth(int resizeHandleWidthInPixels, bool updateStorage = true)
    {
        if (resizeHandleWidthInPixels < AppOptionsState.MINIMUM_RESIZE_HANDLE_WIDTH_IN_PIXELS)
            resizeHandleWidthInPixels = AppOptionsState.MINIMUM_RESIZE_HANDLE_WIDTH_IN_PIXELS;
    
        var inState = GetAppOptionsState();
        
        _appOptionsState = inState with
        {
            Options = inState.Options with
            {
                ResizeHandleWidthInPixels = resizeHandleWidthInPixels
            }
        };
        
        Options_ResizeHandleCssWidth = $"width: {GetAppOptionsState().Options.ResizeHandleWidthInPixels.ToCssValue()}px";
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.AppOptionsStateChanged);

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetResizeHandleHeight(int resizeHandleHeightInPixels, bool updateStorage = true)
    {
        if (resizeHandleHeightInPixels < AppOptionsState.MINIMUM_RESIZE_HANDLE_HEIGHT_IN_PIXELS)
            resizeHandleHeightInPixels = AppOptionsState.MINIMUM_RESIZE_HANDLE_HEIGHT_IN_PIXELS;
    
        var inState = GetAppOptionsState();
        
        _appOptionsState = inState with
        {
            Options = inState.Options with
            {
                ResizeHandleHeightInPixels = resizeHandleHeightInPixels
            }
        };
        
        Options_ResizeHandleCssHeight = $"height: {GetAppOptionsState().Options.ResizeHandleHeightInPixels.ToCssValue()}px";
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.AppOptionsStateChanged);

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetIconSize(int iconSizeInPixels, bool updateStorage = true)
    {
        if (iconSizeInPixels < AppOptionsState.MINIMUM_ICON_SIZE_IN_PIXELS)
            iconSizeInPixels = AppOptionsState.MINIMUM_ICON_SIZE_IN_PIXELS;
    
        var inState = GetAppOptionsState();
        
        _appOptionsState = inState with
        {
            Options = inState.Options with
            {
                IconSizeInPixels = iconSizeInPixels
            }
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.AppOptionsStateChanged);

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

        var matchedTheme = GetThemeState().ThemeList.FirstOrDefault(x => x.Key == optionsJson.ThemeKey);
        Options_SetTheme(matchedTheme == default ? CommonFacts.VisualStudioDarkThemeClone : matchedTheme, false);

        if (optionsJson.FontFamily is not null)
            Options_SetFontFamily(optionsJson.FontFamily, false);

        Options_SetFontSize(optionsJson.FontSizeInPixels, false);
        Options_SetResizeHandleWidth(optionsJson.ResizeHandleWidthInPixels, false);
        Options_SetResizeHandleHeight(optionsJson.ResizeHandleHeightInPixels, false);
        Options_SetIconSize(optionsJson.IconSizeInPixels, false);
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
        var usingTheme = GetThemeState().ThemeList.FirstOrDefault(x => x.Key == GetAppOptionsState().Options.ThemeKey);
        usingTheme = usingTheme == default ? CommonFacts.VisualStudioDarkThemeClone : usingTheme;
        
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
