using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

/// <summary>
/// TODO: I copy and pasted <see cref="Diffs.Models.TextEditorDiffDecorationMapper"/>...
/// ...to make this class. The decorations need to be made. I only
/// implemented the highlighting itself for now, and pick the colors later.(2024-02-01)
/// </summary>
public sealed class FindOverlayDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (FindOverlayDecorationKind)decorationByte;
        
        return decoration switch
        {
            FindOverlayDecorationKind.None => string.Empty,
            FindOverlayDecorationKind.LongestCommonSubsequence => "background-color: var(--di_te_diff-longest-common-subsequence-background-color);",
            FindOverlayDecorationKind.Insertion => "background-color: var(--di_te_diff-insertion-background-color);",
            FindOverlayDecorationKind.Deletion => "background-color: var(--di_te_diff-deletion-background-color);",
            FindOverlayDecorationKind.Modification => "background-color: var(--di_te_diff-modification-background-color);",
            _ => string.Empty,
        };
    }
}
