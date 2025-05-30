using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Common.RazorLib.Htmls.Models;

/// <summary>
/// TODO: SphagettiCode - I don't like this file (2023-09-19)
/// Why does SizeFacts.cs, HtmlFacts.Button.cs, and HtmlFacts.Main.cs exist? Merge all three of these?
/// </summary>
public static partial class HtmlFacts
{
    public static class Button
    {
        public const int PADDING_IN_PIXELS = 6;

        public static string ButtonPaddingHorizontalTotalInPixelsCssValue =>
            $"2 * {PADDING_IN_PIXELS.ToCssValue()}px";
    }
}