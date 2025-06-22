using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;

namespace Walk.Common.RazorLib.Contexts.Models;

public interface IContextService
{
	public event Action? ContextStateChanged;
    
    public ContextState GetContextState();
    
    public ContextRecord GetContextRecord(Key<ContextRecord> contextKey);
    
    public ContextSwitchState GetContextSwitchState();
    
    public void SetFocusedContextKey(Key<ContextRecord> contextKey);
    public void SetContextKeymap(Key<ContextRecord> contextKey, IKeymap keymap);
    
    public void RegisterContextSwitchGroup(ContextSwitchGroup contextSwitchGroup);
}
