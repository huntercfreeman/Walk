using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Dynamics.Models;

public interface IDynamicViewModel
{
    public Key<IDynamicViewModel> DynamicViewModelKey { get; }
    public string Title { get; }
    public string TitleVerbose { get; }
    public Type ComponentType { get; }
    public Dictionary<string, object?>? ComponentParameterMap { get; }
    public string? SetFocusOnCloseElementId { get; set; }
    
    public static readonly string DefaultSetFocusOnCloseElementId = CommonFacts.RootHtmlElementId;
}
