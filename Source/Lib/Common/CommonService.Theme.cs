using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Themes.Models;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    private ThemeState _themeState = new();
    
    public ThemeState GetThemeState() => _themeState;
}
