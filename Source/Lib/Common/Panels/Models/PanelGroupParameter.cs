using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Common.RazorLib.Panels.Models;

public struct PanelGroupParameter
{
    public PanelGroupParameter(
        Key<PanelGroup> panelGroupKey,
        string cssClassString)
    {
        PanelGroupKey = panelGroupKey;
        CssClassString = cssClassString;
    }

    public Key<PanelGroup> PanelGroupKey { get; set; } = Key<PanelGroup>.Empty;
    public string CssClassString { get; set; } = null!;
    
    public string PanelPositionCss { get; set; }
    public string HtmlIdTabs { get; set; }
}
