using System.Text.Json;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.RenderStates.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.JavaScriptObjects.Models;

using Walk.Common.RazorLib.BackgroundTasks.Models;

namespace Walk.TextEditor.RazorLib.Options.Models;

public sealed class TextEditorOptionsApi
{
	public const int TAB_WIDTH_MIN = 2;
	public const int TAB_WIDTH_MAX = 4;

    private readonly TextEditorService _textEditorService;
    private readonly WalkTextEditorConfig _textEditorConfig;
    private readonly IContextService _contextService;
    private readonly ICommonUtilityService _commonUtilityService;

    public TextEditorOptionsApi(
        TextEditorService textEditorService,
        WalkTextEditorConfig textEditorConfig,
        ICommonUtilityService commonUtilityService,
        IContextService contextService)
    {
        _textEditorService = textEditorService;
        _textEditorConfig = textEditorConfig;
        _contextService = contextService;
        _commonUtilityService = commonUtilityService;
    }
    
    private TextEditorOptionsState _textEditorOptionsState = new();

    private IDialog? _findAllDialog;

    /// <summary>
    /// Step 1: Notifies the TextEditorViewModelDisplay to recalculate `_componentData.SetWrapperCssAndStyle();`
    ///         and invoke `StateHasChanged()`.
    /// </summary>
	public event Action? StaticStateChanged;
	/// <summary>
    /// Step 1: Notifies the WalkTextEditorInitializer to measure a tiny UI element that has the options applied to it.
    /// Step 2: WalkTextEditorInitializer then invokes `MeasuredStateChanged`.
    /// Step 3: TextEditorViewModelDisplay sees that second event fire, it enqueues a re-calculation of the virtualization result.
    /// Step 4: Eventually that virtualization result is finished and the editor re-renders.
    /// </summary>
	public event Action? NeedsMeasured;
	/// <summary>
	/// Step 1: Notifies TextEditorViewModelDisplay to enqueue a re-calculation of the virtualization result.
	/// Step 2: Eventually that virtualization result is finished and the editor re-renders.
	/// </summary>
    public event Action? MeasuredStateChanged;
    /// <summary>
    /// This event communicates from the text editor UI to the header and footer.
    /// </summary>
    public event Action? TextEditorWrapperCssStateChanged;

	public TextEditorOptionsState GetTextEditorOptionsState() => _textEditorOptionsState;

    public TextEditorOptions GetOptions()
    {
        return _textEditorService.OptionsApi.GetTextEditorOptionsState().Options;
    }
    
    public void InvokeTextEditorWrapperCssStateChanged()
    {
        TextEditorWrapperCssStateChanged?.Invoke();
    }

    public void ShowSettingsDialog(bool? isResizableOverride = null, string? cssClassString = null)
    {
        // TODO: determine the actively focused element at time of invocation,
        //       then restore focus to that element when this dialog is closed.
        var settingsDialog = new DialogViewModel(
            Key<IDynamicViewModel>.NewKey(),
            "Text Editor Settings",
            _textEditorConfig.SettingsDialogConfig.ComponentRendererType,
            null,
            cssClassString,
            isResizableOverride ?? _textEditorConfig.SettingsDialogConfig.ComponentIsResizable,
            null);

        _commonUtilityService.Dialog_ReduceRegisterAction(settingsDialog);
    }

    public void ShowFindAllDialog(bool? isResizableOverride = null, string? cssClassString = null)
    {
        // TODO: determine the actively focused element at time of invocation,
        //       then restore focus to that element when this dialog is closed.
        _findAllDialog ??= new DialogViewModel(
            Key<IDynamicViewModel>.NewKey(),
            "Find All",
            _textEditorConfig.FindAllDialogConfig.ComponentRendererType,
            null,
            cssClassString,
            isResizableOverride ?? _textEditorConfig.FindAllDialogConfig.ComponentIsResizable,
            null);

        _commonUtilityService.Dialog_ReduceRegisterAction(_findAllDialog);
    }

    public void SetTheme(ThemeRecord theme, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                CommonOptions = inState.Options.CommonOptions with
                {
                    ThemeKey = theme.Key
                },
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        
        // I'm optimizing all the expression bound properties that construct
		// a string, and specifically the ones that are rendered in the UI many times.
		//
		// Can probably use 'theme' variable here but
		// I don't want to touch that right now -- incase there are unexpected consequences.
        var usingThemeCssClassString = _commonUtilityService.GetThemeState().ThemeList
        	.FirstOrDefault(x => x.Key == GetTextEditorOptionsState().Options.CommonOptions.ThemeKey)
        	?.CssClassString
            ?? ThemeFacts.VisualStudioDarkThemeClone.CssClassString;
        _textEditorService.ThemeCssClassString = usingThemeCssClassString;
        
        StaticStateChanged?.Invoke();

        if (updateStorage)
            WriteToStorage();
    }

    public void SetShowWhitespace(bool showWhitespace, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                ShowWhitespace = showWhitespace,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        // ShowWhitespace needs virtualization result to be re-calculated.
        MeasuredStateChanged?.Invoke();

        if (updateStorage)
            WriteToStorage();
    }

    public void SetUseMonospaceOptimizations(bool useMonospaceOptimizations, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
		_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                UseMonospaceOptimizations = useMonospaceOptimizations,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        StaticStateChanged?.Invoke();
        
        if (updateStorage)
            WriteToStorage();
    }

    public void SetShowNewlines(bool showNewlines, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
		_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                ShowNewlines = showNewlines,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        StaticStateChanged?.Invoke();
        
        if (updateStorage)
            WriteToStorage();
    }
    
    public void SetTabKeyBehavior(bool tabKeyBehavior, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
		_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                TabKeyBehavior = tabKeyBehavior,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        StaticStateChanged?.Invoke();
        
        if (updateStorage)
            WriteToStorage();
    }
    
    public void SetTabWidth(int tabWidth, bool updateStorage = true)
    {
    	if (tabWidth < TAB_WIDTH_MIN || tabWidth > TAB_WIDTH_MAX)
    		return;
    
    	var inState = GetTextEditorOptionsState();
    
		_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                TabWidth = tabWidth,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        MeasuredStateChanged?.Invoke();
        
        if (updateStorage)
            WriteToStorage();
    }

    public void SetKeymap(ITextEditorKeymap keymap, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                Keymap = keymap,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        StaticStateChanged?.Invoke();

        /*var activeKeymap = _textEditorService.OptionsApi.GetTextEditorOptionsState().Options.Keymap;

        if (activeKeymap is not null)
        {
            _contextService.SetContextKeymap(
                ContextFacts.TextEditorContext.ContextKey,
                activeKeymap);
        }

        if (updateStorage)
            WriteToStorage();*/
    }

    public void SetHeight(int? heightInPixels, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                TextEditorHeightInPixels = heightInPixels,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        StaticStateChanged?.Invoke();

        if (updateStorage)
            WriteToStorage();
    }

    public void SetFontSize(int fontSizeInPixels, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                CommonOptions = inState.Options.CommonOptions with
                {
                    FontSizeInPixels = fontSizeInPixels
                },
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        NeedsMeasured?.Invoke();

        if (updateStorage)
            WriteToStorage();
    }

    public void SetFontFamily(string? fontFamily, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                CommonOptions = inState.Options.CommonOptions with
                {
                    FontFamily = fontFamily
                },
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        NeedsMeasured?.Invoke();

        if (updateStorage)
            WriteToStorage();
    }

    public void SetCursorWidth(double cursorWidthInPixels, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();

        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                CursorWidthInPixels = cursorWidthInPixels,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        StaticStateChanged?.Invoke();

        if (updateStorage)
            WriteToStorage();
    }

    public void SetRenderStateKey(Key<RenderState> renderStateKey)
    {
    	var inState = GetTextEditorOptionsState();
    
        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                RenderStateKey = renderStateKey
            },
        };
        StaticStateChanged?.Invoke();
    }
    
    public void SetCharAndLineMeasurements(TextEditorEditContext editContext, CharAndLineMeasurements charAndLineMeasurements)
    {
    	var inState = GetTextEditorOptionsState();

        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                CharAndLineMeasurements = charAndLineMeasurements,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        
    	MeasuredStateChanged?.Invoke();
    }

    public void WriteToStorage()
    {
        _commonUtilityService.Enqueue(new CommonWorkArgs
        {
    		WorkKind = CommonWorkKind.WriteToLocalStorage,
        	WriteToLocalStorage_Key = _textEditorService.StorageKey,
            WriteToLocalStorage_Value = new TextEditorOptionsJsonDto(_textEditorService.OptionsApi.GetTextEditorOptionsState().Options),
        });
    }

    public async Task SetFromLocalStorageAsync()
    {
        var optionsJsonString = await _commonUtilityService.Storage_GetValue(_textEditorService.StorageKey).ConfigureAwait(false) as string;

        if (string.IsNullOrWhiteSpace(optionsJsonString))
            return;

        var optionsJson = JsonSerializer.Deserialize<TextEditorOptionsJsonDto>(optionsJsonString);

        if (optionsJson is null)
            return;

        if (optionsJson.CommonOptionsJsonDto?.ThemeKey is not null)
        {
            var matchedTheme = _textEditorService.CommonUtilityService.GetThemeState().ThemeList.FirstOrDefault(
                x => x.Key == optionsJson.CommonOptionsJsonDto.ThemeKey);

            SetTheme(matchedTheme ?? ThemeFacts.VisualStudioDarkThemeClone, false);
        }

        /*if (optionsJson.Keymap is not null)
        {
            var matchedKeymap = TextEditorKeymapFacts.AllKeymapsList.FirstOrDefault(
                x => x.Key == optionsJson.Keymap.Key);

            SetKeymap(matchedKeymap ?? TextEditorKeymapFacts.DefaultKeymap, false);

            var activeKeymap = _textEditorService.OptionsApi.GetTextEditorOptionsState().Options.Keymap;

            if (activeKeymap is not null)
            {
                _contextService.SetContextKeymap(
                    ContextFacts.TextEditorContext.ContextKey,
                    activeKeymap);
            }
        }*/

        if (optionsJson.CommonOptionsJsonDto?.FontSizeInPixels is not null)
            SetFontSize(optionsJson.CommonOptionsJsonDto.FontSizeInPixels.Value, false);

        if (optionsJson.CursorWidthInPixels is not null)
            SetCursorWidth(optionsJson.CursorWidthInPixels.Value, false);

        if (optionsJson.TextEditorHeightInPixels is not null)
            SetHeight(optionsJson.TextEditorHeightInPixels.Value, false);

        if (optionsJson.ShowNewlines is not null)
        	SetShowNewlines(optionsJson.ShowNewlines.Value, false);
        
        if (optionsJson.TabKeyBehavior is not null)
            SetTabKeyBehavior(optionsJson.TabKeyBehavior.Value, false);
        
        if (optionsJson.TabWidth is not null)
            SetTabWidth(optionsJson.TabWidth.Value, false);

        // TODO: OptionsSetUseMonospaceOptimizations will always get set to false (default for bool)
        // for a first time user. This leads to a bad user experience since the proportional
        // font logic is still being optimized. Therefore don't read in UseMonospaceOptimizations
        // from local storage.
        //
        // OptionsSetUseMonospaceOptimizations(options.UseMonospaceOptimizations);

        if (optionsJson.ShowWhitespace is not null)
            SetShowWhitespace(optionsJson.ShowWhitespace.Value, false);
    }
}
