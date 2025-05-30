using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.TextEditor.RazorLib.Diffs.Models;

public class TextEditorDiffDecorationMapper : IDecorationMapper
{
    public string Map(byte decorationByte)
    {
        var decoration = (TextEditorDiffDecorationKind)decorationByte;

        return decoration switch
        {
            TextEditorDiffDecorationKind.None => string.Empty,
            TextEditorDiffDecorationKind.LongestCommonSubsequence => "di_te_diff-longest-common-subsequence",
            TextEditorDiffDecorationKind.Insertion => "di_te_diff-insertion",
            TextEditorDiffDecorationKind.InsertionLine => "di_te_diff-insertion-line",
            TextEditorDiffDecorationKind.Deletion => "di_te_diff-deletion",
            TextEditorDiffDecorationKind.Modification => "di_te_diff-modification",
            _ => string.Empty,
        };
    }
}