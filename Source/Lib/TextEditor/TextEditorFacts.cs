using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models.Defaults;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Diffs.Models;

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
    
    /* Start DiffPresentationFacts */
    public const string Diff_CssClassString = "di_te_diff-presentation";

    public static readonly Key<TextEditorPresentationModel> Diff_InPresentationKey = Key<TextEditorPresentationModel>.NewKey();
    public static readonly Key<TextEditorPresentationModel> Diff_OutPresentationKey = Key<TextEditorPresentationModel>.NewKey();

    /// <summary>
    /// TODO: Change the name of this from 'EmptyInPresentationModel' because its confusingly named.
    /// </summary>
    public static readonly TextEditorPresentationModel Diff_EmptyInPresentationModel = new(
        Diff_InPresentationKey,
        0,
        Diff_CssClassString,
        new TextEditorDiffDecorationMapper());

    /// <summary>
    /// TODO: Change the name of this from 'EmptyOutPresentationModel' because its confusingly named.
    /// </summary>
    public static readonly TextEditorPresentationModel Diff_EmptyOutPresentationModel = new(
        Diff_OutPresentationKey,
        0,
        Diff_CssClassString,
        new TextEditorDiffDecorationMapper());
    /* End DiffPresentationFacts */
    
    /* Start Aaa */
    /* End Aaa */
    
    /* Start Aaa */
    /* End Aaa */
    
    /* Start Aaa */
    /* End Aaa */
}
