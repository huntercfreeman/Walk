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
    /* Start TextEditorKeymapFacts */
    public static readonly ITextEditorKeymap Keymap_DefaultKeymap = new TextEditorKeymapDefault();
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
    
    /* Start ScrollbarFacts */
    public const int SCROLLBAR_SIZE_IN_PIXELS = 10;
    /* End ScrollbarFacts */
    
    /* Start Aaa */
    /* End Aaa */
}
