using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Common.RazorLib.Resizes.Models;

public struct ResizableRowParameter
{
    public ResizableRowParameter(
        ElementDimensions topElementDimensions,
        ElementDimensions bottomElementDimensions,
        Func<Task> reRenderFuncAsync)
    {
        TopElementDimensions = topElementDimensions;
        BottomElementDimensions = bottomElementDimensions;
        ReRenderFuncAsync = reRenderFuncAsync;
    }
    
    public ElementDimensions TopElementDimensions { get; set; }
    public ElementDimensions BottomElementDimensions { get; set; }
    public Func<Task> ReRenderFuncAsync { get; set; } = null!;
}
