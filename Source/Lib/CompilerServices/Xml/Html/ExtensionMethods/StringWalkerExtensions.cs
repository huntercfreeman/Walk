using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.Xml.Html.InjectedLanguage;

namespace Walk.CompilerServices.Xml.Html.ExtensionMethods;

public static class StringWalkerExtensions
{
    public static bool AtInjectedLanguageCodeBlockTag(
        this StringWalker stringWalker,
        InjectedLanguageDefinition injectedLanguageDefinition)
    {
        var isMatch = stringWalker.PeekForSubstring(injectedLanguageDefinition.TransitionSubstring);
        var isEscaped = stringWalker.PeekForSubstring(injectedLanguageDefinition.TransitionSubstringEscaped);

        return isMatch && !isEscaped;
    }
}