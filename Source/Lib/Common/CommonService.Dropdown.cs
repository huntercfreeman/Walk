using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Common.RazorLib.Dropdowns.Models;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    private DropdownState _dropdownState = new();
    
    public DropdownState GetDropdownState() => _dropdownState;
    
    public void Dropdown_ReduceRegisterAction(DropdownRecord dropdown)
    {
        var inState = GetDropdownState();
    
        var indexExistingDropdown = inState.DropdownList.FindIndex(
            x => x.Key == dropdown.Key);

        if (indexExistingDropdown != -1)
        {
            CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
            return;
        }

        var outDropdownList = new List<DropdownRecord>(inState.DropdownList);
        outDropdownList.Add(dropdown);

        _dropdownState = inState with
        {
            DropdownList = outDropdownList
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
        return;
    }

    public void Dropdown_ReduceDisposeAction(Key<DropdownRecord> key)
    {
        var inState = GetDropdownState();
    
        var indexExistingDropdown = inState.DropdownList.FindIndex(
            x => x.Key == key);

        if (indexExistingDropdown == -1)
        {
            CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
            return;
        }
            
        var outDropdownList = new List<DropdownRecord>(inState.DropdownList);
        outDropdownList.RemoveAt(indexExistingDropdown);

        _dropdownState = inState with
        {
            DropdownList = outDropdownList
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
        return;
    }

    public void Dropdown_ReduceClearAction()
    {
        var inState = GetDropdownState();
    
        var outDropdownList = new List<DropdownRecord>();
    
        _dropdownState = inState with
        {
            DropdownList = outDropdownList
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
        return;
    }

    public void Dropdown_ReduceFitOnScreenAction(DropdownRecord dropdown)
    {
        var inState = GetDropdownState();
    
        var indexExistingDropdown = inState.DropdownList.FindIndex(
            x => x.Key == dropdown.Key);

        if (indexExistingDropdown == -1)
        {
            CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
            return;
        }
        
        var inDropdown = inState.DropdownList[indexExistingDropdown];

        var outDropdown = inDropdown with
        {
            Width = dropdown.Width,
            Height = dropdown.Height,
            Left = dropdown.Left,
            Top = dropdown.Top
        };
        
        var outDropdownList = new List<DropdownRecord>(inState.DropdownList);
        outDropdownList[indexExistingDropdown] = outDropdown;

        _dropdownState = inState with
        {
            DropdownList = outDropdownList
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
        return;
    }
}
