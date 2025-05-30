using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Dropdowns.Models;

public interface IDropdownService
{
	public event Action? DropdownStateChanged;
	
	public DropdownState GetDropdownState();
	
    public void ReduceRegisterAction(DropdownRecord dropdown);
    public void ReduceDisposeAction(Key<DropdownRecord> key);
    public void ReduceClearAction();
    public void ReduceFitOnScreenAction(DropdownRecord dropdown);
}