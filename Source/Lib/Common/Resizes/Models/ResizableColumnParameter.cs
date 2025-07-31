using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Common.RazorLib.Resizes.Models;

public struct ResizableColumnParameter
{
    public ResizableColumnParameter(
        ElementDimensions leftElementDimensions,
        ElementDimensions rightElementDimensions,
        Func<Task> reRenderFuncAsync)
    {
        LeftElementDimensions = leftElementDimensions;
        RightElementDimensions = rightElementDimensions;
        ReRenderFuncAsync = reRenderFuncAsync;
    }
    
    public ElementDimensions LeftElementDimensions { get; set; }
    public ElementDimensions RightElementDimensions { get; set; }
    public Func<Task> ReRenderFuncAsync { get; set; } = null!;
}
