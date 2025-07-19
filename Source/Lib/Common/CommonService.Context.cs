using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.ListExtensions;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
	private ContextState _contextState = new();
	private ContextSwitchState _contextSwitchState = new();
    
    public event Action? ContextStateChanged;
    public event Action? ContextSwitchStateChanged;
    
    public ContextState GetContextState() => _contextState;
    
    public ContextRecord GetContextRecord(Key<ContextRecord> contextKey) =>
    	_contextState.AllContextsList.FirstOrDefault(x => x.ContextKey == contextKey);
    
    public ContextSwitchState GetContextSwitchState() => _contextSwitchState;
    
    public void SetFocusedContextKey(Key<ContextRecord> contextKey)
    {
    	lock (_stateModificationLock)
    	{
	        _contextState = _contextState with
	        {
	            FocusedContextKey = contextKey
	        };
    	}

        ContextStateChanged?.Invoke();
    }
    
    public void SetContextKeymap(Key<ContextRecord> contextKey, IKeymap keymap)
    {
    	lock (_stateModificationLock)
    	{
	    	var inState = GetContextState();
	    	
	        var inContextRecord = inState.AllContextsList.FirstOrDefault(
	            x => x.ContextKey == contextKey);
	
	        if (inContextRecord != default)
	        {
	            _contextState = inState;
				goto finalize;
            }
	            
	        var index = inState.AllContextsList.FindIndex(x => x.ContextKey == inContextRecord.ContextKey);
	        if (index == -1)
	        {
	        	_contextState = inState;
                goto finalize;
            }
            
            var outAllContextsList = new List<ContextRecord>(inState.AllContextsList);
	
	        outAllContextsList[index] = inContextRecord with
	        {
	            Keymap = keymap
	        };
	
	        _contextState = inState with { AllContextsList = outAllContextsList };
            goto finalize;
        }

        finalize:
        ContextStateChanged?.Invoke();
    }
	
    public void RegisterContextSwitchGroup(ContextSwitchGroup contextSwitchGroup)
    {
    	lock (_stateModificationLock)
    	{
	    	var inState = GetContextSwitchState();
	    
	    	if (inState.ContextSwitchGroupList.Any(x =>
	    			x.Key == contextSwitchGroup.Key))
	    	{
                goto finalize;
            }
	    
	    	var outContextSwitchGroupList = new List<ContextSwitchGroup>(inState.ContextSwitchGroupList);
	    	outContextSwitchGroupList.Add(contextSwitchGroup);
	    
	        _contextSwitchState = inState with
	        {
	            ContextSwitchGroupList = outContextSwitchGroupList
	        };
	        
	        goto finalize;
	    }

        finalize:
        ContextStateChanged?.Invoke();
    }
}
