using System.Text.Json;
using System.Text;
using System.Collections.Concurrent;
using System.Text;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Common.RazorLib.JsRuntimes.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Tooltips.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.JsRuntimes.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Installations.Displays;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Tabs.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Drags.Models;

namespace Walk.Common.RazorLib.Options.Models;

public class CommonUtilityService : ICommonUtilityService
{
    private readonly object _stateModificationLock = new();
	
	public WalkCommonJavaScriptInteropApi JsRuntimeCommonApi { get; }
	public ICommonComponentRenderers CommonComponentRenderers { get; }
	
	public WalkHostingInformation WalkHostingInformation { get; }

    public CommonUtilityService(
        WalkHostingInformation hostingInformation,
        ICommonComponentRenderers commonComponentRenderers,
        BackgroundTaskService backgroundTaskService,
        ITreeViewService treeViewService,
        WalkCommonConfig commonConfig,
        IJSRuntime jsRuntime)
    {
        WalkHostingInformation = hostingInformation;
    
        CommonComponentRenderers = commonComponentRenderers;
    
        _backgroundTaskService = backgroundTaskService;
        
        _treeViewService = treeViewService;
        CommonConfig = commonConfig;
            
        _treeViewService.CommonUtilityService = this;
    
        switch (hostingInformation.WalkHostingKind)
        {
            case WalkHostingKind.Photino:
                EnvironmentProvider = new LocalEnvironmentProvider();
                FileSystemProvider = new LocalFileSystemProvider(this);
                break;
            default:
                EnvironmentProvider = new InMemoryEnvironmentProvider();
                FileSystemProvider = new InMemoryFileSystemProvider(this);
                break;
        }
        
        JsRuntimeCommonApi = jsRuntime.GetWalkCommonApi();
    
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
    private readonly BackgroundTaskService _backgroundTaskService;
    
    private AppOptionsState _appOptionsState = new();

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
        var optionsJsonString = await Storage_GetValue(Options_StorageKey).ConfigureAwait(false) as string;

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
    
    /* Start IClipboardService, JavaScriptInteropClipboardService */
    public async Task<string> ReadClipboard()
    {
        try
        {
            return await JsRuntimeCommonApi
                .ReadClipboard()
                .ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            return string.Empty;
        }
    }

    public async Task SetClipboard(string value)
    {
        try
        {
            await JsRuntimeCommonApi
                .SetClipboard(value)
                .ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
        }
    }
    /* End IClipboardService, JavaScriptInteropClipboardService */
    
    /* Start IStorageService, LocalStorageService */
    public async ValueTask Storage_SetValue(string key, object? value)
    {
        await JsRuntimeCommonApi.LocalStorageSetItem(
                key,
                value)
            .ConfigureAwait(false);
    }

    public async ValueTask<object?> Storage_GetValue(string key)
    {
        return await JsRuntimeCommonApi.LocalStorageGetItem(
                key)
            .ConfigureAwait(false);
    }
    /* End IStorageService, LocalStorageService */
    
    public IEnvironmentProvider EnvironmentProvider { get; }
    public IFileSystemProvider FileSystemProvider { get; }
    
	public event Action<CommonUiEventKind>? CommonUiStateChanged;
	
    /* Start IOutlineService */
	private OutlineState _outlineState = new();
	
	public OutlineState GetOutlineState() => _outlineState;

	public void SetOutline(
		string? elementId,
		MeasuredHtmlElementDimensions? measuredHtmlElementDimensions,
		bool needsMeasured)
	{
		lock (_stateModificationLock)
		{
			_outlineState = _outlineState with
			{
				ElementId = elementId,
				MeasuredHtmlElementDimensions = measuredHtmlElementDimensions,
				NeedsMeasured = needsMeasured,
			};
		}

        if (needsMeasured && elementId is not null)
        {
            _ = Task.Run(async () =>
            {
                var elementDimensions = await JsRuntimeCommonApi
                    .MeasureElementById(elementId)
                    .ConfigureAwait(false);

                Outline_SetMeasurements(
                    elementId,
                    elementDimensions);
            });

            return; // The state has changed will occur in 'ReduceSetMeasurementsAction'
        }
        else
        {
            goto finalize;
        }

        finalize:
        CommonUiStateChanged?.Invoke(CommonUiEventKind.OutlineStateChanged);
    }
	
	public void Outline_SetMeasurements(
		string? elementId,
		MeasuredHtmlElementDimensions? measuredHtmlElementDimensions)
	{
		lock (_stateModificationLock)
		{
			if (_outlineState.ElementId == elementId)
			{
    			_outlineState = _outlineState with
    			{
    				MeasuredHtmlElementDimensions = measuredHtmlElementDimensions,
    				NeedsMeasured = false,
    			};
    		}
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.OutlineStateChanged);
    }
    /* End IOutlineService */
    
    /* Start IPanelService */
	private PanelState _panelState = new();
	
	public PanelState GetPanelState() => _panelState;

    public void RegisterPanel(Panel panel)
    {
        lock (_stateModificationLock)
        {
    	    var inState = GetPanelState();
    
            if (!inState.PanelList.Any(x => x.Key == panel.Key))
            {
                var outPanelList = new List<Panel>(inState.PanelList);
                outPanelList.Add(panel);
    
                _panelState = inState with { PanelList = outPanelList };
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
    }

    public void DisposePanel(Key<Panel> panelKey)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();

            var indexPanel = inState.PanelList.FindIndex(x => x.Key == panelKey);
            if (indexPanel != -1)
            {
                var outPanelList = new List<Panel>(inState.PanelList);
                outPanelList.RemoveAt(indexPanel);
    
                _panelState = inState with { PanelList = outPanelList };
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
    }
	
    public void RegisterPanelGroup(PanelGroup panelGroup)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();

            if (!inState.PanelGroupList.Any(x => x.Key == panelGroup.Key))
            {
                var outPanelGroupList = new List<PanelGroup>(inState.PanelGroupList);
                outPanelGroupList.Add(panelGroup);
    
                _panelState = inState with { PanelGroupList = outPanelGroupList };
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
    }

    public void DisposePanelGroup(Key<PanelGroup> panelGroupKey)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();

            var indexPanelGroup = inState.PanelGroupList.FindIndex(x => x.Key == panelGroupKey);
            if (indexPanelGroup != -1)
            {
                var outPanelGroupList = new List<PanelGroup>(inState.PanelGroupList);
                outPanelGroupList.RemoveAt(indexPanelGroup);
    
                _panelState = inState with { PanelGroupList = outPanelGroupList };
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
    }

    public void RegisterPanelTab(
    	Key<PanelGroup> panelGroupKey,
    	IPanelTab panelTab,
    	bool insertAtIndexZero)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();

            var indexPanelGroup = inState.PanelGroupList.FindIndex(x => x.Key == panelGroupKey);
            if (indexPanelGroup != -1)
            {
                var inPanelGroup = inState.PanelGroupList[indexPanelGroup];
                if (!inPanelGroup.TabList.Any(x => x.Key == panelTab.Key))
                {
                    var outTabList = new List<IPanelTab>(inPanelGroup.TabList);
        
                    var insertionPoint = insertAtIndexZero
                        ? 0
                        : outTabList.Count;
        
                    outTabList.Insert(insertionPoint, panelTab);
        
                    var outPanelGroupList = new List<PanelGroup>(inState.PanelGroupList);
        
                    outPanelGroupList[indexPanelGroup] = inPanelGroup with
                    {
                        TabList = outTabList
                    };
        
                    _panelState = inState with
                    {
                        PanelGroupList = outPanelGroupList
                    };
                }
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
    }

    public void DisposePanelTab(Key<PanelGroup> panelGroupKey, Key<Panel> panelTabKey)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();

            var indexPanelGroup = inState.PanelGroupList.FindIndex(x => x.Key == panelGroupKey);
            if (indexPanelGroup != -1)
            {
                var inPanelGroup = inState.PanelGroupList[indexPanelGroup];
                var indexPanelTab = inPanelGroup.TabList.FindIndex(x => x.Key == panelTabKey);
                if (indexPanelTab != -1)
                {
                    var outTabList = new List<IPanelTab>(inPanelGroup.TabList);
                    outTabList.RemoveAt(indexPanelTab);
        
                    var outPanelGroupList = new List<PanelGroup>(inState.PanelGroupList);
                    outPanelGroupList[indexPanelGroup] = inPanelGroup with
                    {
                        TabList = outTabList
                    };
        
                    _panelState = inState with { PanelGroupList = outPanelGroupList };
                }
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
    }

    public void SetActivePanelTab(Key<PanelGroup> panelGroupKey, Key<Panel> panelTabKey)
    {
        var sideEffect = false;

        lock (_stateModificationLock)
        {
            var inState = GetPanelState();

            var indexPanelGroup = inState.PanelGroupList.FindIndex(x => x.Key == panelGroupKey);
            if (indexPanelGroup != -1)
            {
                var inPanelGroup = inState.PanelGroupList[indexPanelGroup];
                var outPanelGroupList = new List<PanelGroup>(inState.PanelGroupList);
    
                outPanelGroupList[indexPanelGroup] = inPanelGroup with
                {
                    ActiveTabKey = panelTabKey
                };
    
                _panelState = inState with { PanelGroupList = outPanelGroupList };
                sideEffect = true;
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);

        if (sideEffect)
            AppDimension_NotifyIntraAppResize();
    }

    public void SetPanelTabAsActiveByContextRecordKey(Key<ContextRecord> contextRecordKey)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();

            var inPanelGroup = inState.PanelGroupList.FirstOrDefault(x => x.TabList
                .Any(y => y.ContextRecordKey == contextRecordKey));
                
            if (inPanelGroup is not null)
            {
                var inPanelTab = inPanelGroup.TabList.FirstOrDefault(
                    x => x.ContextRecordKey == contextRecordKey);
                    
                if (inPanelTab is not null)
                {
                    // TODO: This should be thread safe yes?...
                    // ...Only ever would the same thread access the inner lock from invoking this which is the current lock so no deadlock?
                    SetActivePanelTab(inPanelGroup.Key, inPanelTab.Key);
                    return; // Inner reduce will trigger finalize.
                }
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
    }

    public void Panel_SetDragEventArgs((IPanelTab PanelTab, PanelGroup PanelGroup)? dragEventArgs)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();
    
            _panelState = inState with
            {
                DragEventArgs = dragEventArgs
            };
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
    }
    
    public void Panel_InitializeResizeHandleDimensionUnit(Key<PanelGroup> panelGroupKey, DimensionUnit dimensionUnit)
    {
        lock (_stateModificationLock)
        {
            var inState = GetPanelState();

            var inPanelGroup = inState.PanelGroupList.FirstOrDefault(
                x => x.Key == panelGroupKey);

            if (inPanelGroup is not null)
            {
                if (dimensionUnit.Purpose == DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_ROW ||
                    dimensionUnit.Purpose == DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_COLUMN)
                {
                    if (dimensionUnit.Purpose == DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_ROW)
                    {
                        if (inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList is not null)
                        {
                            var existingDimensionUnit = inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList
                                .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
            
                            if (existingDimensionUnit.Purpose is null)
                                inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                        }
                    }
                    else if (dimensionUnit.Purpose != DimensionUnitFacts.Purposes.RESIZABLE_HANDLE_COLUMN)
                    {
                        if (inPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList is not null)
                        {
                            var existingDimensionUnit = inPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList
                                .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
            
                            if (existingDimensionUnit.Purpose is null)
                                inPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                        }
                    }
                }
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.PanelStateChanged);
    }
    /* End IPanelService */
    
    /* Start IWidgetService */
	private WidgetState _widgetState = new();
	
	public WidgetState GetWidgetState() => _widgetState;

	/// <summary>
	/// When this action causes the transition from a widget being rendered,
	/// to NO widget being rendered.
	///
	/// Then the user's focus will go "somewhere" so we want
	/// redirect it to the main layout at least so they can use IDE keybinds
	///
	/// As if the "somewhere" their focus moves to is outside the blazor app components
	/// they IDE keybinds won't fire.
	///
	/// TODO: Where does focus go when you delete the element which the user is focused on.
	///
	/// TODO: Prior to focusing the widget (i.e.: NO widget transitions to a widget being rendered)
	///           we should track where the user's focus is, then restore that focus once the
	///           widget is closed.
	/// </summary>
    public void SetWidget(WidgetModel? widget)
    {
		var sideEffect = false;

		lock (_stateModificationLock)
		{
			var inState = GetWidgetState();

			if (widget != inState.Widget && (widget is null))
				sideEffect = true;

			_widgetState = inState with
			{
				Widget = widget,
			};
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.WidgetStateChanged);

		if (sideEffect)
		{
            _ = Task.Run(async () =>
            {
                await JsRuntimeCommonApi
                    .FocusHtmlElementById(IDynamicViewModel.DefaultSetFocusOnCloseElementId)
                    .ConfigureAwait(false);
            });
        }
    }
    /* End IWidgetService */
    
    /* Start IDialogService */
    /// <summary>
    /// TODO: Some methods just invoke a single method, so remove the redundant middle man.
    /// TODO: Thread safety.
    /// </summary>
    private DialogState _dialogState = new();
	
	public DialogState GetDialogState() => _dialogState;
    
    public void Dialog_ReduceRegisterAction(IDialog dialog)
    {
    	var inState = GetDialogState();
    	
        if (inState.DialogList.Any(x => x.DynamicViewModelKey == dialog.DynamicViewModelKey))
        {
        	_ = Task.Run(async () =>
        		await JsRuntimeCommonApi
	                .FocusHtmlElementById(dialog.DialogFocusPointHtmlElementId)
	                .ConfigureAwait(false));
        	
        	CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
        	return;
        }

		var outDialogList = new List<IDialog>(inState.DialogList);
        outDialogList.Add(dialog);

        _dialogState = inState with 
        {
            DialogList = outDialogList,
            ActiveDialogKey = dialog.DynamicViewModelKey,
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
        return;
    }

    public void Dialog_ReduceSetIsMaximizedAction(
        Key<IDynamicViewModel> dynamicViewModelKey,
        bool isMaximized)
    {
    	var inState = GetDialogState();
    	
        var indexDialog = inState.DialogList.FindIndex(
            x => x.DynamicViewModelKey == dynamicViewModelKey);

        if (indexDialog == -1)
        {
            CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
        	return;
        }
            
        var inDialog = inState.DialogList[indexDialog];

        var outDialogList = new List<IDialog>(inState.DialogList);
        
        outDialogList[indexDialog] = inDialog.SetDialogIsMaximized(isMaximized);

        _dialogState = inState with { DialogList = outDialogList };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
        return;
    }
    
    public void Dialog_ReduceSetActiveDialogKeyAction(Key<IDynamicViewModel> dynamicViewModelKey)
    {
    	var inState = GetDialogState();
    	
        _dialogState = inState with { ActiveDialogKey = dynamicViewModelKey };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.ActiveDialogKeyChanged);
        return;
    }

    public void Dialog_ReduceDisposeAction(Key<IDynamicViewModel> dynamicViewModelKey)
    {
    	var inState = GetDialogState();
    
        var indexDialog = inState.DialogList.FindIndex(
            x => x.DynamicViewModelKey == dynamicViewModelKey);

        if (indexDialog == -1)
        {
        	CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
        	return;
        }

		var inDialog = inState.DialogList[indexDialog];

        var outDialogList = new List<IDialog>(inState.DialogList);
        outDialogList.RemoveAt(indexDialog);

        _dialogState = inState with { DialogList = outDialogList };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DialogStateChanged);
        return;
    }
    /* End IDialogService */
    
    /* Start INotificationService */
	private NotificationState _notificationState = new();
	
	public NotificationState GetNotificationState() => _notificationState;

    public void Notification_ReduceRegisterAction(INotification notification)
    {
    	lock (_stateModificationLock)
    	{
	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
	        outDefaultList.Add(notification);
	        _notificationState = _notificationState with { DefaultList = outDefaultList };
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceDisposeAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var indexNotification = _notificationState.DefaultList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (indexNotification != -1)
	        {
    	        var inNotification = _notificationState.DefaultList[indexNotification];
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.RemoveAt(indexNotification);
    	        _notificationState = _notificationState with { DefaultList = outDefaultList };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceMakeReadAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.DefaultList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.DefaultList[inNotificationIndex];
    	
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.RemoveAt(inNotificationIndex);
    	        
    	        var outReadList = new List<INotification>(_notificationState.ReadList);
    	        outReadList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            ReadList = outReadList
    	        };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }
    
    public void Notification_ReduceUndoMakeReadAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.ReadList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.ReadList[inNotificationIndex];
    	
    	        var outReadList = new List<INotification>(_notificationState.ReadList);
    	        outReadList.RemoveAt(inNotificationIndex);
    	        
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            ReadList = outReadList
    	        };
    	    }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceMakeDeletedAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.DefaultList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.DefaultList[inNotificationIndex];
    	
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.RemoveAt(inNotificationIndex);
    	        
    	        var outDeletedList = new List<INotification>(_notificationState.DeletedList);
    	        outDeletedList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            DeletedList = outDeletedList
    	        };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceUndoMakeDeletedAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.DeletedList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.DeletedList[inNotificationIndex];
    	
    	        var outDeletedList = new List<INotification>(_notificationState.DeletedList);
    	        outDeletedList.RemoveAt(inNotificationIndex);
    	
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            DeletedList = outDeletedList
    	        };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceMakeArchivedAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.DefaultList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.DefaultList[inNotificationIndex];
    	
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.RemoveAt(inNotificationIndex);
    	
    	        var outArchivedList = new List<INotification>(_notificationState.ArchivedList);
    	        outArchivedList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            ArchivedList = outArchivedList
    	        };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }
    
    public void Notification_ReduceUndoMakeArchivedAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.ArchivedList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.ArchivedList[inNotificationIndex];
    	
    	        var outArchivedList = new List<INotification>(_notificationState.ArchivedList);
    	        outArchivedList.RemoveAt(inNotificationIndex);
    	        
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            ArchivedList = outArchivedList
    	        };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceClearDefaultAction()
    {
    	lock (_stateModificationLock)
    	{
	        _notificationState = _notificationState with
	        {
	            DefaultList = new List<INotification>()
	        };
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }
    
    public void Notification_ReduceClearReadAction()
    {
    	lock (_stateModificationLock)
    	{
	        _notificationState = _notificationState with
	        {
	            ReadList = new List<INotification>()
	        };
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }
    
    public void Notification_ReduceClearDeletedAction()
    {
    	lock (_stateModificationLock)
    	{
	        _notificationState = _notificationState with
	        {
	            DeletedList = new List<INotification>()
	        };
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceClearArchivedAction()
    {
    	lock (_stateModificationLock)
    	{
	        _notificationState = _notificationState with
	        {
	            ArchivedList = new List<INotification>()
	        };
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }
    /* End INotificationService */
    
    /* Start IDropdownService */
	private DropdownState _dropdownState = new();
	
	public DropdownState GetDropdownState() => _dropdownState;
	
    public void Dropdown_ReduceRegisterAction(DropdownRecord dropdown)
    {
    	var inState = GetDropdownState();
    
		var indexExistingDropdown = inState.DropdownList.FindIndex(
			x => x.Key == dropdown.Key);

		if (indexExistingDropdown != -1)
		{
			CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
			return;
		}

		var outDropdownList = new List<DropdownRecord>(inState.DropdownList);
		outDropdownList.Add(dropdown);

        _dropdownState = inState with
        {
            DropdownList = outDropdownList
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
		return;
    }

    public void Dropdown_ReduceDisposeAction(Key<DropdownRecord> key)
    {
    	var inState = GetDropdownState();
    
		var indexExistingDropdown = inState.DropdownList.FindIndex(
			x => x.Key == key);

		if (indexExistingDropdown == -1)
		{
			CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
			return;
		}
			
		var outDropdownList = new List<DropdownRecord>(inState.DropdownList);
		outDropdownList.RemoveAt(indexExistingDropdown);

        _dropdownState = inState with
        {
            DropdownList = outDropdownList
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
		return;
    }

    public void Dropdown_ReduceClearAction()
    {
    	var inState = GetDropdownState();
    
    	var outDropdownList = new List<DropdownRecord>();
    
        _dropdownState = inState with
        {
            DropdownList = outDropdownList
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
		return;
    }

    public void Dropdown_ReduceFitOnScreenAction(DropdownRecord dropdown)
    {
    	var inState = GetDropdownState();
    
		var indexExistingDropdown = inState.DropdownList.FindIndex(
			x => x.Key == dropdown.Key);

		if (indexExistingDropdown == -1)
		{
			CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
			return;
		}
		
		var inDropdown = inState.DropdownList[indexExistingDropdown];

		var outDropdown = inDropdown with
		{
			Width = dropdown.Width,
			Height = dropdown.Height,
			Left = dropdown.Left,
			Top = dropdown.Top
		};
		
		var outDropdownList = new List<DropdownRecord>(inState.DropdownList);
		outDropdownList[indexExistingDropdown] = outDropdown;

        _dropdownState = inState with
        {
            DropdownList = outDropdownList
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
		return;
    }
    /* End IDropdownService */
    
    /* Start ITooltipService */
    public TooltipState _tooltipState = new(tooltipModel: null);

	public static readonly Guid Tooltip_htmlElementIdSalt = Guid.NewGuid();
    
	public string Tooltip_HtmlElementId { get; } = $"di_dropdown_{Tooltip_htmlElementIdSalt}";
	public MeasuredHtmlElementDimensions Tooltip_HtmlElementDimensions { get; set; }
	public MeasuredHtmlElementDimensions Tooltip_GlobalHtmlElementDimensions { get; set; }
	public bool Tooltip_IsOffScreenHorizontally { get; }
	public bool Tooltip_IsOffScreenVertically { get; }
	public int Tooltip_RenderCount { get; } = 1;
	
	public StringBuilder Tooltip_StyleBuilder { get; } = new();
	
	public TooltipState GetTooltipState() => _tooltipState;
	
	public void SetTooltipModel(ITooltipModel tooltipModel)
	{
	    _tooltipState = new TooltipState(tooltipModel);
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.TooltipStateChanged);
	}
    /* End ITooltipService */

    /* Start CommonBackgroundTaskApi */
    private readonly ITreeViewService _treeViewService;
    
    private readonly ConcurrentQueue<CommonWorkArgs> _workQueue = new();
    
    public Key<IBackgroundTaskGroup> BackgroundTaskKey { get; } = Key<IBackgroundTaskGroup>.NewKey();
    
    public bool __TaskCompletionSourceWasCreated { get; set; }
    
    public StringBuilder UiStringBuilder { get; } = new();
    
    public WalkCommonConfig CommonConfig { get; }

    public void Enqueue(CommonWorkArgs commonWorkArgs)
    {
		_workQueue.Enqueue(commonWorkArgs);
        _backgroundTaskService.Continuous_EnqueueGroup(this);
    }

    private async ValueTask Do_WalkCommonInitializer(Key<ContextSwitchGroup> contextSwitchGroupKey)
    {
        Options_SetActiveThemeRecordKey(CommonConfig.InitialThemeKey, false);

        await Options_SetFromLocalStorageAsync()
            .ConfigureAwait(false);

        GetContextSwitchState().FocusInitiallyContextSwitchGroupKey = contextSwitchGroupKey;
        RegisterContextSwitchGroup(
            new ContextSwitchGroup(
                contextSwitchGroupKey,
                "Contexts",
                () =>
                {
                    var contextState = GetContextState();
                    var panelState = GetPanelState();
                    var dialogState = GetDialogState();
                    var menuOptionList = new List<MenuOptionRecord>();

                    foreach (var panel in panelState.PanelList)
                    {
                        var menuOptionPanel = new MenuOptionRecord(
                            panel.Title,
                            MenuOptionKind.Delete,
                            async () =>
                            {
                                var panelGroup = panel.TabGroup as PanelGroup;

                                if (panelGroup is not null)
                                {
                                    SetActivePanelTab(panelGroup.Key, panel.Key);

                                    var contextRecord = ContextFacts.AllContextsList.FirstOrDefault(x => x.ContextKey == panel.ContextRecordKey);

                                    if (contextRecord != default)
                                    {
                                        var command = ContextHelper.ConstructFocusContextElementCommand(
                                            contextRecord,
                                            nameof(ContextHelper.ConstructFocusContextElementCommand),
                                            nameof(ContextHelper.ConstructFocusContextElementCommand),
                                            JsRuntimeCommonApi,
                                            this);

                                        await command.CommandFunc.Invoke(null).ConfigureAwait(false);
                                    }
                                }
                                else
                                {
                                    var existingDialog = dialogState.DialogList.FirstOrDefault(
                                        x => x.DynamicViewModelKey == panel.DynamicViewModelKey);

                                    if (existingDialog is not null)
                                    {
                                        Dialog_ReduceSetActiveDialogKeyAction(existingDialog.DynamicViewModelKey);

                                        await JsRuntimeCommonApi
                                            .FocusHtmlElementById(existingDialog.DialogFocusPointHtmlElementId)
                                            .ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        RegisterPanelTab(PanelFacts.LeftPanelGroupKey, panel, true);
                                        SetActivePanelTab(PanelFacts.LeftPanelGroupKey, panel.Key);

                                        var contextRecord = ContextFacts.AllContextsList.FirstOrDefault(x => x.ContextKey == panel.ContextRecordKey);

                                        if (contextRecord != default)
                                        {
                                            var command = ContextHelper.ConstructFocusContextElementCommand(
                                                contextRecord,
                                                nameof(ContextHelper.ConstructFocusContextElementCommand),
                                                nameof(ContextHelper.ConstructFocusContextElementCommand),
                                                JsRuntimeCommonApi,
                                                this);

                                            await command.CommandFunc.Invoke(null).ConfigureAwait(false);
                                        }
                                    }
                                }
                            });

                        menuOptionList.Add(menuOptionPanel);
                    }

                    var menu = menuOptionList.Count == 0
                        ? new MenuRecord(MenuRecord.NoMenuOptionsExistList)
                        : new MenuRecord(menuOptionList);

                    return Task.FromResult(menu);
                }));
    }

    public async ValueTask Do_WriteToLocalStorage(string key, object value)
    {
        var valueJson = System.Text.Json.JsonSerializer.Serialize(value);
        await Storage_SetValue(key, valueJson).ConfigureAwait(false);
    }

    public async ValueTask Do_Tab_ManuallyPropagateOnContextMenu(
        Func<TabContextMenuEventArgs, Task> localHandleTabButtonOnContextMenu, TabContextMenuEventArgs tabContextMenuEventArgs)
    {
        await localHandleTabButtonOnContextMenu.Invoke(tabContextMenuEventArgs).ConfigureAwait(false);
    }

    public async ValueTask Do_TreeView_HandleTreeViewOnContextMenu(
        Func<TreeViewCommandArgs, Task>? onContextMenuFunc, TreeViewCommandArgs treeViewContextMenuCommandArgs)
    {
        if (onContextMenuFunc is not null)
        {
            await onContextMenuFunc
                .Invoke(treeViewContextMenuCommandArgs)
                .ConfigureAwait(false);
        }
    }

    public async ValueTask Do_TreeView_HandleExpansionChevronOnMouseDown(TreeViewNoType localTreeViewNoType, TreeViewContainer treeViewContainer)
    {
        await localTreeViewNoType.LoadChildListAsync().ConfigureAwait(false);
        _treeViewService.ReduceReRenderNodeAction(treeViewContainer.Key, localTreeViewNoType);
    }

    public async ValueTask Do_TreeView_ManuallyPropagateOnContextMenu(Func<MouseEventArgs?, Key<TreeViewContainer>, TreeViewNoType?, Task> handleTreeViewOnContextMenu, MouseEventArgs mouseEventArgs, Key<TreeViewContainer> key, TreeViewNoType treeViewNoType)
    {
        await handleTreeViewOnContextMenu.Invoke(
                mouseEventArgs,
                key,
                treeViewNoType)
            .ConfigureAwait(false);
    }

    public async ValueTask Do_TreeViewService_LoadChildList(Key<TreeViewContainer> containerKey, TreeViewNoType treeViewNoType)
    {
        try
        {
            await treeViewNoType.LoadChildListAsync().ConfigureAwait(false);

            _treeViewService.ReduceReRenderNodeAction(
                containerKey,
                treeViewNoType);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public ValueTask HandleEvent()
	{
		if (!_workQueue.TryDequeue(out CommonWorkArgs workArgs))
			return ValueTask.CompletedTask;
			
		switch (workArgs.WorkKind)
		{
			case CommonWorkKind.WalkCommonInitializerWork:
				return Do_WalkCommonInitializer(WalkCommonInitializer.ContextSwitchGroupKey);
			case CommonWorkKind.WriteToLocalStorage:
				return Do_WriteToLocalStorage(workArgs.WriteToLocalStorage_Key, workArgs.WriteToLocalStorage_Value);
			case CommonWorkKind.Tab_ManuallyPropagateOnContextMenu:
				return Do_Tab_ManuallyPropagateOnContextMenu(workArgs.HandleTabButtonOnContextMenu, workArgs.TabContextMenuEventArgs);
			case CommonWorkKind.TreeView_HandleTreeViewOnContextMenu:
				return Do_TreeView_HandleTreeViewOnContextMenu(workArgs.OnContextMenuFunc, workArgs.TreeViewContextMenuCommandArgs);
            case CommonWorkKind.TreeView_HandleExpansionChevronOnMouseDown:
				return Do_TreeView_HandleExpansionChevronOnMouseDown(workArgs.TreeViewNoType, workArgs.TreeViewContainer);
            case CommonWorkKind.TreeView_ManuallyPropagateOnContextMenu:
				return Do_TreeView_ManuallyPropagateOnContextMenu(workArgs.HandleTreeViewOnContextMenu, workArgs.MouseEventArgs, workArgs.ContainerKey, workArgs.TreeViewNoType);
            case CommonWorkKind.TreeViewService_LoadChildList:
				return Do_TreeViewService_LoadChildList(workArgs.ContainerKey, workArgs.TreeViewNoType);
			default:
				Console.WriteLine($"{nameof(CommonUtilityService)} {nameof(HandleEvent)} default case");
				return ValueTask.CompletedTask;
		}
	}
	/* End CommonBackgroundTaskApi */

    /* Start IContextService */
	private ContextState _contextState = new();
	private ContextSwitchState _contextSwitchState = new();
    
    public event Action? ContextStateChanged;
    public event Action? ContextSwitchStateChanged;
    
    public ContextState GetContextState() => _contextState;
    
    public ContextRecord GetContextRecord(Key<ContextRecord> contextKey) =>
    	_contextState.AllContextsList.FirstOrDefault(x => x.ContextKey == contextKey);
    
    public ContextSwitchState GetContextSwitchState() => _contextSwitchState;
    
    public void SetFocusedContextKey(Key<ContextRecord> contextKey)
    {
    	lock (_stateModificationLock)
    	{
	        _contextState = _contextState with
	        {
	            FocusedContextKey = contextKey
	        };
    	}

        ContextStateChanged?.Invoke();
    }
    
    public void SetContextKeymap(Key<ContextRecord> contextKey, IKeymap keymap)
    {
    	lock (_stateModificationLock)
    	{
	    	var inState = GetContextState();
	    	
	        var inContextRecord = inState.AllContextsList.FirstOrDefault(
	            x => x.ContextKey == contextKey);
	
	        if (inContextRecord != default)
	        {
	            _contextState = inState;
				goto finalize;
            }
	            
	        var index = inState.AllContextsList.FindIndex(x => x.ContextKey == inContextRecord.ContextKey);
	        if (index == -1)
	        {
	        	_contextState = inState;
                goto finalize;
            }
            
            var outAllContextsList = new List<ContextRecord>(inState.AllContextsList);
	
	        outAllContextsList[index] = inContextRecord with
	        {
	            Keymap = keymap
	        };
	
	        _contextState = inState with { AllContextsList = outAllContextsList };
            goto finalize;
        }

        finalize:
        ContextStateChanged?.Invoke();
    }
	
    public void RegisterContextSwitchGroup(ContextSwitchGroup contextSwitchGroup)
    {
    	lock (_stateModificationLock)
    	{
	    	var inState = GetContextSwitchState();
	    
	    	if (inState.ContextSwitchGroupList.Any(x =>
	    			x.Key == contextSwitchGroup.Key))
	    	{
                goto finalize;
            }
	    
	    	var outContextSwitchGroupList = new List<ContextSwitchGroup>(inState.ContextSwitchGroupList);
	    	outContextSwitchGroupList.Add(contextSwitchGroup);
	    
	        _contextSwitchState = inState with
	        {
	            ContextSwitchGroupList = outContextSwitchGroupList
	        };
	        
	        goto finalize;
	    }

        finalize:
        ContextStateChanged?.Invoke();
    }
    /* End IContextService */
    
    /* Start IDragService */
    private DragState _dragState = new();
    
    public event Action? DragStateChanged;
    
    public DragState GetDragState() => _dragState;
    
    public void Drag_ShouldDisplayAndMouseEventArgsSetAction(
        bool shouldDisplay,
        MouseEventArgs? mouseEventArgs)
    {
    	var inState = GetDragState();
    
        _dragState = inState with
        {
        	ShouldDisplay = shouldDisplay,
            MouseEventArgs = mouseEventArgs,
        };
        
        DragStateChanged?.Invoke();
        return;
    }
    
    public void Drag_ShouldDisplayAndMouseEventArgsAndDragSetAction(
        bool shouldDisplay,
		MouseEventArgs? mouseEventArgs,
		IDrag? drag)
    {
    	var inState = GetDragState();
    	
        _dragState = inState with
        {
        	ShouldDisplay = shouldDisplay,
            MouseEventArgs = mouseEventArgs,
            Drag = drag,
        };
        
        DragStateChanged?.Invoke();
        return;
    }
    /* End IDragService */
}
