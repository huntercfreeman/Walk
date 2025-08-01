namespace Walk.Common.RazorLib;

public partial class CommonService
{
    private ThemeState _themeState = new();
    
    public ThemeState GetThemeState() => _themeState;
}
