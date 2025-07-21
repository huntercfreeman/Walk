using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Themes.Models;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    private ThemeState _themeState = new();
    
    public event Action? ThemeStateChanged;
    
    public ThemeState GetThemeState() => _themeState;

    public void Theme_RegisterAction(ThemeRecord theme)
    {
        var inTheme = _themeState.ThemeList.FirstOrDefault(
            x => x.Key == theme.Key);

        if (inTheme is not null)
            return;

        var outThemeList = new List<ThemeRecord>(_themeState.ThemeList);
        outThemeList.Add(theme);

        _themeState = new ThemeState { ThemeList = outThemeList };
        ThemeStateChanged?.Invoke();
    }
    
    public void Theme_RegisterRangeAction(IEnumerable<ThemeRecord> themeList)
    {
        var outThemeList = new List<ThemeRecord>(_themeState.ThemeList);
        
        foreach (var theme in themeList)
        {
            var inTheme = _themeState.ThemeList.FirstOrDefault(
                x => x.Key == theme.Key);
    
            if (inTheme is not null)
                return;
    
            outThemeList.Add(theme);
    
            _themeState = new ThemeState { ThemeList = outThemeList };
            ThemeStateChanged?.Invoke();
        }
    }

    public void Theme_DisposeAction(int themeKey)
    {
        var inTheme = _themeState.ThemeList.FirstOrDefault(
            x => x.Key == themeKey);

        if (inTheme is null)
            return;

        var outThemeList = new List<ThemeRecord>(_themeState.ThemeList);
        outThemeList.Remove(inTheme);

        _themeState = new ThemeState { ThemeList = outThemeList };
        ThemeStateChanged?.Invoke();
    }
}
