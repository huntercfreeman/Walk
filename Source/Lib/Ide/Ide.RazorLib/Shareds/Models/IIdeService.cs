using Walk.Common.RazorLib.Badges.Models;
using Walk.Common.RazorLib.Menus.Models;

namespace Walk.Ide.RazorLib.Shareds.Models;

public interface IIdeService
{
    public event Action? IdeStateChanged;
    
    public IdeState GetIdeState();
    
    public void RegisterFooterBadge(IBadgeModel badgeModel);
	
	public void SetMenuFile(MenuRecord menu);
	public void SetMenuTools(MenuRecord menu);
	public void SetMenuView(MenuRecord menu);
	public void SetMenuRun(MenuRecord menu);
	
	public void ModifyMenuFile(Func<MenuRecord, MenuRecord> menuFunc);
	public void ModifyMenuTools(Func<MenuRecord, MenuRecord> menuFunc);
	public void ModifyMenuView(Func<MenuRecord, MenuRecord> menuFunc);
	public void ModifyMenuRun(Func<MenuRecord, MenuRecord> menuFunc);
}


