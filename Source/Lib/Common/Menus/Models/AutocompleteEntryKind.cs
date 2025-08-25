namespace Walk.Common.RazorLib.Menus.Models;

public enum AutocompleteEntryKind
{
    Word,
    Snippet,
    Variable,
    Function,
    Type,
    Namespace,
    /// <summary>
    /// This is a hack.
    /// Currently the only MenuOptionRecord that have a submenu
    /// don't have an IconKind being used.
    ///
    /// So this kind can be marked Chevron as a hacky
    /// place to store the information
    /// that there ought to be a chevron indicating a submenu.
    /// </summary>
    Chevron,
}
