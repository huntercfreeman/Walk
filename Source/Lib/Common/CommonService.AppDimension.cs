namespace Walk.Common.RazorLib;

public partial class CommonService
{
    /// <summary>
    /// To avoid unexpected HTML movements when responding to a AppDimensionStateChanged
    /// this debounce will add 1 extra event after everything has "settled".
    ///
    /// `byte` is just a throwaway generic type, it isn't used.
    /// </summary>
    private readonly Debounce<byte> _debounceExtraEvent;
    
    private AppDimensionState _appDimensionState;
	
	public event Action? AppDimensionStateChanged;
	
	public AppDimensionState GetAppDimensionState() => _appDimensionState;
	
	public void SetAppDimensions(Func<AppDimensionState, AppDimensionState> withFunc)
	{
		lock (_stateModificationLock)
		{
			_appDimensionState = withFunc.Invoke(_appDimensionState);
        }

        AppDimensionStateChanged?.Invoke();
    }

	public void AppDimension_NotifyIntraAppResize(bool useExtraEvent = true)
	{
		AppDimensionStateChanged?.Invoke();
		
		if (useExtraEvent)
		    _debounceExtraEvent.Run(default);
    }

	public void AppDimension_NotifyUserAgentResize(bool useExtraEvent = true)
	{
		AppDimensionStateChanged?.Invoke();
		
		if (useExtraEvent)
		    _debounceExtraEvent.Run(default);
    }
}
