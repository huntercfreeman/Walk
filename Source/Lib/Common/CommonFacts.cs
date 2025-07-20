using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Exceptions;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib;

public static class CommonFacts
{
    /* Start ThemeFacts */
	public static readonly ThemeRecord VisualStudioLightThemeClone = new ThemeRecord(
        new Key<ThemeRecord>(Guid.Parse("3ea6a4a5-02b3-4b84-9d6f-e663465d3126")),
        "Visual Studio Light Clone",
        "di_visual-studio-light-theme-clone",
        ThemeContrastKind.Default,
        ThemeColorKind.Light,
        new ThemeScope[] { ThemeScope.App, ThemeScope.TextEditor });

    public static readonly ThemeRecord VisualStudioDarkThemeClone = new ThemeRecord(
        new Key<ThemeRecord>(Guid.Parse("8eaabd97-186d-40d0-a57b-5fec1c158902")),
        "Visual Studio Dark Clone",
        "di_visual-studio-dark-theme-clone",
        ThemeContrastKind.Default,
        ThemeColorKind.Dark,
        new ThemeScope[] { ThemeScope.App, ThemeScope.TextEditor });
    /* End ThemeFacts */
    
    /* Start ContextFacts */
    public static readonly ContextRecord GlobalContext = new(
        Key<ContextRecord>.NewKey(),
        "Global",
        "global",
        new Keymap(Key<Keymap>.NewKey(), "Global"));

    public static readonly ContextRecord ActiveContextsContext = new(
        Key<ContextRecord>.NewKey(),
        "Active Contexts",
        "active-contexts",
        IKeymap.Empty);

	public static readonly ContextRecord FindAllReferencesContext = new(
        Key<ContextRecord>.NewKey(),
        "Find All References",
        "find-all-references",
        IKeymap.Empty);

    public static readonly ContextRecord FolderExplorerContext = new(
        Key<ContextRecord>.NewKey(),
        "Folder Explorer",
        "folder-explorer",
        IKeymap.Empty);

    public static readonly ContextRecord SolutionExplorerContext = new(
        Key<ContextRecord>.NewKey(),
        "Solution Explorer",
        "solution-explorer",
        IKeymap.Empty);

    public static readonly ContextRecord CompilerServiceExplorerContext = new(
        Key<ContextRecord>.NewKey(),
        "Compiler Service Explorer",
        "compiler-service-explorer",
        IKeymap.Empty);
    
    public static readonly ContextRecord CompilerServiceEditorContext = new(
        Key<ContextRecord>.NewKey(),
        "Compiler Service Editor",
        "compiler-service-editor",
        IKeymap.Empty);

	public static readonly ContextRecord TestExplorerContext = new(
        Key<ContextRecord>.NewKey(),
        "Test Explorer",
        "test-explorer",
        IKeymap.Empty);
    
    public static readonly ContextRecord CSharpReplContext = new(
        Key<ContextRecord>.NewKey(),
        "C# REPL",
        "c-sharp-repl",
        IKeymap.Empty);

    public static readonly ContextRecord BackgroundServicesContext = new(
        Key<ContextRecord>.NewKey(),
        "Background Services",
        "background-services",
        IKeymap.Empty);

    public static readonly ContextRecord DialogDisplayContext = new(
        Key<ContextRecord>.NewKey(),
        "Dialog Display",
        "dialog-display",
        IKeymap.Empty);

    public static readonly ContextRecord MainLayoutHeaderContext = new(
        Key<ContextRecord>.NewKey(),
        "MainLayout Header",
        "main-layout-header",
        IKeymap.Empty);

    public static readonly ContextRecord MainLayoutFooterContext = new(
        Key<ContextRecord>.NewKey(),
        "MainLayout Footer",
        "main-layout-footer",
        IKeymap.Empty);

    public static readonly ContextRecord EditorContext = new(
        Key<ContextRecord>.NewKey(),
        "Editor",
        "editor",
        IKeymap.Empty);

    public static readonly ContextRecord TextEditorContext = new(
        Key<ContextRecord>.NewKey(),
        "Text Editor",
        "text-editor",
        IKeymap.Empty);

    public static readonly ContextRecord ErrorListContext = new(
        Key<ContextRecord>.NewKey(),
        "Error List",
        "error-list",
        IKeymap.Empty);

    public static readonly ContextRecord OutputContext = new(
        Key<ContextRecord>.NewKey(),
        "Output",
        "output",
        IKeymap.Empty);

    public static readonly ContextRecord NuGetPackageManagerContext = new(
        Key<ContextRecord>.NewKey(),
        "NuGetPackageManager",
        "nu-get-package-manager",
        IKeymap.Empty);

    public static readonly ContextRecord GitContext = new(
        Key<ContextRecord>.NewKey(),
        "Git",
        "git",
        IKeymap.Empty);
    
    public static readonly ContextRecord TerminalContext = new(
        Key<ContextRecord>.NewKey(),
        "Terminal",
        "terminal",
        IKeymap.Empty);
        
    public static readonly ContextRecord NotificationContext = new(
        Key<ContextRecord>.NewKey(),
        "Notification",
        "notification",
        IKeymap.Empty);
    
    public static readonly ContextRecord DialogContext = new(
        Key<ContextRecord>.NewKey(),
        "Dialog",
        "dialog",
        IKeymap.Empty);
        
    public static readonly ContextRecord WidgetContext = new(
        Key<ContextRecord>.NewKey(),
        "Widget",
        "widget",
        IKeymap.Empty);
    
    public static readonly ContextRecord DropdownContext = new(
        Key<ContextRecord>.NewKey(),
        "Dropdown",
        "dropdown",
        IKeymap.Empty);

    public static readonly IReadOnlyList<ContextRecord> AllContextsList = new List<ContextRecord>()
    {
        GlobalContext,
        ActiveContextsContext,
        FolderExplorerContext,
        SolutionExplorerContext,
        CompilerServiceExplorerContext,
        CompilerServiceEditorContext,
        CSharpReplContext,
        BackgroundServicesContext,
        DialogDisplayContext,
        MainLayoutHeaderContext,
        MainLayoutFooterContext,
        EditorContext,
        TextEditorContext,
		ErrorListContext,
        OutputContext,
        NuGetPackageManagerContext,
        GitContext,
        TerminalContext,
        NotificationContext,
        DialogContext,
        WidgetContext,
        DropdownContext,
    };
    
    /// <summary>
    /// Used when repositioning a dropdown so that it appears on screen.
    /// </summary>
    public static string RootHtmlElementId { get; set; } = GlobalContext.ContextElementId;
    /* End ContextFacts */
    
    /* Start PanelFacts */
    public static readonly Key<PanelGroup> LeftPanelGroupKey = Key<PanelGroup>.NewKey();
    public static readonly Key<PanelGroup> RightPanelGroupKey = Key<PanelGroup>.NewKey();
    public static readonly Key<PanelGroup> BottomPanelGroupKey = Key<PanelGroup>.NewKey();

    public static PanelGroup GetTopLeftPanelGroup(PanelState panelState)
    {
        return panelState.PanelGroupList.First(x => x.Key == LeftPanelGroupKey);
    }

    public static PanelGroup GetTopRightPanelGroup(PanelState panelState)
    {
        return panelState.PanelGroupList.First(x => x.Key == RightPanelGroupKey);
    }

    public static PanelGroup GetBottomPanelGroup(PanelState panelState)
    {
        return panelState.PanelGroupList.First(x => x.Key == BottomPanelGroupKey);
    }
    /* End PanelFacts */
    
    /* Start DimensionUnitFacts */
    public const string PURPOSE_OFFSET = "OFFSET";
	public const string PURPOSE_RESIZABLE_HANDLE_ROW = "RESIZABLE_HANDLE_ROW";
	public const string PURPOSE_RESIZABLE_HANDLE_COLUMN = "RESIZABLE_HANDLE_COLUMN";
    /* End DimensionUnitFacts */
    
    /* Start SizeFacts */
    public static readonly DimensionUnit Ide_Header_Height = new(3, DimensionUnitKind.RootCharacterHeight);
    /* End SizeFacts */
    
    /* Start BackgroundTaskFacts */
    public static Key<BackgroundTaskQueue> ContinuousQueueKey { get; } = new Key<BackgroundTaskQueue>(Guid.Parse("78912ee9-1b3f-4bc3-ab8b-5681fbf0b131"));
	public static Key<BackgroundTaskQueue> IndefiniteQueueKey { get; } = new Key<BackgroundTaskQueue>(Guid.Parse("7905c763-c3fd-418e-b73d-4ca18666c20c"));
    /* End BackgroundTaskFacts */
    
    /* Start KeyboardKeyFacts */
    public static bool IsMetaKey(KeyboardEventArgs keyboardEventArgs)
    {
        return IsMetaKey(keyboardEventArgs.Key, keyboardEventArgs.Code);
    }
    
    public static bool IsMetaKey(KeymapArgs keymapArgs)
    {
        return IsMetaKey(keymapArgs.Key, keymapArgs.Code);
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
    
    public static bool CheckIsContextMenuEvent(KeymapArgs keymapArgs)
    {
        return CheckIsContextMenuEvent(
            keymapArgs.Key,
            keymapArgs.Code,
            keymapArgs.ShiftKey,
            keymapArgs.AltKey);
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

    public static char ConvertWhitespaceCodeToCharacter(string code)
    {
        switch (code)
        {
            case TAB_CODE:
                return '\t';
            case ENTER_CODE:
                return '\n';
            case SPACE_CODE:
                return ' ';
            default:
                throw new WalkCommonException($"Unrecognized Whitespace code of: {code}");
        }
    }

    /// <summary>
    /// TODO: This method is not fully implemented.
    /// </summary>
    public static string ConvertCodeToKey(string code)
    {
        switch (code)
        {
            case "Digit1":
                return "1";
            case "Digit2":
                return "2";
            case "Digit3":
                return "3";
            case "Digit4":
                return "4";
            case "Digit5":
                return "5";
            case "Digit6":
                return "6";
            case "Digit7":
                return "7";
            case "Digit8":
                return "8";
            case "Digit9":
                return "9";
            case "KeyA":
                return "a";
            case "KeyB":
                return "b";
            case "KeyC":
                return "c";
            case "KeyD":
                return "d";
            case "KeyE":
                return "e";
            case "KeyF":
                return "f";
            case "KeyG":
                return "g";
            case "KeyH":
                return "h";
            case "KeyI":
                return "i";
            case "KeyJ":
                return "j";
            case "KeyK":
                return "k";
            case "KeyL":
                return "l";
            case "KeyM":
                return "m";
            case "KeyN":
                return "n";
            case "KeyO":
                return "o";
            case "KeyP":
                return "p";
            case "KeyQ":
                return "q";
            case "KeyR":
                return "r";
            case "KeyS":
                return "s";
            case "KeyT":
                return "t";
            case "KeyU":
                return "u";
            case "KeyV":
                return "v";
            case "KeyW":
                return "w";
            case "KeyX":
                return "x";
            case "KeyY":
                return "y";
            case "KeyZ":
                return "z";
            default:
                return code;
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
}
