using System.Text.Json;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.RenderStates.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.TextEditor.RazorLib.JavaScriptObjects.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.Options.Models;

namespace Walk.TextEditor.RazorLib;

public partial class TextEditorService
{
    public const int Options_TAB_WIDTH_MIN = 2;
    public const int Options_TAB_WIDTH_MAX = 4;

    private readonly TextEditorService Options_textEditorService;
    private readonly CommonService Options_commonUtilityService;

    private TextEditorOptionsState Options_textEditorOptionsState = new();

    private IDialog? Options_findAllDialog;

    public TextEditorOptionsState Options_GetTextEditorOptionsState() => Options_textEditorOptionsState;

    public TextEditorOptions Options_GetOptions()
    {
        return Options_GetTextEditorOptionsState().Options;
    }

    public void Options_InvokeTextEditorWrapperCssStateChanged()
    {
        OptionsChanged?.Invoke(OptionsChangedKind.TextEditorWrapperCssStateChanged);
    }

    public void Options_ShowSettingsDialog(bool? isResizableOverride = null, string? cssClassString = null)
    {
        // TODO: determine the actively focused element at time of invocation,
        //       then restore focus to that element when this dialog is closed.
        var settingsDialog = new DialogViewModel(
            Key<IDynamicViewModel>.NewKey(),
            "Text Editor Settings",
            TextEditorConfig.SettingsDialogConfig.ComponentRendererType,
            null,
            cssClassString,
            isResizableOverride ?? TextEditorConfig.SettingsDialogConfig.ComponentIsResizable,
            null);

        CommonService.Dialog_ReduceRegisterAction(settingsDialog);
    }

    public void Options_ShowFindAllDialog(bool? isResizableOverride = null, string? cssClassString = null)
    {
        // TODO: determine the actively focused element at time of invocation,
        //       then restore focus to that element when this dialog is closed.
        Options_findAllDialog ??= new DialogViewModel(
            Key<IDynamicViewModel>.NewKey(),
            "Find All",
            TextEditorConfig.FindAllDialogConfig.ComponentRendererType,
            null,
            cssClassString,
            isResizableOverride ?? TextEditorConfig.FindAllDialogConfig.ComponentIsResizable,
            null);

        CommonService.Dialog_ReduceRegisterAction(Options_findAllDialog);
    }

    public void Options_SetTheme(ThemeRecord theme, bool updateStorage = true)
    {
        var inState = Options_GetTextEditorOptionsState();

        Options_textEditorOptionsState = new TextEditorOptionsState
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
        var usingThemeCssClassString = CommonService.GetThemeState().ThemeList
            .FirstOrDefault(x => x.Key == Options_GetTextEditorOptionsState().Options.CommonOptions.ThemeKey)
            ?.CssClassString
            ?? CommonFacts.VisualStudioDarkThemeClone.CssClassString;
        ThemeCssClassString = usingThemeCssClassString;

        OptionsChanged?.Invoke(OptionsChangedKind.StaticStateChanged);

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetShowWhitespace(bool showWhitespace, bool updateStorage = true)
    {
        var inState = Options_GetTextEditorOptionsState();

        Options_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                ShowWhitespace = showWhitespace,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        // ShowWhitespace needs virtualization result to be re-calculated.
        OptionsChanged?.Invoke(OptionsChangedKind.MeasuredStateChanged);

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetUseMonospaceOptimizations(bool useMonospaceOptimizations, bool updateStorage = true)
    {
        var inState = Options_GetTextEditorOptionsState();

        Options_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                UseMonospaceOptimizations = useMonospaceOptimizations,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        OptionsChanged?.Invoke(OptionsChangedKind.StaticStateChanged);

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetShowNewlines(bool showNewlines, bool updateStorage = true)
    {
        var inState = Options_GetTextEditorOptionsState();

        Options_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                ShowNewlines = showNewlines,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        OptionsChanged?.Invoke(OptionsChangedKind.StaticStateChanged);

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetTabKeyBehavior(bool tabKeyBehavior, bool updateStorage = true)
    {
        var inState = Options_GetTextEditorOptionsState();

        Options_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                TabKeyBehavior = tabKeyBehavior,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        OptionsChanged?.Invoke(OptionsChangedKind.StaticStateChanged);

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetTabWidth(int tabWidth, bool updateStorage = true)
    {
        if (tabWidth < Options_TAB_WIDTH_MIN || tabWidth > Options_TAB_WIDTH_MAX)
            return;

        var inState = Options_GetTextEditorOptionsState();

        Options_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                TabWidth = tabWidth,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        OptionsChanged?.Invoke(OptionsChangedKind.MeasuredStateChanged);

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetKeymap(ITextEditorKeymap keymap, bool updateStorage = true)
    {
        var inState = Options_GetTextEditorOptionsState();

        Options_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                Keymap = keymap,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        OptionsChanged?.Invoke(OptionsChangedKind.StaticStateChanged);

        /*var activeKeymap = _textEditorService.Options_GetTextEditorOptionsState().Options.Keymap;

        if (activeKeymap is not null)
        {
            _contextService.SetContextKeymap(
                ContextFacts.TextEditorContext.ContextKey,
                activeKeymap);
        }

        if (updateStorage)
            WriteToStorage();*/
    }

    public void Options_SetHeight(int? heightInPixels, bool updateStorage = true)
    {
        var inState = Options_GetTextEditorOptionsState();

        Options_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                TextEditorHeightInPixels = heightInPixels,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        OptionsChanged?.Invoke(OptionsChangedKind.StaticStateChanged);

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetFontSize(int fontSizeInPixels, bool updateStorage = true)
    {
        var inState = Options_GetTextEditorOptionsState();

        Options_textEditorOptionsState = new TextEditorOptionsState
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
        OptionsChanged?.Invoke(OptionsChangedKind.NeedsMeasured);

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetFontFamily(string? fontFamily, bool updateStorage = true)
    {
        var inState = Options_GetTextEditorOptionsState();

        Options_textEditorOptionsState = new TextEditorOptionsState
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
        OptionsChanged?.Invoke(OptionsChangedKind.NeedsMeasured);

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetCursorWidth(double cursorWidthInPixels, bool updateStorage = true)
    {
        var inState = Options_GetTextEditorOptionsState();

        Options_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                CursorWidthInPixels = cursorWidthInPixels,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        OptionsChanged?.Invoke(OptionsChangedKind.StaticStateChanged);

        if (updateStorage)
            Options_WriteToStorage();
    }

    public void Options_SetRenderStateKey(Key<RenderState> renderStateKey)
    {
        var inState = Options_GetTextEditorOptionsState();

        Options_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                RenderStateKey = renderStateKey
            },
        };
        OptionsChanged?.Invoke(OptionsChangedKind.StaticStateChanged);
    }

    public void Options_SetCharAndLineMeasurements(TextEditorEditContext editContext, CharAndLineMeasurements charAndLineMeasurements)
    {
        var inState = Options_GetTextEditorOptionsState();

        Options_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                CharAndLineMeasurements = charAndLineMeasurements,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };

        OptionsChanged?.Invoke(OptionsChangedKind.MeasuredStateChanged);
    }

    public void Options_WriteToStorage()
    {
        CommonService.Enqueue(new CommonWorkArgs
        {
            WorkKind = CommonWorkKind.WriteToLocalStorage,
            WriteToLocalStorage_Key = StorageKey,
            WriteToLocalStorage_Value = new TextEditorOptionsJsonDto(Options_GetTextEditorOptionsState().Options),
        });
    }

    public async Task Options_SetFromLocalStorageAsync()
    {
        var optionsJsonString = await CommonService.Storage_GetValue(StorageKey).ConfigureAwait(false) as string;

        if (string.IsNullOrWhiteSpace(optionsJsonString))
            return;

        TextEditorOptionsJsonDto? optionsJson = null;
        
        try
        {
            optionsJson = JsonSerializer.Deserialize<TextEditorOptionsJsonDto>(optionsJsonString);
        }
        catch (System.Text.Json.JsonException)
        {
            // TODO: Preserve the values that do parse.
            await CommonService.Storage_SetValue(StorageKey, null).ConfigureAwait(false);
        }
        
        if (optionsJson is null)
            return;
        
        if (optionsJson.CommonOptionsJsonDto?.ThemeKey is not null)
        {
            var matchedTheme = CommonService.GetThemeState().ThemeList.FirstOrDefault(
                x => x.Key == optionsJson.CommonOptionsJsonDto.ThemeKey);

            Options_SetTheme(matchedTheme ?? CommonFacts.VisualStudioDarkThemeClone, false);
        }

        /*if (optionsJson.Keymap is not null)
        {
            var matchedKeymap = TextEditorKeymapFacts.AllKeymapsList.FirstOrDefault(
                x => x.Key == optionsJson.Keymap.Key);

            SetKeymap(matchedKeymap ?? TextEditorKeymapFacts.DefaultKeymap, false);

            var activeKeymap = _textEditorService.Options_GetTextEditorOptionsState().Options.Keymap;

            if (activeKeymap is not null)
            {
                _contextService.SetContextKeymap(
                    ContextFacts.TextEditorContext.ContextKey,
                    activeKeymap);
            }
        }*/

        if (optionsJson.CommonOptionsJsonDto?.FontSizeInPixels is not null)
            Options_SetFontSize(optionsJson.CommonOptionsJsonDto.FontSizeInPixels.Value, false);

        if (optionsJson.CursorWidthInPixels is not null)
            Options_SetCursorWidth(optionsJson.CursorWidthInPixels.Value, false);

        if (optionsJson.TextEditorHeightInPixels is not null)
            Options_SetHeight(optionsJson.TextEditorHeightInPixels.Value, false);

        if (optionsJson.ShowNewlines is not null)
            Options_SetShowNewlines(optionsJson.ShowNewlines.Value, false);

        if (optionsJson.TabKeyBehavior is not null)
            Options_SetTabKeyBehavior(optionsJson.TabKeyBehavior.Value, false);

        if (optionsJson.TabWidth is not null)
            Options_SetTabWidth(optionsJson.TabWidth.Value, false);

        // TODO: OptionsSetUseMonospaceOptimizations will always get set to false (default for bool)
        // for a first time user. This leads to a bad user experience since the proportional
        // font logic is still being optimized. Therefore don't read in UseMonospaceOptimizations
        // from local storage.
        //
        // OptionsSetUseMonospaceOptimizations(options.UseMonospaceOptimizations);

        if (optionsJson.ShowWhitespace is not null)
            Options_SetShowWhitespace(optionsJson.ShowWhitespace.Value, false);
    }
}
