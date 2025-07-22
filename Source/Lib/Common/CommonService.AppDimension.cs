using Walk.Common.RazorLib.Reactives.Models;

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
    
    public AppDimensionState GetAppDimensionState() => _appDimensionState;
    
    public void SetAppDimensions(Func<AppDimensionState, AppDimensionState> withFunc)
    {
        lock (_stateModificationLock)
        {
            _appDimensionState = withFunc.Invoke(_appDimensionState);
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.AppDimensionStateChanged);
    }

    public void AppDimension_NotifyIntraAppResize(bool useExtraEvent = true)
    {
        CommonUiStateChanged?.Invoke(CommonUiEventKind.AppDimensionStateChanged);
        
        if (useExtraEvent)
            _debounceExtraEvent.Run(default);
    }

    public void AppDimension_NotifyUserAgentResize(bool useExtraEvent = true)
    {
        CommonUiStateChanged?.Invoke(CommonUiEventKind.AppDimensionStateChanged);
        
        if (useExtraEvent)
            _debounceExtraEvent.Run(default);
    }
}
