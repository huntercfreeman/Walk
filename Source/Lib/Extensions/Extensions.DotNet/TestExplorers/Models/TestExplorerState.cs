using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Extensions.DotNet.TestExplorers.Models;

public record TestExplorerState(
    string? SolutionFilePath,
    int TotalTestCount,
    HashSet<string> NotRanTestHashSet,
    HashSet<string> PassedTestHashSet,
    HashSet<string> FailedTestHashSet)
{
    public static readonly Key<TreeViewContainer> TreeViewTestExplorerKey = Key<TreeViewContainer>.NewKey();

    public TestExplorerState() : this(
        SolutionFilePath: null,
        TotalTestCount: 0,
        NotRanTestHashSet: new(),
        PassedTestHashSet: new(),
        FailedTestHashSet: new())
    {
        // TreeView ElementDimensions
        TreeViewElementDimensions.Width_Base_0 = new DimensionUnit(50, DimensionUnitKind.Percentage);
        TreeViewElementDimensions.Width_Offset = new DimensionUnit(0, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract, DimensionUnitPurposeKind.Offset);

        // Details ElementDimensions
        DetailsElementDimensions.Width_Base_0 = new DimensionUnit(50, DimensionUnitKind.Percentage);
        DetailsElementDimensions.Width_Offset = new DimensionUnit(0, DimensionUnitKind.Pixels, DimensionOperatorKind.Subtract, DimensionUnitPurposeKind.Offset);
    }

    public List<ProjectTestModel> ProjectTestModelList { get; init; } = new();
    
    public ElementDimensions TreeViewElementDimensions { get; init; } = new();
    public ElementDimensions DetailsElementDimensions { get; init; } = new();
}
