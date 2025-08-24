using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Exceptions;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Menus.Displays;
using Walk.Common.RazorLib.JsRuntimes.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib;

public static class CommonFacts
{
    /* Start ThemeFacts */
    public static readonly ThemeRecord VisualStudioLightThemeClone = new ThemeRecord(
        Key: 0,
        "Visual Studio Light Clone",
        "di_visual-studio-light-theme-clone",
        ThemeContrastKind.Default,
        ThemeColorKind.Light,
        IncludeScopeApp: true,
        IncludeScopeTextEditor: true);

    public static readonly ThemeRecord VisualStudioDarkThemeClone = new ThemeRecord(
        Key: 1,
        "Visual Studio Dark Clone",
        "di_visual-studio-dark-theme-clone",
        ThemeContrastKind.Default,
        ThemeColorKind.Dark,
        IncludeScopeApp: true,
        IncludeScopeTextEditor: true);
    /* End ThemeFacts */
    
    /* Start WalkTextEditorCustomThemeFacts */
    public static readonly ThemeRecord LightTheme = new ThemeRecord(
        Key: 2,
        "Walk IDE Light Theme",
        "di_light-theme",
        ThemeContrastKind.Default,
        ThemeColorKind.Light,
        IncludeScopeApp: false,
        IncludeScopeTextEditor: true);

    public static readonly ThemeRecord DarkTheme = new ThemeRecord(
        Key: 3,
        "Walk IDE Dark Theme",
        "di_dark-theme",
        ThemeContrastKind.Default,
        ThemeColorKind.Dark,
        IncludeScopeApp: false,
        IncludeScopeTextEditor: true);
    /* End WalkTextEditorCustomThemeFacts */
    
    /* Start ContextFacts */
    /// <summary>
    /// Used when repositioning a dropdown so that it appears on screen.
    /// </summary>
    public static string RootHtmlElementId { get; set; } = "di_global";
    /* End ContextFacts */
    
    /* Start PanelFacts */
    public static readonly Key<PanelGroup> LeftPanelGroupKey = Key<PanelGroup>.NewKey();
    public static readonly Key<PanelGroup> RightPanelGroupKey = Key<PanelGroup>.NewKey();
    public static readonly Key<PanelGroup> BottomPanelGroupKey = Key<PanelGroup>.NewKey();
    /* End PanelFacts */
    
    /* Start BackgroundTaskFacts */
    public static int ContinuousQueueKey { get; } = 0;
    public static int IndefiniteQueueKey { get; } = 1;
    /* End BackgroundTaskFacts */
    
    /* Start ExtensionNoPeriodFacts */
    public const string DOT_NET_SOLUTION = "sln";
    public const string DOT_NET_SOLUTION_X = "slnx";
    public const string C_SHARP_PROJECT = "csproj";
    public const string C_SHARP_CLASS = "cs";
    public const string CSHTML_CLASS = "cshtml";
    public const string RAZOR_MARKUP = "razor";
    public const string RAZOR_CODEBEHIND = "razor.cs";
    public const string JSON = "json";
    public const string HTML = "html";
    public const string XML = "xml";
    public const string CSS = "css";
    public const string JAVA_SCRIPT = "js";
    public const string TYPE_SCRIPT = "ts";
    public const string TERMINAL = "terminal";
    public const string TXT = "txt";
    /* End ExtensionNoPeriodFacts */
    
    /* Start KeyboardKeyFacts */
    public static bool IsMetaKey(KeyboardEventArgs keyboardEventArgs)
    {
        return IsMetaKey(keyboardEventArgs.Key, keyboardEventArgs.Code);
    }
    
    public static bool IsMetaKey(string key, string code)
    {
        if (key.Length > 1 && !IsWhitespaceCode(code))
            return true;

        return false;
    }

    public static bool IsWhitespaceCharacter(char character)
    {
        switch (character)
        {
            case TAB:
            case CARRIAGE_RETURN:
            case NEW_LINE:
            case SPACE:
                return true;
            default:
                return false;
        }
    }

    public static bool IsPunctuationCharacter(char character)
    {
        switch (character)
        {
            case OPEN_CURLY_BRACE:
            case CLOSE_CURLY_BRACE:
            case OPEN_PARENTHESIS:
            case CLOSE_PARENTHESIS:
            case OPEN_SQUARE_BRACKET:
            case CLOSE_SQUARE_BRACKET:
            case BANG:
            case QUESTION_MARK:
            case PERIOD:
            case COMMA:
            case HASHTAG:
            case DOLLARS:
            case PERCENT:
            case AMPERSAND:
            case CARET:
            case STAR:
            case PLUS:
            case SEMICOLON:
            case EQUAL:
            case AT:
            case DASH:
            // TODO: Should 'PunctuationCharacters.UNDER_SCORE' count as punctuation? It makes expand selection (double mouse click) on a private field with the leading '_' convention annoying to select the whole word.
            //case PunctuationCharacters.UNDER_SCORE:
            case ACCENT:
            case TILDE:
            case PIPE:
            case COLON:
            case DOUBLE_QUOTE:
            case SINGLE_QUOTE:
            case OPEN_ARROW_BRACKET:
            case CLOSE_ARROW_BRACKET:
            case FORWARD_SLASH:
            case BACK_SLASH:
                return true;
            default:
                return false;
        }
    }

    public static char? MatchPunctuationCharacter(char character)
    {
        switch (character)
        {
            case OPEN_CURLY_BRACE:
                return CLOSE_CURLY_BRACE;
            case CLOSE_CURLY_BRACE:
                return OPEN_CURLY_BRACE;
            case OPEN_PARENTHESIS:
                return CLOSE_PARENTHESIS;
            case CLOSE_PARENTHESIS:
                return OPEN_PARENTHESIS;
            case OPEN_SQUARE_BRACKET:
                return CLOSE_SQUARE_BRACKET;
            case CLOSE_SQUARE_BRACKET:
                return OPEN_SQUARE_BRACKET;
            case OPEN_ARROW_BRACKET:
                return CLOSE_ARROW_BRACKET;
            case CLOSE_ARROW_BRACKET:
                return OPEN_ARROW_BRACKET;
            default:
                return null;
        }
    }

    public static int? DirectionToFindMatchingPunctuationCharacter(char character)
    {
        switch (character)
        {
            case OPEN_CURLY_BRACE:
                return 1;
            case CLOSE_CURLY_BRACE:
                return -1;
            case OPEN_PARENTHESIS:
                return 1;
            case CLOSE_PARENTHESIS:
                return -1;
            case OPEN_SQUARE_BRACKET:
                return 1;
            case CLOSE_SQUARE_BRACKET:
                return -1;
            case OPEN_ARROW_BRACKET:
                return 1;
            case CLOSE_ARROW_BRACKET:
                return -1;
            default:
                return null;
        }
    }

    public static bool IsWhitespaceCode(string code)
    {
        switch (code)
        {
            case TAB_CODE:
            case ENTER_CODE:
            case SPACE_CODE:
                return true;
            default:
                return false;
        }
    }

    public static bool CheckIsAlternateContextMenuEvent(
        string key, string code, bool shiftWasPressed, bool altWasPressed)
    {
        var wasShiftF10 = (key == "F10" || key == "f10") &&
                          shiftWasPressed;

        return wasShiftF10;
    }

    public static bool CheckIsContextMenuEvent(
        string key, string code, bool shiftWasPressed, bool altWasPressed)
    {
        return key == "ContextMenu" ||
               CheckIsAlternateContextMenuEvent(key, code, shiftWasPressed, altWasPressed);
    }

    public static bool CheckIsContextMenuEvent(KeyboardEventArgs keyboardEventArgs)
    {
        return CheckIsContextMenuEvent(
            keyboardEventArgs.Key,
            keyboardEventArgs.Code,
            keyboardEventArgs.ShiftKey,
            keyboardEventArgs.AltKey);
    }
    
    public static bool IsMetaKey(KeymapArgs keymapArgs)
    {
        return IsMetaKey(keymapArgs.Key, keymapArgs.Code);
    }

    public static bool IsMovementKey(string key)
    {
        switch (key)
        {
            case ARROW_LEFT_KEY:
            case ARROW_DOWN_KEY:
            case ARROW_UP_KEY:
            case ARROW_RIGHT_KEY:
            case HOME_KEY:
            case END_KEY:
                return true;
            default:
                return false;
        }
    }

    public static bool IsLineEndingCharacter(char character)
    {
        return character switch
        {
            NEW_LINE => true,
            CARRIAGE_RETURN => true,
            _ => false,
        };
    }

    // Metakeys
    public const string BACKSPACE = "Backspace";
    public const string ESCAPE = "Escape";
    public const string DELETE = "Delete";
    public const string F10 = "F10";
    public const string PAGE_UP = "PageUp";
    public const string PAGE_DOWN = "PageDown";

    // WhitespaceCharacters
    public const char TAB = '\t';
    public const char CARRIAGE_RETURN = '\r';
    public const char NEW_LINE = '\n';
    public const char SPACE = ' ';

    // WhitespaceCodes
    public const string TAB_CODE = "Tab";
    // TODO: Get CARRIAGE_RETURN_CODE code
    // public const string CARRIAGE_RETURN_CODE = "";
    public const string ENTER_CODE = "Enter";
    public const string SPACE_CODE = "Space";

    /// <summary>
    /// Added characters that were found in
    /// https://www.scintilla.org/ScintillaDoc.html
    /// source code, CharacterType.h:79
    /// </summary>
    // PunctuationCharacters
    public const char OPEN_CURLY_BRACE = '{';
    public const char CLOSE_CURLY_BRACE = '}';
    public const char OPEN_PARENTHESIS = '(';
    public const char CLOSE_PARENTHESIS = ')';
    public const char OPEN_SQUARE_BRACKET = '[';
    public const char CLOSE_SQUARE_BRACKET = ']';
    public const char BANG = '!';
    public const char QUESTION_MARK = '?';
    public const char PERIOD = '.';
    public const char COMMA = ',';
    public const char HASHTAG = '#';
    public const char DOLLARS = '$';
    public const char PERCENT = '%';
    public const char AMPERSAND = '&';
    public const char CARET = '^';
    public const char STAR = '*';
    public const char PLUS = '+';
    public const char SEMICOLON = ';';
    public const char EQUAL = '=';
    public const char AT = '@';
    public const char DASH = '-';
    public const char UNDER_SCORE = '_';
    public const char ACCENT = '`';
    public const char TILDE = '~';
    public const char PIPE = '|';
    public const char COLON = ':';
    public const char DOUBLE_QUOTE = '\"';
    public const char SINGLE_QUOTE = '\'';
    public const char OPEN_ARROW_BRACKET = '<';
    public const char CLOSE_ARROW_BRACKET = '>';
    public const char FORWARD_SLASH = '/';
    public const char BACK_SLASH = '\\';

    // MovementKeys
    public const string ARROW_LEFT_KEY = "ArrowLeft";
    public const string ARROW_DOWN_KEY = "ArrowDown";
    public const string ARROW_UP_KEY = "ArrowUp";
    public const string ARROW_RIGHT_KEY = "ArrowRight";
    public const string HOME_KEY = "Home";
    public const string END_KEY = "End";
    
    // MovementCodes
    public const string ARROW_LEFT_CODE = "ArrowLeft";
    public const string ARROW_DOWN_CODE = "ArrowDown";
    public const string ARROW_UP_CODE = "ArrowUp";
    public const string ARROW_RIGHT_CODE = "ArrowRight";
    public const string HOME_CODE = "Home";
    public const string END_CODE = "End";

    // AlternateMovementKeys
    public const string ARROW_LEFT_ALTKEY = "h";
    public const string ARROW_DOWN_ALTKEY = "j";
    public const string ARROW_UP_ALTKEY = "k";
    public const string ARROW_RIGHT_ALTKEY = "l";
    
    // AlternateMovementCodes
    public const string ARROW_LEFT_ALTCODE = "KeyH";
    public const string ARROW_DOWN_ALTCODE = "KeyJ";
    public const string ARROW_UP_ALTCODE = "KeyK";
    public const string ARROW_RIGHT_ALTCODE = "KeyL";
    /* End KeyboardKeyFacts */
    
    /* Start FileTemplateFacts */
    private static readonly List<IFileTemplate> _emptyFileTemplateList = new();

    public static readonly IFileTemplate CSharpClass = new FileTemplate(
        "C# Class",
        FileTemplateKind.CSharp,
        CommonFacts.C_SHARP_CLASS,
        _ => _emptyFileTemplateList,
        true,
        CSharpClassCreateFileFunc);

    public static readonly IFileTemplate RazorCodebehind = new FileTemplate(
        "Razor codebehind",
        FileTemplateKind.Razor,
        CommonFacts.RAZOR_CODEBEHIND,
        _ => _emptyFileTemplateList,
        true,
        RazorCodebehindCreateFileFunc);

    public static readonly IFileTemplate RazorMarkup = new FileTemplate(
        "Razor markup",
        FileTemplateKind.Razor,
        CommonFacts.RAZOR_MARKUP,
        fileName => new List<IFileTemplate>
        {
            RazorCodebehind
        },
        true,
        RazorMarkupCreateFileFunc);

    /// <summary>
    /// Template should be:
    /// -------------------
    /// namespace Walk.Ide.ClassLib.FileTemplates;
    ///
    /// public class Asdf
    /// {
    ///     
    /// }
    /// 
    /// </summary>
    private static FileTemplateResult CSharpClassCreateFileFunc(FileTemplateParameter templateParameter)
    {
        var emptyFileAbsolutePathString = templateParameter.ParentDirectory.Value +
            templateParameter.Filename;

        // Create AbsolutePath as to leverage it for knowing the file extension and other details
        var emptyFileAbsolutePath = templateParameter.EnvironmentProvider.AbsolutePathFactory(
            emptyFileAbsolutePathString,
            false,
            tokenBuilder: new StringBuilder(),
            formattedBuilder: new StringBuilder(),
            AbsolutePathNameKind.NameNoExtension);

        var templatedFileContent = GetContent(
            emptyFileAbsolutePath.Name,
            templateParameter.ParentDirectoryNamespace);

        var templatedFileAbsolutePathString = templateParameter.ParentDirectory.Value +
            emptyFileAbsolutePath.Name +
            '.' +
            CommonFacts.C_SHARP_CLASS;

        var templatedFileAbsolutePath = templateParameter.EnvironmentProvider.AbsolutePathFactory(
            templatedFileAbsolutePathString,
            false,
            tokenBuilder: new StringBuilder(),
            formattedBuilder: new StringBuilder(),
            AbsolutePathNameKind.NameWithExtension);

        return new FileTemplateResult(templatedFileAbsolutePath, templateParameter.ParentDirectoryNamespace, templatedFileContent);

        string GetContent(string fileNameNoExtension, string namespaceString) =>
            $@"namespace {namespaceString};

public class {fileNameNoExtension}
{{
    
}}
".ReplaceLineEndings();
    }

    /// <summary>
    /// Template should be:
    /// -------------------
    /// <h3>Asdf</h3>
    /// 
    /// @code {
    ///     
    /// }
    /// 
    /// </summary>
    private static FileTemplateResult RazorMarkupCreateFileFunc(FileTemplateParameter templateParameter)
    {
        var emptyFileAbsolutePathString = templateParameter.ParentDirectory.Value +
            templateParameter.Filename;

        // Create AbsolutePath as to leverage it for knowing the file extension and other details
        var emptyFileAbsolutePath = templateParameter.EnvironmentProvider.AbsolutePathFactory(
            emptyFileAbsolutePathString,
            false,
            tokenBuilder: new StringBuilder(),
            formattedBuilder: new StringBuilder(),
            AbsolutePathNameKind.NameNoExtension);

        var templatedFileContent = GetContent(emptyFileAbsolutePath.Name);

        var templatedFileAbsolutePathString = templateParameter.ParentDirectory.Value +
            emptyFileAbsolutePath.Name +
            '.' +
            CommonFacts.RAZOR_MARKUP;

        var templatedFileAbsolutePath = templateParameter.EnvironmentProvider.AbsolutePathFactory(
            templatedFileAbsolutePathString,
            false,
            tokenBuilder: new StringBuilder(),
            formattedBuilder: new StringBuilder(),
            AbsolutePathNameKind.NameWithExtension);

        return new FileTemplateResult(templatedFileAbsolutePath, templateParameter.ParentDirectoryNamespace, templatedFileContent);

        string GetContent(string fileNameNoExtension) =>
            $@"<h3>{fileNameNoExtension}</h3>

@code {{
    
}}".ReplaceLineEndings();
    }

    /// <summary>
    /// Template should be:
    /// -------------------
    /// using Microsoft.AspNetCore.Components;
    /// 
    /// namespace Walk.Ide.RazorLib.Menu;
    /// 
    /// public partial class Asdf : ComponentBase
    /// {
    ///     
    /// }
    /// </summary>
    private static FileTemplateResult RazorCodebehindCreateFileFunc(FileTemplateParameter templateParameter)
    {
        string GetContent(string fileNameNoExtension, string namespaceString)
        {
            var className = fileNameNoExtension.Replace('.' + CommonFacts.RAZOR_MARKUP, string.Empty);

            var interpolatedResult = $@"using Microsoft.AspNetCore.Components;

namespace {namespaceString};

public partial class {className} : ComponentBase
{{
    
}}".ReplaceLineEndings();

            return interpolatedResult;
        }

        var emptyFileAbsolutePathString = templateParameter.ParentDirectory.Value +
            templateParameter.Filename;

        // Create AbsolutePath as to leverage it for knowing the file extension and other details
        var emptyFileAbsolutePath = templateParameter.EnvironmentProvider.AbsolutePathFactory(
            emptyFileAbsolutePathString,
            false,
            tokenBuilder: new StringBuilder(),
            formattedBuilder: new StringBuilder(),
            AbsolutePathNameKind.NameNoExtension);

        var templatedFileContent = GetContent(
            emptyFileAbsolutePath.Name,
            templateParameter.ParentDirectoryNamespace);

        var templatedFileAbsolutePathString = templateParameter.ParentDirectory.Value +
            emptyFileAbsolutePath.Name;

        if (templatedFileAbsolutePathString.EndsWith('.' + CommonFacts.RAZOR_MARKUP))
            templatedFileAbsolutePathString += '.' + CommonFacts.C_SHARP_CLASS;
        else
            templatedFileAbsolutePathString += '.' + CommonFacts.RAZOR_CODEBEHIND;

        var templatedFileAbsolutePath = templateParameter.EnvironmentProvider.AbsolutePathFactory(
            templatedFileAbsolutePathString,
            false,
            tokenBuilder: new StringBuilder(),
            formattedBuilder: new StringBuilder(),
            AbsolutePathNameKind.NameWithExtension);

        return new FileTemplateResult(templatedFileAbsolutePath, templateParameter.ParentDirectoryNamespace, templatedFileContent);
    }
    /* End FileTemplateFacts */
    
    /* Start DialogHelper */
    public static ElementDimensions ConstructDefaultElementDimensions()
    {
        var elementDimensions = new ElementDimensions
        {
            ElementPositionKind = ElementPositionKind.Fixed
        };

        elementDimensions.Width_Base_0 = new DimensionUnit(60, DimensionUnitKind.ViewportWidth);

        elementDimensions.Height_Base_0 = new DimensionUnit(60, DimensionUnitKind.ViewportHeight);

        elementDimensions.Left_Base_0 = new DimensionUnit(20, DimensionUnitKind.ViewportWidth);

        elementDimensions.Top_Base_0 = new DimensionUnit(20, DimensionUnitKind.ViewportHeight);

        return elementDimensions;
    }
    /* End DialogHelper */
    
    /* Start DropdownHelper */
    public static Task RenderDropdownAsync(
        CommonService commonService,
        WalkCommonJavaScriptInteropApi walkCommonJavaScriptInteropApi,
        string anchorHtmlElementId,
        DropdownOrientation dropdownOrientation,
        Key<DropdownRecord> dropdownKey,
        MenuRecord menu,
        string? elementHtmlIdForReturnFocus,
        bool preventScroll)
    {
        return RenderDropdownAsync(
            commonService,
            walkCommonJavaScriptInteropApi,
            anchorHtmlElementId,
            dropdownOrientation,
            dropdownKey,
            menu,
            async () => 
            {
                try
                {
                    await walkCommonJavaScriptInteropApi
                        .FocusHtmlElementById(elementHtmlIdForReturnFocus, preventScroll)
                        .ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // TODO: Capture specifically the exception that is fired when the JsRuntime...
                    //       ...tries to set focus to an HTML element, but that HTML element
                    //       was not found.
                }
            });
    }

    public static Task RenderDropdownAsync(
        CommonService commonService,
        WalkCommonJavaScriptInteropApi walkCommonJavaScriptInteropApi,
        string anchorHtmlElementId,
        DropdownOrientation dropdownOrientation,
        Key<DropdownRecord> dropdownKey,
        MenuRecord menu,
        ElementReference? elementReferenceForReturnFocus)
    {
        return RenderDropdownAsync(
            commonService,
            walkCommonJavaScriptInteropApi,
            anchorHtmlElementId,
            dropdownOrientation,
            dropdownKey,
            menu,
            async () => 
            {
                try
                {
                    if (elementReferenceForReturnFocus is not null)
                    {
                        await elementReferenceForReturnFocus.Value
                            .FocusAsync()
                            .ConfigureAwait(false);
                    }
                }
                catch (Exception)
                {
                    // TODO: Capture specifically the exception that is fired when the JsRuntime...
                    //       ...tries to set focus to an HTML element, but that HTML element
                    //       was not found.
                }
            });
    }
    
    public static async Task RenderDropdownAsync(
        CommonService commonService,
        WalkCommonJavaScriptInteropApi walkCommonJavaScriptInteropApi,
        string anchorHtmlElementId,
        DropdownOrientation dropdownOrientation,
        Key<DropdownRecord> dropdownKey,
        MenuRecord menu,
        Func<Task>? restoreFocusOnClose)
    {
        var buttonDimensions = await walkCommonJavaScriptInteropApi
            .MeasureElementById(anchorHtmlElementId)
            .ConfigureAwait(false);

        var leftInitial = dropdownOrientation == DropdownOrientation.Right
            ? buttonDimensions.LeftInPixels + buttonDimensions.WidthInPixels
            : buttonDimensions.LeftInPixels;
        
        var topInitial = dropdownOrientation == DropdownOrientation.Bottom
            ? buttonDimensions.TopInPixels + buttonDimensions.HeightInPixels
            : buttonDimensions.TopInPixels;

        var dropdownRecord = new DropdownRecord(
            dropdownKey,
            leftInitial,
            topInitial,
            typeof(MenuDisplay),
            new Dictionary<string, object?>
            {
                {
                    nameof(MenuDisplay.Menu),
                    menu
                }
            },
            restoreFocusOnClose);

        commonService.Dropdown_ReduceRegisterAction(dropdownRecord);
    }
    /* End DropdownHelper */
    
    /* Start HtmlFacts */
    public const int HtmlFacts_Button_PADDING_IN_PIXELS = 6;

    public static string HtmlFacts_Button_ButtonPaddingHorizontalTotalInPixelsCssValue =>
        $"2 * {HtmlFacts_Button_PADDING_IN_PIXELS.ToCssValue()}px";
    /* End HtmlFacts */
    
    /* Start DialogFacts */
    public static readonly Key<IDynamicViewModel> InputFileDialogKey = Key<IDynamicViewModel>.NewKey();
    /* End DialogFacts */
    
    /* Start ThrottleFacts */
    /// <summary>
    /// This <see cref="TimeSpan"/> represents '1000ms / 60 = 16.6...ms', whether this is equivalent to 60fps is unknown.
    /// </summary>
    public static readonly TimeSpan ThrottleFacts_Sixty_Frames_Per_Second = TimeSpan.FromMilliseconds(17);

    /// <summary>
    /// This <see cref="TimeSpan"/> represents '1000ms / 30 = 33.3...ms', whether this is equivalent to 30fps is unknown.
    /// </summary>
    public static readonly TimeSpan ThrottleFacts_Thirty_Frames_Per_Second = TimeSpan.FromMilliseconds(34);
    
    /// <summary>
    /// This <see cref="TimeSpan"/> represents '1000ms / 24 = 41.6...ms', whether this is equivalent to 24fps is unknown.
    /// </summary>
    public static readonly TimeSpan ThrottleFacts_TwentyFour_Frames_Per_Second = TimeSpan.FromMilliseconds(42);
    /* End ThrottleFacts */
    
    /* Start PathHelper */
    /// <summary>
    /// Given: "/Dir/Homework/math.txt" and "../Games/"<br/>
    /// Then: "/Dir/Games/"<br/><br/>
    /// |<br/>
    /// Calculate an absolute-path-string by providing a starting AbsolutePath, then
    /// a relative-path-string from the starting position.<br/>
    /// |<br/>
    /// Magic strings:<br/>
    ///     -"./" (same directory) token<br/>
    ///     -"../" (move up directory)<br/>
    ///     -If the relative path does not start with the previously
    ///      mentioned magic strings, then "./" (same directory) token is implicitly used.<br/>
    /// |<br/>
    /// This method accepts starting AbsolutePath that can be either a directory, or not.<br/>
    ///     -If provided a directory, then a "./" (same directory) token will target the
    ///      directory which the provided starting AbsolutePath is pointing to.<br/>
    ///     -If provided a file, then a "./" (same directory) token will target the
    ///      parent-directory in which the file is contained.<br/>
    ///     -If provided a directory, then a "../" (move up directory) token will target the
    ///      parent of the directory which the provided starting AbsolutePath is pointing to.<br/>
    ///     -If provided a file, then a "../" (move up directory) token will target the
    ///      parent directory of the parent directory in which the file is contained.<br/><br/>
    /// </summary>
    public static string GetAbsoluteFromAbsoluteAndRelative(
        AbsolutePath absolutePath,
        string relativePathString,
        IEnvironmentProvider environmentProvider,
        StringBuilder tokenBuilder,
        StringBuilder formattedBuilder,
        string moveUpDirectoryToken,
        string sameDirectoryToken,
        List<string> ancestorDirectoryList)
    {
        // Normalize the directory separator character
        relativePathString = relativePathString.Replace(
            environmentProvider.AltDirectorySeparatorChar,
            environmentProvider.DirectorySeparatorChar);

        // "../" is being called the 'moveUpDirectoryToken'
        var moveUpDirectoryCount = 0;

        // Count all usages of "../",
        // and each time one is found: remove it from the relativePathString.
        while (relativePathString.StartsWith(moveUpDirectoryToken, StringComparison.InvariantCulture))
        {
            moveUpDirectoryCount++;
            relativePathString = relativePathString[moveUpDirectoryToken.Length..];
        }

        if (relativePathString.StartsWith(sameDirectoryToken))
        {
            if (moveUpDirectoryCount > 0)
            {
                // TODO: A filler expression is written here currently...
                //       ...but perhaps throwing an exception here is the way to go?
                //       |
                //       This if-branch implies that the relative path used
                //       "../" (move up directory) and
                //       "./" (same directory) tokens.
                _ = 0;
            }

            // Remove the same directory token text.
            relativePathString = relativePathString[sameDirectoryToken.Length..];
        }

        if (moveUpDirectoryCount > 0)
        {
            if (moveUpDirectoryCount < ancestorDirectoryList.Count)
            {
                var nearestSharedAncestor = ancestorDirectoryList[^(1 + moveUpDirectoryCount)];
                return nearestSharedAncestor + relativePathString;
            }
            else
            {
                // TODO: This case seems nonsensical?...
                //       ...It was written here originally,
                //       it is (2024-05-18) so this must have been here for a few months.
                //       |
                //       But, the root directory would always be a shared directory (I think)?
                return environmentProvider.DirectorySeparatorChar + relativePathString;
            }
        }
        else
        {
            // Side Note: A lack of both "../" (move up directory) and "./" (same directory) tokens,
            //            Implicitly implies: the "./" (same directory) token
            if (absolutePath.IsDirectory)
            {
                return absolutePath.Value + relativePathString;
            }
            else
            {
                var parentDirectory = absolutePath.CreateSubstringParentDirectory();
                if (parentDirectory is null)
                    throw new NotImplementedException();
                else
                    return parentDirectory + relativePathString;
            }
        }
    }

    /// <summary>
    /// Given: "/Dir/Homework/math.txt" and "/Dir/Games/"<br/>
    /// Then: "../Games/"<br/><br/>
    /// 
    /// Calculate an absolute-path-string by providing a starting AbsolutePath, then
    /// an ending AbsolutePath. The relative-path-string to travel from start to end, will
    /// be returned as a string.
    /// </summary>
    public static string GetRelativeFromTwoAbsolutes(
        AbsolutePath startingPath,
        string startingPathParentDirectory,
        AbsolutePath endingPath,
        string endingPathParentDirectory,
        IEnvironmentProvider environmentProvider,
        StringBuilder tokenBuilder,
        StringBuilder formattedBuilder)
    {
        var pathBuilder = new StringBuilder();
        
        var startingPathAncestorDirectoryList = startingPath.GetAncestorDirectoryList(environmentProvider, tokenBuilder, formattedBuilder, AbsolutePathNameKind.NameWithExtension);
        var endingPathAncestorDirectoryList = endingPath.GetAncestorDirectoryList(environmentProvider, tokenBuilder, formattedBuilder, AbsolutePathNameKind.NameWithExtension);
        
        var commonPath = startingPathAncestorDirectoryList.First();

        if (startingPathParentDirectory == endingPathParentDirectory)
        {
            // TODO: Will this code break when the mounted drives are different, and parent directories share same name?

            // Use './' because they share the same parent directory.
            pathBuilder.Append($".{environmentProvider.DirectorySeparatorChar}");

            commonPath = startingPathParentDirectory;
        }
        else
        {
            // Use '../' but first calculate how many UpDir(s) are needed
            int limitingIndex = Math.Min(
                startingPathAncestorDirectoryList.Count,
                endingPathAncestorDirectoryList.Count);

            var i = 0;

            for (; i < limitingIndex; i++)
            {
                var startingPathAncestor = environmentProvider.AbsolutePathFactory(
                    startingPathAncestorDirectoryList[i],
                    true,
                    tokenBuilder,
                    formattedBuilder,
                    AbsolutePathNameKind.NameWithExtension);

                var endingPathAncestor = environmentProvider.AbsolutePathFactory(
                    endingPathAncestorDirectoryList[i],
                    true,
                    tokenBuilder,
                    formattedBuilder,
                    AbsolutePathNameKind.NameWithExtension);

                if (startingPathAncestor.Name == endingPathAncestor.Name)
                    commonPath = startingPathAncestor.Value;
                else
                    break;
            }

            var upDirCount = startingPathAncestorDirectoryList.Count - i;

            for (int appendUpDir = 0; appendUpDir < upDirCount; appendUpDir++)
            {
                pathBuilder.Append("../");
            }
        }

        var notCommonPath = new string(endingPath.Value.Skip(commonPath.Length).ToArray());

        return pathBuilder.Append(notCommonPath).ToString();
    }

    public static string CalculateNameWithExtension(
        string nameNoExtension,
        string extensionNoPeriod,
        bool isDirectory)
    {
        if (isDirectory)
        {
            return nameNoExtension + extensionNoPeriod;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(extensionNoPeriod))
                return nameNoExtension;
            else
                return nameNoExtension + '.' + extensionNoPeriod;
        }
    }
    /* End PathHelper */
}
