using Walk.Common.RazorLib.Keys.Models;

namespace Walk.TextEditor.RazorLib.Decorations.Models;

/// <summary>
/// IMPORTANT: For `TextEditorPresentationModel` the `DecorationMapper` maps to css style, NOT to a class.
/// This comes "out of nowhere" but it is massively impactful for optimization since then you only are writing out a single css attribute.
/// Because no matter what you'll need the style attribute so you can position the presentation. So you might as well put everything in the style attribute.
/// Themes aren't an issue, because the style will just reference the CSS variable.
/// </summary>
///
/// <param name="TextEditorPresentationKey">Unique Identifier. Perhaps one wants to repaint the Diff presentation only. They use this to identify the Diff presentation</param>
/// <param name="Rank">The order the presentations get rendered. A given a ranks of [ 7, -3, 1, 2 ] the rendering order is ascending therefore it will be reordered as [ -3, 1, 2, 7 ] when iterating over the containing presentation layer</param>
/// <param name="CssClassString">A div is rendered with this css class. Inside this div are the divs gotten from the text spans. One likely only will use this css class as an identifier while viewing the browser's debugging tools.</param>
/// <param name="DecorationMapper">
/// The decoration mapper that is responsible for converting a decoration byte to css style.
///
/// IMPORTANT: For `TextEditorPresentationModel` the `DecorationMapper` maps to css style, NOT to a class.
/// This comes "out of nowhere" but it is massively impactful for optimization since then you only are writing out a single css attribute.
/// Because no matter what you'll need the style attribute so you can position the presentation. So you might as well put everything in the style attribute.
/// Themes aren't an issue, because the style will just reference the CSS variable.
/// </param>
/// <param name="TextEditorTextSpans">The list of position indices that are to painted with certain decoration bytes.</param>
public record TextEditorPresentationModel(
    Key<TextEditorPresentationModel> TextEditorPresentationKey,
    int Rank,
    string CssClassString,
    IDecorationMapper DecorationMapper)
{
    public TextEditorPresentationModelCalculation? CompletedCalculation { get; init; }
    public TextEditorPresentationModelCalculation? PendingCalculation { get; init; }
}
