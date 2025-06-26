namespace Walk.Ide.RazorLib.Shareds.Models;

public class IdeMainLayoutService : IIdeMainLayoutService
{
    private readonly object _stateModificationLock = new();

    private IdeMainLayoutState _ideMainLayoutState = new();
	
	public event Action? IdeMainLayoutStateChanged;
	
	public IdeMainLayoutState GetIdeMainLayoutState() => _ideMainLayoutState;

	public void RegisterFooterJustifyEndComponent(FooterJustifyEndComponent footerJustifyEndComponent)
	{
		lock (_stateModificationLock)
		{
			var existingComponent = _ideMainLayoutState.FooterJustifyEndComponentList.FirstOrDefault(x =>
				x.Key == footerJustifyEndComponent.Key);

			if (existingComponent is null)
            {
    			var outFooterJustifyEndComponentList = new List<FooterJustifyEndComponent>(_ideMainLayoutState.FooterJustifyEndComponentList);
    			outFooterJustifyEndComponentList.Add(footerJustifyEndComponent);
    
    			_ideMainLayoutState = _ideMainLayoutState with
    			{
    				FooterJustifyEndComponentList = outFooterJustifyEndComponentList
    			};
    	    }
		}

        IdeMainLayoutStateChanged?.Invoke();
    }
}
