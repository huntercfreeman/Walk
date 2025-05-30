using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Themes.Models;

public interface IThemeService
{
	public event Action? ThemeStateChanged;
	
	public ThemeState GetThemeState();

    public void ReduceRegisterAction(ThemeRecord theme);
    public void ReduceDisposeAction(Key<ThemeRecord> themeKey);
}
