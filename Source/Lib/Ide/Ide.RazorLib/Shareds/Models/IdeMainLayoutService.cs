using Walk.Common.RazorLib.Badges.Models;

namespace Walk.Ide.RazorLib.Shareds.Models;

public class IdeMainLayoutService : IIdeMainLayoutService
{
    private readonly object _stateModificationLock = new();

    private IdeMainLayoutState _ideMainLayoutState = new();
	
	public event Action? IdeMainLayoutStateChanged;
	
	public IdeMainLayoutState GetIdeMainLayoutState() => _ideMainLayoutState;

	public void RegisterFooterJustifyEndComponent(IBadgeModel badgeModel)
	{
		lock (_stateModificationLock)
		{
			var existingComponent = _ideMainLayoutState.FooterJustifyEndComponentList.FirstOrDefault(x =>
				x.Key == badgeModel.Key);

			if (existingComponent is null)
            {
    			var outFooterJustifyEndComponentList = new List<IBadgeModel>(_ideMainLayoutState.FooterJustifyEndComponentList);
    			outFooterJustifyEndComponentList.Add(badgeModel);
    
    			_ideMainLayoutState = _ideMainLayoutState with
    			{
    				FooterJustifyEndComponentList = outFooterJustifyEndComponentList
    			};
    	    }
		}

        IdeMainLayoutStateChanged?.Invoke();
    }
}
