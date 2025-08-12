using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.Ide.RazorLib.CodeSearches.Models;

public record struct CodeSearchState(
    string Query,
    string? StartingAbsolutePathForSearch,
    CodeSearchFilterKind CodeSearchFilterKind,
    IReadOnlyList<string> ResultList,
    string PreviewFilePath,
    Key<TextEditorViewModel> PreviewViewModelKey)
{
    public static readonly Key<TreeViewContainer> TreeViewCodeSearchContainerKey = Key<TreeViewContainer>.NewKey();
    
    public CodeSearchState() : this(
        string.Empty,
        null,
        CodeSearchFilterKind.None,
        Array.Empty<string>(),
        string.Empty,
        Key<TextEditorViewModel>.Empty)
    {
        // topContentHeight
        TopContentElementDimensions.Height_Base_0 = new DimensionUnit(40, DimensionUnitKind.Percentage);
        TopContentElementDimensions.Height_Offset = new DimensionUnit(0, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract);

        // bottomContentHeight
        BottomContentElementDimensions.Height_Base_0 = new DimensionUnit(60, DimensionUnitKind.Percentage);
        BottomContentElementDimensions.Height_Offset = new DimensionUnit(0, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract);
    }

    public ElementDimensions TopContentElementDimensions = new();
    public ElementDimensions BottomContentElementDimensions = new();
}
