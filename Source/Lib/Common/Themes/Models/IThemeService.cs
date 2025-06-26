using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Themes.Models;

public interface IThemeService
{
	public event Action? ThemeStateChanged;
	
	public ThemeState GetThemeState();

    public void RegisterAction(ThemeRecord theme);
    public void RegisterRangeAction(IReadOnlyList<ThemeRecord> theme);
    public void DisposeAction(Key<ThemeRecord> themeKey);
}
