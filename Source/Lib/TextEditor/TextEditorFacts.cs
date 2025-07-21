using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models.Defaults;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

namespace Walk.TextEditor.RazorLib;

public static class TextEditorFacts
{
    /* Start TextEditorKeymapDefaultFacts */
	public static readonly KeymapLayer KeymapDefault_DefaultLayer = new KeymapLayer(
        new Key<KeymapLayer>(Guid.Parse("d0ac9354-6671-44fd-b281-e652a6aa1f56")),
        "Default Layer",
        "default-layer");

    public static readonly KeymapLayer KeymapDefault_HasSelectionLayer = new KeymapLayer(
        new Key<KeymapLayer>(Guid.Parse("3ac23ee9-ea25-4b8a-bed4-f10367ad095e")),
        "If Has Selection",
        "if-has-selection");
    /* End TextEditorKeymapDefaultFacts */
    
    /* Start TextEditorKeymapFacts */
    public static readonly ITextEditorKeymap Keymap_DefaultKeymap = new TextEditorKeymapDefault();

    public static List<ITextEditorKeymap> Keymap_AllKeymapsList { get; } =
        new()
        {
            Keymap_DefaultKeymap,
		};
    /* End TextEditorKeymapFacts */
    
    /* Start CompilerServiceDiagnosticPresentationFacts */
    public const string CompilerServiceDiagnosticPresentation_CssClassString = "di_te_compiler-service-diagnostic-presentation";

	public static readonly Key<TextEditorPresentationModel> CompilerServiceDiagnosticPresentation_PresentationKey = Key<TextEditorPresentationModel>.NewKey();

	public static readonly TextEditorPresentationModel CompilerServiceDiagnosticPresentation_EmptyPresentationModel = new(
		CompilerServiceDiagnosticPresentation_PresentationKey,
		0,
		CompilerServiceDiagnosticPresentation_CssClassString,
		new CompilerServiceDiagnosticDecorationMapper());
    /* End CompilerServiceDiagnosticPresentationFacts */
    
    /* Start FindOverlayPresentationFacts */
    public const string FindOverlayPresentation_CssClassString = "di_te_find-overlay-presentation";

    public static readonly Key<TextEditorPresentationModel> FindOverlayPresentation_PresentationKey = Key<TextEditorPresentationModel>.NewKey();

    public static readonly TextEditorPresentationModel FindOverlayPresentation_EmptyPresentationModel = new(
        FindOverlayPresentation_PresentationKey,
        0,
        FindOverlayPresentation_CssClassString,
        new FindOverlayDecorationMapper());
    /* End FindOverlayPresentationFacts */
    
    /* Start TextEditorDevToolsPresentationFacts */
    public const string DevToolsPresentation_CssClassString = "di_te_dev-tools-presentation";

    public static readonly Key<TextEditorPresentationModel> DevToolsPresentation_PresentationKey = Key<TextEditorPresentationModel>.NewKey();

    public static readonly TextEditorPresentationModel DevToolsPresentation_EmptyPresentationModel = new(
        DevToolsPresentation_PresentationKey,
        0,
        DevToolsPresentation_CssClassString,
        new TextEditorDevToolsDecorationMapper());
    /* End TextEditorDevToolsPresentationFacts */
    
    /* Start WalkTextEditorCustomThemeFacts */
    public static readonly ThemeRecord LightTheme = new ThemeRecord(
        new Key<ThemeRecord>(Guid.Parse("8165209b-0cea-45b4-b6dd-e5661b319c73")),
        "Walk IDE Light Theme",
        "di_light-theme",
        ThemeContrastKind.Default,
        ThemeColorKind.Light,
        IncludeScopeApp: false,
        IncludeScopeTextEditor: true);

    public static readonly ThemeRecord DarkTheme = new ThemeRecord(
        new Key<ThemeRecord>(Guid.Parse("56d64327-03c2-48a3-b086-11b101826efb")),
        "Walk IDE Dark Theme",
        "di_dark-theme",
        ThemeContrastKind.Default,
        ThemeColorKind.Dark,
        IncludeScopeApp: false,
        IncludeScopeTextEditor: true);

    public static readonly List<ThemeRecord> AllCustomThemesList = new()
    {
        LightTheme,
        DarkTheme
    };
    /* End WalkTextEditorCustomThemeFacts */
    
    /* Start Aaa */
    /* End Aaa */
    
    /* Start Aaa */
    /* End Aaa */
}
